using System;

namespace EzrSquared;

/// <summary>
/// The identifying type of a <see cref="Token"/>.
/// </summary>
public enum TokenType : ushort
{
    /// <summary>
    /// An integer, part of the <see cref="TokenTypeGroup.Value"/> type-group.
    /// </summary>
    Integer,

    /// <summary>
    /// A floating-point number, part of the <see cref="TokenTypeGroup.Value"/> type-group.
    /// </summary>
    FloatingPoint,

    /// <summary>
    /// A text-string, string, or a sequence of characters, part of the <see cref="TokenTypeGroup.Value"/> type-group.
    /// </summary>
    String,

    /// <summary>
    /// A character, part of the <see cref="TokenTypeGroup.Value"/> type-group.
    /// </summary>
    Character,

    /// <summary>
    /// A character list, which is a mutable string, part of the <see cref="TokenTypeGroup.Value"/> type-group.
    /// </summary>
    CharacterList,

    /// <summary>
    /// The keyword "and", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordAnd,

    /// <summary>
    /// The keyword "or", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordOr,

    /// <summary>
    /// The keyword "invert", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordInvert,

    /// <summary>
    /// The keyword "not", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordNot,

    /// <summary>
    /// The keyword "in", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordIn,

    /// <summary>
    /// The keyword "global", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordGlobal,

    /// <summary>
    /// The keyword "private", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordPrivate,

    /// <summary>
    /// The keyword "constant", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordConstant,

    /// <summary>
    /// The keyword "readonly", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordReadonly,

    /// <summary>
    /// The keyword "item", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordItem,

    /// <summary>
    /// The keyword "if", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordIf,

    /// <summary>
    /// The keyword "else", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordElse,

    /// <summary>
    /// The keyword "count", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordCount,

    /// <summary>
    /// The keyword "for", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordFor,

    /// <summary>
    /// The keyword "each", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordEach,

    /// <summary>
    /// The keyword "from", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordFrom,

    /// <summary>
    /// The keyword "to", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordTo,

    /// <summary>
    /// The keyword "step", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordStep,

    /// <summary>
    /// The keyword "as", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordAs,

    /// <summary>
    /// The keyword "while", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordWhile,

    /// <summary>
    /// The keyword "skip", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordSkip,

    /// <summary>
    /// The keyword "stop", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordStop,

    /// <summary>
    /// The keyword "more", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordMore,

    /// <summary>
    /// The keyword "named", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordNamed,

    /// <summary>
    /// The keyword "function", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordFunction,

    /// <summary>
    /// The keyword "special", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    /// <remarks>
    /// This keyword was previously used in the "special functions" structure,<br/>
    /// which were like magic functions in Python, i.e. operator overloading functions.<br/>
    /// <br/>
    /// Since the structure has now been replaced with normal functions with dedicated names,<br/>
    /// this keyword is no longer in use and will be removed in later versions of ezr² if no<br/>
    /// new uses are found.
    /// </remarks>
    [Obsolete("The \"special\" keyword, used in the \"special functions\" structure which was used for operator overloading, has become obsolete. It will likely be removed in future versions of ezr² due to the structure being replaced with named functions.")]
    KeywordSpecial,

    /// <summary>
    /// The keyword "object", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordObject,

    /// <summary>
    /// The keyword "with", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordWith,

    /// <summary>
    /// The keyword "return", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordReturn,

    /// <summary>
    /// The keyword "last", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordLast,

    /// <summary>
    /// The keyword "try", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordTry,

    /// <summary>
    /// The keyword "catch", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordCatch,

    /// <summary>
    /// The keyword "define", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordDefine,

    /// <summary>
    /// The keyword "static", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordStatic,

    /// <summary>
    /// The keyword "do", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordDo,

    /// <summary>
    /// The keyword "end", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordEnd,

    /// <summary>
    /// The keyword "include", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordInclude,

    /// <summary>
    /// The keyword "all", case-insensitive, part of the <see cref="TokenTypeGroup.Keyword"/> type-group.
    /// </summary>
    KeywordAll,

    /// <summary>
    /// The QuickSyntax keyword "f", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordF,

    /// <summary>
    /// The QuickSyntax keyword "l", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordL,

    /// <summary>
    /// The QuickSyntax keyword "e", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordE,

    /// <summary>
    /// The QuickSyntax keyword "c", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordC,

    /// <summary>
    /// The QuickSyntax keyword "t", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordT,

    /// <summary>
    /// The QuickSyntax keyword "n", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordN,

    /// <summary>
    /// The QuickSyntax keyword "w", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordW,

    /// <summary>
    /// The QuickSyntax keyword "fd", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordFd,

    /// <summary>
    /// The QuickSyntax keyword "sd", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordSd,

    /// <summary>
    /// The QuickSyntax keyword "sb", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordSb,

    /// <summary>
    /// The QuickSyntax keyword "od", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordOd,

    /// <summary>
    /// The QuickSyntax keyword "i", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordI,

    /// <summary>
    /// The QuickSyntax keyword "s", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordS,

    /// <summary>
    /// The QuickSyntax keyword "d", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordD,

    /// <summary>
    /// The QuickSyntax keyword "g", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordG,

    /// <summary>
    /// The QuickSyntax keyword "p", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordP,

