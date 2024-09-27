namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Base of all error types.
/// </summary>
public interface IEzrRuntimeError : IEzrObject
{
    /// <summary>
    /// The title of the error.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// The reason why the error occurred.
    /// </summary>
    public string Details { get; }

    /// <summary>
    /// The context where the error occurred.
    /// </summary>
    public Context ErrorContext { get; }

    /// <summary>
    /// The starting position of the error.
    /// </summary>
    public Position ErrorStartPosition { get; }

    /// <summary>
    /// The ending position of the error.
    /// </summary>
    public Position ErrorEndPosition { get; }
}
