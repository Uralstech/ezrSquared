using EzrSquared.Runtime.WrapperAttributes;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Error type for when a key-not-found error occurs.
/// </summary>
/// <param name="details">Details on why the error happened.</param>
/// <param name="context">The context in which the error occurred.</param>
/// <param name="startPosition">The starting position of the error.</param>
/// <param name="endPosition">The ending position of the error.</param>
[SharpTypeWrapper("key_not_found_error", nameof(WrapperConstructor))]
public class EzrKeyNotFoundError(string details, Context context, Position startPosition, Position endPosition) : EzrRuntimeError("Key not found", details, context, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "key not found error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.KeyNotFoundError";

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

        result.Success(ReferencePool.Get(new EzrKeyNotFoundError(details, arguments.ExecutionContext, arguments.StartPosition, arguments.EndPosition), AccessMod.PrivateConstant));
    }
}
