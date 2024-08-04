using EzrSquared.Runtime.Types;
using System.Collections.Generic;

namespace EzrSquared.Runtime.Collections;

/// <summary>
/// A Dictionary for <see cref="IEzrObject"/>s.
/// </summary>
/// \bug If the keys can be access and are mutable objects, they can still be changed.
public class RuntimeEzrObjectDictionary : IMutable<RuntimeEzrObjectDictionary>
{
    /// <summary>
    /// The collection of <see cref="IEzrObject"/>s stored in the <see cref="RuntimeEzrObjectDictionary"/>, in the format Dictionary&lt;hashOfKey, KeyValuePair&lt;key, valueReference&gt;&gt;.
    /// </summary>
    private readonly Dictionary<int, KeyValuePair<IEzrObject, Reference>> _items;

    /// <summary>
    /// Creates a new <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    public RuntimeEzrObjectDictionary()
    {
        _items = [];
    }

    /// <summary>
    /// Creates a new <see cref="RuntimeEzrObjectDictionary"/> from an existing <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    public RuntimeEzrObjectDictionary(Dictionary<int, KeyValuePair<IEzrObject, Reference>> items)
    {
        _items = items;
    }

    /// <summary>
    /// The number of <see cref="IEzrObject"/>s in the <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    public int Length => _items.Count;

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
            if (key is IEzrMutableObject mutableElement)
            {
                IEzrObject? keyCopy = (IEzrObject?)mutableElement.DeepCopy(result);
                if (result.ShouldReturn)
                    return;

                key = keyCopy!;
            }

            Reference valueReference = ReferencePool.Get(value);
            valueReference.UpdateRegister(true);

            _items.Add(hash, new KeyValuePair<IEzrObject, Reference>(key, valueReference));
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
    public bool RemoveHash(int key)
    {
        return _items.Remove(key);
    }

    /// <summary>
    /// Merges a <see cref="RuntimeEzrObjectDictionary"/> to the current <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <param name="other">The other <see cref="RuntimeEzrObjectDictionary"/> to be merged.</param>
    public void Merge(RuntimeEzrObjectDictionary other)
    {
        foreach (KeyValuePair<int, KeyValuePair<IEzrObject, Reference>> pair in other._items)
            _items[pair.Key] = pair.Value;
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
    public bool HasKey(IEzrObject key, RuntimeResult result)
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
    /// Retrieves all keys from the <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <returns>The keys.</returns>
    public IEzrObject[] GetKeys()
    {
        KeyValuePair<IEzrObject, Reference>[] pairs = [.. _items.Values];
        IEzrObject[] keys = new IEzrObject[pairs.Length];

        for (int i = 0; i < pairs.Length; i++)
            keys[i] = pairs[i].Key;
        return keys;
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
    /// Retrieves all values from the <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <returns>The values.</returns>
    public IEzrObject[] GetValues()
    {
        KeyValuePair<IEzrObject, Reference>[] pairs = [.. _items.Values];
        IEzrObject[] values = new IEzrObject[pairs.Length];

        for (int i = 0; i < pairs.Length; i++)
            values[i] = pairs[i].Value.Object;
        return values;
    }

    /// <summary>
    /// Retrieves all keys and values from the <see cref="RuntimeEzrObjectDictionary"/>.
    /// </summary>
    /// <returns>The keys and values as an array of <see cref="KeyValuePair"/>s.</returns>
    public KeyValuePair<IEzrObject, IEzrObject>[] GetPairs()
    {
        KeyValuePair<IEzrObject, Reference>[] pairs = [.. _items.Values];
        KeyValuePair<IEzrObject, IEzrObject>[] purePairs = new KeyValuePair<IEzrObject, IEzrObject>[pairs.Length];

        for (int i = 0; i < pairs.Length; i++)
            purePairs[i] = new KeyValuePair<IEzrObject, IEzrObject>(pairs[i].Key, pairs[i].Value.Object);
        return purePairs;
    }

    /// <inheritdoc/>
    public IMutable<RuntimeEzrObjectDictionary>? DeepCopy(RuntimeResult result)
    {
        KeyValuePair<IEzrObject, Reference>[] keyValuePairs = [.. _items.Values];
        Dictionary<int, KeyValuePair<IEzrObject, Reference>> copiedItems = new(_items.Count);

        for (int i = 0; i < keyValuePairs.Length; i++)
        {
            (IEzrObject key, Reference value) = (keyValuePairs[i].Key, keyValuePairs[i].Value);
            if (key is IEzrMutableObject mutableKey)
            {
                IEzrObject? keyCopy = (IEzrObject?)mutableKey.DeepCopy(result);
                if (result.ShouldReturn)
                    return null;

                key = keyCopy!;
            }

            IEzrObject valueObject = value.Object;
            if (valueObject is IEzrMutableObject mutableValue)
            {
                IEzrObject? valueObjectCopy = (IEzrObject?)mutableValue.DeepCopy(result);
                if (result.ShouldReturn)
                    return null;

                valueObject = valueObjectCopy!;
            }

            Reference valueCopy = ReferencePool.Get(valueObject);
            valueCopy.UpdateRegister(true);

            int hash = key.ComputeHashCode(result);
            if (result.ShouldReturn)
                return null;

            copiedItems.Add(hash, new KeyValuePair<IEzrObject, Reference>(key, valueCopy));
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
        _items.TrimExcess();
    }

    /// <summary>Destructor.</summary>
    ~RuntimeEzrObjectDictionary() => Release();
}
