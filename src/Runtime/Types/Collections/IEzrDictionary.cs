using System.Collections.Generic;

namespace EzrSquared.Runtime.Types.Collections;

/// <summary>
/// Interface for a keyed collection of ezr² objects.
/// </summary>
public interface IEzrDictionary : IEzrObject, IEzrEnumerable, IReadOnlyCollection<IEzrObject>
{
    /// <summary>
    /// Gets the object at the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="result">Runtime result for operations on the <paramref name="key"/>.</param>
    /// <returns>The object at the key.</returns>
    public IEzrObject At(IEzrObject key, RuntimeResult result);

    /// <summary>
    /// Tries to get the object at the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="result">Runtime result for operations on the <paramref name="key"/>.</param>
    /// <returns>The object at the key or <see langword="null"/> if not found or something went wrong.</returns>
    public IEzrObject? TryAt(IEzrObject key, RuntimeResult result);
}
