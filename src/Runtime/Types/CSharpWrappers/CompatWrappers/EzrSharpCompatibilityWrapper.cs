using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using EzrSquared.Runtime.Types.Core.Text;
using EzrSquared.Runtime.WrapperAttributes;
using EzrSquared.Util;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers;

/// <summary>
/// Parent class for all automatic wrappers which wrap existing C# objects and members so that they can be used in ezr².
/// </summary>
public abstract class EzrSharpCompatibilityWrapper : EzrObject
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp wrapper";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpWrapper";

    /// <summary>
    /// The <see cref="SharpAutoWrapperAttribute"/> of the wrapped object, if defined.
    /// </summary>
    public readonly SharpAutoWrapperAttribute? AutoWrapperAttribute;

    /// <summary>
    /// The name of the wrapped member in snake_case.
    /// </summary>
    public readonly string SharpMemberName;

    /// <summary>
    /// Reflection info for the current object being wrapped.
    /// </summary>
    public readonly MemberInfo SharpMember;

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityWrapper"/>.
    /// </summary>
    /// <param name="wrappedMember">Reflection info on the wrapped C# member.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrSharpCompatibilityWrapper(MemberInfo wrappedMember, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        SharpMember = wrappedMember;
        AutoWrapperAttribute = wrappedMember.GetCustomAttribute<SharpAutoWrapperAttribute>();
        SharpMemberName = !string.IsNullOrEmpty(AutoWrapperAttribute?.Name) ? AutoWrapperAttribute.Name : Utils.PascalToSnakeCase(wrappedMember.Name);
    }

    /// <summary>
    /// Validates the the current object for wrapping.
    /// </summary>
    /// <param name="result">Optional runtime result for carrying runtime errors.</param>
    /// <returns><see langword="true"/> if the member can be wrapped, <see langword="false"/> otherwise.</returns>
    public bool Validate(RuntimeResult? result=null)
    {
        Exception? validationException = SharpAutoWrapperAttribute.Validate(SharpMember);
        if (validationException is null)
            return true;

        if (AutoWrapperAttribute is not null)
            throw validationException;

        result?.Failure(new EzrUnsupportedWrappingError($"C# type member \"{SharpMemberName}\" cannot be wrapped into an ezr² type!", Context, StartPosition, EndPosition));
        return false;
    }

    /// <summary>
    /// Checks if the given type is supported by the compatibility wrappers.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if yes, <see langword="false"/> otherwise.</returns>
    public static bool IsSupportedType(Type type)
    {
        return typeof(IEzrObject).IsAssignableFrom(type) || Type.GetTypeCode(type) is TypeCode.Empty
                        or TypeCode.Int16
                        or TypeCode.Int32
                        or TypeCode.Int64
                        or TypeCode.UInt16
                        or TypeCode.UInt32
                        or TypeCode.UInt64
                        or TypeCode.Byte
                        or TypeCode.SByte
                        or TypeCode.Single
                        or TypeCode.Double
                        or TypeCode.Decimal
                        or TypeCode.Boolean
                        or TypeCode.Char
                        or TypeCode.String;
    }

    /// <summary>
    /// Checks if the given type is supported by the compatibility wrappers, including generic <see cref="Task"/> objects.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if yes, <see langword="false"/> otherwise.</returns>
    public static bool IsSupportedReturnType(Type type)
    {
        return Type.GetTypeCode(type) switch
        {
            TypeCode.Object when typeof(Task).IsAssignableFrom(type) => type.GenericTypeArguments.Length == 0 || IsSupportedType(type.GenericTypeArguments[0]),
            _ => IsSupportedType(type),
        };
    }

    /// <summary>
    /// Converts an ezr² type to a C# type.
    /// </summary>
    /// <param name="value">The <see cref="IEzrObject"/> to convert.</param>
    /// <param name="targetType">The type to convert it to.</param>
    /// <param name="result">Runtime result for carrying any errors.</param>
    /// <returns>The converted object.</returns>
    protected internal object? EzrObjectToCSharp(IEzrObject value, Type targetType, RuntimeResult result)
    {
        if (targetType.IsAssignableFrom(value.GetType()))
            return value;
        else if (typeof(IEzrObject).IsAssignableFrom(targetType))
        {
            result.Failure(new EzrUnexpectedTypeError($"Expected ezr² object of type \"{targetType.Name}\" (this is the name of the object in C#), but got object of type \"{value.TypeName}\".", Context, value.StartPosition, value.EndPosition));
            return null;
        }

        switch (Type.GetTypeCode(targetType))
        {
            case TypeCode.Int16:
                if (value is EzrInteger integer16Value)
                    if (integer16Value.TryGetIntRepresentation(out int output))
                        if (output is < short.MinValue or > short.MaxValue)
                            result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                        else
                            return (short)output;
                    else
                        result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                else
                    result.Failure(new EzrUnexpectedTypeError($"Expected integer, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;
            case TypeCode.Int32:
                if (value is EzrInteger integer32Value)
                    if (integer32Value.TryGetIntRepresentation(out int output))
                        return output;
                    else
                        result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                else
                    result.Failure(new EzrUnexpectedTypeError($"Expected integer, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;
            case TypeCode.Int64:
                if (value is EzrInteger integer64Value)
                    if (integer64Value.TryGetLongRepresentation(out long output))
                        return output;
                    else
                        result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                else
                    result.Failure(new EzrUnexpectedTypeError($"Expected integer, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;
            case TypeCode.UInt16:
                if (value is EzrInteger unsignedInteger16Value)
                    if (unsignedInteger16Value.TryGetIntRepresentation(out int output))
                        if (output < 0)
                            result.Failure(new EzrValueOutOfRangeError($"Expected integer of value greater than or equal to 0, but got {output}!", Context, value.StartPosition, value.EndPosition));
                        else if (output > ushort.MaxValue)
                            result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                        else
                            return (ushort)output;
                    else
                        result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                else
                    result.Failure(new EzrUnexpectedTypeError($"Expected integer, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;
            case TypeCode.UInt32:
                if (value is EzrInteger unsignedInteger32Value)
                    if (unsignedInteger32Value.TryGetIntRepresentation(out int output))
                        if (output < 0)
                            result.Failure(new EzrValueOutOfRangeError($"Expected integer of value greater than or equal to 0, but got {output}!", Context, value.StartPosition, value.EndPosition));
                        else
                            return (uint)output;
                    else
                        result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                else
                    result.Failure(new EzrUnexpectedTypeError($"Expected integer, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;
            case TypeCode.UInt64:
                if (value is EzrInteger unsignedInteger64Value)
                    if (unsignedInteger64Value.TryGetLongRepresentation(out long output))
                        if (output < 0)
                            result.Failure(new EzrValueOutOfRangeError($"Expected integer of value greater than or equal to 0, but got {output}!", Context, value.StartPosition, value.EndPosition));
                        else
                            return (ulong)output;
                    else
                        result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                else
                    result.Failure(new EzrUnexpectedTypeError($"Expected integer, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;
            case TypeCode.Byte:
                if (value is EzrInteger byteValue)
                    if (byteValue.TryGetIntRepresentation(out int output))
                        if (output < 0)
                            result.Failure(new EzrValueOutOfRangeError($"Expected integer of value greater than or equal to 0, but got {output}!", Context, value.StartPosition, value.EndPosition));
                        else if (output > byte.MaxValue)
                            result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                        else
                            return (byte)output;
                    else
                        result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                else
                    result.Failure(new EzrUnexpectedTypeError($"Expected integer, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;
            case TypeCode.SByte:
                if (value is EzrInteger signedByteValue)
                    if (signedByteValue.TryGetIntRepresentation(out int output))
                        if (output is < sbyte.MinValue or > sbyte.MaxValue)
                            result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                        else
                            return (sbyte)output;
                    else
                        result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                else
                    result.Failure(new EzrUnexpectedTypeError($"Expected integer, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;
            case TypeCode.Single:
                if (value is EzrFloat floatValue)
                {
                    double output = floatValue.Value;
                    if (output is < float.MinValue or > float.MaxValue)
                        result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                    else
                        return (float)output;
                }
                else if (value is EzrInteger integerValue)
                {
                    double output = integerValue.GetDoubleRepresentation();
                    if (output is < float.MinValue or > float.MaxValue)
                        result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                    else
                        return (float)output;
                }
                else
                    result.Failure(new EzrUnexpectedTypeError($"Expected float, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;
            case TypeCode.Double:
                if (value is EzrFloat doubleValue)
                    return doubleValue.Value;
                else if (value is EzrInteger integerValue)
                {
                    double output = integerValue.GetDoubleRepresentation();
                    if (output is < double.MinValue or > double.MaxValue)
                        result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                    else
                        return output;
                }
                else
                    result.Failure(new EzrUnexpectedTypeError($"Expected float, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;
            case TypeCode.Decimal:
                if (value is EzrFloat decimalValue)
                {
                    double output = decimalValue.Value;
                    if (output is < (double)decimal.MinValue or > (double)decimal.MaxValue)
                        result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                    else
                        return (decimal)output;
                }
                else if (value is EzrInteger integerValue)
                {
                    double output = integerValue.GetDoubleRepresentation();
                    if (output is < (double)decimal.MinValue or > (double)decimal.MaxValue)
                        result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", Context, value.StartPosition, value.EndPosition));
                    else
                        return (decimal)output;
                }
                else
                    result.Failure(new EzrUnexpectedTypeError($"Expected float, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
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
            case TypeCode.Empty:
                if (value is EzrNothing)
                    return null;

                result.Failure(new EzrUnexpectedTypeError($"Expected type nothing, but got object of type \"{value.TypeName}\"!", Context, value.StartPosition, value.EndPosition));
                break;
            default:
                result.Failure(new EzrUnsupportedWrappingError($"Object of type \"{value.TypeName}\" cannot be converted to CSharp type \"{targetType.Name}\"!", Context, value.StartPosition, value.EndPosition));
                break;
        }

        return 0;
    }

    /// <summary>
    /// Converts a C# primitive type object or a task which returns a C# primitive type object to an ezr² object.
    /// </summary>
    /// <param name="value">The C# object to convert.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    /// <returns>The converted <see cref="IEzrObject"/>.</returns>
    protected internal void CSharpToEzrObject(object? value, RuntimeResult result)
    {
        Type? valueType = value?.GetType();
        if (value is null || valueType is null)
        {
            result.Success(NewNothingConstant());
            return;
        }

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
            case TypeCode.Object when typeof(IEzrObject).IsAssignableFrom(valueType):
                result.Success(ReferencePool.Get((IEzrObject)value, AccessMod.PrivateConstant));
                break;
            case TypeCode.Object when typeof(Task).IsAssignableFrom(valueType):
                HandleAsynchronousObject(value, valueType, result);
                break;
            default:
                result.Failure(new EzrUnsupportedWrappingError($"CSharp type \"{value.GetType().Name}\" cannot be converted to an ezr² type!", Context, StartPosition, EndPosition));
                break;
        }
    }

    /// <summary>
    /// Waits for a task to complete and returns the result as an ezr² object.
    /// </summary>
    /// <param name="value">The task to await.</param>
    /// <param name="type">The type of the task.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    protected internal void HandleAsynchronousObject(
        object value,

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
        Type type,

        RuntimeResult result)
    {
        try
        {
            ((Task)value).Wait();

            // Get the Result property of the Task<TResult>
            PropertyInfo? resultProperty = type.GetProperty("Result");

            // Check if it's a Task<TResult>
            if (resultProperty is not null)
            {
                // Get the value of the Result property
                object taskResult = resultProperty.GetValue(value)!;

                CSharpToEzrObject(taskResult, result);
            }
            else
                result.Success(NewNothingConstant());
        }
        catch (Exception error)
        {
            result.Failure(new EzrWrapperExecutionError(error.InnerException?.Message ?? error.Message, Context, StartPosition, EndPosition));
        }
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
}
