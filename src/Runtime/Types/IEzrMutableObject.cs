namespace EzrSquared.Runtime.Types;

/// <summary>
/// A mutable object.
/// </summary>
/// <typeparam name="T">The type inheriting this interface.</typeparam>
public interface IMutable<T>
{
    /// <summary>
    /// Creates a deep copy of the <see cref="IMutable{T}"/>.
    /// </summary>
    /// <remarks>
    /// The deep copy here means that all <see cref="IMutable{T}"/> properties and fields in the object are also copied.
    /// </remarks>
    /// <param name="result">Runtime result for raising errors./</param>
    /// <returns>The copy, or, <see langword="null"/> if failed.</returns>
    public IMutable<T>? DeepCopy(RuntimeResult result);
}

/// <summary>
/// A mutable <see cref="IEzrObject"/>.
/// </summary>
public interface IEzrMutableObject : IMutable<IEzrMutableObject>, IEzrObject { }
