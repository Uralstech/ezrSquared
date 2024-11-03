namespace EzrSquared.Syntax.Errors;

/// <summary>
/// Error class for all syntax errors.
/// </summary>
/// <param name="title">The title of the <see cref="EzrSyntaxError"/>.</param>
/// <param name="details">The reason why the <see cref="EzrSyntaxError"/> occurred.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="EzrSyntaxError"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="EzrSyntaxError"/>.</param>
public class EzrSyntaxError(string title, string details, Position startPosition, Position endPosition) : IEzrError
{
    /// <summary>An unexpected character was encountered.</summary>
    public const string UnexpectedCharacter = "Unexpected character";

    /// <summary>An invalid hexadecimal value was encountered.</summary>
    public const string InvalidHexValue = "Invalid hexadecimal value";

    /// <summary>Invalid grammar was encountered.</summary>
    public const string InvalidGrammar = "Invalid grammar";

    /// <inheritdoc/>
    public string Title { get; } = title;

    /// <inheritdoc/>
    public string Details { get; } = details;

    /// <inheritdoc/>
    public Position ErrorStartPosition { get; } = startPosition;

    /// <inheritdoc/>
    public Position ErrorEndPosition { get; } = endPosition;

    /// <summary>
    /// Creates the formatted text representation of the <see cref="EzrSyntaxError"/>.
    /// </summary>
    /// <returns>The formatted text.</returns>
    public override string ToString()
    {
        (int adjustedLineNumber, string sourceWithUnderline) = IEzrError.SourceWithUnderline(ErrorStartPosition, ErrorEndPosition);
        return $"{Title} in {ErrorStartPosition.File}, line {adjustedLineNumber}: {Details}\n{sourceWithUnderline}";
    }
}
