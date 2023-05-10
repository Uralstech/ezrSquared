namespace EzrSquared.EzrGeneral
{
    /// <summary>
    /// The representation of a position in the script.
    /// </summary>
    public class Position
    {
        /// <summary>
        /// The index of the <see cref="Position"/> in the script.
        /// </summary>
        public int Index;

        /// <summary>
        /// The line number of the <see cref="Position"/> in the script.
        /// </summary>
        public int Line;

        /// <summary>
        /// The file name/path of the script.
        /// </summary>
        public readonly string File;

        /// <summary>
        /// The script as text.
        /// </summary>
        public readonly string Script;

        /// <summary>
        /// Creates a new <see cref="Position"/> object.
        /// </summary>
        /// <param name="index">The index of the <see cref="Position"/> in the script.</param>
        /// <param name="line">The line number of the <see cref="Position"/> in the script.</param>
        /// <param name="file">The file name/path of the script.</param>
        /// <param name="script">The script as text.</param>
        public Position(int index, int line, string file, string script)
        {
            Index = index;
            Line = line;
            File = file;
            Script = script;
        }

        /// <summary>
        /// Advances the <see cref="Position"/> and increments <see cref="Index"/> by 1. If <paramref name="currentChar"/> is a new-line character, <see cref="Line"/> is also incremented by 1.
        /// </summary>
        /// <param name="currentChar">The character associated with the <see cref="Position"/> before advancing.</param>
        public void Advance(char currentChar = '\0')
        {
            Index++;

            if (currentChar == '\n')
                Line++;
        }

        /// <summary>
        /// Creates a copy of the <see cref="Position"/> object.
        /// </summary>
        /// <returns>The copy.</returns>
        public Position Copy()
        {
            return new Position(Index, Line, File, Script);
        }
    }
    
    /// <summary>
    /// The identifying type of a <see cref="Token"/>.
    /// </summary>
    public enum TokenType : int
    {
        // Values
        Integer,
        FloatingPoint,
        String,
        Character,
        CharacterList,
        Identifier,

        // Keywords
        KeywordAnd,
        KeywordOr,
        KeywordInvert,
        KeywordIn,
        KeywordGlobal,
        KeywordItem,
        KeywordIf,
        KeywordElse,
        KeywordCount,
        KeywordFrom,
        KeywordTo,
        KeywordStep,
        KeywordAs,
        KeywordWhile,
        KeywordSkip,
        KeywordStop,
        KeywordFunction,
        KeywordSpecial,
        KeywordObject,
        KeywordWith,
        KeywordReturn,
        KeywordTry,
        KeywordError,
        KeywordDo,
        KeywordEnd,
        KeywordInclude,
        KeywordAll,

        // Qeywords
        QeywordF,
        QeywordL,
        QeywordE,
        QeywordC,
        QeywordT,
        QeywordN,
        QeywordW,
        QeywordFD,
        QeywordSD,
        QeywordOD,
        QeywordI,
        QeywordS,
        QeywordD,
        QeywordG,
        QeywordV,

        // One-character types
        Plus,
        HyphenMinus,
        Asterisk,
        Slash,
        PercentSign,
        Caret,
        Ampersand,
        VerticalBar,
        Backslash,
        Tilde,
        Colon,
        LeftParenthesis,
        RightParenthesis,
        LeftSquareBracket,
        RightSquareBracket,
        LeftCurlyBracket,
        RightCurlyBracket,
        EqualSign,
        ExclamationMark,
        LessThanSign,
        GreaterThanSign,
        Comma,
        Period,

        // Two-character types
        BitwiseLeftShift,
        BitwiseRightShift,
        AssignmentAddition,
        AssignmentSubtraction,
        AssignmentMultiplication,
        AssignmentDivision,
        AssignmentModulo,
        AssignmentPower,
        AssignmentBitwiseAnd,
        AssignmentBitwiseOr,
        AssignmentBitwiseXOr,
        AssignmentBitwiseLeftShift,
        AssignmentBitwiseRightShift,
        LessThanOrEqual,
        GreaterThanOrEqual,
        Arrow,

        // Special
        NewLine,
        EndOfFile
    }

    /// <summary>
    /// The smallest component in the script, grouped together in Nodes (TODO) to from structures of code (math expressions, variable assignment, if statements, etc).
    /// </summary>
    public class Token
    {
        /// <summary>
        /// The identifying <see cref="TokenType"/> of the <see cref="Token"/>.
        /// </summary>
        public readonly TokenType Type;

        /// <summary>
        /// The value of the <see cref="Token"/>; may be empty.
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// The starting <see cref="Position"/> of the <see cref="Token"/> in the script.
        /// </summary>
        public readonly Position StartPosition;

        /// <summary>
        /// The ending <see cref="Position"/> of the <see cref="Token"/> in the script.
        /// </summary>
        public readonly Position EndPosition;

        /// <summary>
        /// Creates a new <see cref="Token"/> object.
        /// </summary>
        /// <param name="type">The identifying <see cref="TokenType"/> of the <see cref="Token"/>.</param>
        /// <param name="value">The value of the <see cref="Token"/>; may be empty.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="Token"/> in the script.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="Token"/> in the script. If not given, copies <paramref name="startPosition"/> and advances it.</param>
        public Token(TokenType type, string value, Position startPosition, Position? endPosition = null)
        {
            Type = type;
            Value = value;

            StartPosition = startPosition;
            if (endPosition != null)
                EndPosition = endPosition;
            else
            {
                EndPosition = startPosition.Copy();
                EndPosition.Advance();
            }
        }

        /// <summary>
        /// Compares <see cref="Type"/> and <see cref="Value"/> with the given <paramref name="type"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="type">The value to compare to <see cref="Type"/>.</param>
        /// <param name="value">The value to compare to <see cref="Value"/>.</param>
        /// <returns><see langword="true"/> if both <paramref name="type"/> and <paramref name="value"/> match <see cref="Type"/> and <see cref="Value"/> respectively; otherwise <see langword="false"/>.</returns>
        public bool CheckToken(TokenType type, string value)
        {
            return Type == type && Value == value;
        }
    }
}