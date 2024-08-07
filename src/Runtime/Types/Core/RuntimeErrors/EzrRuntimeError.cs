using EzrSquared.Runtime.Types.Core.Text;
using EzrSquared.Runtime.WrapperAttributes;
using EzrSquared.Util;
using System;
using System.Text;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Base of all error type objects.
/// </summary>
[SharpTypeWrapper("runtime_error", nameof(WrapperConstructor))]
public class EzrRuntimeError : EzrObject
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "runtime error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.RuntimeError";

    /// <summary>
    /// The name of the <see cref="EzrRuntimeError"/>.
    /// </summary>
    public readonly string Title;

    /// <summary>
    /// The reason why the <see cref="EzrRuntimeError"/> occurred.
    /// </summary>
    public readonly string Details;

    /// <summary>
    /// The context where the error occurred.
    /// </summary>
    public readonly Context ErrorContext;

    /// <summary>
    /// The starting position of the error.
    /// </summary>
    public readonly Position ErrorStartPosition;

    /// <summary>
    /// The ending position of the error.
    /// </summary>
    public readonly Position ErrorEndPosition;

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
    /// Converts the given argument to a string.
    /// </summary>
    /// <param name="argumentName">The name of the argument.</param>
    /// <param name="ezrObject">The argument object.</param>
    /// <param name="context">The context of the argument.</param>
    /// <param name="result">Runtime result for carrying errors.</param>
    /// <returns>The string, or <see cref="string.Empty"/> if failed.</returns>
    protected internal static string GetStringArgument(string argumentName, IEzrObject ezrObject, Context context, RuntimeResult result)
    {
        switch (ezrObject)
        {
            case EzrString ezrString:
                return ezrString.Value;

            case EzrCharacter ezrCharacter:
                return ezrCharacter.Value.ToString();

            case EzrCharacterList ezrCharacterList:
                return ezrCharacterList.StringValue;

            default:
                result.Failure(new EzrUnexpectedTypeError($"Expected {argumentName} of type string, character or character list, but got object of type \"{ezrObject.TypeName}\"!", context, ezrObject.StartPosition, ezrObject.EndPosition));
                return string.Empty;
        }
    }

    /// <summary>
    /// Wrapper constructor for creating the error object.
    /// </summary>
    /// <param name="arguments">The constructor arguments.</param>
    [SharpMethodWrapper(RequiredParameters = ["title", "details"])]
    public static void WrapperConstructor(SharpMethodParameters arguments)
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

        result.Success(ReferencePool.Get(new EzrRuntimeError(title, details, arguments.ExecutionContext, arguments.StartPosition, arguments.EndPosition), AccessMod.PrivateConstant));
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
        return $"<{TypeName} \"{Title}\">";
    }

    /// <inheritdoc/>
    public override string ToPureString(RuntimeResult result)
    {
        (int adjustedLineNumber, string sourceWithUnderline) = Utils.SourceWithUnderline(ErrorStartPosition, ErrorEndPosition);

        return $"{GenerateTraceback(adjustedLineNumber)}\n{Title}: {Details}\n{sourceWithUnderline}";
    }
}
