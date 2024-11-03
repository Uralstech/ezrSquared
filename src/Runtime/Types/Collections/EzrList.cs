using EzrSquared.Runtime.Collections;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers;
using EzrSquared.Runtime.WrapperAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace EzrSquared.Runtime.Types.Collections;

/// <summary>
/// The mutable, list type object.
/// </summary>
public class EzrList : EzrObject, IEzrMutableObject, IEzrIndexedCollection
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "list";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.List";

    /// <summary>
    /// The list value.
    /// </summary>
    public readonly RuntimeEzrObjectList Value;

    /// <inheritdoc/>
    [SharpAutoWrapper(isReadOnly: true)]
    public int Length => Value.Count;

    /// <param name="elements">The base value.</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrList(RuntimeEzrObjectList elements, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        Value = elements;

        Context.Set(null, "length", ReferencePool.Get(new EzrSharpCompatibilityProperty(GetMemberInfo<PropertyInfo, EzrList>(nameof(Length))!, this, Context, StartPosition, EndPosition), AccessMod.Constant));
    }

    /// <summary>
    /// Compares the current list with another collection.
    /// </summary>
    /// <param name="other">The other collection.</param>
    /// <param name="result">Runtime result to carray any errors.</param>
    /// <returns>The result of the comparison.</returns>
    private bool Compare(IEzrIndexedCollection other, RuntimeResult result)
    {
        if (Value.Count != other.Length)
            return false;

        for (int i = 0; i < Value.Count; i++)
            if (!Value[i].Object.StrictEquals(other.At(i), result) || result.ShouldReturn)
                return false;

        return true;
    }

    /// <summary>
    /// Checks if the specified object is contained in the current list.
    /// </summary>
    /// <param name="ezrObject">The object to check.</param>
    /// <param name="result">Runtime result to carray any errors.</param>
    /// <returns>The result of the check.</returns>
    private bool Contains(IEzrObject ezrObject, RuntimeResult result)
    {
        for (int i = 0; i < Value.Count; i++)
        {
            bool isEqual = ezrObject.StrictEquals(Value[i].Object, result);
            if (isEqual || result.ShouldReturn)
                return isEqual;
        }

        return false;
    }

    /// <inheritdoc/>
    public IEzrObject At(int index)
    {
        return Value[index].Object;
    }

    /// <inheritdoc/>
    public IEnumerator<IEzrObject> GetEnumerator()
    {
        return Value.ConvertAll(reference => reference.Object).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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
            case EzrInteger when Value.Count == 0:
            case IEzrIndexedCollection when Value.Count == 0:
                result.Failure(new EzrValueOutOfRangeError("The list is empty and cannot be indexed!", _executionContext, StartPosition, EndPosition)); break;

            case EzrInteger otherInteger when otherInteger.Value < -Value.Count || otherInteger.Value >= Value.Count:
                result.Failure(new EzrValueOutOfRangeError($"Index must be in range 0 to {Value.Count - 1} or -1 to {-Value.Count}!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int index):
                result.Success(index >= 0 ? Value[index] : Value[^-index]); break;

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

                if (startIndexInt < -Value.Count || startIndexInt >= Value.Count)
                {
                    result.Failure(new EzrValueOutOfRangeError($"Starting index must be in range 0 to {Value.Count - 1} or -1 to {-Value.Count}!", _executionContext, startIndex.StartPosition, startIndex.EndPosition));
                    break;
                }

                if (startIndexInt < 0)
                {
                    if (endIndexInt > startIndexInt || endIndexInt < -Value.Count)
                    {
                        result.Failure(new EzrValueOutOfRangeError($"Ending index must be in range {startIndexInt} to {-Value.Count}!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                        break;
                    }

                    result.Success(NewListConstant(Value.GetRange(Value.Count + endIndexInt, -(endIndexInt - startIndexInt) + 1)));
                    break;
                }

                if (endIndexInt < startIndexInt || endIndexInt >= Value.Count)
                {
                    result.Failure(new EzrValueOutOfRangeError($"Ending index must be in range {startIndexInt} to {Value.Count - 1}!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                    break;
                }

                result.Success(NewListConstant(Value.GetRange(startIndexInt, endIndexInt - startIndexInt + 1)));
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
    /// Appends the current object with the other object. If the other object is a list, appends its elements.
    /// </summary>
    /// <inheritdoc/>
    public override void Addition(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrList otherList:
                Value.AddRange(otherList.Value); break;

            default:
                Reference reference = ReferencePool.Get(other);
                reference.UpdateRegister(true);

                Value.Add(reference); break;
        }

        result.Success(NewNothingConstant());
    }

    /// <summary>
    /// Removes the object(s) at the specified index/indices.
    /// </summary>
    /// <inheritdoc/>
    public override void Subtraction(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrInteger when Value.Count == 0:
            case IEzrIndexedCollection when Value.Count == 0:
                result.Failure(new EzrValueOutOfRangeError("The list is empty and cannot be removed from!", _executionContext, StartPosition, EndPosition));
                break;

            case EzrInteger otherInteger when otherInteger.Value < -Value.Count || otherInteger.Value >= Value.Count:
                result.Failure(new EzrValueOutOfRangeError($"Index must be in range 0 to {Value.Count - 1} or -1 to {-Value.Count}!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int index):
                Value.RemoveAt(index >= 0 ? index : Value.Count + index);
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

                if (startIndexInt < -Value.Count || startIndexInt >= Value.Count)
                {
                    result.Failure(new EzrValueOutOfRangeError($"Starting index must be in range 0 to {Value.Count - 1} or -1 to {-Value.Count}!", _executionContext, startIndex.StartPosition, startIndex.EndPosition));
                    break;
                }

                if (startIndexInt < 0)
                {
                    if (endIndexInt > startIndexInt || endIndexInt < -Value.Count)
                    {
                        result.Failure(new EzrValueOutOfRangeError($"Ending index must be in range {startIndexInt} to {-Value.Count}!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                        break;
                    }

                    Value.RemoveRange(Value.Count + endIndexInt, -(endIndexInt - startIndexInt) + 1);
                    result.Success(NewNothingConstant());
                    break;
                }

                if (endIndexInt < startIndexInt || endIndexInt >= Value.Count)
                {
                    result.Failure(new EzrValueOutOfRangeError($"Ending index must be in range {startIndexInt} to {Value.Count - 1}!", _executionContext, endIndex.StartPosition, endIndex.EndPosition));
                    break;
                }

                Value.RemoveRange(startIndexInt, endIndexInt - startIndexInt + 1);
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
        switch (other)
        {
            case EzrInteger otherInteger:
                BigInteger newIntegerLength = Value.Count * otherInteger.Value;
                if (newIntegerLength < int.MinValue || newIntegerLength > int.MaxValue)
                {
                    result.Failure(new EzrValueOutOfRangeError("The multiplied length is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition));
                    return;
                }

                newLength = (int)newIntegerLength;
                break;

            case EzrFloat otherFloat:
                newLength = (int)(Value.Count * otherFloat.Value);
                break;

            default:
                result.Failure(IllegalOperation(other));
                return;
        }

        switch (newLength)
        {
            case int length when length < 0:
                result.Failure(new EzrValueOutOfRangeError("The multiplied length of the list cannot be negative!", _executionContext, other.StartPosition, other.EndPosition));
                return;

            case int length when length == Value.Count: break;
            case int length when length == 0: Value.Clear(); break;

            case int length when length < Value.Count:
                Value.RemoveRange(newLength, Value.Count - newLength); break;

            default:
                Reference[] original = new Reference[Value.Count];
                Value.CopyTo(original);

                int loops = (newLength / Value.Count) - 1;

                int i;
                for (i = 0; i < loops; i++)
                    Value.AddRange(Array.ConvertAll(original, element =>
                    {
                        Reference copy = element.ShallowCopy();
                        copy.UpdateRegister(true);

                        return copy;
                    }));

                int currentEnd = (loops == 0 ? 1 : i + 1) * original.Length;
                if (newLength > currentEnd)
                    Value.AddRange(Array.ConvertAll(original[..(newLength - currentEnd)], element =>
                    {
                        Reference copy = element.ShallowCopy();
                        copy.UpdateRegister(true);

                        return copy;
                    }));

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
                result.Failure(new EzrMathError("Division error", "Divisor cannot be less than or equal to zero in list division!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrFloat when Value.Count == 0:
            case EzrInteger when Value.Count == 0:
                result.Success(NewNothingConstant()); break;

            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int divisor):
                newLength = Value.Count / divisor;
                Value.RemoveRange(newLength, Value.Count - newLength);

                result.Success(NewNothingConstant());
                break;

            case EzrInteger:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case EzrFloat otherFloat:
                newLength = (int)(Value.Count / otherFloat.Value);

                if (newLength < Value.Count)
                    Value.RemoveRange(newLength, Value.Count - newLength);
                else
                {
                    Reference[] original = new Reference[Value.Count];
                    Value.CopyTo(original);

                    int loops = (newLength / Value.Count) - 1;

                    int i;
                    for (i = 0; i < loops; i++)
                        Value.AddRange(Array.ConvertAll(original, element =>
                        {
                            Reference copy = element.ShallowCopy();
                            copy.UpdateRegister(true);

                            return copy;
                        }));

                    int currentEnd = (loops == 0 ? 1 : i + 1) * original.Length;
                    if (newLength > currentEnd)
                        Value.AddRange(Array.ConvertAll(original[..(newLength - currentEnd)], element =>
                        {
                            Reference copy = element.ShallowCopy();
                            copy.UpdateRegister(true);

                            return copy;
                        }));
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
        return Value.Count > 0;
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return other is EzrList otherList && Compare(otherList, result) && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        int hash = HashTag;
        for (int i = 0; i < Value.Count; i++)
        {
            hash = HashCode.Combine(hash, Value[i].Object.ComputeHashCode(result));
            if (result.ShouldReturn)
                return int.MinValue;
        }

        return hash;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        string[] elements = new string[Value.Count];
        for (int i = 0; i < Value.Count; i++)
        {
            elements[i] = Value[i].Object.ToString(result);
            if (result.ShouldReturn)
                return string.Empty;
        }

        return $"[{string.Join(", ", elements)}]";
    }

    /// <inheritdoc/>
    public IMutable<IEzrMutableObject>? DeepCopy(RuntimeResult result)
    {
        RuntimeEzrObjectList? copy = Value.DeepCopy(result) as RuntimeEzrObjectList;
        return result.ShouldReturn ? null
            : new EzrList(copy!, Context, StartPosition, EndPosition);
    }

    /// <summary>Destructor.</summary>
    ~EzrList()
    {
        Value.Release();
        Context.Release();
    }
}
