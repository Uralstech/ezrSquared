using EzrSquared.Syntax.Errors;
using System;
using System.Collections.Generic;
using System.Text;

namespace EzrSquared.Syntax;

/// <summary>
/// The ezr² Lexer or Tokenizer. The job of the Lexer is to convert the user input (code) into <see cref="Token"/> objects to be given as the input to the <see cref="Parser"/>.
/// </summary>
public class Lexer
{
    /// <summary>
    /// The file name/path of the script.
    /// </summary>
    private readonly string _file;

    /// <summary>
    /// The script to be tokenized.
    /// </summary>
    private readonly string _script;

    /// <summary>
    /// The <see cref="Position"/> of the current lexing iteration in the script.
    /// </summary>
    private Position _position;

    /// <summary>
    /// The character in the <see cref="Position"/> of the current lexing iteration in the script.
    /// </summary>
    private char _currentChar;

    /// <summary>
    /// The value for checking if the <see cref="Lexer"/> has reached the end of the script.
    /// </summary>
    private bool _reachedEnd;

    /// <summary>
    /// Creates a new <see cref="Lexer"/> object.
    /// </summary>
    /// <param name="file">The file name/path of the script.</param>
    /// <param name="script">The script to be tokenized.</param>
    public Lexer(string file, string script)
    {
        _file = file;
        _script = script;
        _position = new Position(-1, 1, _file, _script);
        _currentChar = char.MinValue;
        Advance();
    }

    /// <summary>
    /// Advances the current <see cref="Position"/> in the script.
    /// </summary>
    private void Advance()
    {
        if (_reachedEnd)
            return;

        _position = _position.Advance(_currentChar);
        if (_script.Length > _position.Index)
            _currentChar = _script[_position.Index];
        else
        {
            _reachedEnd = true;
            _currentChar = '\0';
        }
    }

    /// <summary>
    /// Reverses to the given index.
    /// </summary>
    /// <remarks>
    /// Warning: The <see cref="Position.Line"/> decrement is hard set to one if the character at <paramref name="index"/> is a newline, and zero otherwise.
    /// </remarks>
    /// <param name="index">The index to reverse to.</param>
    private void ReverseTo(int index)
    {
        _currentChar = _script[index];
        _position.ReverseTo(index, _currentChar == '\n' ? 1 : 0);
    }

