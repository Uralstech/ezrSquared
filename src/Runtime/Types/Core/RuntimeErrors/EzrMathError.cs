using EzrSquared.Runtime.WrapperAttributes;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Error type for when a generic mathematical error occurs.
/// </summary>
/// <param name="title">The title of the error.</param>
/// <param name="details">Details on why the error happened.</param>
/// <param name="context">The context in which the error occurred.</param>
/// <param name="startPosition">The starting position of the error.</param>
/// <param name="endPosition">The ending position of the error.</param>
[SharpTypeWrapper("math_error")]
public class EzrMathError(string title, string details, Context context, Position startPosition, Position endPosition) : EzrRuntimeError(title, details, context, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "math error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.MathError";

    /// <summary>
    /// Wrapper constructor for creating the error object.
    /// </summary>
    /// <param name="arguments">The constructor arguments.</param>
    [SharpMethodWrapper(RequiredParameters = ["title", "details"])]
    public EzrMathError(SharpMethodParameters arguments) : this(
        GetStringArgument("title", arguments.ArgumentReferences["title"].Object, arguments.ExecutionContext, arguments.Result),
        GetStringArgument("details", arguments.ArgumentReferences["details"].Object, arguments.ExecutionContext, arguments.Result),
        arguments.ExecutionContext,
        arguments.StartPosition,
        arguments.EndPosition
    ) { }
}
