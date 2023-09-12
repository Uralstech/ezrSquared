using System;
using System.Text;
using System.Collections.Generic;

namespace EzrSquared.EzrLexer
{
    using EzrCommon;
    using EzrErrors;

    /// <summary>
    /// The ezrSquared Lexer or Tokenizer. The job of the Lexer is to convert the user input (code) into <see cref="Token"/> objects to be given as the input to the <see cref="EzrParser.Parser"/>.
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
        private bool _reachedEndFlag;

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
            _position.Advance(_currentChar);

            if (_position.Index < _script.Length)
                _currentChar = _script[_position.Index];
            else
                _reachedEndFlag = true;
        }

        /// <summary>
        /// Peeks one character in front of the current <see cref="Position"/> in the script.
        /// </summary>
        /// <returns>The character in front of the current <see cref="Position"/>; <see langword="\0"/> if the next character is the end of the script.</returns>
        private char Peek()
        {
            int peekIndex = _position.Index + 1;
            if (peekIndex < _script.Length)
                return _script[peekIndex];
            else
                return '\0';
        }

        /// <summary>
        /// Creates a <see cref="List{T}"/> of <see cref="Token"/> objects from the given script.
        /// </summary>
        /// <param name="tokens">The created <see cref="List{T}"/> of <see cref="Token"/> objects.</param>
        /// <param name="error">Any <see cref="Error"/> that occurred in the lexing; <see langword="null"/> if none occurred.</param>
        /// <returns><see langword="true"/> if the lexing succeeded without any errors; otherwise <see langword="false"/>.</returns>
        public bool Tokenize(out List<Token> tokens, out Error? error)
        {
            tokens = new List<Token>();
            error = null;
        
            while (!_reachedEndFlag)
            {
                switch (_currentChar)
                {
                    case '\t':
                    case ' ':
                        Advance();
                        break;
                    case ';':
                    case '\n':
                        tokens.Add(new Token(TokenType.NewLine, TokenTypeGroup.Special, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '@':
                        Advance();
                        while (!_reachedEndFlag && _currentChar != '\n')
                            Advance();
                        break;
                    case '"':
                        tokens.Add(CompileStringLike(_currentChar, TokenType.String, out error));
                        if (error != null)
                            return false;
                        break;
                    case '`':
                        tokens.Add(CompileStringLike(_currentChar, TokenType.Character, out error));
                        if (error != null)
                            return false;
                        break;
                    case '\'':
                        tokens.Add(CompileStringLike(_currentChar, TokenType.CharacterList, out error));
                        if (error != null)
                            return false;
                        break;
                    case ':':
                        tokens.Add(CompileColon());
                        break;
                    case '<':
                        Position lessThanTokenStartPosition = _position.Copy();
                        Advance();

                        if (_currentChar == '=')
                        {
                            Advance();
                            tokens.Add(new Token(TokenType.LessThanOrEqual, TokenTypeGroup.Symbol, string.Empty, lessThanTokenStartPosition, _position.Copy()));
                        }
                        else if (_currentChar == '<')
                        {
                            Advance();
                            tokens.Add(new Token(TokenType.BitwiseLeftShift, TokenTypeGroup.Symbol, string.Empty, lessThanTokenStartPosition, _position.Copy()));
                        }
                        else
                            tokens.Add(new Token(TokenType.LessThanSign, TokenTypeGroup.Symbol, string.Empty, lessThanTokenStartPosition, _position.Copy()));
                        break;
                    case '>':
                        Position greaterThanTokenStartPosition = _position.Copy();
                        Advance();

                        if (_currentChar == '=')
                        {
                            Advance();
                            tokens.Add(new Token(TokenType.GreaterThanOrEqual, TokenTypeGroup.Symbol, string.Empty, greaterThanTokenStartPosition, _position.Copy()));
                        }
                        else if (_currentChar == '>')
                        {
                            Advance();
                            tokens.Add(new Token(TokenType.BitwiseRightShift, TokenTypeGroup.Symbol, string.Empty, greaterThanTokenStartPosition, _position.Copy()));
                        }
                        else
                            tokens.Add(new Token(TokenType.GreaterThanSign, TokenTypeGroup.Symbol, string.Empty, greaterThanTokenStartPosition, _position.Copy()));
                        break;
                    case '-':
                        Position minusTokenStartPosition = _position.Copy();
                        Advance();

                        if (_currentChar == '>')
                        {
                            Advance();
                            tokens.Add(new Token(TokenType.Arrow, TokenTypeGroup.Symbol, string.Empty, minusTokenStartPosition, _position.Copy()));
                        }
                        else
                            tokens.Add(new Token(TokenType.HyphenMinus, TokenTypeGroup.Symbol, string.Empty, minusTokenStartPosition, _position.Copy()));
                        break;
                    case '+':
                        tokens.Add(new Token(TokenType.Plus, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '*':
                        tokens.Add(new Token(TokenType.Asterisk, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '/':
                        tokens.Add(new Token(TokenType.Slash, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '%':
                        tokens.Add(new Token(TokenType.PercentSign, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '^':
                        tokens.Add(new Token(TokenType.Caret, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '=':
                        tokens.Add(new Token(TokenType.EqualSign, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '!':
                        tokens.Add(new Token(TokenType.ExclamationMark, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case ',':
                        tokens.Add(new Token(TokenType.Comma, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '.':
                        tokens.Add(new Token(TokenType.Period, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '(':
                        tokens.Add(new Token(TokenType.LeftParenthesis, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case ')':
                        tokens.Add(new Token(TokenType.RightParenthesis, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '[':
                        tokens.Add(new Token(TokenType.LeftSquareBracket, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case ']':
                        tokens.Add(new Token(TokenType.RightSquareBracket, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '{':
                        tokens.Add(new Token(TokenType.LeftCurlyBracket, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '}':
                        tokens.Add(new Token(TokenType.RightCurlyBracket, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '&':
                        tokens.Add(new Token(TokenType.Ampersand, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '|':
                        tokens.Add(new Token(TokenType.VerticalBar, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '\\':
                        tokens.Add(new Token(TokenType.Backslash, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    case '~':
                        tokens.Add(new Token(TokenType.Tilde, TokenTypeGroup.Symbol, string.Empty, _position.Copy()));
                        Advance();
                        break;
                    default:
                        if (char.IsLetter(_currentChar) || _currentChar == '_')
                        {
                            tokens.Add(CompileIdentifier());
                            break;
                        }
                        else if (char.IsDigit(_currentChar))
                        {
                            StringBuilder numberValue = new StringBuilder();
                            Position numberTokenStartPosition = _position.Copy();
                            bool hasPeriod = false;

                            while(!_reachedEndFlag && (char.IsDigit(_currentChar) || _currentChar == '.'))
                            {
                                if (_currentChar == '.')
                                {
                                    if (hasPeriod || !char.IsDigit(Peek()))
                                        break;
                                    hasPeriod = true;
                                }

                                numberValue.Append(_currentChar);
                                Advance();
                            }

                            if (hasPeriod)
                                tokens.Add(new Token(TokenType.FloatingPoint, TokenTypeGroup.Value, numberValue.ToString(), numberTokenStartPosition, _position.Copy()));
                            else
                                tokens.Add(new Token(TokenType.Integer, TokenTypeGroup.Value, numberValue.ToString(), numberTokenStartPosition, _position.Copy()));
                            break;
                        }
                        else
                        {
                            Position errorStartPosition = _position.Copy();
                            char unknownCharacter = _currentChar;
                            Advance();

                            error = new UnexpectedCharacterError(unknownCharacter, errorStartPosition, _position);
                            return false;
                        }
                }
            }

            tokens.Add(new Token(TokenType.EndOfFile, TokenTypeGroup.Special, string.Empty, _position.Copy()));
            return true;
        }

        /// <summary>
        /// Creates a stringlike type (<see cref="TokenType.String"/>, <see cref="TokenType.Character"/>, <see cref="TokenType.CharacterList"/>) <see cref="Token"/> object.
        /// </summary>
        /// <param name="enclosingChar">The value to enclose the stringlike.</param>
        /// <param name="type">The identifying <see cref="TokenType"/> of the <see cref="Token"/>.</param>
        /// <param name="error">Any <see cref="Error"/> that occurred in creating the stringlike; <see langword="null"/> if none occurred.</param>
        /// <returns>The created <see cref="Token"/>.</returns>
        private Token CompileStringLike(char enclosingChar, TokenType type, out Error? error)
        {
            error = null;
        
            StringBuilder toReturn = new StringBuilder();
            Position startPosition = _position.Copy();
            bool escapeChar = false;
            bool countingUTF16 = false;
            bool countingUTF32 = false;
            int UTFCount = 0;
            string hexValue = string.Empty;
            Position hexStartPosition = startPosition;
            Advance();

            while (!_reachedEndFlag && (_currentChar != enclosingChar || escapeChar || countingUTF16 || countingUTF32))
            {
                if (countingUTF16)
                {
                    if ((_currentChar >= 'a' && _currentChar <= 'f') || (_currentChar >= 'A' && _currentChar <= 'F') || (_currentChar >= '0' && _currentChar <= '9'))
                    {
                        UTFCount++;
                        hexValue += _currentChar;
                    }
                    else
                    {
                        Position endPosition = _position.Copy();
                        endPosition.Advance();

                        error = new InvalidHexValueError("UTF-16 hexadecimal values must be 4 characters long and only contain digits and/or the letters A to F", hexStartPosition, endPosition);
                        break;
                    }

                    if (UTFCount >= 4)
                    {
                        toReturn.Append(Encoding.Unicode.GetChars(new byte[2] { Convert.ToByte(hexValue[2..4], 16), Convert.ToByte(hexValue[0..2], 16) }));
                        countingUTF16 = false;
                    }
                }
                else if (countingUTF32)
                {
                    if ((_currentChar >= 'a' && _currentChar <= 'f') || (_currentChar >= 'A' && _currentChar <= 'F') || (_currentChar >= '0' && _currentChar <= '9'))
                    {
                        UTFCount++;
                        hexValue += _currentChar;
                    }
                    else
                    {
                        Position endPosition = _position.Copy();
                        endPosition.Advance();

                        error = new InvalidHexValueError("UTF-32 hexadecimal values must be 6 characters long and only contain digits and/or the letters A to F", hexStartPosition, endPosition);
                        break;
                    }

                    if (UTFCount >= 6)
                    {
                        if (Convert.ToInt32(hexValue, 16) > 1114111)
                        {
                            Position endPosition = _position.Copy();
                            endPosition.Advance();

                            error = new InvalidHexValueError("UTF-32 hexadecimal values must be in range 000000 - 10FFFF", hexStartPosition, endPosition);
                            break;
                        }
                        else
                            toReturn.Append(Encoding.UTF32.GetChars(new byte[4] { Convert.ToByte(hexValue[4..6], 16), Convert.ToByte(hexValue[2..4], 16), Convert.ToByte(hexValue[0..2], 16), 0 }));
                        countingUTF32 = false;
                    }
                }
                else if (escapeChar)
                {
                    switch (_currentChar)
                    {
                        case 'u':
                            hexValue = string.Empty;
                            countingUTF16 = true;

                            hexStartPosition = _position.Copy();
                            hexStartPosition.Advance();
                            break;
                        case 'U':
                            hexValue = string.Empty;
                            countingUTF32 = true;

                            hexStartPosition = _position.Copy();
                            hexStartPosition.Advance();
                            break;
                        case 'n':
                            toReturn.Append('\n');
                            break;
                        case 't':
                            toReturn.Append('\t');
                            break;
                        case 'b':
                            toReturn.Append('\b');
                            break;
                        case 'r':
                            toReturn.Append('\r');
                            break;
                        case '0':
                            toReturn.Append('\0');
                            break;
                        case 'f':
                            toReturn.Append('\f');
                            break;
                        case 'v':
                            toReturn.Append('\v');
                            break;
                        default:
                            toReturn.Append(_currentChar);
                            break;
                    }

                    escapeChar = false;
                }
                else if (_currentChar == '\\')
                    escapeChar = true;
                else
                    toReturn.Append(_currentChar);

                Advance();
            }

            if (_reachedEndFlag || _currentChar != enclosingChar)
                error = new InvalidGrammarError($"Expected '{enclosingChar}'", _position.Copy(), _position);
            
            Advance();
            return new Token(type, TokenTypeGroup.Value, toReturn.ToString(), startPosition, _position.Copy());
        }

        /// <summary>
        /// Creates <see cref="TokenType.Colon"/> and assignment type (<see cref="TokenType.AssignmentAddition"/>, <see cref="TokenType.AssignmentMultiplication"/>, etc) <see cref="Token"/> objects.
        /// </summary>
        /// <returns>The created <see cref="Token"/>.</returns>
        private Token CompileColon()
        {
            Position startPosition = _position.Copy();
            Advance();

            switch (_currentChar)
            {
                case '+':
                    Advance();
                    return new Token(TokenType.AssignmentAddition, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position.Copy());
                case '-':
                    Advance();
                    return new Token(TokenType.AssignmentSubtraction, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position.Copy());
                case '*':
                    Advance();
                    return new Token(TokenType.AssignmentMultiplication, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position.Copy());
                case '/':
                    Advance();
                    return new Token(TokenType.AssignmentDivision, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position.Copy());
                case '%':
                    Advance();
                    return new Token(TokenType.AssignmentModulo, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position.Copy());
                case '^':
                    Advance();
                    return new Token(TokenType.AssignmentPower, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position.Copy());
                case '&':
                    Advance();
                    return new Token(TokenType.AssignmentBitwiseAnd, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position.Copy());
                case '|':
                    Advance();
                    return new Token(TokenType.AssignmentBitwiseOr, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position.Copy());
                case '\\':
                    Advance();
                    return new Token(TokenType.AssignmentBitwiseXOr, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position.Copy());
                case '<':
                    Advance();
                    return new Token(TokenType.AssignmentBitwiseLeftShift, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position.Copy());
                case '>':
                    Advance();
                    return new Token(TokenType.AssignmentBitwiseRightShift, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position.Copy());
                default:
                    return new Token(TokenType.Colon, TokenTypeGroup.AssignmentSymbol, string.Empty, startPosition, _position.Copy());
            }
        }

        /// <summary>
        /// Creates <see cref="TokenType.Identifier"/>, keyword type (<see cref="TokenType.KeywordItem"/>, <see cref="TokenType.KeywordFunction"/>, etc) and qeyword type (<see cref="TokenType.QeywordC"/>, <see cref="TokenType.QeywordFD"/>, etc) <see cref="Token"/> objects.
        /// </summary>
        /// <returns>The created <see cref="Token"/>.</returns>
        private Token CompileIdentifier()
        {
            Position startPosition = _position.Copy();
            StringBuilder idValue = new StringBuilder();

            while (!_reachedEndFlag && (char.IsLetterOrDigit(_currentChar) || _currentChar == '_'))
            {
                idValue.Append(_currentChar);
                Advance();
            }

            string value = idValue.ToString();
            switch (value)
            {
                case "item":
                    return new Token(TokenType.KeywordItem, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "and":
                    return new Token(TokenType.KeywordAnd, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "or":
                    return new Token(TokenType.KeywordOr, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "invert":
                    return new Token(TokenType.KeywordInvert, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "if":
                    return new Token(TokenType.KeywordIf, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "else":
                    return new Token(TokenType.KeywordElse, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "do":
                    return new Token(TokenType.KeywordDo, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "count":
                    return new Token(TokenType.KeywordCount, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "from":
                    return new Token(TokenType.KeywordFrom, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "as":
                    return new Token(TokenType.KeywordAs, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "to":
                    return new Token(TokenType.KeywordTo, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "step":
                    return new Token(TokenType.KeywordStep, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "while":
                    return new Token(TokenType.KeywordWhile, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "function":
                    return new Token(TokenType.KeywordFunction, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "special":
                    return new Token(TokenType.KeywordSpecial, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "with":
                    return new Token(TokenType.KeywordWith, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "end":
                    return new Token(TokenType.KeywordEnd, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "return":
                    return new Token(TokenType.KeywordReturn, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "skip":
                    return new Token(TokenType.KeywordSkip, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "stop":
                    return new Token(TokenType.KeywordStop, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "try":
                    return new Token(TokenType.KeywordTry, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "error":
                    return new Token(TokenType.KeywordError, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "not":
                    return new Token(TokenType.KeywordNot, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "in":
                    return new Token(TokenType.KeywordIn, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "object":
                    return new Token(TokenType.KeywordObject, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "global":
                    return new Token(TokenType.KeywordGlobal, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "include":
                    return new Token(TokenType.KeywordInclude, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "all":
                    return new Token(TokenType.KeywordAll, TokenTypeGroup.Keyword, string.Empty, startPosition, _position.Copy());
                case "f":
                    return new Token(TokenType.QeywordF, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "l":
                    return new Token(TokenType.QeywordL, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "e":
                    return new Token(TokenType.QeywordE, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "c":
                    return new Token(TokenType.QeywordC, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "t":
                    return new Token(TokenType.QeywordT, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "n":
                    return new Token(TokenType.QeywordN, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "w":
                    return new Token(TokenType.QeywordW, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "fd":
                    return new Token(TokenType.QeywordFD, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "sd":
                    return new Token(TokenType.QeywordSD, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "od":
                    return new Token(TokenType.QeywordOD, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "i":
                    return new Token(TokenType.QeywordI, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "s":
                    return new Token(TokenType.QeywordS, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "d":
                    return new Token(TokenType.QeywordD, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "g":
                    return new Token(TokenType.QeywordG, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                case "v":
                    return new Token(TokenType.QeywordV, TokenTypeGroup.Qeyword, value, startPosition, _position.Copy());
                default:
                    return new Token(TokenType.Identifier, TokenTypeGroup.Special, value, startPosition, _position.Copy());
            }
        }
    }
}