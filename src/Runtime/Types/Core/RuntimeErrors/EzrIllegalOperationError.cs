using EzrSquared.Runtime.Types.Wrappers;
using EzrSquared.Runtime.Types.Wrappers.Members.Methods;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Error type for when a generic illegal operation occurs.
/// </summary>
/// <param name="details">Details on why the error happened.</param>
/// <param name="context">The context in which the error occurred.</param>
/// <param name="startPosition">The starting position of the error.</param>
/// <param name="endPosition">The ending position of the error.</param>
[WrapMember("illegal_operation_error")]
public class EzrIllegalOperationError(string details, Context context, Position startPosition, Position endPosition) : EzrRuntimeError("Illegal operation", details, context, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "illegal operation error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.IllegalOperationError";

    /// <summary>
    /// Wrapper constructor for creating the error object.
    /// </summary>
    /// <param name="details">Details on why the error happened.</param>
    /// <param name="wrapper">The caller.</param>
    /// <param name="executionContext">The execution context.</param>
    [WrapMember, PrimaryConstructor]
    public EzrIllegalOperationError(string details,
        [FeatureParameter(Feature.CallerRef)] IEzrObject wrapper,
        [FeatureParameter(Feature.ExecutionRef)] Context executionContext)
        : this(
            details,
            executionContext,
            wrapper.StartPosition,
            wrapper.EndPosition)
    { }
}
