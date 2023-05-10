using System;
using System.Text;
using System.Collections.Generic;

namespace EzrSquared.EzrLexer
{
    using EzrGeneral;
    using EzrErrors;

    /// <summary>
    /// The ezrSquared Lexer or Tokenizer. The job of the Lexer is to convert the user input (code) into <see cref="Token"/> objects to be given as the input to the Parser (TODO).
    /// </summary>
    public class Lexer
    {
        /// <summary>
        /// The file name/path of the script.
        /// </summary>
        private readonly string _file;

        /// <summary>
        /// The script as text.
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
        /// <param name="script">The script as text.</param>
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
            {
                _currentChar = '\0';
                _reachedEndFlag = true;
            }
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
        /// <param name="error">Any <see cref="Error"/> that occured in the lexing; <see langword="null"/> if none occured.</param>
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
                        Advance();
                        tokens.Add(new Token(TokenType.NewLine, string.Empty, _position.Copy()));
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
                            tokens.Add(new Token(TokenType.LessThanOrEqual, string.Empty, lessThanTokenStartPosition, _position.Copy()));
                        }
                        else if (_currentChar == '<')
                        {
                            Advance();
                            tokens.Add(new Token(TokenType.BitwiseLeftShift, string.Empty, lessThanTokenStartPosition, _position.Copy()));
                        }
                        else
                            tokens.Add(new Token(TokenType.LessThanSign, string.Empty, lessThanTokenStartPosition, _position.Copy()));
                        break;
                    case '>':
                        Position greaterThanTokenStartPosition = _position.Copy();
                        Advance();

                        if (_currentChar == '=')
                        {
                            Advance();
                            tokens.Add(new Token(TokenType.GreaterThanOrEqual, string.Empty, greaterThanTokenStartPosition, _position.Copy()));
                        }
                        else if (_currentChar == '>')
                        {
                            Advance();
                            tokens.Add(new Token(TokenType.BitwiseRightShift, string.Empty, greaterThanTokenStartPosition, _position.Copy()));
                        }
                        else
                            tokens.Add(new Token(TokenType.GreaterThanSign, string.Empty, greaterThanTokenStartPosition, _position.Copy()));
                        break;
                    case '-':
                        Position minusTokenStartPosition = _position.Copy();
                        Advance();

