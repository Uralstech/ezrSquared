using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.Attributes;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers;
using EzrSquared.Runtime.WrapperAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace EzrSquared.Runtime.Types.Collections;

/// <summary>
/// The immutable, array type object.
/// </summary>
public class EzrArray : EzrObject, IEzrIndexedCollection
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "array";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.Array";

    /// <summary>
    /// The array value.
    /// </summary>
    public readonly IEzrObject[] Value;

    /// <inheritdoc/>
    [WrappedMember("length")]
    public int Count { get; }

    /// <summary>
    /// Creates a new <see cref="EzrArray"/>.
    /// </summary>
    /// <param name="elements">The base value.</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrArray(IEzrObject[] elements, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        Value = elements;
        Count = elements.Length;

        Context.Set(null, "length", ReferencePool.Get(new EzrSharpCompatibilityProperty(GetMemberInfo<PropertyInfo, EzrArray>(nameof(Count))!, this, Context, StartPosition, EndPosition), AccessMod.Constant));
    }

    /// <summary>
    /// Compares the current array with another collection.
    /// </summary>
    /// <param name="other">The other collection.</param>
    /// <param name="result">Runtime result to carray any errors.</param>
    /// <returns>The result of the comparison.</returns>
    private bool Compare(IEzrIndexedCollection other, RuntimeResult result)
    {
        if (Value.Length != other.Count)
            return false;

        for (int i = 0; i < Value.Length; i++)
            if (!Value[i].StrictEquals(other.At(i), result) || result.ShouldReturn)
                return false;

        return true;
    }

    /// <summary>
    /// Checks if the specified object is contained in the current array.
    /// </summary>
    /// <param name="ezrObject">The object to check.</param>
    /// <param name="result">Runtime result to carray any errors.</param>
    /// <returns>The result of the check.</returns>
    private bool Contains(IEzrObject ezrObject, RuntimeResult result)
    {
        for (int i = 0; i < Value.Length; i++)
        {
            bool isEqual = ezrObject.StrictEquals(Value[i], result);
            if (isEqual || result.ShouldReturn)
                return isEqual;
        }

        return false;
    }

    /// <inheritdoc/>
    public IEzrObject At(int index)
    {
        return Value[index];
    }

    /// <inheritdoc/>
    public IEnumerator<IEzrObject> GetEnumerator()
    {
        return ((IEnumerable<IEzrObject>)Value).GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<IEzrObject> GetEnumerator(RuntimeResult result)
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return Value.GetEnumerator();
    }

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        bool compareResult;
        switch (other)
        {
            case IEzrIndexedCollection otherCollection:
                compareResult = Compare(otherCollection, result);
                if (result.ShouldReturn)
                    break;

                result.Success(NewBooleanConstant(compareResult)); break;

            default:
                result.Success(NewBooleanConstant(false)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonNotEqual(IEzrObject other, RuntimeResult result)
    {
        bool compareResult;
        switch (other)
        {
            case IEzrIndexedCollection otherCollection:
                compareResult = Compare(otherCollection, result);
                if (result.ShouldReturn)
                    break;

                result.Success(NewBooleanConstant(!compareResult)); break;

            default:
                result.Success(NewBooleanConstant(true)); break;
        }
    }

    /// <summary>
    /// Gets the object(s) at the specified index/indices.
    /// </summary>
    /// <inheritdoc/>
    public override void ComparisonLessThan(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger when Value.Length == 0:
            case IEzrIndexedCollection when Value.Length == 0:
                result.Failure(new EzrValueOutOfRangeError("The array is empty and cannot be indexed!", _executionContext, StartPosition, EndPosition)); break;

            case EzrInteger otherInteger when otherInteger.Value < -Value.Length || otherInteger.Value >= Value.Length:
                result.Failure(new EzrValueOutOfRangeError($"Index must be in range 0 to {Value.Length - 1} or -1 to {-Value.Length}!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int index):
                result.Success(ReferencePool.Get(index >= 0 ? Value[index] : Value[^-index], AccessMod.PrivateConstant)); break;

            case EzrInteger:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection { Count: < 2 }:
                result.Failure(new EzrIllegalOperationError($"The indices {other.TypeName} must contain two values, the starting index and the ending index!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection { Count: > 2 }:
                result.Failure(new EzrIllegalOperationError($"The indices {other.TypeName} must only contain two values, the starting index and the ending index!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection otherCollection:
                (IEzrObject index1, IEzrObject index2) = (otherCollection.At(0), otherCollection.At(1));
                if (index1 is not EzrInteger startIndex || index2 is not EzrInteger endIndex)
                {
                    result.Failure(new EzrUnexpectedTypeError($"Both indices must be integers! Given indices were of types \"{index1.TypeName}\" (start) and \"{index2.TypeName}\" (end).", _executionContext, other.StartPosition, other.EndPosition));
                    break;
                }

                if (!startIndex.TryGetIntRepresentation(out int startIndexInt))
                {
                    result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, startIndex.StartPosition, startIndex.EndPosition));
                    break;
                }

                if (!endIndex.TryGetIntRepresentation(out int endIndexInt))
                {
                    result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                    break;
                }

                if (startIndexInt < -Value.Length || startIndexInt >= Value.Length)
                {
                    result.Failure(new EzrValueOutOfRangeError($"Starting index must be in range 0 to {Value.Length - 1} or -1 to {-Value.Length}!", _executionContext, startIndex.StartPosition, startIndex.EndPosition));
                    break;
                }

                if (startIndexInt < 0)
                {
                    if (endIndexInt > startIndexInt || endIndexInt < -Value.Length)
                    {
                        result.Failure(new EzrValueOutOfRangeError($"Ending index must be in range {startIndexInt} to {-Value.Length}!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                        break;
                    }

                    result.Success(NewArrayConstant(Value[^(-endIndexInt)..^(-startIndexInt - 1)]));
                    break;
                }

                if (endIndexInt < startIndexInt || endIndexInt >= Value.Length)
                {
                    result.Failure(new EzrValueOutOfRangeError($"Ending index must be in range {startIndexInt} to {Value.Length - 1}!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                    break;
                }

                result.Success(NewArrayConstant(Value[startIndexInt..(endIndexInt + 1)]));
                break;

            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <summary>
    /// Gets the object(s) at the specified index/indices.
    /// </summary>
    /// <inheritdoc/>
    public override void ComparisonLessThanOrEqual(IEzrObject other, RuntimeResult result)
    {
        ComparisonLessThan(other, result);
    }

    /// <summary>
    /// Creates a copy of the current object with the other object appended. If the other object is an array, appends its elements.
    /// </summary>
    /// <inheritdoc/>
    public override void Addition(IEzrObject other, RuntimeResult result)
    {
        IEzrObject[] newArray;
        switch (other)
        {
            case EzrArray otherArray:
                newArray = new IEzrObject[Value.Length + otherArray.Count];

                Array.Copy(Value, newArray, Value.Length);
                Array.Copy(otherArray.Value, 0, newArray, Value.Length, otherArray.Count);

                break;

            default:
                newArray = new IEzrObject[Value.Length + 1];
                Array.Copy(Value, newArray, Value.Length);

                newArray[^1] = other; break;
        }

        result.Success(NewArrayConstant(newArray));
    }

    /// <summary>
    /// Creates a copy of the current object with the objects at the specified index/indices removed.
    /// </summary>
    /// <inheritdoc/>
    public override void Subtraction(IEzrObject other, RuntimeResult result)
    {
        IEzrObject[] newArray;
        switch (other)
        {
            case EzrInteger when Value.Length == 0:
            case IEzrIndexedCollection when Value.Length == 0:
                result.Failure(new EzrValueOutOfRangeError("The array is empty and cannot be removed from!", _executionContext, StartPosition, EndPosition));
                break;

            case EzrInteger otherInteger when otherInteger.Value < -Value.Length || otherInteger.Value >= Value.Length:
                result.Failure(new EzrValueOutOfRangeError($"Index must be in range 0 to {Value.Length - 1} or -1 to {-Value.Length}!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int index):
                newArray = new IEzrObject[Value.Length - 1];
                if (index < 0)
                    index = Value.Length + index;

                Array.Copy(Value, newArray, index);
                Array.Copy(Value, index + 1, newArray, index, Value.Length - (index + 1));

                result.Success(NewArrayConstant(newArray));
                break;

            case EzrInteger:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection { Count: < 2 }:
                result.Failure(new EzrIllegalOperationError($"The indices {other.TypeName} must contain two values, the starting index and the ending index!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection { Count: > 2 }:
                result.Failure(new EzrIllegalOperationError($"The indices {other.TypeName} must only contain two values, the starting index and the ending index!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection otherCollection:
                (IEzrObject index1, IEzrObject index2) = (otherCollection.At(0), otherCollection.At(1));
                if (index1 is not EzrInteger startIndex || index2 is not EzrInteger endIndex)
                {
                    result.Failure(new EzrUnexpectedTypeError($"Both indices must be integers! Given indices were of types \"{index1.TypeName}\" (start) and \"{index2.TypeName}\" (end).", _executionContext, other.StartPosition, other.EndPosition));
                    break;
                }

                if (!startIndex.TryGetIntRepresentation(out int startIndexInt))
                {
                    result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, startIndex.StartPosition, startIndex.EndPosition));
                    break;
                }

                if (!endIndex.TryGetIntRepresentation(out int endIndexInt))
                {
                    result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                    break;
                }

                if (startIndexInt < -Value.Length || startIndexInt >= Value.Length)
                {
                    result.Failure(new EzrValueOutOfRangeError($"Starting index must be in range 0 to {Value.Length - 1} or -1 to {-Value.Length}!", _executionContext, startIndex.StartPosition, startIndex.EndPosition));
                    break;
                }

                if (startIndexInt < 0)
                {
                    if (endIndexInt > startIndexInt || endIndexInt < -Value.Length)
                    {
                        result.Failure(new EzrValueOutOfRangeError($"Ending index must be in range {startIndexInt} to {-Value.Length}!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                        break;
                    }

                    (startIndexInt, endIndexInt) = (Value.Length + endIndexInt, Value.Length + startIndexInt);
                }
                else if (endIndexInt < startIndexInt || endIndexInt >= Value.Length)
                {
                    result.Failure(new EzrValueOutOfRangeError($"Ending index must be in range {startIndexInt} to {Value.Length - 1}!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                    break;
                }

                newArray = new IEzrObject[Value.Length - (endIndexInt - startIndexInt + 1)];

                Array.Copy(Value, newArray, startIndexInt);
                Array.Copy(Value, endIndexInt + 1, newArray, startIndexInt, Value.Length - (endIndexInt + 1));

                result.Success(NewArrayConstant(newArray));
                break;

            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <remarks>Here, "multiply" means "duplicate/decrease the current value X times".</remarks>
    /// <inheritdoc/>
    public override void Multiplication(IEzrObject other, RuntimeResult result)
    {
        int newLength;
        switch (other)
        {
            case EzrInteger otherInteger:
                BigInteger newIntegerLength = Value.Length * otherInteger.Value;
                if (newIntegerLength < int.MinValue || newIntegerLength > int.MaxValue)
                {
                    result.Failure(new EzrValueOutOfRangeError("The multiplied length is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition));
                    return;
                }

                newLength = (int)newIntegerLength;
                break;

            case EzrFloat otherFloat:
                newLength = (int)(Value.Length * otherFloat.Value);
                break;

            default:
                result.Failure(IllegalOperation(other));
                return;
        }

        IEzrObject[] newValue;
        switch (newLength)
        {
            case int length when length < 0:
                result.Failure(new EzrValueOutOfRangeError("The multiplied length of the array cannot be negative!", _executionContext, other.StartPosition, other.EndPosition));
                return;

            case int length when length == Value.Length:
                newValue = Value; break;

            case int length when length == 0:
                newValue = []; break;

            case int length when length < Value.Length:
                newValue = Value[..newLength]; break;

            default:
                newValue = new IEzrObject[newLength];
                int loops = newLength / Value.Length;

                int i;
                for (i = 0; i < loops; i++)
                    Array.Copy(Value, 0, newValue, i * Value.Length, Value.Length);

                int currentEnd = i * Value.Length;
                if (newLength > currentEnd)
                    Array.Copy(Value, 0, newValue, currentEnd, newLength - currentEnd);

                break;
        }

        result.Success(NewArrayConstant(newValue));
    }

    /// <remarks>Here, "divide" means "duplicate/decrease the current value X times".</remarks>
    /// <inheritdoc/>
    public override void Division(IEzrObject other, RuntimeResult result)
    {
        int newLength;
        switch (other)
        {
            case EzrFloat { Value: <= 0 }:
            case EzrInteger otherInteger when otherInteger.Value <= 0:
                result.Failure(new EzrMathError("Division error", "Divisor cannot be less than or equal to zero in array division!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrFloat when Value.Length == 0:
            case EzrInteger when Value.Length == 0:
                result.Success(NewStringConstant(string.Empty)); break;

            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int divisor):
                newLength = Value.Length / divisor;
                result.Success(NewArrayConstant(Value[0..newLength]));
                break;

            case EzrInteger:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrFloat otherFloat:
                newLength = (int)(Value.Length / otherFloat.Value);

                if (newLength < Value.Length)
                    result.Success(NewArrayConstant(Value[0..newLength]));
                else
                {
                    IEzrObject[] newValue = new IEzrObject[newLength];
                    int loops = newLength / Value.Length;

                    int i;
                    for (i = 0; i < loops; i++)
                        Array.Copy(Value, 0, newValue, i * Value.Length, Value.Length);

                    int currentEnd = i * Value.Length;
                    if (newLength > currentEnd)
                        Array.Copy(Value, 0, newValue, currentEnd, newLength - currentEnd);

                    result.Success(NewArrayConstant(newValue));
                    break;
                }

                break;

            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void HasValueContained(IEzrObject other, RuntimeResult result)
    {
        bool contains = Contains(other, result);
        if (result.ShouldReturn)
            return;

        result.Success(NewBooleanConstant(contains));
    }

    /// <inheritdoc/>
    public override void NotHasValueContained(IEzrObject other, RuntimeResult result)
    {
        bool contains = Contains(other, result);
        if (result.ShouldReturn)
            return;

        result.Success(NewBooleanConstant(!contains));
    }

    /// <inheritdoc/>
    public override bool EvaluateBoolean(RuntimeResult result)
    {
        return Value.Length > 0;
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return other is EzrArray otherArray && Compare(otherArray, result) && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        int hash = HashTag;
        for (int i = 0; i < Value.Length; i++)
        {
            hash = HashCode.Combine(hash, Value[i].ComputeHashCode(result));
            if (result.ShouldReturn)
                return int.MinValue;
        }

        return hash;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        string[] elements = new string[Value.Length];
        for (int i = 0; i < Value.Length; i++)
        {
            elements[i] = Value[i].ToString(result);
            if (result.ShouldReturn)
                return string.Empty;
        }

        return $"({string.Join(", ", elements)})";
    }
}
