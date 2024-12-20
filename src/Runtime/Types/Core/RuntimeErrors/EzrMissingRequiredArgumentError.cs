using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.Attributes;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables.Attributes;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Error type for when an argument is missing.
/// </summary>
/// <param name="details">Details on why the error happened.</param>
/// <param name="context">The context in which the error occurred.</param>
/// <param name="startPosition">The starting position of the error.</param>
/// <param name="endPosition">The ending position of the error.</param>
[WrappedMember("missing_required_argument_error")]
public class EzrMissingRequiredArgumentError(string details, Context context, Position startPosition, Position endPosition) : EzrRuntimeError("Missing required argument", details, context, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "missing required argument error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.MissingRequiredArgumentError";

    /// <summary>
    /// Wrapper constructor for creating the error object.
    /// </summary>
    /// <param name="details">Details on why the error happened.</param>
    /// <param name="wrapper">The caller.</param>
    /// <param name="executionContext">The execution context.</param>
    [WrappedMember, PrimaryConstructor]
    public EzrMissingRequiredArgumentError(string details,
        [Runtime(Feature.CallerRef)] IEzrObject wrapper,
        [Runtime(Feature.ExecutionRef)] Context executionContext)
        : this(
            details,
            executionContext,
            wrapper.StartPosition,
            wrapper.EndPosition) { }
}
