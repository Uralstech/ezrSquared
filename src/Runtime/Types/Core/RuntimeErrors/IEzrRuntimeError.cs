namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Base of all error types.
/// </summary>
public interface IEzrRuntimeError : IEzrObject, IEzrError
{
    /// <summary>
    /// The context where the error occurred.
    /// </summary>
    public Context ErrorContext { get; }
}
