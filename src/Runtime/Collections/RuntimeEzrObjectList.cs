using EzrSquared.Runtime.Types;
using System.Collections.Generic;

namespace EzrSquared.Runtime.Collections;

/// <summary>
/// A List for <see cref="Reference"/>s.
/// </summary>
public class RuntimeEzrObjectList : List<Reference>, IMutable<RuntimeEzrObjectList>
{
    /// <summary>
    /// Creates a new <see cref="RuntimeEzrObjectList"/>.
    /// </summary>
    public RuntimeEzrObjectList() : base() { }

    /// <summary>
    /// Creates a new <see cref="RuntimeEzrObjectList"/> with the specified capacity.
    /// </summary>
    /// <param name="capacity">The capacity of the list.</param>
    public RuntimeEzrObjectList(int capacity) : base(capacity) { }

    /// <summary>
    /// Creates a new <see cref="RuntimeEzrObjectList"/> based on another <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="collection">The base collection of <see cref="Reference"/>s.</param>
    public RuntimeEzrObjectList(IEnumerable<Reference> collection) : base(collection) { }

    /// <summary>
    /// Removes an element in the list at the given index.
    /// </summary>
    /// <param name="index">The index.</param>
    public new void RemoveAt(int index)
    {
        Reference reference = this[index];

        reference.UpdateRegister(false);
        ReferencePool.TryRelease(reference);

        base.RemoveAt(index);
    }

    /// <summary>
    /// Removes a range of elements in the list.
    /// </summary>
    /// <param name="index">The index to start the range.</param>
    /// <param name="length">The length of the range.</param>
    public new void RemoveRange(int index, int length)
    {
        foreach (Reference reference in GetRange(index, length))
        {
            reference.UpdateRegister(false);
            ReferencePool.TryRelease(reference);
        }

        base.RemoveRange(index, length);
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the list.
    /// </summary>
    /// <param name="index">The index to start the range.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>The copy.</returns>
    public new RuntimeEzrObjectList GetRange(int index, int length)
    {
        return new RuntimeEzrObjectList(base.GetRange(index, length));
    }

    /// <inheritdoc/>
    public IMutable<RuntimeEzrObjectList>? DeepCopy(RuntimeResult result)
    {
        RuntimeEzrObjectList copy = [];
        for (int i = 0; i < Count; i++)
        {
            Reference newReference = ReferencePool.Get(this[i].Object, this[i].AccessibilityModifiers);
            newReference.UpdateRegister(true);

            if (newReference.Object is IEzrMutableObject mutableElement)
            {
                IEzrObject? objectCopy = (IEzrObject?)mutableElement.DeepCopy(result);
                if (result.ShouldReturn)
                    return null;

                newReference.UpdateObject(objectCopy!);
            }

            copy.Add(newReference);
        }

        return copy;
    }

    /// <summary>
    /// Releases the references associated with the list, and clears it.
    /// </summary>
    public void Release()
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].UpdateRegister(false);
            ReferencePool.TryRelease(this[i]);
        }

        Clear();
        TrimExcess();
    }

    /// <summary>Destructor.</summary>
    ~RuntimeEzrObjectList() => Release();
}
