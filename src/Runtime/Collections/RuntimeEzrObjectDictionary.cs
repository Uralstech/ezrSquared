using EzrSquared.Runtime.Types;
using EzrSquared.Runtime.Types.Collections;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.Attributes;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables.Attributes;
using System.Collections;
using System.Collections.Generic;

namespace EzrSquared.Runtime.Collections;

/// <summary>
/// A Dictionary for <see cref="IEzrObject"/>s.
/// </summary>
public class RuntimeEzrObjectDictionary : IMutable<RuntimeEzrObjectDictionary>, IEnumerable<KeyValuePair<int, KeyValuePair<IEzrObject, Reference>>>
{
    /// <summary>
    /// The collection of <see cref="IEzrObject"/>s stored in the <see cref="RuntimeEzrObjectDictionary"/>, in the format Dictionary&lt;hashOfKey, KeyValuePair&lt;key, valueReference&gt;&gt;.
    /// </summary>
    private readonly IDictionary<int, KeyValuePair<IEzrObject, Reference>> _items;

    /// <summary>
    /// The number of <see cref="IEzrObject"/>s in the <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    [WrappedMember("length")]
    public int Count => _items.Count;

    /// <summary>
    /// Creates a new <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    public RuntimeEzrObjectDictionary()
    {
        _items = new Dictionary<int, KeyValuePair<IEzrObject, Reference>>();
    }

    /// <summary>
    /// Creates a new <see cref="RuntimeEzrObjectDictionary"/> from an existing <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    public RuntimeEzrObjectDictionary(IDictionary<int, KeyValuePair<IEzrObject, Reference>> items)
    {
        _items = items;
    }

    /// <summary>
    /// Tries to get a copy of the given object.
    /// </summary>
    /// <param name="ezrObject">The object to copy.</param>
    /// <param name="result">The <see cref="RuntimeResult"/> object for returning errors.</param>
    /// <returns>The object, its copy or <see langword="null"/> if something went wrong.</returns>
    private static IEzrObject? TryCopyObject(IEzrObject ezrObject, RuntimeResult result)
    {
        if (ezrObject is IEzrMutableObject mutableElement)
        {
            IEzrObject? keyCopy = (IEzrObject?)mutableElement.DeepCopy(result);
            if (result.ShouldReturn)
                return null;

            ezrObject = keyCopy!;
        }

        return ezrObject;
    }

    /// <summary>
    /// Updates the <see cref="RuntimeEzrObjectDictionary"/> with a new value.
    /// </summary>
    /// <param name="key">The key to update.</param>
    /// <param name="value">The value to assign to <paramref name="key"/>.</param>
    /// <param name="result">The <see cref="RuntimeResult"/> object for returning errors.</param>
    public void Update(IEzrObject key, IEzrObject value, RuntimeResult result)
    {
        int hash = key.ComputeHashCode(result);
        if (result.ShouldReturn)
            return;

        if (_items.TryGetValue(hash, out KeyValuePair<IEzrObject, Reference> reference))
            reference.Value.UpdateObject(value);
        else
        {
            if (TryCopyObject(key, result) is not IEzrObject keyCopy)
                return;

            Reference valueReference = ReferencePool.Get(value);
            valueReference.UpdateRegister(true);

            _items.Add(hash, new KeyValuePair<IEzrObject, Reference>(keyCopy, valueReference));
        }
    }

    /// <summary>
    /// Removes a key, value pair from the <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <param name="key">The key to be removed.</param>
    /// <param name="result">The <see cref="RuntimeResult"/> object for returning errors.</param>
    /// <returns><see langword="true"/> if the operation was successful, <see langword="false"/> if not.</returns>
    public bool Remove(IEzrObject key, RuntimeResult result)
    {
        int hash = key.ComputeHashCode(result);
        if (result.ShouldReturn)
            return false;

        if (_items.TryGetValue(hash, out KeyValuePair<IEzrObject, Reference> pair))
        {
            pair.Value.UpdateRegister(false);
            ReferencePool.TryRelease(pair.Value);

            return _items.Remove(hash);
        }

        return false;
    }

    /// <summary>
    /// Removes a key, value pair from the <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <remarks>
    /// This method removes pairs using the hash of the <see cref="IEzrObject"/> keys, <paramref name="key"/>.
    /// </remarks>
    /// <param name="key">The key (hash) to be removed.</param>
    /// <returns><see langword="true"/> if the operation was successful, <see langword="false"/> if not.</returns>
    [WrappedMember("remove_by_hash")]
    public bool RemoveHash(int key)
    {
        if (_items.TryGetValue(key, out KeyValuePair<IEzrObject, Reference> pair))
        {
            pair.Value.UpdateRegister(false);
            ReferencePool.TryRelease(pair.Value);

            return _items.Remove(key);
        }

        return false;
    }

