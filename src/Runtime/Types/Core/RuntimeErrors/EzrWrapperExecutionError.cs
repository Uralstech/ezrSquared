using EzrSquared.Runtime.WrapperAttributes;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Error type for when an error related to the execution of C# wrappers occurs.
/// </summary>
/// <param name="details">Details on why the error happened.</param>
/// <param name="context">The context in which the error occurred.</param>
/// <param name="startPosition">The starting position of the error.</param>
/// <param name="endPosition">The ending position of the error.</param>
[SharpTypeWrapper("wrapper_execution_error")]
public class EzrWrapperExecutionError(string details, Context context, Position startPosition, Position endPosition) : EzrRuntimeError("Wrapper execution error", details, context, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp wrapper execution error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpWrapperExecutionError";

    /// <summary>
    /// Wrapper constructor for creating the error object.
    /// </summary>
    /// <param name="arguments">The constructor arguments.</param>
    [SharpMethodWrapper(RequiredParameters = ["details"])]
    public EzrWrapperExecutionError(SharpMethodParameters arguments) : this(
        GetStringArgument("details", arguments.ArgumentReferences["details"].Object, arguments.ExecutionContext, arguments.Result),
        arguments.ExecutionContext,
        arguments.StartPosition,
        arguments.EndPosition
    )
    { }
}
