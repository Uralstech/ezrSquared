using EzrSquared.Runtime.Collections;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using EzrSquared.Runtime.Types.Wrappers;
using EzrSquared.Runtime.Types.Wrappers.Members;
using EzrSquared.Runtime.Types.Wrappers.Members.Methods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace EzrSquared.Runtime.Types.Collections;

/// <summary>
/// The mutable dictionary type object.
/// </summary>
public class EzrDictionary : EzrObject, IEzrMutableObject, IEzrDictionary
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "dictionary";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.Dictionary";

    /// <summary>
    /// The dictionary value.
    /// </summary>
    public readonly RuntimeEzrObjectDictionary Value;

    /// <inheritdoc/>
    [WrapMember("length")]
    public int Count => Value.Count;

    /// <summary>
    /// Creates a new <see cref="EzrDictionary"/>.
    /// </summary>
    /// <param name="value">The base value.</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrDictionary(RuntimeEzrObjectDictionary value, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        Value = value;

        Context.Set(null, "length", ReferencePool.Get(new EzrPropertyWrapper(GetMemberInfo<PropertyInfo, RuntimeEzrObjectDictionary>(nameof(Value.Count))!, Value, Context, StartPosition, EndPosition), AccessMod.Constant));
        Context.Set(null, "remove_by_hash", ReferencePool.Get(new EzrMethodWrapper(Value.RemoveHash, Context, StartPosition, EndPosition), AccessMod.Constant));
        Context.Set(null, "has_key", ReferencePool.Get(new EzrMethodWrapper(Value.HasKey, Context, StartPosition, EndPosition), AccessMod.Constant));
    }

    /// <inheritdoc/>
    /// <exception cref="KeyNotFoundException">Thrown if the key could not be hashed or was not found.</exception>
    public IEzrObject At(IEzrObject key, RuntimeResult result)
    {
        Reference value = Value.Get(key, result);
        return !value.IsEmpty
            ? value.Object
            : throw new KeyNotFoundException("The key could not be hashed or was not found in the dictionary!");
    }

    /// <inheritdoc/>
    public IEzrObject? TryAt(IEzrObject key, RuntimeResult result)
    {
        Reference value = Value.Get(key, result);
        return !value.IsEmpty ? value.Object : null;
    }

    /// <inheritdoc/>
    public IEnumerator<IEzrObject> GetEnumerator(RuntimeResult result)
    {
        KeyValuePair<IEzrObject, IEzrObject>[]? pairs = Value.GetPairs(result);
        if (result.ShouldReturn)
            yield break;

        foreach (KeyValuePair<IEzrObject, IEzrObject> pair in pairs!)
            yield return new EzrArray([pair.Key, pair.Value], _executionContext, StartPosition, EndPosition);
    }

    /// <inheritdoc/>
    public IEnumerator<IEzrObject> GetEnumerator()
    {
        foreach (KeyValuePair<int, KeyValuePair<IEzrObject, Reference>> keyValuePair in Value)
            yield return new EzrArray([keyValuePair.Value.Key, keyValuePair.Value.Value.Object], _executionContext, StartPosition, EndPosition);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        switch (other)
        {
            case EzrDictionary otherDictionary:
                bool equalDictionaries = Value.IsEqual(otherDictionary.Value, result);
                if (result.ShouldReturn)
                    return;

                result.Success(NewBooleanConstant(equalDictionaries)); break;

            case IEzrDictionary otherIDictionary:
                bool equalIDictionaries = Value.IsEqual(otherIDictionary, _executionContext, result);
                if (result.ShouldReturn)
                    break;

                result.Success(NewBooleanConstant(equalIDictionaries)); break;

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
                bool equalDictionaries = Value.IsEqual(otherDictionary.Value, result);
                if (result.ShouldReturn)
                    return;

                result.Success(NewBooleanConstant(!equalDictionaries)); break;


            case IEzrDictionary otherIDictionary:
                bool equalIDictionaries = Value.IsEqual(otherIDictionary, _executionContext, result);
                if (result.ShouldReturn)
                    break;

                result.Success(NewBooleanConstant(!equalIDictionaries)); break;

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
            case IEzrIndexedCollection { Count: < 2 }:
                result.Failure(new EzrIllegalOperationError($"The {other.TypeName} must contain two values, the key and value!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection { Count: > 2 }:
                result.Failure(new EzrIllegalOperationError($"The {other.TypeName} must only contain two values, the key and value!", _executionContext, other.StartPosition, other.EndPosition)); break;

            case IEzrIndexedCollection otherCollection:
                Value.Update(otherCollection.At(0), otherCollection.At(1), result);
                if (result.ShouldReturn)
                    break;

                result.Success(ReferencePool.Get(this, AccessMod.PrivateConstant)); break;

            case EzrDictionary otherDictionary:
                Value.Merge(otherDictionary.Value, result);
                if (result.ShouldReturn)
                    break;

                result.Success(ReferencePool.Get(this, AccessMod.PrivateConstant)); break;

            case IEzrDictionary otherIDictionary:
                Value.Merge(otherIDictionary, _executionContext, result);
                if (result.ShouldReturn)
                    break;

                result.Success(ReferencePool.Get(this, AccessMod.PrivateConstant)); break;

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
            result.Success(ReferencePool.Get(this, AccessMod.PrivateConstant));
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

            case EzrFloat when Value.Count == 0:
            case EzrInteger when Value.Count == 0:
                result.Success(ReferencePool.Get(this, AccessMod.PrivateConstant)); return;

            case EzrInteger otherInteger when otherInteger.TryGetIntRepresentation(out int divisor):
                newLength = Value.Count / divisor; break;

            case EzrInteger:
                result.Failure(new EzrValueOutOfRangeError("The value is too large for this operation!", _executionContext, other.StartPosition, other.EndPosition)); return;

            case EzrFloat otherFloat:
                newLength = (int)(Value.Count / otherFloat.Value);

                if (newLength > Value.Count)
                {
                    result.Failure(new EzrIllegalOperationError($"Divided length of dictionary is greater than its length ({Value.Count} / {otherFloat.Value} = {newLength})!", _executionContext, other.StartPosition, other.EndPosition));
                    return;
                }

                break;

            default:
                result.Failure(IllegalOperation(other)); return;
        }

        int[] keys = Value.GetRealKeys();
        for (int i = Value.Count - 1; i >= newLength; i--)
        {
            if (!Value.RemoveHash(keys[i]))
            {
                result.Failure(new EzrIllegalOperationError($"Key \"{keys[i]}\" was not found in the dictionary during deletion.", _executionContext, StartPosition, EndPosition));
                return;
            }
        }

        result.Success(ReferencePool.Get(this, AccessMod.PrivateConstant));
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
        int hash = HashTag;
        foreach (KeyValuePair<int, KeyValuePair<IEzrObject, Reference>> keyValuePair in Value)
        {
            int keyHash = keyValuePair.Key;

            int valueHash = keyValuePair.Value.Value.Object.ComputeHashCode(result);
            if (result.ShouldReturn)
                return int.MinValue;

            hash = HashCode.Combine(hash, keyHash, valueHash);
        }

        return hash;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        using IEnumerator<KeyValuePair<int, KeyValuePair<IEzrObject, Reference>>> keyValuePairs = Value.GetEnumerator();

        string[] pairsAsString = new string[Value.Count];
        for (int i = 0; i < pairsAsString.Length; i++)
        {
            keyValuePairs.MoveNext();
            KeyValuePair<IEzrObject, Reference> pair = keyValuePairs.Current.Value;

            string key = pair.Key.ToString(result);
            if (result.ShouldReturn)
                return string.Empty;

            string value = pair.Value.Object.ToString(result);
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
