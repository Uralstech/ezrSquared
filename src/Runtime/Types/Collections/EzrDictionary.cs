using EzrSquared.Runtime.Collections;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using System;
using System.Collections.Generic;

namespace EzrSquared.Runtime.Types.Collections;

/// <summary>
/// The mutable dictionary type object.
/// </summary>
/// <param name="value">The base value.</param>
/// <param name="parentContext">The parent context.</param>
/// <param name="startPosition">The starting position of the object.</param>
/// <param name="endPosition">The ending position of the object.</param>
public class EzrDictionary(RuntimeEzrObjectDictionary value, Context parentContext, Position startPosition, Position endPosition) : EzrObject(parentContext, startPosition, endPosition), IEzrMutableObject
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "dictionary";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.Dictionary";

    /// <summary>
    /// The dictionary value.
    /// </summary>
    public readonly RuntimeEzrObjectDictionary Value = value;

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrDictionary otherDictionary:
                bool equal = Value.IsEqual(otherDictionary.Value, result);
                if (result.ShouldReturn)
                    return;

                result.Success(NewBooleanConstant(equal)); break;

            default:
                result.Success(NewBooleanConstant(false)); break;
        }
    }

    /// <inheritdoc/>
    public override void ComparisonNotEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrDictionary otherDictionary:
                bool equal = Value.IsEqual(otherDictionary.Value, result);
                if (result.ShouldReturn)
                    return;

                result.Success(NewBooleanConstant(!equal)); break;

            default:
                result.Success(NewBooleanConstant(true)); break;
        }
    }

    /// <summary>
    /// Gets the object at the specified key.
    /// </summary>
    /// <inheritdoc/>
    public override void ComparisonLessThan(IEzrObject other, RuntimeResult result)
    {
        Reference value = Value.Get(other, result);
        if (value.IsEmpty)
            result.Failure(new EzrKeyNotFoundError("The key was not found in the dictionary!", _executionContext, other.StartPosition, other.EndPosition));
        else
            result.Success(value);
    }

    /// <summary>
    /// Gets the object at the specified key.
    /// </summary>
    /// <inheritdoc/>
    public override void ComparisonLessThanOrEqual(IEzrObject other, RuntimeResult result)
    {
        ComparisonLessThan(other, result);
    }

    /// <summary>
    /// Appends the current object with the other object and its key. If the other object is a dictionary, merges it with the current object.
    /// </summary>
    /// <inheritdoc/>
    public override void Addition(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case IEzrIndexedCollection { Length: < 2 }:
                result.Failure(new EzrIllegalOperationError($"The {other.TypeName} must contain two values, the key and value!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection { Length: > 2 }:
                result.Failure(new EzrIllegalOperationError($"The {other.TypeName} must only contain two values, the key and value!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection otherCollection:
                Value.Update(otherCollection.At(0), otherCollection.At(1), result);
                if (result.ShouldReturn)
                    return;

                result.Success(NewNothingConstant()); break;

            case EzrDictionary otherDictionary:
                Value.Merge(otherDictionary.Value);
                result.Success(NewNothingConstant());

                break;

            default:
                result.Failure(IllegalOperation(other)); break;
        }
    }

    /// <summary>
    /// Removes the object at the specified key.
    /// </summary>
    /// <inheritdoc/>
    public override void Subtraction(IEzrObject other, RuntimeResult result)
    {
        bool success = Value.Remove(other, result);
        if (result.ShouldReturn)
            return;

        if (!success)
            result.Failure(new EzrKeyNotFoundError("The key not found in the dictionary and connot be removed!", _executionContext, other.StartPosition, other.EndPosition));
        else
            result.Success(NewNothingConstant());
    }

    /// <remarks>Here, "divide" means "decrease the number of pairs in the dictionary X times".</remarks>
    /// <inheritdoc/>
    public override void Division(IEzrObject other, RuntimeResult result)
    {
        int newLength;
        switch (other)
        {
            case EzrFloat { Value: <= 0 }:
            case EzrInteger otherInteger when otherInteger.Value <= 0:
                result.Failure(new EzrMathError("Division error", "Divisor cannot be less than or equal to zero in dictionary division!", _executionContext, other.StartPosition, other.EndPosition)); return;

            case EzrFloat when Value.Length == 0:
            case EzrInteger when Value.Length == 0:
                result.Success(NewNothingConstant()); return;

            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int divisor):
                newLength = Value.Length / divisor; break;

            case EzrInteger:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition)); return;

            case EzrFloat otherFloat:
                newLength = (int)(Value.Length / otherFloat.Value);

                if (newLength > Value.Length)
                {
                    result.Failure(new EzrIllegalOperationError($"Divided length of dictionary is greater than its length ({Value.Length} / {otherFloat.Value} = {newLength})!", _executionContext, other.StartPosition, other.EndPosition));
                    return;
                }

                break;

            default:
                result.Failure(IllegalOperation(other)); return;
        }

        int[] keys = Value.GetRealKeys();
        for (int i = Value.Length - 1; i >= newLength; i--)
        {
            if (!Value.RemoveHash(keys[i]))
            {
                result.Failure(new EzrIllegalOperationError($"Key \"{keys[i]}\" was not found in the dictionary during deletion.", _executionContext, StartPosition, EndPosition));
                return;
            }
        }

        result.Success(NewNothingConstant());
    }

    /// <inheritdoc/>
    public override void HasValueContained(IEzrObject other, RuntimeResult result)
    {
        bool contains = Value.HasKey(other, result);
        if (result.ShouldReturn)
            return;

        result.Success(NewBooleanConstant(contains));
    }

    /// <inheritdoc/>
    public override void NotHasValueContained(IEzrObject other, RuntimeResult result)
    {
        bool contains = Value.HasKey(other, result);
        if (result.ShouldReturn)
            return;

        result.Success(NewBooleanConstant(!contains));
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return other is EzrDictionary otherDictionary && Value.IsEqual(otherDictionary.Value, result) && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        KeyValuePair<IEzrObject, IEzrObject>[] pairs = Value.GetPairs();
        int hash = HashTag;

        for (int i = 0; i < pairs.Length; i++)
        {
            int hash1 = pairs[i].Key.ComputeHashCode(result);
            if (result.ShouldReturn)
                return int.MinValue;

            int hash2 = pairs[i].Value.ComputeHashCode(result);
            if (result.ShouldReturn)
                return int.MinValue;

            hash = HashCode.Combine(hash, hash1, hash2);
        }

        return hash;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        KeyValuePair<IEzrObject, IEzrObject>[] pairs = Value.GetPairs();
        string[] pairsAsString = new string[pairs.Length];

        for (int i = 0; i < pairs.Length; i++)
        {
            string key = pairs[i].Key.ToString(result);
            if (result.ShouldReturn)
                return string.Empty;

            string value = pairs[i].Value.ToString(result);
            if (result.ShouldReturn)
                return string.Empty;

            pairsAsString[i] = $"{key} : {value}";
        }

        return $"{{{string.Join(", ", pairsAsString)}}}";
    }

    /// <inheritdoc/>
    public IMutable<IEzrMutableObject>? DeepCopy(RuntimeResult result)
    {
        RuntimeEzrObjectDictionary? copy = (RuntimeEzrObjectDictionary?)Value.DeepCopy(result);

        return result.ShouldReturn ? null
            : new EzrDictionary(copy!, Context, StartPosition, EndPosition);
    }

    /// <summary>Destructor.</summary>
    ~EzrDictionary()
    {
        Value.Release();
        Context.Release();
    }
}
