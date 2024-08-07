using EzrSquared.Runtime.WrapperAttributes;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Error type for when an unexpected type is encountered.
/// </summary>
/// <param name="details">Details on why the error happened.</param>
/// <param name="context">The context in which the error occurred.</param>
/// <param name="startPosition">The starting position of the error.</param>
/// <param name="endPosition">The ending position of the error.</param>
[SharpTypeWrapper("unexpected_type_error", nameof(WrapperConstructor))]
public class EzrUnexpectedTypeError(string details, Context context, Position startPosition, Position endPosition) : EzrRuntimeError("Unexpected type", details, context, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "unexpected type error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.UnexpectedTypeError";

    /// <summary>
    /// Wrapper constructor for creating the error object.
    /// </summary>
    /// <param name="arguments">The constructor arguments.</param>
    [SharpMethodWrapper(RequiredParameters = ["details"])]
    public static new void WrapperConstructor(SharpMethodParameters arguments)
    {
        RuntimeResult result = arguments.Result;
        Reference detailsReference = arguments.ArgumentReferences["details"];

        string details = GetStringArgument("details", detailsReference.Object, arguments.ExecutionContext, result);
        if (result.ShouldReturn)
            return;

        result.Success(ReferencePool.Get(new EzrUnexpectedTypeError(details, arguments.ExecutionContext, arguments.StartPosition, arguments.EndPosition), AccessMod.PrivateConstant));
    }
}
