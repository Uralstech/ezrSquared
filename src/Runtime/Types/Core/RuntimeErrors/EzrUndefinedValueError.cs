using EzrSquared.Runtime.WrapperAttributes;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Error type for when an undefined value is encountered.
/// </summary>
/// <param name="details">Details on why the error happened.</param>
/// <param name="context">The context in which the error occurred.</param>
/// <param name="startPosition">The starting position of the error.</param>
/// <param name="endPosition">The ending position of the error.</param>
[SharpTypeWrapper("undefined_value_error", nameof(WrapperConstructor))]
public class EzrUndefinedValueError(string details, Context context, Position startPosition, Position endPosition) : EzrRuntimeError("Undefined value", details, context, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "undefined value error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.UndefinedValueError";

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

        result.Success(ReferencePool.Get(new EzrUndefinedValueError(details, arguments.ExecutionContext, arguments.StartPosition, arguments.EndPosition), AccessMod.PrivateConstant));
    }
}