    /// <summary>
    /// Merges a <see cref="RuntimeEzrObjectDictionary"/> to the current <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <param name="other">The other <see cref="RuntimeEzrObjectDictionary"/> to be merged.</param>
    /// <param name="result">The <see cref="RuntimeResult"/> object for returning errors.</param>
    public void Merge(RuntimeEzrObjectDictionary other, RuntimeResult result)
    {
        foreach (KeyValuePair<int, KeyValuePair<IEzrObject, Reference>> pair in other._items)
        {
            if (TryCopyObject(pair.Value.Key, result) is not IEzrObject keyObject)
                return;

            Reference newReference = ReferencePool.Get(pair.Value.Value.Object);
            newReference.UpdateRegister(true);

            _items[pair.Key] = new KeyValuePair<IEzrObject, Reference>(keyObject, newReference);
        }
    }

    /// <summary>
    /// Merges an <see cref="IEzrDictionary"/> to the current <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <param name="other">The other <see cref="IEzrDictionary"/> to be merged.</param>
    /// <param name="executionContext">The context under which this operation is being executed.</param>
    /// <param name="result">The <see cref="RuntimeResult"/> object for returning errors.</param>
    public void Merge(IEzrDictionary other, Context executionContext, RuntimeResult result)
    {
        foreach (IEzrObject ezrObject in other)
        {
            if (ezrObject is not IEzrIndexedCollection pair || pair.Count is > 2 or < 2)
            {
                result.Failure(new EzrUnexpectedTypeError($"Object of type \"{other.TypeName}\" is in an unexpected format and could not be fully merged into this dictionary!", executionContext, other.StartPosition, other.EndPosition));
                return;
            }

            (IEzrObject keyObject, IEzrObject valueObject) = (pair.At(0), pair.At(1));

            Update(keyObject, valueObject, result);
            if (result.ShouldReturn)
                return;
        }
    }

    /// <summary>
    /// Tries retrieving a value from the <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <param name="key">The key of the value to be returned.</param>
    /// <param name="result">The <see cref="RuntimeResult"/> object for returning errors.</param>
    /// <returns>The retrieved value or <see cref="Reference.Empty"/> if it was not found.</returns>
    public Reference Get(IEzrObject key, RuntimeResult result)
    {
        int hash = key.ComputeHashCode(result);
        return (!result.ShouldReturn && _items.TryGetValue(hash, out KeyValuePair<IEzrObject, Reference> pair)) ? pair.Value : Reference.Empty;
    }

    /// <summary>
    /// Checks if <paramref name="key"/> exists in the <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <param name="key">The key to be checked.</param>
    /// <param name="result">The <see cref="RuntimeResult"/> object for returning errors.</param>
    /// <returns><see langword="true"/> if the key was found, <see langword="false"/> if any error occured or the key was not found.</returns>
    [WrappedMember]
    public bool HasKey(IEzrObject key, [Runtime(Feature.ResultRef)] RuntimeResult result)
    {
        int hash = key.ComputeHashCode(result);
        return !result.ShouldReturn && _items.ContainsKey(hash);
    }

