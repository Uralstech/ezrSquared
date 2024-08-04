using EzrSquared.Runtime.WrapperAttributes;
using System.Collections.Generic;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Error type for when wrapping an unsupported type is attempted.
/// </summary>
/// <param name="details">Details on why the error happened.</param>
/// <param name="context">The context in which the error occurred.</param>
/// <param name="startPosition">The starting position of the error.</param>
/// <param name="endPosition">The ending position of the error.</param>
[SharpTypeWrapper("unsupported_wrapping_error", nameof(WrapperConstructor))]
public class EzrUnsupportedWrappingError(string details, Context context, Position startPosition, Position endPosition) : EzrRuntimeError("Unsupported wrapping", details, context, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "unsupported csharp wrapping error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.UnsupportedCSharpWrappingError";

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

        result.Success(ReferencePool.Get(new EzrUnsupportedWrappingError(details, arguments.ExecutionContext, arguments.StartPosition, arguments.EndPosition), AccessMod.PrivateConstant));
    }
}
