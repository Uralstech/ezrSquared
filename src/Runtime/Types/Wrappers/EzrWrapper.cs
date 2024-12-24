using EzrSquared.Runtime.Types.Collections;
using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using EzrSquared.Runtime.Types.Core.Text;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EzrSquared.Runtime.Types.Wrappers;

/// <summary>
/// Parent class for all automatic wrappers which wrap existing C# objects and members so that they can be used in ezr².
/// </summary>
/// <typeparam name="TMemberInfo">The <see cref="MemberInfo"/> type for the C# member being wrapped.</typeparam>

#if NET7_0_OR_GREATER
public abstract partial class EzrWrapper<TMemberInfo> : EzrObject
    where TMemberInfo : MemberInfo
#else
public abstract class EzrWrapper<TMemberInfo> : EzrObject
    where TMemberInfo : MemberInfo
#endif
{
    /// <summary>
    /// Reflection info for <see cref="Task.FromResult{TResult}(TResult)"/>/
    /// </summary>
    private static readonly MethodInfo s_taskFromResultMethod = typeof(Task).GetMethod(nameof(Task.FromResult))!;

    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp wrapper";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpWrapper";

    /// <summary>
    /// The <see cref="WrapMemberAttribute"/> of the wrapped object, if defined.
    /// </summary>
    public readonly WrapMemberAttribute? AutoWrapperAttribute;

    /// <summary>
    /// The name of the wrapped member in snake_case.
    /// </summary>
    public readonly string SharpMemberName;

    /// <summary>
    /// Reflection info for the current object being wrapped.
    /// </summary>
    public readonly TMemberInfo SharpMember;

    /// <summary>
    /// The object which contains the wrapped member, <see langword="null"/> if static.
    /// </summary>
    public readonly object? Instance;

    /// <summary>
    /// Creates a new <see cref="EzrWrapper{TMemberInfo}"/>.
    /// </summary>
    /// <param name="wrappedMember">Reflection info on the wrapped C# member.</param>
    /// <param name="instance">The object which contains the wrapped member, <see langword="null"/> if static.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrWrapper(TMemberInfo wrappedMember, object? instance, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        SharpMember = wrappedMember;
        AutoWrapperAttribute = wrappedMember.GetCustomAttribute<WrapMemberAttribute>();
        SharpMemberName = !string.IsNullOrEmpty(AutoWrapperAttribute?.Name) ? AutoWrapperAttribute.Name : PascalToSnakeCase(wrappedMember.Name)!;
        Instance = instance;
    }

    /// <summary>
    /// Regex for converting PascalCase/camelCase to snake_case.
    /// </summary>
    /// <remarks>
    /// Regex explanation:<br/>
    /// 1. (?&lt;!^)(?=[A-Z][a-z]) - Add underscore before capital letter followed by a lowercase letter, except at the start.<br/>
    /// 2. (?&lt;=[a-z0-9])(?=[A-Z]) - Add underscore when transitioning from lowercase or number to uppercase.<br/>
    /// 3. (?&lt;=[A-Z])(?=[A-Z][a-z]) - Add underscore between uppercase sequences followed by lowercase (e.g., "TCProtocol").
    /// </remarks>
    private static readonly Regex s_caseConverterRegex = GetCaseConverterRegex();

    /// <summary>
    /// Regex for matching non-alphanumeric + underscore characters.
    /// </summary>
    private static readonly Regex s_alphaNumericUnderscoreOnlyFilterRegex = GetAlphaNumericUnderscoreOnlyFilterRegex();

    /// <summary>
    /// Converts a string from PascalCase to snake_case.
    /// </summary>
    /// <param name="text">The text to convert in PascalCase.</param>
    /// <returns>The converted text in snake_case.</returns>
    internal protected static string? PascalToSnakeCase(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Remove any leading underscore
        if (text.StartsWith('_'))
            text = text[1..];

        string alphaNumPlusUnderscoreFiltered = s_alphaNumericUnderscoreOnlyFilterRegex.Replace(text, string.Empty);
        string snakeCase = s_caseConverterRegex.Replace(alphaNumPlusUnderscoreFiltered, "_");

        // Convert entire string to lowercase
        return snakeCase.ToLower();
    }

    /// <summary>
    /// Converts an ezr² object to a C# object.
    /// </summary>
    /// <param name="value">The <see cref="IEzrObject"/> to convert.</param>
    /// <param name="targetType">The type to convert it to.</param>
    /// <param name="result">Runtime result for carrying any errors.</param>
    /// <returns>The converted object.</returns>
    protected internal object? EzrObjectToCSharp(IEzrObject value, Type targetType, RuntimeResult result)
    {
        switch (Type.GetTypeCode(targetType))
        {
            case TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 or TypeCode.Byte or TypeCode.SByte when value is not EzrFloat and not EzrInteger:
                result.Failure(new EzrUnexpectedTypeError($"Expected integer or float, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.Int16:
                double outputShort = (value as EzrFloat)?.Value ?? ((EzrInteger)value).GetDoubleRepresentation();
                if (outputShort is >= short.MinValue and <= short.MaxValue)
                    return (short)outputShort;

                result.Failure(new EzrValueOutOfRangeError($"Expected value to be between {short.MinValue} - {short.MaxValue} (inclusive)!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.Int32:
                double outputInt = (value as EzrFloat)?.Value ?? ((EzrInteger)value).GetDoubleRepresentation();
                if (outputInt is >= int.MinValue and <= int.MaxValue)
                    return (int)outputInt;

                result.Failure(new EzrValueOutOfRangeError($"Expected value to be between {int.MinValue} - {int.MaxValue} (inclusive)!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.Int64:
                double outputLong = (value as EzrFloat)?.Value ?? ((EzrInteger)value).GetDoubleRepresentation();
                if (outputLong is >= long.MinValue and <= long.MaxValue)
                    return (long)outputLong;

                result.Failure(new EzrValueOutOfRangeError($"Expected value to be between {long.MinValue} - {long.MaxValue} (inclusive)!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.Object when typeof(BigInteger).IsAssignableFrom(targetType):
                if (value is EzrInteger or EzrFloat)
                {
                    return value is EzrInteger ezrInteger
                        ? ezrInteger.Value
                        : new BigInteger(((EzrFloat)value).Value);
                }

                result.Failure(new EzrUnexpectedTypeError($"Expected integer or float, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.UInt16:
                double outputUShort = (value as EzrFloat)?.Value ?? ((EzrInteger)value).GetDoubleRepresentation();
                if (outputUShort is >= ushort.MinValue and <= ushort.MaxValue)
                    return (ushort)outputUShort;

                result.Failure(new EzrValueOutOfRangeError($"Expected value to be between {ushort.MinValue} - {ushort.MaxValue} (inclusive)!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.UInt32:
                double outputUInt = (value as EzrFloat)?.Value ?? ((EzrInteger)value).GetDoubleRepresentation();
                if (outputUInt is >= uint.MinValue and <= uint.MaxValue)
                    return (uint)outputUInt;

                result.Failure(new EzrValueOutOfRangeError($"Expected value to be between {uint.MinValue} - {uint.MaxValue} (inclusive)!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.UInt64:
                double outputULong = (value as EzrFloat)?.Value ?? ((EzrInteger)value).GetDoubleRepresentation();
                if (outputULong is >= ulong.MinValue and <= ulong.MaxValue)
                    return (ulong)outputULong;

                result.Failure(new EzrValueOutOfRangeError($"Expected value to be between {ulong.MinValue} - {ulong.MaxValue} (inclusive)!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.Byte:
                double outputByte = (value as EzrFloat)?.Value ?? ((EzrInteger)value).GetDoubleRepresentation();
                if (outputByte is >= byte.MinValue and <= byte.MaxValue)
                    return (byte)outputByte;

                result.Failure(new EzrValueOutOfRangeError($"Expected value to be between {byte.MinValue} - {byte.MaxValue} (inclusive)!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.SByte:
                double outputSByte = (value as EzrFloat)?.Value ?? ((EzrInteger)value).GetDoubleRepresentation();
                if (outputSByte is >= sbyte.MinValue and <= sbyte.MaxValue)
                    return (sbyte)outputSByte;

                result.Failure(new EzrValueOutOfRangeError($"Expected value to be between {sbyte.MinValue} - {sbyte.MaxValue} (inclusive)!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.Single or TypeCode.Double or TypeCode.Decimal when value is not EzrFloat and not EzrInteger:
                result.Failure(new EzrUnexpectedTypeError($"Expected integer or float, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.Single:
                double outputSingle = (value as EzrFloat)?.Value ?? ((EzrInteger)value).GetDoubleRepresentation();
                if (outputSingle is >= float.MinValue and <= float.MaxValue)
                    return (float)outputSingle;

                result.Failure(new EzrValueOutOfRangeError($"Expected value to be between {float.MinValue} - {float.MaxValue} (inclusive)!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.Double:
                double outputDouble = (value as EzrFloat)?.Value ?? ((EzrInteger)value).GetDoubleRepresentation();
                if (outputDouble is >= double.MinValue or <= double.MaxValue)
                    return outputDouble;

                result.Failure(new EzrValueOutOfRangeError($"Expected value to be between {double.MinValue} - {double.MaxValue} (inclusive)!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.Decimal:
                double outputDecimal = (value as EzrFloat)?.Value ?? ((EzrInteger)value).GetDoubleRepresentation();
                if (outputDecimal >= (double)decimal.MinValue && outputDecimal <= (double)decimal.MaxValue)
                    return (decimal)outputDecimal;

                result.Failure(new EzrValueOutOfRangeError($"Expected value to be between {decimal.MinValue} - {decimal.MaxValue} (inclusive)!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.Boolean:
                if (value is EzrBoolean booleanValue)
                    return booleanValue.Value;

                result.Failure(new EzrUnexpectedTypeError($"Expected boolean, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.Char:
                if (value is EzrCharacter characterValue)
                    return characterValue.Value;

                result.Failure(new EzrUnexpectedTypeError($"Expected character, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.String:
                if (value is IEzrString ezrString)
                    return ezrString.StringValue;

                result.Failure(new EzrUnexpectedTypeError($"Expected string, character or character list, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.Object when Nullable.GetUnderlyingType(targetType) is Type underlyingType:
                return value is EzrNothing ? null : EzrObjectToCSharp(value, underlyingType, result);

            case TypeCode.Object when targetType.IsAssignableFrom(value.GetType()):
                return value;

            case TypeCode.Object when typeof(IEzrObject).IsAssignableFrom(targetType):
                result.Failure(new EzrUnexpectedTypeError($"Expected ezr² object of type \"{targetType.Name}\" (this is the name of the type in C#), but got object of type \"{value.TypeName}\".", Context, value.StartPosition, value.EndPosition));
                break;

            case TypeCode.Object when targetType.IsArray && targetType.HasElementType:
                return HandleEzrArrayLikeToCSharp(value, targetType, result);

            case TypeCode.Object when targetType == typeof(Task):
                if (value is EzrObjectWrapper taskWrapper && targetType.IsAssignableFrom(taskWrapper.SharpMember))
                    return (Task)taskWrapper.Instance!;

                return s_taskFromResultMethod.MakeGenericMethod(value.GetType()).Invoke(null, [value]);

            case TypeCode.Object when typeof(Task).IsAssignableFrom(targetType) && !targetType.IsGenericTypeDefinition:
                Type taskTargetType = targetType.GetGenericArguments()[0];
                object? convertedObject = EzrObjectToCSharp(value, taskTargetType, result);
                if (result.ShouldReturn)
                    break;

                MethodInfo completedTaskMethod = s_taskFromResultMethod.MakeGenericMethod(taskTargetType);
                return completedTaskMethod.Invoke(null, [convertedObject]);

            case TypeCode.Empty:
                if (value is not EzrNothing)
                    result.Failure(new EzrUnexpectedTypeError($"Expected type nothing, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));

                break;

            default:
                if (value is EzrObjectWrapper wrapper && targetType.IsAssignableFrom(wrapper.SharpMember))
                    return wrapper.Instance;

                result.Failure(new EzrUnexpectedTypeError($"Expected wrapped object of C# type \"{targetType.Name}\", but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;
        }

        return null;
    }

    /// <summary>
    /// Converts an ezr² array-like object to a C# array.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="result">Runtime result for carrying any errors.</param>
    protected internal object? HandleEzrArrayLikeToCSharp(IEzrObject value, Type targetType, RuntimeResult result)
    {
        if (value is not IEzrIndexedCollection ezrIndexedCollection)
        {
            result.Failure(new EzrUnexpectedTypeError($"Expected array or list, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
            return null;
        }

        Type arrayElementType = targetType.GetElementType()!;
        Array array = Array.CreateInstance(arrayElementType, ezrIndexedCollection.Count);

        using IEnumerator<IEzrObject> enumerator = ezrIndexedCollection.GetEnumerator();
        for (int i = 0; enumerator.MoveNext(); i++)
        {
            object? element = EzrObjectToCSharp(enumerator.Current, arrayElementType, result);
            if (result.ShouldReturn)
                return null;

            array.SetValue(element, i);
        }

        return array;
    }

    /// <summary>
    /// Converts a C# object to an ezr² object.
    /// </summary>
    /// <param name="value">The C# object to convert.</param>
    /// <param name="valueType">The type of <paramref name="value"/>.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    /// <returns>The converted <see cref="IEzrObject"/>.</returns>
    protected internal void CSharpToEzrObject(object? value, Type valueType, RuntimeResult result)
    {
        if (value is null)
        {
            result.Success(NewNothingConstant());
            return;
        }

        if (valueType == typeof(object))
            valueType = value.GetType();

        switch (Type.GetTypeCode(valueType))
        {
            case TypeCode.Int16:
                result.Success(NewIntegerConstant((short)value));
                break;
            case TypeCode.Int32:
                result.Success(NewIntegerConstant((int)value));
                break;
            case TypeCode.Int64:
                result.Success(NewIntegerConstant((long)value));
                break;
            case TypeCode.UInt16:
                result.Success(NewIntegerConstant((ushort)value));
                break;
            case TypeCode.UInt32:
                result.Success(NewIntegerConstant((uint)value));
                break;
            case TypeCode.UInt64:
                result.Success(NewIntegerConstant((ulong)value));
                break;
            case TypeCode.Byte:
                result.Success(NewIntegerConstant((byte)value));
                break;
            case TypeCode.SByte:
                result.Success(NewIntegerConstant((sbyte)value));
                break;
            case TypeCode.Single:
                result.Success(NewFloatConstant((float)value));
                break;
            case TypeCode.Double:
                result.Success(NewFloatConstant((double)value));
                break;
            case TypeCode.Decimal:
                result.Success(NewFloatConstant((double)(decimal)value));
                break;
            case TypeCode.Boolean:
                result.Success(NewBooleanConstant((bool)value));
                break;
            case TypeCode.Char:
                result.Success(NewCharacterConstant((char)value));
                break;
            case TypeCode.String:
                result.Success(NewStringConstant((string)value));
                break;
            case TypeCode.Object when typeof(BigInteger).IsAssignableFrom(valueType):
                result.Success(NewIntegerConstant((BigInteger)value));
                break;
            case TypeCode.Object when valueType.IsArray && valueType.HasElementType:
                HandleCSharpArrayToEzrObject((Array)value, valueType, result);
                break;
            case TypeCode.Object when typeof(IEzrObject).IsAssignableFrom(valueType):
                result.Success(ReferencePool.Get((IEzrObject)value, AccessMod.PrivateConstant));
                break;
            case TypeCode.Empty:
                result.Success(NewNothingConstant());
                break;
            default:
                result.Success(ReferencePool.Get(new EzrObjectWrapper(value, valueType, _executionContext, StartPosition, EndPosition), AccessMod.PrivateConstant));
                break;
        }
    }

    /// <summary>
    /// Converts a C# array to an ezr² array-like object.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="valueType">The type to convert from.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    protected internal void HandleCSharpArrayToEzrObject(Array value, Type valueType, RuntimeResult result)
    {
        Type arrayElementType = valueType.GetElementType()!;
        IEzrObject[] elements = new IEzrObject[value.Length];

        for (int i = 0; i < value.Length; i++)
        {
            CSharpToEzrObject(value.GetValue(i), arrayElementType, result);
            if (result.ShouldReturn)
                return;

            elements[i] = result.Reference.Object;
        }

        result.Success(NewArrayConstant(elements));
    }

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        bool equal = StrictEquals(other, result);
        if (result.ShouldReturn)
            return;

        result.Success(NewBooleanConstant(equal));
    }

    /// <inheritdoc/>
    public override void ComparisonNotEqual(IEzrObject other, RuntimeResult result)
    {
        bool equal = StrictEquals(other, result);
        if (result.ShouldReturn)
            return;

        result.Success(NewBooleanConstant(!equal));
    }

    /// <inheritdoc/>
    public override bool EvaluateBoolean(RuntimeResult result)
    {
        return true;
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return other is EzrWrapper<TMemberInfo> otherWrapper
            && otherWrapper.SharpMember == SharpMember
            && otherWrapper.Instance?.GetHashCode() == Instance?.GetHashCode()
            && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return Instance is not null
            ? HashCode.Combine(HashTag, SharpMember, Instance)
            : HashCode.Combine(HashTag, SharpMember);
    }


#if NET7_0_OR_GREATER
    [GeneratedRegex(@"(?<!^)(?=[A-Z][a-z])|(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex GetCaseConverterRegex();

    [GeneratedRegex(@"[^a-zA-Z0-9_]", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex GetAlphaNumericUnderscoreOnlyFilterRegex();
#else
    private static Regex GetCaseConverterRegex()
    {
        return new Regex(@"(?<!^)(?=[A-Z][a-z])|(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }

    private static Regex GetAlphaNumericUnderscoreOnlyFilterRegex()
    {
        return new Regex(@"[^a-zA-Z0-9_]", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }
#endif
}
