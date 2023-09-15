using System.Text;

namespace EzrSquared.EzrErrors
{
    using EzrCommon;

    /// <summary>
    /// The base of all other, more specific, error classes.
    /// </summary>
    public abstract class Error
    {
        /// <summary>
        /// The name of the <see cref="Error"/>.
        /// </summary>
        internal readonly string _title;

        /// <summary>
        /// The reason why the <see cref="Error"/> occurred.
        /// </summary>
        internal readonly string _details;

        /// <summary>
        /// The starting <see cref="Position"/> of the <see cref="Error"/>.
        /// </summary>
        internal readonly Position _startPosition;

        /// <summary>
        /// The ending <see cref="Position"/> of the <see cref="Error"/>.
        /// </summary>
        internal readonly Position _endPosition;

        /// <summary>
        /// Creates a new <see cref="Error"/> object.
        /// </summary>
        /// <param name="title">The title of the <see cref="Error"/>.</param>
        /// <param name="details">The reason why the <see cref="Error"/> occurred.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="Error"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="Error"/>.</param>
        public Error(string title, string details, Position startPosition, Position endPosition)
        {
            _title = title;
            _details = details;
            _startPosition = startPosition;
            _endPosition = endPosition;
        }

        /// <summary>
        /// Creates the formatted text representation of the <see cref="Error"/>.
        /// </summary>
        /// <returns>The formatted text.</returns>
        public override string ToString()
        {
            return $"{_title} in {_startPosition.File}, line {_startPosition.Line}: {_details}\n{StringWithUnderline()}";
        }

        /// <summary>
        /// Creates formatted text which contains the text between <see cref="_startPosition"/> and <see cref="_endPosition"/>, underlined with tilde symbols.
        /// </summary>
        /// <returns>The formatted text.</returns>
        internal string StringWithUnderline()
        {
            string text = _startPosition.Script;
            int start;
            int end;

            int indexOfLastNewLine = text[0..((_startPosition.Index < text.Length) ? _startPosition.Index : text.Length)].LastIndexOf('\n');
            if (indexOfLastNewLine != -1)
                start = indexOfLastNewLine;
            else
                start = 0;

            while (char.IsWhiteSpace(text[start]) && start < text.Length - 1)
                start++;

            end = text.IndexOf('\n', start + 1);
            if (end == -1)
                end = text.Length;

            return new StringBuilder("  ")
                .Append(text[start..end])
                .Append('\n')
                .Append(' ', (_startPosition.Index - start) + 2)
                .Append('~', _endPosition.Index - _startPosition.Index)
                .ToString();
        }
    }

    /// <summary>
    /// The <see cref="Error"/> returned when an unknown character is identified in the script.
    /// </summary>
    internal class UnexpectedCharacterError : Error
    {
        /// <summary>
        /// Creates a new <see cref="UnexpectedCharacterError"/> object.
        /// </summary>
        /// <param name="character">The character that caused the <see cref="UnexpectedCharacterError"/>.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="UnexpectedCharacterError"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="UnexpectedCharacterError"/>.</param>
        public UnexpectedCharacterError(char character, Position startPosition, Position endPosition) : base("Unexpected character", $"{character}", startPosition, endPosition) { }
    }

    /// <summary>
    /// The <see cref="Error"/> returned when an invalid hexadecimal value is identified in the script.
    /// </summary>
    internal class InvalidHexValueError : Error
    {
        /// <summary>
        /// Creates a new <see cref="InvalidHexValueError"/> object.
        /// </summary>
        /// <param name="details">The reason why the <see cref="InvalidHexValueError"/> occurred.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="InvalidHexValueError"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="InvalidHexValueError"/>.</param>
        public InvalidHexValueError(string details, Position startPosition, Position endPosition) : base("Invalid hexadecimal value", details, startPosition, endPosition) { }
    }

    /// <summary>
    /// The <see cref="Error"/> returned when invalid code syntax is identified in the script.
    /// </summary>
    internal class InvalidGrammarError : Error
    {
        /// <summary>
        /// Creates a new <see cref="InvalidGrammarError"/> object.
        /// </summary>
        /// <param name="details">The reason why the <see cref="InvalidGrammarError"/> occurred.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="InvalidGrammarError"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="InvalidGrammarError"/>.</param>
        public InvalidGrammarError(string details, Position startPosition, Position endPosition) : base("Invalid grammar", details, startPosition, endPosition) { }
    }

    /// <summary>
    /// The <see cref="Error"/> returned when multiple <see cref="Error"/> objects need to be returned to the user.
    /// </summary>
    internal class StackedError : Error
    {
        /// <summary>
        /// Creates a new <see cref="StackedError"/> object.
        /// </summary>
        /// <param name="parent">The 'parent' error, or the error that occurred first.</param>
        /// <param name="child">The 'child' error, or the error that occurred because of the <paramref name="parent"/>.</param>
        public StackedError(Error parent, Error child) : base("Multiple errors", string.Join<Error>("\n\nDue to the above error, another one occurred:\n", new Error[2] { parent, child }), parent._startPosition, parent._endPosition) { }

        /// <summary>
        /// Creates the formatted text representation of the <see cref="StackedError"/>, which shows all child <see cref="Error"/> objects as the 'details'.
        /// </summary>
        /// <returns>The formatted text.</returns>
        public override string ToString()
        {
            return _details;
        }
    }
}