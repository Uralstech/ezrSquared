using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.Attributes;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables.Attributes;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Error type for when an unexpected type is encountered.
/// </summary>
/// <param name="details">Details on why the error happened.</param>
/// <param name="context">The context in which the error occurred.</param>
/// <param name="startPosition">The starting position of the error.</param>
/// <param name="endPosition">The ending position of the error.</param>
[WrappedMember("unexpected_type_error")]
public class EzrUnexpectedTypeError(string details, Context context, Position startPosition, Position endPosition) : EzrRuntimeError("Unexpected type", details, context, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "unexpected type error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.UnexpectedTypeError";

    /// <summary>
    /// Wrapper constructor for creating the error object.
    /// </summary>
    /// <param name="details">Details on why the error happened.</param>
    /// <param name="wrapper">The caller.</param>
    /// <param name="executionContext">The execution context.</param>
    [WrappedMember, PrimaryConstructor]
    public EzrUnexpectedTypeError(string details,
        [Runtime(Feature.CallerRef)] IEzrObject wrapper,
        [Runtime(Feature.ExecutionRef)] Context executionContext)
        : this(
            details,
            executionContext,
            wrapper.StartPosition,
            wrapper.EndPosition)
    { }
}