    /// <summary>
    /// Checks if the current dictionary is equal to the given dictionary.
    /// </summary>
    /// <param name="other">The other dictionary.</param>
    /// <param name="result">The <see cref="RuntimeResult"/> object for returning errors.</param>
    /// <returns>The comparison result.</returns>
    public bool IsEqual(RuntimeEzrObjectDictionary other, RuntimeResult result)
    {
        if (other._items.Count != _items.Count)
            return false;

        foreach (KeyValuePair<int, KeyValuePair<IEzrObject, Reference>> pair in _items)
        {
            if (!other._items.TryGetValue(pair.Key, out KeyValuePair<IEzrObject, Reference> otherPair))
                return false;

            bool keyEquals = pair.Value.Key.StrictEquals(otherPair.Key, result);
            if (result.ShouldReturn || !keyEquals)
                return false;

            bool valueEquals = pair.Value.Value.Object.StrictEquals(otherPair.Value.Object, result);
            if (result.ShouldReturn || !valueEquals)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the current dictionary is equal to the given keyed collection.
    /// </summary>
    /// <param name="other">The other keyed collection.</param>
    /// <param name="executionContext">The context under which this operation is being executed.</param>
    /// <param name="result">The <see cref="RuntimeResult"/> object for returning errors.</param>
    /// <returns>The comparison result.</returns>
    public bool IsEqual(IEzrDictionary other, Context executionContext, RuntimeResult result)
    {
        if (other.Count != _items.Count)
            return false;

        foreach (IEzrObject ezrObject in other)
        {
            if (ezrObject is not IEzrIndexedCollection pair || pair.Count is > 2 or < 2)
            {
                result.Failure(new EzrUnexpectedTypeError($"Object of type \"{other.TypeName}\" is in an unexpected format and cannot be compared with this dictionary!", executionContext, other.StartPosition, other.EndPosition));
                return false;
            }

            (IEzrObject keyObject, IEzrObject valueObject) = (pair.At(0), pair.At(1));

            int keyHashCode = keyObject.ComputeHashCode(result);
            if (result.ShouldReturn || !_items.TryGetValue(keyHashCode, out KeyValuePair<IEzrObject, Reference> thisPair))
                return false;

            bool keyEquals = keyObject.StrictEquals(thisPair.Key, result);
            if (result.ShouldReturn || !keyEquals)
                return false;

            bool valueEquals = valueObject.StrictEquals(thisPair.Value.Object, result);
            if (result.ShouldReturn || !valueEquals)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns the <i>actual</i> integer (hash) keys from the <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <returns>The integer hashes.</returns>
    public int[] GetRealKeys()
    {
        return [.. _items.Keys];
    }

    /// <summary>
    /// Retrieves all keys from the <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <param name="result">The <see cref="RuntimeResult"/> object for returning errors.</param>
    /// <returns>The keys or <see langword="null"/> if something went wrong.</returns>
    public IEzrObject[]? GetKeys(RuntimeResult result)
    {
        IEzrObject[] keys = new IEzrObject[_items.Count];
        using IEnumerator<KeyValuePair<IEzrObject, Reference>> pairs = _items.Values.GetEnumerator();

        for (int i = 0; i < keys.Length; i++)
        {
            pairs.MoveNext();

            if (TryCopyObject(pairs.Current.Key, result) is not IEzrObject keyObject)
                return null;

            keys[i] = keyObject;
        }

        return keys;
    }

    /// <summary>
    /// Retrieves all values from the <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <returns>The values.</returns>
    public IEzrObject[] GetValues()
    {
        IEzrObject[] values = new IEzrObject[_items.Count];
        using IEnumerator<KeyValuePair<IEzrObject, Reference>> pairs = _items.Values.GetEnumerator();

        for (int i = 0; i < values.Length; i++)
        {
            pairs.MoveNext();
            values[i] = pairs.Current.Value.Object;
        }

        return values;
    }

    /// <summary>
    /// Retrieves all keys and values from the <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <param name="result">The <see cref="RuntimeResult"/> object for returning errors.</param>
    /// <returns>The keys and values as an array of <see cref="KeyValuePair"/>s or <see langword="null"/> if something went wrong.</returns>
    public KeyValuePair<IEzrObject, IEzrObject>[]? GetPairs(RuntimeResult result)
    {
        KeyValuePair<IEzrObject, IEzrObject>[] pairsArray = new KeyValuePair<IEzrObject, IEzrObject>[_items.Count];
        using IEnumerator<KeyValuePair<IEzrObject, Reference>> pairs = _items.Values.GetEnumerator();

        for (int i = 0; i < pairsArray.Length; i++)
        {
            pairs.MoveNext();

            if (TryCopyObject(pairs.Current.Key, result) is not IEzrObject keyObject)
                return null;

            pairsArray[i] = new KeyValuePair<IEzrObject, IEzrObject>(keyObject, pairs.Current.Value.Object);
        }

        return pairsArray;
    }

    /// <inheritdoc/>
    public IMutable<RuntimeEzrObjectDictionary>? DeepCopy(RuntimeResult result)
    {
        Dictionary<int, KeyValuePair<IEzrObject, Reference>> copiedItems = new(_items.Count);

        foreach (KeyValuePair<IEzrObject, Reference> pair in _items.Values)
        {
            if (TryCopyObject(pair.Key, result) is not IEzrObject keyObject)
                return null;

            if (TryCopyObject(pair.Value.Object, result) is not IEzrObject valueObject)
                return null;

            Reference valueCopy = ReferencePool.Get(valueObject);
            valueCopy.UpdateRegister(true);

            int hash = keyObject.ComputeHashCode(result);
            if (result.ShouldReturn)
                return null;

            copiedItems.Add(hash, new KeyValuePair<IEzrObject, Reference>(keyObject, valueCopy));
        }

        return new RuntimeEzrObjectDictionary(copiedItems);
    }

    /// <summary>
    /// Releases the references associated with the dictionary, and clears it.
    /// </summary>
    public void Release()
    {
        foreach (KeyValuePair<int, KeyValuePair<IEzrObject, Reference>> item in _items)
        {
            item.Value.Value.UpdateRegister(false);
            ReferencePool.TryRelease(item.Value.Value);
        }

        _items.Clear();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Unlike <see cref="GetKeys(RuntimeResult)"/> or <see cref="GetPairs(RuntimeResult)"/> this
    /// method does NOT copy the keys. So it's best not to expose the key values from this to the
    /// ezr² runtime as mutable key objects can be changed.
    /// </remarks>
    public IEnumerator<KeyValuePair<int, KeyValuePair<IEzrObject, Reference>>> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Unlike <see cref="GetKeys(RuntimeResult)"/> or <see cref="GetPairs(RuntimeResult)"/> this
    /// method does NOT copy the keys. So it's best not to expose the key values from this to the
    /// ezr² runtime as mutable key objects can be changed.
    /// </remarks>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>Destructor.</summary>
    ~RuntimeEzrObjectDictionary() => Release();
}