                        if (_currentChar == '>')
                        {
                            Advance();
                            tokens.Add(new Token(TokenType.Arrow, string.Empty, minusTokenStartPosition, _position.Copy()));
                        }
                        else
                            tokens.Add(new Token(TokenType.HyphenMinus, string.Empty, minusTokenStartPosition, _position.Copy()));
                        break;
                    case '+':
                        Advance();
                        tokens.Add(new Token(TokenType.Plus, string.Empty, _position.Copy()));
                        break;
                    case '*':
                        Advance();
                        tokens.Add(new Token(TokenType.Asterisk, string.Empty, _position.Copy()));
                        break;
                    case '/':
                        Advance();
                        tokens.Add(new Token(TokenType.Slash, string.Empty, _position.Copy()));
                        break;
                    case '%':
                        Advance();
                        tokens.Add(new Token(TokenType.PercentSign, string.Empty, _position.Copy()));
                        break;
                    case '^':
                        Advance();
                        tokens.Add(new Token(TokenType.Caret, string.Empty, _position.Copy()));
                        break;
                    case '=':
                        Advance();
                        tokens.Add(new Token(TokenType.EqualSign, string.Empty, _position.Copy()));
                        break;
                    case '!':
                        Advance();
                        tokens.Add(new Token(TokenType.ExclamationMark, string.Empty, _position.Copy()));
                        break;
                    case ',':
                        Advance();
                        tokens.Add(new Token(TokenType.Comma, string.Empty, _position.Copy()));
                        break;
                    case '.':
                        Advance();
                        tokens.Add(new Token(TokenType.Period, string.Empty, _position.Copy()));
                        break;
                    case '(':
                        Advance();
                        tokens.Add(new Token(TokenType.LeftParenthesis, string.Empty, _position.Copy()));
                        break;
                    case ')':
                        Advance();
                        tokens.Add(new Token(TokenType.RightParenthesis, string.Empty, _position.Copy()));
                        break;
                    case '[':
                        Advance();
                        tokens.Add(new Token(TokenType.LeftSquareBracket, string.Empty, _position.Copy()));
                        break;
                    case ']':
                        Advance();
                        tokens.Add(new Token(TokenType.RightSquareBracket, string.Empty, _position.Copy()));
                        break;
                    case '{':
                        Advance();
                        tokens.Add(new Token(TokenType.LeftCurlyBracket, string.Empty, _position.Copy()));
                        break;
                    case '}':
                        Advance();
                        tokens.Add(new Token(TokenType.RightCurlyBracket, string.Empty, _position.Copy()));
                        break;
                    case '&':
                        Advance();
                        tokens.Add(new Token(TokenType.Ampersand, string.Empty, _position.Copy()));
                        break;
                    case '|':
                        Advance();
                        tokens.Add(new Token(TokenType.VerticalBar, string.Empty, _position.Copy()));
                        break;
                    case '\\':
                        Advance();
                        tokens.Add(new Token(TokenType.Backslash, string.Empty, _position.Copy()));
                        break;
                    case '~':
                        Advance();
                        tokens.Add(new Token(TokenType.Tilde, string.Empty, _position.Copy()));
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
                                tokens.Add(new Token(TokenType.FloatingPoint, numberValue.ToString(), numberTokenStartPosition, _position.Copy()));
                            else
                                tokens.Add(new Token(TokenType.Integer, numberValue.ToString(), numberTokenStartPosition, _position.Copy()));
                            break;
                        }
                        else
                        {
                            Position errorStartPosition = _position.Copy();
                            char unknownCharacter = _currentChar;
                            Advance();

                            error = new UnknownCharacterError($"'{unknownCharacter}'", errorStartPosition, _position);
                            return false;
                        }
                }
            }

            tokens.Add(new Token(TokenType.EndOfFile, string.Empty, _position.Copy()));
            return true;
        }

        /// <summary>
        /// Creates a stringlike type (<see cref="TokenType.String"/>, <see cref="TokenType.Character"/>, <see cref="TokenType.CharacterList"/>) <see cref="Token"/> object.
        /// </summary>
        /// <param name="enclosingChar">The value to enclose the stringlike.</param>
        /// <param name="type">The identifying <see cref="TokenType"/> of the <see cref="Token"/>.</param>
        /// <param name="error">Any <see cref="Error"/> that occured in creating the stringlike; <see langword="null"/> if none occured.</param>
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
            return new Token(type, toReturn.ToString(), startPosition, _position.Copy());
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
                    return new Token(TokenType.AssignmentAddition, string.Empty, startPosition, _position.Copy());
                case '-':
                    Advance();
                    return new Token(TokenType.AssignmentSubtraction, string.Empty, startPosition, _position.Copy());
                case '*':
                    Advance();
                    return new Token(TokenType.AssignmentMultiplication, string.Empty, startPosition, _position.Copy());
                case '/':
                    Advance();
                    return new Token(TokenType.AssignmentDivision, string.Empty, startPosition, _position.Copy());
                case '%':
                    Advance();
                    return new Token(TokenType.AssignmentModulo, string.Empty, startPosition, _position.Copy());
                case '^':
                    Advance();
                    return new Token(TokenType.AssignmentPower, string.Empty, startPosition, _position.Copy());
                case '&':
                    Advance();
                    return new Token(TokenType.AssignmentBitwiseAnd, string.Empty, startPosition, _position.Copy());
                case '|':
                    Advance();
                    return new Token(TokenType.AssignmentBitwiseOr, string.Empty, startPosition, _position.Copy());
                case '\\':
                    Advance();
                    return new Token(TokenType.AssignmentBitwiseXOr, string.Empty, startPosition, _position.Copy());
                case '<':
                    Advance();
                    return new Token(TokenType.AssignmentBitwiseLeftShift, string.Empty, startPosition, _position.Copy());
                case '>':
                    Advance();
                    return new Token(TokenType.AssignmentBitwiseRightShift, string.Empty, startPosition, _position.Copy());
                default:
                    return new Token(TokenType.Colon, string.Empty, startPosition, _position.Copy());
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
                    return new Token(TokenType.KeywordItem, string.Empty, startPosition, _position.Copy());
                case "and":
                    return new Token(TokenType.KeywordAnd, string.Empty, startPosition, _position.Copy());
                case "or":
                    return new Token(TokenType.KeywordOr, string.Empty, startPosition, _position.Copy());
                case "invert":
                    return new Token(TokenType.KeywordInvert, string.Empty, startPosition, _position.Copy());
                case "if":
                    return new Token(TokenType.KeywordIf, string.Empty, startPosition, _position.Copy());
                case "else":
                    return new Token(TokenType.KeywordElse, string.Empty, startPosition, _position.Copy());
                case "do":
                    return new Token(TokenType.KeywordDo, string.Empty, startPosition, _position.Copy());
                case "count":
                    return new Token(TokenType.KeywordCount, string.Empty, startPosition, _position.Copy());
                case "from":
                    return new Token(TokenType.KeywordFrom, string.Empty, startPosition, _position.Copy());
                case "as":
                    return new Token(TokenType.KeywordAs, string.Empty, startPosition, _position.Copy());
                case "to":
                    return new Token(TokenType.KeywordTo, string.Empty, startPosition, _position.Copy());
                case "step":
                    return new Token(TokenType.KeywordStep, string.Empty, startPosition, _position.Copy());
                case "while":
                    return new Token(TokenType.KeywordWhile, string.Empty, startPosition, _position.Copy());
                case "function":
                    return new Token(TokenType.KeywordFunction, string.Empty, startPosition, _position.Copy());
                case "special":
                    return new Token(TokenType.KeywordSpecial, string.Empty, startPosition, _position.Copy());
                case "with":
                    return new Token(TokenType.KeywordWith, string.Empty, startPosition, _position.Copy());
                case "end":
                    return new Token(TokenType.KeywordEnd, string.Empty, startPosition, _position.Copy());
                case "return":
                    return new Token(TokenType.KeywordReturn, string.Empty, startPosition, _position.Copy());
                case "skip":
                    return new Token(TokenType.KeywordSkip, string.Empty, startPosition, _position.Copy());
                case "stop":
                    return new Token(TokenType.KeywordStop, string.Empty, startPosition, _position.Copy());
                case "try":
                    return new Token(TokenType.KeywordTry, string.Empty, startPosition, _position.Copy());
                case "error":
                    return new Token(TokenType.KeywordError, string.Empty, startPosition, _position.Copy());
                case "in":
                    return new Token(TokenType.KeywordIn, string.Empty, startPosition, _position.Copy());
                case "object":
                    return new Token(TokenType.KeywordObject, string.Empty, startPosition, _position.Copy());
                case "global":
                    return new Token(TokenType.KeywordGlobal, string.Empty, startPosition, _position.Copy());
                case "include":
                    return new Token(TokenType.KeywordInclude, string.Empty, startPosition, _position.Copy());
                case "all":
                    return new Token(TokenType.KeywordAll, string.Empty, startPosition, _position.Copy());
                case "f":
                    return new Token(TokenType.QeywordF, value, startPosition, _position.Copy());
                case "l":
                    return new Token(TokenType.QeywordL, value, startPosition, _position.Copy());
                case "e":
                    return new Token(TokenType.QeywordE, value, startPosition, _position.Copy());
                case "c":
                    return new Token(TokenType.QeywordC, value, startPosition, _position.Copy());
                case "t":
                    return new Token(TokenType.QeywordT, value, startPosition, _position.Copy());
                case "n":
                    return new Token(TokenType.QeywordN, value, startPosition, _position.Copy());
                case "w":
                    return new Token(TokenType.QeywordW, value, startPosition, _position.Copy());
                case "fd":
                    return new Token(TokenType.QeywordFD, value, startPosition, _position.Copy());
                case "sd":
                    return new Token(TokenType.QeywordSD, value, startPosition, _position.Copy());
                case "od":
                    return new Token(TokenType.QeywordOD, value, startPosition, _position.Copy());
                case "i":
                    return new Token(TokenType.QeywordI, value, startPosition, _position.Copy());
                case "s":
                    return new Token(TokenType.QeywordS, value, startPosition, _position.Copy());
                case "d":
                    return new Token(TokenType.QeywordD, value, startPosition, _position.Copy());
                case "g":
                    return new Token(TokenType.QeywordG, value, startPosition, _position.Copy());
                case "v":
                    return new Token(TokenType.QeywordV, value, startPosition, _position.Copy());
                default:
                    return new Token(TokenType.Identifier, value, startPosition, _position.Copy());
            }
        }
    }
}
