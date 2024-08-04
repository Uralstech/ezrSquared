using EzrSquared.Util;

namespace EzrSquared.Syntax.Errors;

/// <summary>
/// Error class for all syntax error.
/// </summary>
/// <param name="title">The title of the <see cref="SyntaxError"/>.</param>
/// <param name="details">The reason why the <see cref="SyntaxError"/> occurred.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="SyntaxError"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="SyntaxError"/>.</param>
public class SyntaxError(string title, string details, Position startPosition, Position endPosition)
{
    /// <summary>An unexpected character was encountered.</summary>
    public const string UnexpectedCharacter = "Unexpected character";

    /// <summary>An invalid hexadecimal value was encountered.</summary>
    public const string InvalidHexValue = "Invalid hexadecimal value";

    /// <summary>Invalid grammar was encountered.</summary>
    public const string InvalidGrammar = "Invalid grammar";

    /// <summary>
    /// The name of the <see cref="SyntaxError"/>.
    /// </summary>
    protected internal readonly string _title = title;

    /// <summary>
    /// The reason why the <see cref="SyntaxError"/> occurred.
    /// </summary>
    protected internal readonly string _details = details;

    /// <summary>
    /// The starting <see cref="Position"/> of the <see cref="SyntaxError"/>.
    /// </summary>
    protected internal readonly Position _startPosition = startPosition;

    /// <summary>
    /// The ending <see cref="Position"/> of the <see cref="SyntaxError"/>.
    /// </summary>
    protected internal readonly Position _endPosition = endPosition;

    /// <summary>
    /// Creates the formatted text representation of the <see cref="SyntaxError"/>.
    /// </summary>
    /// <returns>The formatted text.</returns>
    public override string ToString()
    {
        (int adjustedLineNumber, string sourceWithUnderline) = Utils.SourceWithUnderline(_startPosition, _endPosition);

        return $"{_title} in {_startPosition.File}, line {adjustedLineNumber}: {_details}\n{sourceWithUnderline}";
    }
}
