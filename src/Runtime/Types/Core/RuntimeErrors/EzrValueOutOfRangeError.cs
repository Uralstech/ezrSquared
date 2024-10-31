using EzrSquared.Runtime.WrapperAttributes;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Error type for when a value is out of a range.
/// </summary>
/// <param name="details">Details on why the error happened.</param>
/// <param name="context">The context in which the error occurred.</param>
/// <param name="startPosition">The starting position of the error.</param>
/// <param name="endPosition">The ending position of the error.</param>
[SharpTypeWrapper("value_out_of_range_error")]
public class EzrValueOutOfRangeError(string details, Context context, Position startPosition, Position endPosition) : EzrRuntimeError("Value out of range", details, context, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "value out of range error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.ValueOutOfRangeError";

    /// <summary>
    /// Wrapper constructor for creating the error object.
    /// </summary>
    /// <param name="arguments">The constructor arguments.</param>
    [SharpMethodWrapper(RequiredParameters = ["details"])]
    public EzrValueOutOfRangeError(SharpMethodParameters arguments) : this(
        GetStringArgument("details", arguments.ArgumentReferences["details"].Object, arguments.ExecutionContext, arguments.Result),
        arguments.ExecutionContext,
        arguments.StartPosition,
        arguments.EndPosition
    )
    { }
}
