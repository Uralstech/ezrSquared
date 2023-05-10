using System;
using System.Text;

namespace EzrSquared.EzrErrors
{
    using EzrGeneral;

    /// <summary>
    /// The base of all other, more specific, error classes. Only for inheritance!
    /// </summary>
    public abstract class Error
    {
        /// <summary>
        /// The name of the <see cref="Error"/>.
        /// </summary>
        internal readonly string _name;

        /// <summary>
        /// The reason why the <see cref="Error"/> occured.
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
        /// <param name="name">The name of the <see cref="Error"/>.</param>
        /// <param name="details">The reason why the <see cref="Error"/> occured.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="Error"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="Error"/>.</param>
        public Error(string name, string details, Position startPosition, Position endPosition)
        {
            _name = name;
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
            return $"(error) {_name}: {_details} -> File \"{_startPosition.File}\", line {_startPosition.Line}\n{StringWithUnderline()}";
        }

        /// <summary>
        /// Creates formatted text which contains the text between <see cref="_startPosition"/> and <see cref="_endPosition"/>, underlined with tilde symbols.
        /// </summary>
        /// <returns>The formatted text.</returns>
        internal string StringWithUnderline()
        {
            string text = _startPosition.Script;
            int start = Math.Max(text[0..((_startPosition.Index <= text.Length) ? _startPosition.Index : text.Length)].LastIndexOf('\n'), 0);
            int end = text.IndexOf('\n', start + 1);
            if (end == -1)
                end = text.Length;

            return new StringBuilder(text[start..end]).Append('\n').Append(' ', _startPosition.Index).Append('~', _endPosition.Index - _startPosition.Index).Replace("\t", string.Empty).ToString();
        }
    }

    /// <summary>
    /// The <see cref="Error"/> returned when an unknown character is identified in the script.
    /// </summary>
    internal class UnknownCharacterError : Error
    {
        /// <summary>
        /// Creates a new <see cref="UnknownCharacterError"/> object.
        /// </summary>
        /// <param name="details">The reason why the <see cref="UnknownCharacterError"/> occured.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="UnknownCharacterError"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="UnknownCharacterError"/>.</param>
        public UnknownCharacterError(string details, Position startPosition, Position endPosition) : base("Unknown character", details, startPosition, endPosition) { }
    }

    /// <summary>
    /// The <see cref="Error"/> returned when an invalid hexadecimal value is identified in the script.
    /// </summary>
    internal class InvalidHexValueError : Error
    {
        /// <summary>
        /// Creates a new <see cref="InvalidHexValueError"/> object.
        /// </summary>
        /// <param name="details">The reason why the <see cref="InvalidHexValueError"/> occured.</param>
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
        /// <param name="details">The reason why the <see cref="InvalidGrammarError"/> occured.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="InvalidGrammarError"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="InvalidGrammarError"/>.</param>
        public InvalidGrammarError(string details, Position startPosition, Position endPosition) : base("Invalid grammar", details, startPosition, endPosition) { }
    }
}