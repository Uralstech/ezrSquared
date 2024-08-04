using EzrSquared.Runtime.Types.Collections;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using System;
using System.Numerics;
using System.Text;

namespace EzrSquared.Runtime.Types.Core.Text;

/// <summary>
/// The <see cref="StringBuilder"/> type object.
/// </summary>
/// <param name="value">The base <i>string</i> value.</param>
/// <param name="parentContext">The parent context.</param>
/// <param name="startPosition">The starting position of the object.</param>
/// <param name="endPosition">The ending position of the object.</param>
public class EzrCharacterList(string value, Context parentContext, Position startPosition, Position endPosition) : EzrObject(parentContext, startPosition, endPosition), IEzrMutableObject
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "character list";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CharacterList";

    /// <summary>
    /// The <see cref="StringBuilder"/> value.
    /// </summary>
    public readonly StringBuilder Value = new(value);

    /// <summary>
    /// Calls <see cref="StringBuilder.ToString()"/> and returns the <see cref="StringBuilder"/> value converted to a <see cref="string"/>.
    /// </summary>
    public string StringValue => Value.ToString();

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrString otherString:
                result.Success(NewBooleanConstant(StringValue == otherString.Value)); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(StringValue == otherCharacterList.StringValue)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(StringValue == otherCharacter.Value.ToString())); break;
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
                result.Success(NewBooleanConstant(StringValue != otherString.Value)); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(StringValue != otherCharacterList.StringValue)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(StringValue != otherCharacter.Value.ToString())); break;
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
                result.Failure(new EzrValueOutOfRangeError("The character list is empty and cannot be indexed!", _executionContext, StartPosition, EndPosition)); break;

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

                    result.Success(NewCharacterListConstant(StringValue[(Value.Length + endIndexInt)..(Value.Length + startIndexInt + 1)]));
                    break;
                }

                if (endIndexInt < startIndexInt || endIndexInt >= Value.Length)
                {
                    result.Failure(new EzrValueOutOfRangeError($"Ending index must be in range {startIndexInt} to {Value.Length - 1}!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                    break;
                }

                result.Success(NewCharacterListConstant(StringValue[startIndexInt..(endIndexInt + 1)]));
                break;

            case EzrString otherString:
                result.Success(NewBooleanConstant(string.CompareOrdinal(StringValue, otherString.Value) < 0)); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(string.CompareOrdinal(StringValue, otherCharacterList.StringValue) < 0)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(string.CompareOrdinal(StringValue, otherCharacter.Value.ToString()) < 0)); break;
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
                result.Success(NewBooleanConstant(string.CompareOrdinal(StringValue, otherString.Value) > 0)); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(string.CompareOrdinal(StringValue, otherCharacterList.StringValue) > 0)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(string.CompareOrdinal(StringValue, otherCharacter.Value.ToString()) > 0)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <summary>
    /// Gets the character(s) at the specified index/indices OR compares the current object to other <i>stringlike</i> objects, checks if this is less than or equal to the other.
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
                result.Success(NewBooleanConstant(string.CompareOrdinal(StringValue, otherString.Value) <= 0)); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(string.CompareOrdinal(StringValue, otherCharacterList.StringValue) <= 0)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(string.CompareOrdinal(StringValue, otherCharacter.Value.ToString()) <= 0)); break;
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
                result.Success(NewBooleanConstant(string.CompareOrdinal(StringValue, otherString.Value) >= 0)); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(string.CompareOrdinal(StringValue, otherCharacterList.StringValue) >= 0)); break;
            case EzrCharacter otherCharacter:
                result.Success(NewBooleanConstant(string.CompareOrdinal(StringValue, otherCharacter.Value.ToString()) >= 0)); break;
            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <summary>
    /// Appends the string representation of the other object to the current object.
    /// </summary>
    /// <inheritdoc/>
    public override void Addition(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrCharacterList otherCharacterList:
                Value.Append(otherCharacterList.Value); break;
            case EzrString otherString:
                Value.Append(otherString.Value); break;
            case EzrCharacter otherCharacter:
                Value.Append(otherCharacter.Value); break;
            default:
                string otherStringValue = other.ToPureString(result);
                if (result.ShouldReturn)
                    return;

                Value.Append(otherStringValue);
                break;
        }

        result.Success(NewNothingConstant());
    }

    /// <summary>
    /// Removes the character(s) at the specified index/indices.
    /// </summary>
    /// <inheritdoc/>
    public override void Subtraction(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger when Value.Length == 0:
            case IEzrIndexedCollection when Value.Length == 0:
                result.Failure(new EzrValueOutOfRangeError("The character list is empty and cannot be removed from!", _executionContext, StartPosition, EndPosition)); break;

            case EzrInteger otherInteger when otherInteger.Value < -Value.Length || otherInteger.Value >= Value.Length:
                result.Failure(new EzrValueOutOfRangeError($"Index must be in range 0 to {Value.Length - 1} or -1 to {-Value.Length}!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int index):
                Value.Remove(index >= 0 ? index : Value.Length + index, 1);
                result.Success(NewNothingConstant());
                break;

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

                    Value.Remove(Value.Length + endIndexInt, startIndexInt - endIndexInt + 1);
                    result.Success(NewNothingConstant());
                    break;
                }

                if (endIndexInt < startIndexInt || endIndexInt >= Value.Length)
                {
                    result.Failure(new EzrValueOutOfRangeError($"Ending index must be in range {startIndexInt} to {Value.Length - 1}!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                    break;
                }

                Value.Remove(startIndexInt, endIndexInt - startIndexInt + 1);
                result.Success(NewNothingConstant());
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
        string originalValue;
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

        switch (newLength)
        {
            case int length when length < 0:
                result.Failure(new EzrValueOutOfRangeError("The multiplied length of the character list cannot be negative!", _executionContext, other.StartPosition, other.EndPosition));
                break;

            case int length when length == Value.Length: break;
            case 0: Value.Clear(); break;

            case int length when length < Value.Length:
                originalValue = StringValue;

                Value.Clear();
                Value.Append(originalValue[..newLength]);
                break;

            default:
                originalValue = StringValue;
                int loops = (newLength / originalValue.Length) - 1;

                int i;
                for (i = 0; i < loops; i++)
                    Value.Append(originalValue);

                int currentEnd = (loops == 0 ? 1 : i + 1) * originalValue.Length;
                if (newLength > currentEnd)
                    Value.Append(originalValue[..(newLength - currentEnd)]);

                break;
        }

        result.Success(NewNothingConstant());
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
                result.Failure(new EzrMathError("Division error", "Divisor cannot be less than or equal to zero in character list division!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrFloat when Value.Length == 0:
            case EzrInteger when Value.Length == 0:
                result.Success(NewNothingConstant()); break;

            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int divisor):
                newLength = Value.Length / divisor;
                Value.Remove(newLength, Value.Length - newLength);

                result.Success(NewNothingConstant());
                break;

            case EzrInteger:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrFloat otherFloat:
                newLength = (int)(Value.Length / otherFloat.Value);

                if (newLength < Value.Length)
                    Value.Remove(newLength, Value.Length - newLength);
                else
                {
                    string originalValue = StringValue;
                    while (Value.Length < newLength)
                        Value.Append(originalValue[..Math.Clamp(newLength - originalValue.Length, 0, originalValue.Length)]);
                }

                result.Success(NewNothingConstant());
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
                result.Success(NewBooleanConstant(StringValue.Contains(otherString.Value))); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(StringValue.Contains(otherCharacterList.StringValue))); break;
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
                result.Success(NewBooleanConstant(!StringValue.Contains(otherString.Value))); break;
            case EzrCharacterList otherCharacterList:
                result.Success(NewBooleanConstant(!StringValue.Contains(otherCharacterList.StringValue))); break;
            default:
                result.Failure(IllegalOperation(other, false)); break;
        }
    }

    /// <inheritdoc/>
    public override bool EvaluateBoolean(RuntimeResult result)
    {
        return Value.Length > 0;
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return (other as EzrCharacterList)?.StringValue == StringValue && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        int hash = HashTag;
        for (int i = 0; i < Value.Length; i++)
            hash = HashCode.Combine(hash, Value[i]);

        return hash;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"'{StringValue}'";
    }

    /// <inheritdoc/>
    public override string ToPureString(RuntimeResult result)
    {
        return StringValue;
    }

    /// <inheritdoc/>
    public IMutable<IEzrMutableObject>? DeepCopy(RuntimeResult result)
    {
        return new EzrCharacterList(StringValue, _executionContext, StartPosition, EndPosition);
    }
}
