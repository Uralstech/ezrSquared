namespace EzrSquared.EzrCommon
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
    /// The type group of a <see cref="Token"/>.
    /// </summary>
    public enum TokenTypeGroup : ushort
    {
        Value,
        Keyword,
        Qeyword,
        AssignmentSymbol,
        Symbol,
        Special
    }

    /// <summary>
    /// The identifying type of a <see cref="Token"/>.
    /// </summary>
    public enum TokenType : ushort
    {
        // Values
        Integer,
        FloatingPoint,
        String,
        Character,
        CharacterList,

        // Keywords
        KeywordAnd,
        KeywordOr,
        KeywordInvert,
        KeywordNot,
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

        // Symbols
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
        BitwiseLeftShift,
        BitwiseRightShift,
        LessThanOrEqual,
        GreaterThanOrEqual,
        Arrow,

        // Assignment symbols
        Colon,
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

        // Special
        Identifier,
        NewLine,
        EndOfFile,
        Invalid
    }

    /// <summary>
    /// The smallest component in the script identified by the <see cref="TokenType"/>, grouped together into <see cref="EzrNodes.Node"/> objects to from source code constructs.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// The identifying <see cref="TokenType"/> of the <see cref="Token"/>.
        /// </summary>
        public readonly TokenType Type;

        /// <summary>
        /// The <see cref="TokenTypeGroup"/> of the <see cref="Token"/>.
        /// </summary>
        public readonly TokenTypeGroup TypeGroup;

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

        public static readonly Token Dummy = new Token(TokenType.Invalid, TokenTypeGroup.Special, string.Empty, new Position(int.MinValue, int.MinValue, string.Empty, string.Empty));

        /// <summary>
        /// Creates a new <see cref="Token"/> object.
        /// </summary>
        /// <param name="type">The identifying <see cref="TokenType"/> of the <see cref="Token"/>.</param>
        /// <param name="typeGroup">The <see cref="TokenTypeGroup"/> of the <see cref="Token"/>.</param>
        /// <param name="value">The value of the <see cref="Token"/>; may be empty.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="Token"/> in the script.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="Token"/> in the script. If not given, copies <paramref name="startPosition"/> and advances it.</param>
        public Token(TokenType type, TokenTypeGroup typeGroup, string value, Position startPosition, Position? endPosition = null)
        {
            Type = type;
            TypeGroup = typeGroup;
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

        public override string ToString()
        {
            return $"Token({Type}, {TypeGroup}, \"{Value}\")";
        }
    }
}