    /// <summary>
    /// Creates a <see cref="List{T}"/> of <see cref="Token"/> objects from the given script.
    /// </summary>
    /// <param name="tokens">The created <see cref="List{T}"/> of <see cref="Token"/> objects.</param>
    /// <returns>Any <see cref="EzrSyntaxError"/> that occurred in the lexing; <see langword="null"/> if none occurred.</returns>
    public EzrSyntaxError? Tokenize(out List<Token> tokens)
    {
        tokens = [];
        while (!_reachedEnd)
        {
            switch (_currentChar)
            {
                case '\r':
                case '\t':
                case ' ':
                    Advance();
                    break;
                case ';':
                case '\n':
                    tokens.Add(CompileNewLines());
                    break;
                case '@':
                    SkipComment();
                    break;
                case '"':
                case '`':
                case '\'':
                    tokens.Add(CompileStringLike(out EzrSyntaxError? error));
                    if (error is not null)
                        return error;
                    break;
                case ':':
                    tokens.Add(CompileColon());
                    break;
                case '<':
                    Position lessThanTokenStartPosition = _position;
                    Advance();

                    switch (_currentChar)
                    {
                        case '=':
                            Advance();
                            tokens.Add(new Token(TokenType.LessThanOrEqual, TokenTypeGroup.Symbol, string.Empty, lessThanTokenStartPosition, _position));
                            break;

                        case '<':
                            Advance();
                            tokens.Add(new Token(TokenType.BitwiseLeftShift, TokenTypeGroup.Symbol, string.Empty, lessThanTokenStartPosition, _position));
                            break;

                        default:
                            tokens.Add(new Token(TokenType.LessThanSign, TokenTypeGroup.Symbol, string.Empty, lessThanTokenStartPosition, _position));
                            break;
                    }
                    break;
                case '>':
                    Position greaterThanTokenStartPosition = _position;
                    Advance();

                    switch (_currentChar)
                    {
                        case '=':
                            Advance();
                            tokens.Add(new Token(TokenType.GreaterThanOrEqual, TokenTypeGroup.Symbol, string.Empty, greaterThanTokenStartPosition, _position));
                            break;

                        case '>':
                            Advance();
                            tokens.Add(new Token(TokenType.BitwiseRightShift, TokenTypeGroup.Symbol, string.Empty, greaterThanTokenStartPosition, _position));
                            break;

                        default:
                            tokens.Add(new Token(TokenType.GreaterThanSign, TokenTypeGroup.Symbol, string.Empty, greaterThanTokenStartPosition, _position));
                            break;
                    }
                    break;
                case '-':
                    tokens.Add(new Token(TokenType.HyphenMinus, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '+':
                    tokens.Add(new Token(TokenType.Plus, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '*':
                    tokens.Add(new Token(TokenType.Asterisk, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '/':
                    tokens.Add(new Token(TokenType.Slash, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '%':
                    tokens.Add(new Token(TokenType.PercentSign, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '^':
                    tokens.Add(new Token(TokenType.Caret, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '=':
                    tokens.Add(new Token(TokenType.EqualSign, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '!':
                    tokens.Add(new Token(TokenType.ExclamationMark, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case ',':
                    tokens.Add(new Token(TokenType.Comma, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '.':
                    tokens.Add(new Token(TokenType.Period, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '(':
                    tokens.Add(new Token(TokenType.LeftParenthesis, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case ')':
                    tokens.Add(new Token(TokenType.RightParenthesis, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '[':
                    tokens.Add(new Token(TokenType.LeftSquareBracket, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case ']':
                    tokens.Add(new Token(TokenType.RightSquareBracket, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '{':
                    tokens.Add(new Token(TokenType.LeftCurlyBracket, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '}':
                    tokens.Add(new Token(TokenType.RightCurlyBracket, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '&':
                    tokens.Add(new Token(TokenType.Ampersand, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '|':
                    tokens.Add(new Token(TokenType.VerticalBar, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '\\':
                    tokens.Add(new Token(TokenType.Backslash, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '~':
                    tokens.Add(new Token(TokenType.Tilde, TokenTypeGroup.Symbol, string.Empty, _position));
                    Advance();
                    break;
                case '#':
                case '_':
                case char current when char.IsLetter(current):
                    tokens.Add(CompileIdentifier(out error));
                    if (error is not null)
                        return error;

                    break;
                case char current when char.IsDigit(current):
                    tokens.Add(CompileNumber());
                    break;

                default:
                    return new EzrSyntaxError(EzrSyntaxError.UnexpectedCharacter, _currentChar.ToString(), _position, _position.Advance());
            }
        }

        tokens.Add(new Token(TokenType.EndOfFile, TokenTypeGroup.Special, string.Empty, _position));
        return null;
    }

    /// <summary>
    /// Skips a comment in the ezr² code.
    /// </summary>
    private void SkipComment()
    {
        do
        {
            Advance();
        } while (!_reachedEnd && _currentChar != '\n');
    }

    /// <summary>
    /// Goes through digit and period characters and creates a single <see cref="Token"/> object with <see cref="TokenType"/> <see cref="TokenType.FloatingPoint"/> or <see cref="TokenType.Integer"/>.
    /// </summary>
    /// <returns>The <see cref="Token"/> object.</returns>
    private Token CompileNumber()
    {
        StringBuilder numberValue = new();
        Position numberTokenStartPosition = _position;
        bool hasPeriod = false;

        while (!_reachedEnd && (char.IsDigit(_currentChar) || _currentChar == '.'))
        {
            if (_currentChar == '.')
            {
                int peekIndex = _position.Index + 1;
                char next = peekIndex < _script.Length ? _script[peekIndex] : '\0';

                if (hasPeriod || !char.IsDigit(next))
                    break;

                hasPeriod = true;
            }

            numberValue.Append(_currentChar);
            Advance();
        }

        return new Token(
            hasPeriod ? TokenType.FloatingPoint : TokenType.Integer,
            TokenTypeGroup.Value,
            numberValue.ToString(),
            numberTokenStartPosition,
            _position
        );
    }

    /// <summary>
    /// Goes through newline characters and creates a single <see cref="Token"/> object with <see cref="TokenType"/> <see cref="TokenType.NewLine"/>.
    /// </summary>
    /// <returns>The <see cref="Token"/> object.</returns>
    private Token CompileNewLines()
    {
        int lastNewLineIndex = _position.Index;
        while (!_reachedEnd && (char.IsWhiteSpace(_currentChar) || _currentChar == ';'))
        {
            Advance();
            if (_currentChar is '\n' or ';' or '@')
            {
                if (_currentChar == '@')
                {
                    SkipComment();
                    if (_reachedEnd)
                        break;
                }

                lastNewLineIndex = _position.Index;
            }
        }

        ReverseTo(lastNewLineIndex);
        Position startPosition = _position;

        Advance();
        return new Token(TokenType.NewLine, TokenTypeGroup.Special, string.Empty, startPosition, _position);
    }

    /// <summary>
    /// Creates a <see cref="Token"/> of types <see cref="TokenType.String"/>, <see cref="TokenType.Character"/> or <see cref="TokenType.CharacterList"/>, depending on the enclosing character.
    /// </summary>
    /// <param name="error">Any <see cref="EzrSyntaxError"/> that occurred in creating the stringlike; <see langword="null"/> if none occurred.</param>
    /// <returns>The created <see cref="Token"/>.</returns>
    private Token CompileStringLike(out EzrSyntaxError? error)
    {
        char enclosingChar = _currentChar;

        StringBuilder toReturn = new();
        Position startPosition = _position;
        error = null;

        Advance();

        while (!_reachedEnd && _currentChar != enclosingChar)
        {
            if (_currentChar == '\\')
            {
                ProcessEscapeSequence(toReturn, ref error);
                if (error is not null)
                    return Token.Empty;
            }
            else
            {
                toReturn.Append(_currentChar);
                Advance();
            }
        }

        if (_currentChar != enclosingChar)
        {
            error = new EzrSyntaxError(EzrSyntaxError.InvalidGrammar, $"Expected '{enclosingChar}'!", _position, _position.Advance());
            return Token.Empty;
        }

        Advance();
        if (enclosingChar == '`' && ((toReturn.Length > 1 is bool tooLong && tooLong) || toReturn.Length == 0))
        {
            error = new EzrSyntaxError(EzrSyntaxError.InvalidGrammar,
                tooLong
                ? "Value too long to be a character!"
                : "A character cannot be empty!", startPosition, _position);

            return Token.Empty;
        }

        return new Token(
            enclosingChar switch
            {
                '`'     => TokenType.Character,
                '\''    => TokenType.CharacterList,
                _       => TokenType.String
            },
            TokenTypeGroup.Value,
            toReturn.ToString(),
            startPosition,
            _position);
    }

    /// <summary>
    /// Processes an escape sequence in a stringlike.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> to append the special character to.</param>
    /// <param name="error">Any <see cref="EzrSyntaxError"/> that occurred in the process; <see langword="null"/> if none occurred.</param>
    private void ProcessEscapeSequence(StringBuilder builder, ref EzrSyntaxError? error)
    {
        Position startPosition = _position;
        Advance();

        switch (_currentChar)
        {
            case 'u':
                builder.Append(ProcessUtf16Sequence(ref error));
                break;
            case 'U':
                builder.Append(ProcessUtf32Sequence(ref error));
                break;
            case 'n':
                builder.Append('\n');
                Advance();
                break;
            case 't':
                builder.Append('\t');
                Advance();
                break;
            case 'b':
                builder.Append('\b');
                Advance();
                break;
            case 'r':
                builder.Append('\r');
                Advance();
                break;
            case '0':
                builder.Append('\0');
                Advance();
                break;
            case 'f':
                builder.Append('\f');
                Advance();
                break;
            case 'v':
                builder.Append('\v');
                Advance();
                break;
            case '"':
            case '\'':
            case '`':
            case '\\':
                builder.Append(_currentChar);
                Advance();
                break;
            default:
                error = new EzrSyntaxError(EzrSyntaxError.UnexpectedCharacter, $"Unknown escape sequence '\\{_currentChar}'.", startPosition, _position);
                break;
        }
    }

    /// <summary>
    /// Processes a UTF-16 escaped sequence in a stringlike.
    /// </summary>
    /// <param name="error">Any <see cref="EzrSyntaxError"/> that occurred in the process; <see langword="null"/> if none occurred.</param>
    /// <returns>The UTF-16 character.</returns>
    private char[] ProcessUtf16Sequence(ref EzrSyntaxError? error)
    {
        Advance();
        
        int characterCount = 0;
        string hexValue = string.Empty;
        Position startPosition = _position;
        while (characterCount < 4)
        {
            if (_currentChar is (>= 'a' and <= 'f') or (>= 'A' and <= 'F') or (>= '0' and <= '9'))
            {
                characterCount++;
                hexValue += _currentChar;
                Advance();
            }
            else
            {
                error = new EzrSyntaxError(EzrSyntaxError.InvalidHexValue, "UTF-16 hexadecimal values must be 4 characters long and only contain digits and the letters A to F!", startPosition, _position.Advance());
                return [];
            }
        }

        return Encoding.Unicode.GetChars([Convert.ToByte(hexValue[2..4], 16), Convert.ToByte(hexValue[0..2], 16)]);
    }

    /// <summary>
    /// Processes a UTF-32 escaped sequence in a stringlike.
    /// </summary>
    /// <param name="error">Any <see cref="EzrSyntaxError"/> that occurred in the process; <see langword="null"/> if none occurred.</param>
    /// <returns>The UTF-32 character.</returns>
    private string ProcessUtf32Sequence(ref EzrSyntaxError? error)
    {
        Advance();

        int characterCount = 0;
        string hexValue = string.Empty;
        Position startPosition = _position;
        while (characterCount < 6)
        {
            if (_currentChar is (>= 'a' and <= 'f') or (>= 'A' and <= 'F') or (>= '0' and <= '9'))
            {
                characterCount++;
                hexValue += _currentChar;
                Advance();
            }
            else
            {
                error = new EzrSyntaxError(EzrSyntaxError.InvalidHexValue, "UTF-32 hexadecimal values must be 6 characters long and only contain digits and the letters A to F!", startPosition, _position.Advance());
                return string.Empty;
            }
        }

        int unicodePoint = Convert.ToInt32(hexValue, 16);
        if (unicodePoint > 0x10FFFF)
        {
            error = new EzrSyntaxError(EzrSyntaxError.InvalidHexValue, "UTF-32 hexadecimal values must be in range 000000 - 10FFFF!", startPosition, _position);
            return string.Empty;
        }

        return char.ConvertFromUtf32(unicodePoint);
    }

    /// <summary>
    /// Creates <see cref="TokenType.Colon"/> and assignment type (<see cref="TokenType.AssignmentAddition"/>, <see cref="TokenType.AssignmentMultiplication"/>, etc) <see cref="Token"/> objects.
    /// </summary>
    /// <returns>The created <see cref="Token"/>.</returns>
    private Token CompileColon()
    {
        Position startPosition = _position;
        Advance();

        switch (_currentChar)
        {
            case '+':
                Advance();
                return new Token(TokenType.AssignmentAddition, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position);
            case '-':
                Advance();
                return new Token(TokenType.AssignmentSubtraction, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position);
            case '*':
                Advance();
                return new Token(TokenType.AssignmentMultiplication, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position);
            case '/':
                Advance();
                return new Token(TokenType.AssignmentDivision, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position);
            case '%':
                Advance();
                return new Token(TokenType.AssignmentModulo, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position);
            case '^':
                Advance();
                return new Token(TokenType.AssignmentPower, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position);
            case '&':
                Advance();
                return new Token(TokenType.AssignmentBitwiseAnd, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position);
            case '|':
                Advance();
                return new Token(TokenType.AssignmentBitwiseOr, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position);
            case '\\':
                Advance();
                return new Token(TokenType.AssignmentBitwiseXOr, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position);
            case '<':
                Advance();
                return new Token(TokenType.AssignmentBitwiseLeftShift, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position);
            case '>':
                Advance();
                return new Token(TokenType.AssignmentBitwiseRightShift, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position);
            default:
                return new Token(TokenType.Colon, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position);
        }
    }

    /// <summary>
    /// Creates <see cref="TokenType.Identifier"/>, keyword type (<see cref="TokenType.KeywordItem"/>, <see cref="TokenType.KeywordFunction"/>, etc) and qeyword type (<see cref="TokenType.QeywordC"/>, <see cref="TokenType.QeywordFd"/>, etc) <see cref="Token"/> objects.
    /// </summary>
    /// <param name="error">Any <see cref="EzrSyntaxError"/> that occurred in the process; <see langword="null"/> if none occurred.</param>
    /// <returns>The created <see cref="Token"/>.</returns>
    private Token CompileIdentifier(out EzrSyntaxError? error)
    {
        Position startPosition = _position;
        error = null;

        bool isEscapedIdentifier = _currentChar == '#';
        if (isEscapedIdentifier)
        {
            Advance();
            if (!char.IsLetterOrDigit(_currentChar) && _currentChar != '_')
            {
                error = new EzrSyntaxError(EzrSyntaxError.UnexpectedCharacter, "The hash symbol should only be used before identifiers to escape keyword detection.", startPosition, _position);
                return Token.Empty;
            }
        }

        StringBuilder idValue = new();
        while (!_reachedEnd && (char.IsLetterOrDigit(_currentChar) || _currentChar == '_'))
        {
            idValue.Append(_currentChar);
            Advance();
        }

        string original = idValue.ToString();
        return isEscapedIdentifier
            ? new Token(TokenType.Identifier, TokenTypeGroup.Special, original, startPosition, _position)
            : original.ToLower() switch
            {
            "private"   => new Token(TokenType.KeywordPrivate, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "constant"  => new Token(TokenType.KeywordConstant, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "readonly"  => new Token(TokenType.KeywordReadonly, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "item"      => new Token(TokenType.KeywordItem, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "and"       => new Token(TokenType.KeywordAnd, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "or"        => new Token(TokenType.KeywordOr, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "invert"    => new Token(TokenType.KeywordInvert, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "if"        => new Token(TokenType.KeywordIf, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "else"      => new Token(TokenType.KeywordElse, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "do"        => new Token(TokenType.KeywordDo, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "count"     => new Token(TokenType.KeywordCount, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "for"       => new Token(TokenType.KeywordFor, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "each"      => new Token(TokenType.KeywordEach, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "from"      => new Token(TokenType.KeywordFrom, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "as"        => new Token(TokenType.KeywordAs, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "to"        => new Token(TokenType.KeywordTo, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "step"      => new Token(TokenType.KeywordStep, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "while"     => new Token(TokenType.KeywordWhile, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "function"  => new Token(TokenType.KeywordFunction, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),

#pragma warning disable CS0618
            "special"   => new Token(TokenType.KeywordSpecial, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
#pragma warning restore CS0618

            "with"      => new Token(TokenType.KeywordWith, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "more"      => new Token(TokenType.KeywordMore, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "named"     => new Token(TokenType.KeywordNamed, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "end"       => new Token(TokenType.KeywordEnd, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "return"    => new Token(TokenType.KeywordReturn, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "last"      => new Token(TokenType.KeywordLast, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "skip"      => new Token(TokenType.KeywordSkip, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "stop"      => new Token(TokenType.KeywordStop, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "try"       => new Token(TokenType.KeywordTry, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "catch"     => new Token(TokenType.KeywordCatch, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "not"       => new Token(TokenType.KeywordNot, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "in"        => new Token(TokenType.KeywordIn, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "object"    => new Token(TokenType.KeywordObject, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "global"    => new Token(TokenType.KeywordGlobal, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "include"   => new Token(TokenType.KeywordInclude, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "all"       => new Token(TokenType.KeywordAll, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "static"    => new Token(TokenType.KeywordStatic, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "define"    => new Token(TokenType.KeywordDefine, TokenTypeGroup.Keyword, string.Empty, startPosition, _position),
            "f"         => new Token(TokenType.QeywordF, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "l"         => new Token(TokenType.QeywordL, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "e"         => new Token(TokenType.QeywordE, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "c"         => new Token(TokenType.QeywordC, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "p"         => new Token(TokenType.QeywordP, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "t"         => new Token(TokenType.QeywordT, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "n"         => new Token(TokenType.QeywordN, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "w"         => new Token(TokenType.QeywordW, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "fd"        => new Token(TokenType.QeywordFd, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "sd"        => new Token(TokenType.QeywordSd, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "sb"        => new Token(TokenType.QeywordSb, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "od"        => new Token(TokenType.QeywordOd, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "i"         => new Token(TokenType.QeywordI, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "s"         => new Token(TokenType.QeywordS, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "d"         => new Token(TokenType.QeywordD, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "g"         => new Token(TokenType.QeywordG, TokenTypeGroup.Qeyword, original, startPosition, _position),
            "v"         => new Token(TokenType.QeywordV, TokenTypeGroup.Qeyword, original, startPosition, _position),
            _           => new Token(TokenType.Identifier, TokenTypeGroup.Special, original, startPosition, _position),
            };
    }
}