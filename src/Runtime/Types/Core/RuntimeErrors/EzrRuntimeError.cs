using EzrSquared.Runtime.Types.Core.Text;
using EzrSquared.Runtime.WrapperAttributes;
using EzrSquared.Util;
using System;
using System.Text;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Implementation of <see cref="IEzrRuntimeError"/> with some utility methods.
/// </summary>
[SharpTypeWrapper("runtime_error")]
public class EzrRuntimeError : EzrObject, IEzrRuntimeError
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "runtime error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.RuntimeError";

    /// <inheritdoc/>
    public string Title { get; }

    /// <inheritdoc/>
    public string Details { get; }

    /// <inheritdoc/>
    public Context ErrorContext { get; }

    /// <inheritdoc/>
    public Position ErrorStartPosition { get; }

    /// <inheritdoc/>
    public Position ErrorEndPosition { get; }

    /// <summary>
    /// Creates a new runtime error object.
    /// </summary>
    /// <param name="title">The title of the error.</param>
    /// <param name="details">Details on why the error happened.</param>
    /// <param name="context">The context in which the error occurred.</param>
    /// <param name="startPosition">The starting position of the error.</param>
    /// <param name="endPosition">The ending position of the error.</param>
    public EzrRuntimeError(string title, string details, Context context, Position startPosition, Position endPosition) : base(context, startPosition, endPosition)
    {
        Title = title;
        Details = details;
        ErrorContext = context;
        ErrorStartPosition = startPosition;
        ErrorEndPosition = endPosition;

        Context.Set(null, "title", ReferencePool.Get(new EzrString(Title, Context, StartPosition, EndPosition), AccessMod.Constant));
        Context.Set(null, "details", ReferencePool.Get(new EzrString(Details, Context, StartPosition, EndPosition), AccessMod.Constant));
    }


    /// <summary>
    /// Wrapper constructor for creating the error object.
    /// </summary>
    /// <param name="arguments">The constructor arguments.</param>
    [SharpMethodWrapper(RequiredParameters = ["title", "details"])]
    public EzrRuntimeError(SharpMethodParameters arguments) : this(
        GetStringArgument("title", arguments.ArgumentReferences["title"].Object, arguments.ExecutionContext, arguments.Result),
        GetStringArgument("details", arguments.ArgumentReferences["details"].Object, arguments.ExecutionContext, arguments.Result),
        arguments.ExecutionContext,
        arguments.StartPosition,
        arguments.EndPosition
    ) { }

    /// <summary>
    /// Converts the given argument to a string.
    /// </summary>
    /// <param name="argumentName">The name of the argument.</param>
    /// <param name="ezrObject">The argument object.</param>
    /// <param name="context">The context of the argument.</param>
    /// <param name="result">Runtime result for carrying any errors.</param>
    /// <returns>The string, or <see cref="string.Empty"/> if failed.</returns>
    protected internal static string GetStringArgument(string argumentName, IEzrObject ezrObject, Context context, RuntimeResult result)
    {
        // Added so that if this is being called after a faulty GetStringArgument call, the error can be passed on to the interpreter
        // without being overridden by another error.
        if (result.ShouldReturn)
            return string.Empty;

        switch (ezrObject)
        {
            case IEzrString ezrString:
                return ezrString.StringValue;

            default:
                result.Failure(new EzrUnexpectedTypeError($"Expected {argumentName} to be text, but got object of type \"{ezrObject.TypeName}\"!", context, ezrObject.StartPosition, ezrObject.EndPosition));
                return string.Empty;
        }
    }

    /// <summary>
    /// Generates the trace back to the error.
    /// </summary>
    /// <returns>The trace.</returns>
    protected internal string GenerateTraceback(int correctedLineNumber)
    {
        StringBuilder result = new();
        Context? context = ErrorContext;

        while (context is not null)
        {
            result.Insert(0, $"  File '{ErrorContext.StartPosition.File}', line {correctedLineNumber} - In '{context.Name}'\n");
            context = context.Parent;
        }

        return result.Insert(0, "Traceback - most recent call last:\n").ToString();
    }

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        bool equal = StrictEquals(other, result);
        if (result.ShouldReturn)
            return;

        result.Success(NewBooleanConstant(equal));
    }

    /// <inheritdoc/>
    public override void ComparisonNotEqual(IEzrObject other, RuntimeResult result)
    {
        bool equal = StrictEquals(other, result);
        if (result.ShouldReturn)
            return;

        result.Success(NewBooleanConstant(!equal));
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, Title, Details);
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return other is EzrRuntimeError otherError && otherError.Title == Title && otherError.Details == Details && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"<{TypeName}>";
    }

    /// <inheritdoc/>
    public override string ToPureString(RuntimeResult result)
    {
        (int adjustedLineNumber, string sourceWithUnderline) = Utils.SourceWithUnderline(ErrorStartPosition, ErrorEndPosition);

        return $"{GenerateTraceback(adjustedLineNumber)}\n{Title}: {Details}\n{sourceWithUnderline}";
    }
}