    /// <summary>
    /// The QuickSyntax keyword "v", case-insensitive, part of the <see cref="TokenTypeGroup.Qeyword"/> type-group.
    /// </summary>
    QeywordV,

    /// <summary>
    /// The '+' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    Plus,

    /// <summary>
    /// The '-' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    HyphenMinus,

    /// <summary>
    /// The '*' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    Asterisk,

    /// <summary>
    /// The '/' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    Slash,

    /// <summary>
    /// The '%' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    PercentSign,

    /// <summary>
    /// The '^' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    Caret,

    /// <summary>
    /// The '&amp;' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    Ampersand,

    /// <summary>
    /// The '|' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    VerticalBar,

    /// <summary>
    /// The '\' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    Backslash,

    /// <summary>
    /// The '~' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    Tilde,

    /// <summary>
    /// The '(' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    LeftParenthesis,

    /// <summary>
    /// The ')' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    RightParenthesis,

    /// <summary>
    /// The '[' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    LeftSquareBracket,

    /// <summary>
    /// The ']' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    RightSquareBracket,

    /// <summary>
    /// The '{' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    LeftCurlyBracket,

    /// <summary>
    /// The '}' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    RightCurlyBracket,

    /// <summary>
    /// The '=' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    EqualSign,

    /// <summary>
    /// The '!' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    ExclamationMark,

    /// <summary>
    /// The '&lt;' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    LessThanSign,

    /// <summary>
    /// The '&gt;' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    GreaterThanSign,

    /// <summary>
    /// The ',' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    Comma,

    /// <summary>
    /// The '.' symbol, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    Period,

    /// <summary>
    /// The '&lt;&lt;' symbols, used in the bitwise-left-shift operation, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    BitwiseLeftShift,

    /// <summary>
    /// The '&gt;&gt;' symbols, used in the bitwise-right-shift operation, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    BitwiseRightShift,

    /// <summary>
    /// The '&lt;=' symbols, used in comparison operations, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// The '&gt;=' symbols, used in comparison operations, part of the <see cref="TokenTypeGroup.Symbol"/> type-group.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// The ':' symbol, used in assignment operations, part of the <see cref="TokenTypeGroup.AssignmentSymbol"/> type-group.
    /// </summary>
    Colon,

    /// <summary>
    /// The ':+' symbols, used in assignment operations, part of the <see cref="TokenTypeGroup.AssignmentSymbol"/> type-group.
    /// </summary>
    AssignmentAddition,

    /// <summary>
    /// The ':-' symbols, used in assignment operations, part of the <see cref="TokenTypeGroup.AssignmentSymbol"/> type-group.
    /// </summary>
    AssignmentSubtraction,

    /// <summary>
    /// The ':*' symbols, used in assignment operations, part of the <see cref="TokenTypeGroup.AssignmentSymbol"/> type-group.
    /// </summary>
    AssignmentMultiplication,

    /// <summary>
    /// The ':/' symbols, used in assignment operations, part of the <see cref="TokenTypeGroup.AssignmentSymbol"/> type-group.
    /// </summary>
    AssignmentDivision,

    /// <summary>
    /// The ':%' symbols, used in assignment operations, part of the <see cref="TokenTypeGroup.AssignmentSymbol"/> type-group.
    /// </summary>
    AssignmentModulo,

    /// <summary>
    /// The ':^' symbols, used in assignment operations, part of the <see cref="TokenTypeGroup.AssignmentSymbol"/> type-group.
    /// </summary>
    AssignmentPower,

    /// <summary>
    /// The ':&amp;' symbols, used in assignment operations, part of the <see cref="TokenTypeGroup.AssignmentSymbol"/> type-group.
    /// </summary>
    AssignmentBitwiseAnd,

    /// <summary>
    /// The ':|' symbols, used in assignment operations, part of the <see cref="TokenTypeGroup.AssignmentSymbol"/> type-group.
    /// </summary>
    AssignmentBitwiseOr,

    /// <summary>
    /// The ':\' symbols, used in assignment operations, part of the <see cref="TokenTypeGroup.AssignmentSymbol"/> type-group.
    /// </summary>
    AssignmentBitwiseXOr,

    /// <summary>
    /// The ':&lt;&lt;' symbols, used in assignment operations, part of the <see cref="TokenTypeGroup.AssignmentSymbol"/> type-group.
    /// </summary>
    AssignmentBitwiseLeftShift,

    /// <summary>
    /// The ':&gt;&gt;' symbols, used in assignment operations, part of the <see cref="TokenTypeGroup.AssignmentSymbol"/> type-group.
    /// </summary>
    AssignmentBitwiseRightShift,

    /// <summary>
    /// Represents an identifier, a name that is assigned by the programmer for an element such as variable, class, function, etc.<br/>Part of the <see cref="TokenTypeGroup.Special"/> type-group.
    /// </summary>
    Identifier,

    /// <summary>
    /// Represents a new line, part of the <see cref="TokenTypeGroup.Special"/> type-group.
    /// </summary>
    NewLine,

    /// <summary>
    /// Represents the end of a script, part of the <see cref="TokenTypeGroup.Special"/> type-group.
    /// </summary>
    EndOfFile,

    /// <summary>
    /// Represents an invalid token, part of the <see cref="TokenTypeGroup.Special"/> type-group.
    /// </summary>
    /// <remarks>
    /// If this is passed onto any stage after the Parser, something has gone real wrong.
    /// </remarks>
    Invalid
}
