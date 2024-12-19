namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables.Attributes;

/// <summary>
/// The metadata for <see cref="RuntimeAttribute"/>.
/// </summary>
public enum Feature
{
    /// <summary>
    /// Requests extra keyword arguments.
    /// The parameter must be assignable from <see cref="ExtraKeywordArguments"/>.
    /// </summary>
    KeywordArguments,

    /// <summary>
    /// Requests extra positional arguments.
    /// The parameter must be assignable from <see cref="ExtraPositionalArguments"/>.
    /// </summary>
    PositionalArguments,

    /// <summary>
    /// Requests a reference to the calling <see cref="IEzrObject"/>.
    /// The parameter must be assignable from <see cref="IEzrObject"/>.
    /// </summary>
    CallerRef,

    /// <summary>
    /// Requests a reference to the execution context.
    /// The parameter must be assignable from <see cref="Context"/>.
    /// </summary>
    ExecutionRef,

    /// <summary>
    /// Requests a reference to the execution context.
    /// The parameter must be assignable from <see cref="Interpreter"/>.
    /// </summary>
    InterpreterRef,

    /// <summary>
    /// Requests a reference to the current <see cref="RuntimeResult"/>.
    /// The parameter must be assignable from <see cref="RuntimeResult"/>.
    /// </summary>
    ResultRef,
}