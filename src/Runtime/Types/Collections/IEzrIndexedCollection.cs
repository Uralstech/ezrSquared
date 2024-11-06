using System.Collections.Generic;

namespace EzrSquared.Runtime.Types.Collections;

/// <summary>
/// Interface for an indexed collection of ezr² objects.
/// </summary>
public interface IEzrIndexedCollection : IEzrObject, IEzrEnumerable, IReadOnlyCollection<IEzrObject>
{
    /// <summary>
    /// Gets the object at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The object at the index.</returns>
    public IEzrObject At(int index);
}
