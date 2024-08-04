using EzrSquared.Runtime.Types.Collections;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using System;
using System.Numerics;
using System.Text;

namespace EzrSquared.Runtime.Types.Core.Text;

/// <summary>
/// The string type object.
/// </summary>
/// <param name="value">The base value.</param>
/// <param name="parentContext">The parent context.</param>
/// <param name="startPosition">The starting position of the object.</param>
/// <param name="endPosition">The ending position of the object.</param>
public class EzrString(string value, Context parentContext, Position startPosition, Position endPosition) : EzrObject(parentContext, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "string";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.String";

    /// <summary>
    /// The string value.
    /// </summary>
    public readonly string Value = value;

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrString otherString:
                result.Success(NewBooleanConstant(Value == otherString.Value)); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(Value == otherCharacterList.StringValue)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value == otherCharacter.Value.ToString())); break;
            default:
                result.Success(NewBooleanConstant(false)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonNotEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrString otherString:
                result.Success(NewBooleanConstant(Value != otherString.Value)); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(Value != otherCharacterList.StringValue)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(Value != otherCharacter.Value.ToString())); break;
            default:
                result.Success(NewBooleanConstant(true)); break;
        }
    }

    /// <summary>
    /// Gets the character(s) at the specified index/indices OR compares the current object to other <i>stringlike</i> objects, checks if this is less than the other.
    /// </summary>
    /// <inheritdoc/>
    public override void ComparisonLessThan(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger when Value.Length == 0:
            case IEzrIndexedCollection when Value.Length == 0:
                result.Failure(new EzrValueOutOfRangeError("The string is empty and cannot be indexed!", _executionContext, StartPosition, EndPosition)); break;

            case EzrInteger otherInteger when otherInteger.Value < -Value.Length || otherInteger.Value >= Value.Length:
                result.Failure(new EzrValueOutOfRangeError($"Index must be in range 0 to {Value.Length - 1} or -1 to {-Value.Length}!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int index):
                result.Success(NewCharacterConstant(index >= 0 ? Value[index] : Value[^-index])); break;

            case EzrInteger:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection { Length: < 2 }:
                result.Failure(new EzrIllegalOperationError($"The indices {other.TypeName} must contain two values, the starting index and the ending index!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection { Length: > 2 }:
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

                    result.Success(NewStringConstant(Value[^(-endIndexInt)..^(-startIndexInt - 1)]));
                    break;
                }

                if (endIndexInt < startIndexInt || endIndexInt >= Value.Length)
                {
                    result.Failure(new EzrValueOutOfRangeError($"Ending index must be in range {startIndexInt} to {Value.Length - 1}!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                    break;
                }

                result.Success(NewStringConstant(Value[startIndexInt..(endIndexInt + 1)]));
                break;

            case EzrString otherString:
                result.Success(NewBooleanConstant(string.CompareOrdinal(Value, otherString.Value) < 0)); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(string.CompareOrdinal(Value, otherCharacterList.StringValue) < 0)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(string.CompareOrdinal(Value, otherCharacter.Value.ToString()) < 0)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonGreaterThan(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrString otherString:
                result.Success(NewBooleanConstant(string.CompareOrdinal(Value, otherString.Value) > 0)); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(string.CompareOrdinal(Value, otherCharacterList.StringValue) > 0)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(string.CompareOrdinal(Value, otherCharacter.Value.ToString()) > 0)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <summary>
    /// Gets the character(s) at the specified index/indices OR compares the current object to other <i>stringlike</i> objects, checks if this is less than the other.
    /// </summary>
    /// <inheritdoc/>
    public override void ComparisonLessThanOrEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger:
            case IEzrIndexedCollection:
                ComparisonLessThan(other, result);
                break;

            case EzrString otherString:
                result.Success(NewBooleanConstant(string.CompareOrdinal(Value, otherString.Value) <= 0)); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(string.CompareOrdinal(Value, otherCharacterList.StringValue) <= 0)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(string.CompareOrdinal(Value, otherCharacter.Value.ToString()) <= 0)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonGreaterThanOrEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrString otherString:
                result.Success(NewBooleanConstant(string.CompareOrdinal(Value, otherString.Value) >= 0)); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(string.CompareOrdinal(Value, otherCharacterList.StringValue) >= 0)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(string.CompareOrdinal(Value, otherCharacter.Value.ToString()) >= 0)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <summary>
    /// Creates a copy of the current object with the string representation of the other object appended.
    /// </summary>
    /// <inheritdoc/>
    public override void Addition(IEzrObject other, RuntimeResult result)
    {
        string newValue = Value;
        switch (other)
        {
            case EzrCharacterList otherCharacterList:
                newValue += otherCharacterList.StringValue; break;
            case EzrString otherString:
                newValue += otherString.Value; break;
            case EzrCharacter otherCharacter:
                newValue += otherCharacter.Value; break;
            default:
                newValue += other.ToPureString(result);
                if (result.ShouldReturn)
                    return;
                break;
        }

        result.Success(NewStringConstant(newValue));
    }

    /// <summary>
    /// Creates a copy of the current object with the character(s) at the specified index/indices removed.
    /// </summary>
    /// <inheritdoc/>
    public override void Subtraction(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger when Value.Length == 0:
            case IEzrIndexedCollection when Value.Length == 0:
                result.Failure(new EzrValueOutOfRangeError("The string is empty and cannot be removed from!", _executionContext, StartPosition, EndPosition));
                break;

            case EzrInteger otherInteger when otherInteger.Value < -Value.Length || otherInteger.Value >= Value.Length:
                result.Failure(new EzrValueOutOfRangeError($"Index must be in range 0 to {Value.Length - 1} or -1 to {-Value.Length}!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int index):
                result.Success(NewStringConstant(index >= 0 ? Value.Remove(index, 1) : Value.Remove(Value.Length + index, 1))); break;

            case EzrInteger:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection { Length: < 2 }:
                result.Failure(new EzrIllegalOperationError($"The indices {other.TypeName} must contain two values, the starting index and the ending index!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection { Length: > 2 }:
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

                    result.Success(NewStringConstant(Value.Remove(Value.Length + endIndexInt, startIndexInt - endIndexInt + 1)));
                    break;
                }

                if (endIndexInt < startIndexInt || endIndexInt >= Value.Length)
                {
                    result.Failure(new EzrValueOutOfRangeError($"Ending index must be in range {startIndexInt} to {Value.Length - 1}!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                    break;
                }

                result.Success(NewStringConstant(Value.Remove(startIndexInt, endIndexInt - startIndexInt + 1)));
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

        string newValue;
        switch (newLength)
        {
            case int length when length < 0:
                result.Failure(new EzrValueOutOfRangeError("The multiplied length of the string cannot be negative!", _executionContext, other.StartPosition, other.EndPosition));
                return;

            case int length when length == Value.Length:
                newValue = Value; break;

            case int length when length == 0:
                newValue = string.Empty; break;

            case int length when length < Value.Length:
                newValue = Value[..newLength]; break;

            default:
                StringBuilder builder = new();
                int loops = newLength / Value.Length;

                int i;
                for (i = 0; i < loops; i++)
                    builder.Append(Value);

                int currentEnd = i * Value.Length;
                if (newLength > currentEnd)
                    builder.Append(Value[..(newLength - currentEnd)]);

                newValue = builder.ToString();
                break;
        }

        result.Success(NewStringConstant(newValue));
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
                result.Failure(new EzrMathError("Division error", "Divisor cannot be less than or equal to zero in string division!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrFloat when Value.Length == 0:
            case EzrInteger when Value.Length == 0:
                result.Success(NewStringConstant(string.Empty)); break;

            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int divisor):
                newLength = Value.Length / divisor;
                result.Success(NewStringConstant(Value[0..newLength]));
                break;

            case EzrInteger:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrFloat otherFloat:
                newLength = (int)(Value.Length / otherFloat.Value);

                if (newLength < Value.Length)
                    result.Success(NewStringConstant(Value[0..newLength]));
                else
                {
                    StringBuilder builder = new(Value);
                    while (builder.Length < newLength)
                        builder.Append(Value[..Math.Clamp(newLength - Value.Length, 0, Value.Length)]);

                    result.Success(NewStringConstant(builder.ToString()));
                }

                break;

            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <inheritdoc/>
    public override void HasValueContained(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrString otherString:
                result.Success(NewBooleanConstant(Value.Contains(otherString.Value))); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(Value.Contains(otherCharacterList.StringValue))); break;
            default:
                result.Failure(IllegalOperation(other, false)); break;
        }
    }

    /// <inheritdoc/>
    public override void NotHasValueContained(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrString otherString:
                result.Success(NewBooleanConstant(!Value.Contains(otherString.Value))); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(!Value.Contains(otherCharacterList.StringValue))); break;
            default:
                result.Failure(IllegalOperation(other, false)); break;
        }
    }

    /// <inheritdoc/>
    public override bool EvaluateBoolean(RuntimeResult result)
    {
        return !string.IsNullOrEmpty(Value);
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return (other as EzrString)?.Value == Value && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, Value);
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"\"{Value}\"";
    }

    /// <inheritdoc/>
    public override string ToPureString(RuntimeResult result)
    {
        return Value;
    }
}
