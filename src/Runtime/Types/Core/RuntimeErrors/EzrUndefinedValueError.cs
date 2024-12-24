using EzrSquared.Runtime.Types.Wrappers;
using EzrSquared.Runtime.Types.Wrappers.Members.Methods;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Error type for when an undefined value is encountered.
/// </summary>
/// <param name="details">Details on why the error happened.</param>
/// <param name="context">The context in which the error occurred.</param>
/// <param name="startPosition">The starting position of the error.</param>
/// <param name="endPosition">The ending position of the error.</param>
[WrapMember("undefined_value_error")]
public class EzrUndefinedValueError(string details, Context context, Position startPosition, Position endPosition) : EzrRuntimeError("Undefined value", details, context, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "undefined value error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.UndefinedValueError";

    /// <summary>
    /// Wrapper constructor for creating the error object.
    /// </summary>
    /// <param name="details">Details on why the error happened.</param>
    /// <param name="wrapper">The caller.</param>
    /// <param name="executionContext">The execution context.</param>
    [WrapMember, PrimaryConstructor]
    public EzrUndefinedValueError(string details,
        [FeatureParameter(Feature.CallerRef)] IEzrObject wrapper,
        [FeatureParameter(Feature.ExecutionRef)] Context executionContext)
        : this(
            details,
            executionContext,
            wrapper.StartPosition,
            wrapper.EndPosition)
    { }
}
