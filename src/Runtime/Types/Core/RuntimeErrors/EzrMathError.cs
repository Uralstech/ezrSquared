using EzrSquared.Runtime.WrapperAttributes;
using System.Collections.Generic;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Error type for when a generic mathematical error occurs.
/// </summary>
/// <param name="title">The title of the error.</param>
/// <param name="details">Details on why the error happened.</param>
/// <param name="context">The context in which the error occurred.</param>
/// <param name="startPosition">The starting position of the error.</param>
/// <param name="endPosition">The ending position of the error.</param>
[SharpTypeWrapper("math_error", nameof(WrapperConstructor))]
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
    public static new void WrapperConstructor(SharpMethodParameters arguments)
    {
        RuntimeResult result = arguments.Result;
        Reference titleReference = arguments.ArgumentReferences["title"];
        Reference detailsReference = arguments.ArgumentReferences["details"];

        string title = GetStringArgument("title", titleReference.Object, arguments.ExecutionContext, result);
        if (result.ShouldReturn)
            return;

        string details = GetStringArgument("details", detailsReference.Object, arguments.ExecutionContext, result);
        if (result.ShouldReturn)
            return;

        result.Success(ReferencePool.Get(new EzrMathError(title, details, arguments.ExecutionContext, arguments.StartPosition, arguments.EndPosition), AccessMod.PrivateConstant));
    }
}
