using System.Collections.Generic;

namespace EzrSquared.Runtime.Types.Collections;

/// <summary>
/// A read-only collection of <see cref="IEzrObject"/>s.
/// </summary>
public interface IEzrEnumerable : IEzrObject, IEnumerable<IEzrObject>
{
    /// <summary>
    /// Similar to <see cref="IEnumerable{T}.GetEnumerator"/>.
    /// </summary>
    /// <remarks>
    /// Use this when exposing the enumerator to the ezr² runtime.
    /// For example, this is used by <see cref="EzrDictionary"/> to
    /// copy read-only keys so that they can't be edited at runtime.
    /// </remarks>
    /// <param name="result">Runtime result for carrying errors.</param>
    /// <returns>The <see cref="IEnumerator{T}"/>.</returns>
    public IEnumerator<IEzrObject> GetEnumerator(RuntimeResult result);
}
