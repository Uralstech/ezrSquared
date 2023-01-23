using ezrSquared.General;
using ezrSquared.Errors;
using ezrSquared.Nodes;
using ezrSquared.Values;
using ezrSquared.Helpers;
using static ezrSquared.Constants.constants;
using System.Reflection;

using ezrSquared.Libraries.IO;
using ezrSquared.Libraries.STD;
using ezrSquared.Libraries.Random;

namespace ezrSquared.Main
{
    public class ezr
    {
        public class lexer
        {
            private string file;
            private string input;
            private position pos;
            private char? currentChar;

            public lexer(string file, string input)
            {
                this.file = file;
                this.input = input;

                pos = new position(-1, 0, -1, this.file, this.input);
                currentChar = null;
                advance();
            }

            private void advance()
            {
                pos.advance(currentChar);
                currentChar = (pos.index < input.Length) ? input[pos.index] : null;
            }

            private void reverse(int count = 1)
            {
                pos.reverse(currentChar, count);
                currentChar = (pos.index < input.Length) ? input[pos.index] : null;
            }

            public token[] compileTokens(out error? error)
            {
                token[] emptyTokenArray = new token[0];
                List<token> tokens = new List<token>();
                error = null;

                while (currentChar != null)
                {
                    if (" \t".Contains(currentChar.ToString()))
                        advance();
                    else if (currentChar == '@')
                        skipComment();
                    else if (currentChar == '\'')
                    {
                        tokens.Add(compileString((char)currentChar, TOKENTYPE.CHARLIST, out error));
                        if (error != null) return emptyTokenArray;
                    }
                    else if (currentChar == '"')
                    {
                        tokens.Add(compileString((char)currentChar, TOKENTYPE.STRING, out error));
                        if (error != null) return emptyTokenArray;
                    }
                    else if (currentChar == ':')
                        tokens.Add(compileColon());
                    else if (currentChar == '<')
                        tokens.Add(compileLessThan());
                    else if (currentChar == '>')
                        tokens.Add(compileGreaterThan());
                    else if (currentChar == '-')
                        tokens.Add(compileMinus());
                    else if (LETTERS_UNDERSCORE.Contains((char)currentChar))
                        tokens.Add(compileIdentifier());
                    else if (DIGITS.Contains((char)currentChar))
                    {
                        token? token = compileNumber(out error);
                        if (error != null) return emptyTokenArray;
                        tokens.Add(token);
                    }
                    else if (";\n".Contains(currentChar.ToString()))
                    {
                        tokens.Add(new token(TOKENTYPE.NEWLINE, pos));
                        advance();
                    }
                    else if (currentChar == '+')
                    {
                        tokens.Add(new token(TOKENTYPE.PLUS, pos));
                        advance();
                    }
                    else if (currentChar == '*')
                    {
                        tokens.Add(new token(TOKENTYPE.MUL, pos));
                        advance();
                    }
                    else if (currentChar == '/')
                    {
                        tokens.Add(new token(TOKENTYPE.DIV, pos));
                        advance();
                    }
                    else if (currentChar == '%')
                    {
                        tokens.Add(new token(TOKENTYPE.MOD, pos));
                        advance();
                    }
                    else if (currentChar == '^')
                    {
                        tokens.Add(new token(TOKENTYPE.POW, pos));
                        advance();
                    }
                    else if (currentChar == '=')
                    {
                        tokens.Add(new token(TOKENTYPE.ISEQUAL, pos));
                        advance();
                    }
                    else if (currentChar == '!')
                    {
                        tokens.Add(new token(TOKENTYPE.NOTEQUAL, pos));
                        advance();
                    }
                    else if (currentChar == ',')
                    {
                        tokens.Add(new token(TOKENTYPE.COMMA, pos));
                        advance();
                    }
                    else if (currentChar == '.')
                    {
                        tokens.Add(new token(TOKENTYPE.PERIOD, pos));
                        advance();
                    }
                    else if (currentChar == '(')
                    {
                        tokens.Add(new token(TOKENTYPE.LPAREN, pos));
                        advance();
                    }
                    else if (currentChar == ')')
                    {
                        tokens.Add(new token(TOKENTYPE.RPAREN, pos));
                        advance();
                    }
                    else if (currentChar == '[')
                    {
                        tokens.Add(new token(TOKENTYPE.LSQUARE, pos));
                        advance();
                    }
                    else if (currentChar == ']')
                    {
                        tokens.Add(new token(TOKENTYPE.RSQUARE, pos));
                        advance();
                    }
                    else if (currentChar == '{')
                    {
                        tokens.Add(new token(TOKENTYPE.LCURLY, pos));
                        advance();
                    }
                    else if (currentChar == '}')
                    {
                        tokens.Add(new token(TOKENTYPE.RCURLY, pos));
                        advance();
                    }
                    else if (currentChar == '&')
                    {
                        tokens.Add(new token(TOKENTYPE.BITAND, pos));
                        advance();
                    }
                    else if (currentChar == '|')
                    {
                        tokens.Add(new token(TOKENTYPE.BITOR, pos));
                        advance();
                    }
                    else if (currentChar == '\\')
                    {
                        tokens.Add(new token(TOKENTYPE.BITXOR, pos));
                        advance();
                    }
                    else if (currentChar == '~')
                    {
                        tokens.Add(new token(TOKENTYPE.BITNOT, pos));
                        advance();
                    }
                    else
                    {
                        position startPos = pos.copy();
                        char char_ = (char)currentChar;
                        advance();

                        error = new unknownCharacterError($"'{char_}'", startPos, pos);
                        return emptyTokenArray;
                    }
                }

                tokens.Add(new token(TOKENTYPE.ENDOFFILE, pos));
                return tokens.ToArray();
            }

            private void skipComment()
            {
                advance();
                while (currentChar != '\n' && currentChar != null)
                    advance();
            }

            private token compileColon()
            {
                position startPos = pos.copy();
                advance();

                if (currentChar == '+')
                {
                    advance();
                    return new token(TOKENTYPE.ASSIGNPLUS, startPos, pos);
                }
                else if (currentChar == '-')
                {
                    advance();
                    return new token(TOKENTYPE.ASSIGNMINUS, startPos, pos);
                }
                else if (currentChar == '*')
                {
                    advance();
                    return new token(TOKENTYPE.ASSIGNMUL, startPos, pos);
                }
                else if (currentChar == '/')
                {
                    advance();
                    return new token(TOKENTYPE.ASSIGNDIV, startPos, pos);
                }
                else if (currentChar == '%')
                {
                    advance();
                    return new token(TOKENTYPE.ASSIGNMOD, startPos, pos);
                }
                else if (currentChar == '^')
                {
                    advance();
                    return new token(TOKENTYPE.ASSIGNPOW, startPos, pos);
                }
                else if (currentChar == '&')
                {
                    advance();
                    return new token(TOKENTYPE.ASSIGNBITAND, startPos, pos);
                }
                else if (currentChar == '|')
                {
                    advance();
                    return new token(TOKENTYPE.ASSIGNBITOR, startPos, pos);
                }
                else if (currentChar == '\\')
                {
                    advance();
                    return new token(TOKENTYPE.ASSIGNBITXOR, startPos, pos);
                }
                else if (currentChar == '<')
                {
                    advance();
                    return new token(TOKENTYPE.ASSIGNBITLSHIFT, startPos, pos);
                }
                else if (currentChar == '>')
                {
                    advance();
                    return new token(TOKENTYPE.ASSIGNBITRSHIFT, startPos, pos);
                }

                return new token(TOKENTYPE.COLON, startPos, pos);
            }

            private token compileLessThan()
            {
                position startPos = pos.copy();
                advance();

                if (currentChar == '=')
                {
                    advance();
                    return new token(TOKENTYPE.LESSTHANOREQUAL, startPos, pos);
                }
                else if (currentChar == '<')
                {
                    advance();
                    return new token(TOKENTYPE.BITLSHIFT, startPos, pos);
                }

                return new token(TOKENTYPE.LESSTHAN, startPos, pos);
            }

            private token compileGreaterThan()
            {
                position startPos = pos.copy();
                advance();

                if (currentChar == '=')
                {
                    advance();
                    return new token(TOKENTYPE.GREATERTHANOREQUAL, startPos, pos);
                }
                else if (currentChar == '>')
                {
                    advance();
                    return new token(TOKENTYPE.BITRSHIFT, startPos, pos);
                }

                return new token(TOKENTYPE.GREATERTHAN, startPos, pos);
            }

            private token compileMinus()
            {
                position startPos = pos.copy();
                advance();

                if (currentChar == '>')
                {
                    advance();
                    return new token(TOKENTYPE.ARROW, startPos, pos);
                }

                return new token(TOKENTYPE.MINUS, startPos, pos);
            }

            private token compileString(char stringChar, TOKENTYPE targetToken, out error? error)
            {
                error = null;
                string stringToReturn = "";
                position startPos = pos.copy();
                bool escapeChar = false;
                advance();

                Dictionary<char, char> escapeChars = new Dictionary<char, char>()
                {
                    { 'n', '\n' },
                    { 't', '\t' },
                    { 'r', '\r' }
                };

                while (currentChar != null && (currentChar != stringChar || escapeChar))
                {
                    if (escapeChar)
                    {
                        if (escapeChars.TryGetValue((char)currentChar, out char escaped))
                            stringToReturn += escaped;
                        else
                            stringToReturn += currentChar;

                        escapeChar = false;
                    }
                    else
                    {
                        if (currentChar == '\\')
                            escapeChar = true;
                        else
                            stringToReturn += currentChar;
                    }

                    advance();
                }

                position prevPosition = pos.copy();
                if (currentChar == null || currentChar != stringChar)
                    error = new invalidGrammarError($"Expected '{stringChar}'", prevPosition, pos);

                advance();
                return new token(targetToken, stringToReturn, startPos, pos);
            }

            private token compileIdentifier()
            {
                string idString = "";
                position startPos = pos.copy();

                while (currentChar != null && ALPHANUM_UNDERSCORE.Contains((char)currentChar))
                {
                    idString += currentChar;
                    advance();
                }

                TOKENTYPE tokenType = KEYWORDS.Contains(idString) ? TOKENTYPE.KEY : (QEYWORDS.Contains(idString) ? TOKENTYPE.QEY : TOKENTYPE.ID);
                return new token(tokenType, idString, startPos, pos);
            }

            private token? compileNumber(out error? error)
            {
                error = null;
                string numberString = "";
                position startPos = pos.copy();
                int periodCount = 0;

                while (currentChar != null && DIGITS_PERIOD.Contains((char)currentChar))
                {
                    bool wasCharPeriod = false;
                    if (currentChar == '.')
                    {
                        if (periodCount == 1)
                            break;

                        wasCharPeriod = true;
                        periodCount++;
                    }

                    numberString += currentChar;
                    advance();

                    if (wasCharPeriod && (currentChar == null || !DIGITS.Contains((char)currentChar)))
                    {
                        numberString = numberString.Remove(numberString.Length - 1);
                        reverse();

                        periodCount--;
                        break;
                    }
                }

                if (periodCount == 0 && int.TryParse(numberString, out int int_))
                    return new token(TOKENTYPE.INT, int_, startPos, pos);
                return new token(TOKENTYPE.FLOAT, float.Parse(numberString), startPos, pos);
            }
        }

        public class parser
        {
            private token[] tokens;
            public token currentToken;
            public int index;

            private bool usingQSyntax = false;

            public parser(token[] tokens)
            {
                this.tokens = tokens;
                this.index = -1;

                advance();
            }

            private void advance()
            {
                index++;
                updateCurrentToken();
            }

            private void reverse(int amount = 1)
            {
                index -= amount;
                updateCurrentToken();
            }

            private void updateCurrentToken()
            {
                if (index >= 0 && index < tokens.Length)
                    currentToken = tokens[index];
            }

            public parseResult parse()
            {
                parseResult result = statements();
                if (result.error == null && currentToken.type != TOKENTYPE.ENDOFFILE)
                    return result.failure(new invalidGrammarError("Expected [INT], [FLOAT], [STRING], [CHARACTER-LIST], [IDENTIFIER], 'if', 'count', 'while', 'try', 'function', 'special', 'object', 'include', 'invert', 'global', 'item', 'return', 'skip', 'stop', '!', '(', '[', '{', '+', '-' or '~'", currentToken.startPos, currentToken.endPos));
                return result;
            }

            private bool isAssignmentTokenType(TOKENTYPE type) { return type == TOKENTYPE.ASSIGNPLUS || type == TOKENTYPE.ASSIGNMINUS || type == TOKENTYPE.ASSIGNMUL || type == TOKENTYPE.ASSIGNDIV || type == TOKENTYPE.ASSIGNMOD || type == TOKENTYPE.ASSIGNPOW || type == TOKENTYPE.ASSIGNBITAND || type == TOKENTYPE.ASSIGNBITOR || type == TOKENTYPE.ASSIGNBITXOR || type == TOKENTYPE.ASSIGNBITLSHIFT || type == TOKENTYPE.ASSIGNBITRSHIFT; }

            private int skipNewlines(parseResult result)
            {
                int newlineCount = 0;
                while (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    result.registerAdvance();
                    advance();

                    newlineCount++;
                }

                return newlineCount;
            }

            private parseResult statements()
            {
                parseResult result = new parseResult();
                List<node> statements = new List<node>();
                position startPos = currentToken.startPos.copy();
                skipNewlines(result);

                node statement = result.register(this.statement());
                if (result.error != null)
                    return result;
                statements.Add(statement);

                bool moreStatements = true;
                while (true)
                {
                    int newlineCount = skipNewlines(result);
                    if (newlineCount == 0 ||
                        currentToken.matchString(TOKENTYPE.KEY, "end") ||
                        currentToken.matchString(TOKENTYPE.KEY, "else") ||
                        currentToken.matchString(TOKENTYPE.KEY, "error") ||
                        (usingQSyntax && (currentToken.matchString(TOKENTYPE.QEY, "f") ||
                        currentToken.matchString(TOKENTYPE.QEY, "l") ||
                        currentToken.matchString(TOKENTYPE.QEY, "e") ||
                        currentToken.matchString(TOKENTYPE.QEY, "s"))) ||
                        currentToken.type == TOKENTYPE.ENDOFFILE)
                        moreStatements = false;

                    if (!moreStatements) break;

                    statement = result.register(this.statement());
                    if (result.error != null)
                        return result;
                    statements.Add(statement);
                }

                return result.success(new arrayNode(statements.ToArray(), startPos, currentToken.endPos.copy()));
            }

            private parseResult statement()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (currentToken.matchString(TOKENTYPE.KEY, "return"))
                {
                    result.registerAdvance();
                    advance();

                    node? expression_ = result.tryRegister(this.expression());
                    if (expression_ == null)
                    {
                        reverse(result.reverseCount);
                        return result.success(new returnNode(expression_, startPos, currentToken.startPos.copy()));
                    }
                    else
                        return result.success(new returnNode(expression_, startPos, currentToken.endPos.copy()));
                }
                else if (currentToken.matchString(TOKENTYPE.KEY, "skip"))
                {
                    result.registerAdvance();
                    advance();

                    return result.success(new skipNode(startPos, currentToken.startPos.copy()));
                }
                else if (currentToken.matchString(TOKENTYPE.KEY, "stop"))
                {
                    result.registerAdvance();
                    advance();

                    return result.success(new stopNode(startPos, currentToken.startPos.copy()));
                }

                node expression = result.register(this.expression());
                if (result.error != null)
                    return result.failure(new invalidGrammarError("Expected [INT], [FLOAT], [STRING], [CHARACTER-LIST], [IDENTIFIER], 'if', 'count', 'while', 'try', 'function', 'special', 'object', 'include', 'invert', 'global', 'item', 'return', 'skip', 'stop', '!', '(', '[', '{', '+', '-' or '~'", currentToken.startPos, currentToken.endPos));
                return result.success(expression);
            }

            private parseResult expression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                bool isGlobal = false;
                if (currentToken.type == TOKENTYPE.NOTEQUAL)
                {
                    bool wasUsingQSyntax = usingQSyntax;
                    usingQSyntax = true;

                    result.registerAdvance();
                    advance();

                    if (currentToken.matchString(TOKENTYPE.QEY, "g") || currentToken.matchString(TOKENTYPE.QEY, "d"))
                    {
                        if (currentToken.matchString(TOKENTYPE.QEY, "g"))
                        {
                            isGlobal = true;
                            result.registerAdvance();
                            advance();
                        }

                        if (currentToken.matchString(TOKENTYPE.QEY, "d"))
                        {
                            result.registerAdvance();
                            advance();

                            if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                                return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                            token varName = currentToken;
                            result.registerAdvance();
                            advance();

                            node accessNode = new variableAccessNode(varName, isGlobal, varName.startPos.copy(), varName.endPos.copy());
                            bool isSimple = true;
                            if (currentToken.type == TOKENTYPE.PERIOD)
                            {
                                isSimple = false;

                                while (currentToken.type == TOKENTYPE.PERIOD)
                                {
                                    token operatorToken_ = currentToken;
                                    result.registerAdvance();
                                    advance();

                                    if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                                    token right = currentToken;
                                    result.registerAdvance();
                                    advance();

                                    accessNode = new binaryOperationNode(accessNode, new variableAccessNode(right, false, right.startPos.copy(), right.endPos.copy()), operatorToken_, accessNode.startPos.copy(), right.endPos.copy());
                                }
                            }

                            token? operatorToken = null;
                            if (isAssignmentTokenType(currentToken.type))
                            {
                                operatorToken = currentToken;
                                result.registerAdvance();
                                advance();
                            }

                            node expression = result.register(this.expression());
                            if (result.error != null) return result;

                            if (!wasUsingQSyntax)
                                usingQSyntax = false;

                            if (isSimple)
                                return result.success(new simpleVariableAssignNode(varName, operatorToken, expression, isGlobal, startPos, currentToken.endPos.copy()));
                            else
                                return result.success(new objectVariableAssignNode(accessNode, varName, operatorToken, expression, false, startPos, currentToken.endPos.copy()));
                        }
                        else if (isGlobal)
                        {
                            isGlobal = false;
                            reverse(result.advanceCount);
                        }
                    }
                    else
                        reverse(result.advanceCount);

                    if (!wasUsingQSyntax)
                        usingQSyntax = false;
                }

                if (currentToken.matchString(TOKENTYPE.KEY, "global"))
                {
                    isGlobal = true;
                    result.registerAdvance();
                    advance();
                }

                if (currentToken.matchString(TOKENTYPE.KEY, "item"))
                {
                    result.registerAdvance();
                    advance();

                    if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                    token varName = currentToken;
                    result.registerAdvance();
                    advance();

                    node accessNode = new variableAccessNode(varName, isGlobal, varName.startPos.copy(), varName.endPos.copy());
                    bool isSimple = true;
                    if (currentToken.type == TOKENTYPE.PERIOD)
                    {
                        isSimple = false;

                        while (currentToken.type == TOKENTYPE.PERIOD)
                        {
                            token operatorToken_ = currentToken;
                            result.registerAdvance();
                            advance();

                            if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                                return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                            token right = currentToken;
                            result.registerAdvance();
                            advance();

                            accessNode = new binaryOperationNode(accessNode, new variableAccessNode(right, false, right.startPos.copy(), right.endPos.copy()), operatorToken_, accessNode.startPos.copy(), right.endPos.copy());
                            varName = right;
                        }
                    }

                    if (currentToken.type != TOKENTYPE.COLON && !isAssignmentTokenType(currentToken.type))
                        return result.failure(new invalidGrammarError("Expected ':', ':+', ':-', ':*', ':/', ':%', ':^', ':&', ':|', ':\\', ':<' or ':>'", currentToken.startPos, currentToken.endPos));
                    token operatorToken = currentToken;
                    result.registerAdvance();
                    advance();

                    node expression = result.register(this.expression());
                    if (result.error != null) return result;

                    if (isSimple)
                        return result.success(new simpleVariableAssignNode(varName, operatorToken, expression, isGlobal, startPos, currentToken.endPos.copy()));
                    else
                        return result.success(new objectVariableAssignNode(accessNode, varName, operatorToken, expression, false, startPos, currentToken.endPos.copy()));
                }
                else if (isGlobal)
                    reverse(result.advanceCount);

                node node = result.register(binaryOperation(compExpression, new TypeValuePair[] { new TypeValuePair(TOKENTYPE.KEY, "and"), new TypeValuePair(TOKENTYPE.KEY, "or" ) }));
                if (result.error != null)
                    return result.failure(new invalidGrammarError("Expected [INT], [FLOAT], [STRING], [CHARACTER-LIST], [IDENTIFIER], 'if', 'count', 'while', 'try', 'function', 'special', 'object', 'include', 'invert', 'global', 'item', '!', '(', '[', '{', '+', '-' or '~'", currentToken.startPos, currentToken.endPos));
                return result.success(node);
            }

            private parseResult compExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (currentToken.type == TOKENTYPE.NOTEQUAL)
                {
                    bool wasUsingQSyntax = usingQSyntax;
                    usingQSyntax = true;
                    result.registerAdvance();
                    advance();

                    if (currentToken.matchString(TOKENTYPE.QEY, "v"))
                    {
                        token operatorToken = currentToken;
                        operatorToken.type = TOKENTYPE.KEY;
                        result.registerAdvance();
                        advance();

                        node node_ = result.register(compExpression());
                        if (result.error != null) return result;

                        if (!wasUsingQSyntax)
                            usingQSyntax = false;
                        return result.success(new unaryOperationNode(node_, operatorToken, startPos, currentToken.endPos.copy()));
                    }
                    else
                        reverse(result.advanceCount);

                    if (!wasUsingQSyntax)
                        usingQSyntax = false;
                }

                if (currentToken.matchString(TOKENTYPE.KEY, "invert"))
                {
                    token operatorToken = currentToken;
                    result.registerAdvance();
                    advance();

                    node node_ = result.register(compExpression());
                    if (result.error != null) return result;

                    return result.success(new unaryOperationNode(node_, operatorToken, startPos, currentToken.endPos.copy()));
                }

                node node = result.register(binaryOperation(bitOrExpression, new TypeValuePair[] { new TypeValuePair(TOKENTYPE.ISEQUAL), new TypeValuePair(TOKENTYPE.NOTEQUAL), new TypeValuePair(TOKENTYPE.LESSTHAN), new TypeValuePair(TOKENTYPE.GREATERTHAN), new TypeValuePair(TOKENTYPE.LESSTHANOREQUAL), new TypeValuePair(TOKENTYPE.GREATERTHANOREQUAL) }));
                if (result.error != null)
                    return result.failure(new invalidGrammarError("Expected [INT], [FLOAT], [STRING], [CHARACTER-LIST], [IDENTIFIER], 'if', 'count', 'while', 'try', 'function', 'special', 'object', 'include', 'invert', '!', '(', '[', '{', '+', '-' or '~'", currentToken.startPos, currentToken.endPos));
                return result.success(node);
            }

            private parseResult bitOrExpression() { return binaryOperation(bitXOrExpression, new TypeValuePair[] { new TypeValuePair(TOKENTYPE.BITOR) }); }

            private parseResult bitXOrExpression() { return binaryOperation(bitAndExpression, new TypeValuePair[] { new TypeValuePair(TOKENTYPE.BITXOR) }); }

            private parseResult bitAndExpression() { return binaryOperation(bitShiftExpression, new TypeValuePair[] { new TypeValuePair(TOKENTYPE.BITAND) }); }
            
            private parseResult bitShiftExpression() { return binaryOperation(arithExpression, new TypeValuePair[] { new TypeValuePair(TOKENTYPE.BITLSHIFT), new TypeValuePair(TOKENTYPE.BITRSHIFT) }); }

            private parseResult arithExpression() { return binaryOperation(term, new TypeValuePair[] { new TypeValuePair(TOKENTYPE.PLUS), new TypeValuePair(TOKENTYPE.MINUS) }); }

            private parseResult term() { return binaryOperation(factor, new TypeValuePair[] { new TypeValuePair(TOKENTYPE.MUL), new TypeValuePair(TOKENTYPE.DIV), new TypeValuePair(TOKENTYPE.MOD) }); }

            private parseResult factor()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();
                token token = currentToken;

                if (token.type == TOKENTYPE.PLUS || token.type == TOKENTYPE.MINUS || token.type == TOKENTYPE.BITNOT)
                {
                    result.registerAdvance();
                    advance();

                    node node = result.register(factor());
                    if (result.error != null) return result;
                    
                    return result.success(new unaryOperationNode(node, token, startPos, currentToken.endPos.copy()));
                }

                return power();
            }

            private parseResult power() { return binaryOperation(inExpression, new TypeValuePair[] { new TypeValuePair(TOKENTYPE.POW) }); }

            private parseResult inExpression() { return binaryOperation(objectCall, new TypeValuePair[] { new TypeValuePair(TOKENTYPE.KEY, "in") }); }

            private parseResult objectCall() { return binaryOperation(call, new TypeValuePair[] { new TypeValuePair(TOKENTYPE.PERIOD) }, objectCall); }

            private parseResult call()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                node node = result.register(atom());
                if (result.error != null) return result;

                if (currentToken.type == TOKENTYPE.LPAREN)
                {
                    result.registerAdvance();
                    advance();

                    List<node> argNodes = new List<node>();
                    if (currentToken.type == TOKENTYPE.RPAREN)
                    {
                        result.registerAdvance();
                        advance();
                    }
                    else
                    {
                        argNodes.Add(result.register(expression()));
                        if (result.error != null)
                            return result.failure(new invalidGrammarError("Expected [INT], [FLOAT], [STRING], [CHARACTER-LIST], [IDENTIFIER], 'if', 'count', 'while', 'try', 'function', 'special', 'object', 'include', 'invert', 'global', 'item', '!', '(', '[', '{', '+', '-', '~' or ')'", currentToken.startPos, currentToken.endPos));

                        while (currentToken.type == TOKENTYPE.COMMA)
                        {
                            result.registerAdvance();
                            advance();

                            argNodes.Add(result.register(expression()));
                            if (result.error != null) return result;
                        }

                        if (currentToken.type != TOKENTYPE.RPAREN) return result.failure(new invalidGrammarError("Expected ',' or ')'", currentToken.startPos, currentToken.endPos));
                        result.registerAdvance();
                        advance();
                    }

                    return result.success(new callNode(node, argNodes.ToArray(), startPos, currentToken.startPos.copy()));
                }

                return result.success(node);
            }

            private parseResult atom()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();
                token token = currentToken;

                if (token.type == TOKENTYPE.NOTEQUAL)
                {
                    bool wasUsingQSyntax = usingQSyntax;
                    usingQSyntax = true;
                    node quickSyntaxAtom = result.register(this.quickSyntaxAtom());
                    if (!wasUsingQSyntax)
                        usingQSyntax = false;

                    if (result.error != null) return result;
                    return result.success(quickSyntaxAtom);
                }
                else if (token.type == TOKENTYPE.INT || token.type == TOKENTYPE.FLOAT)
                {
                    result.registerAdvance();
                    advance();

                    return result.success(new numberNode(token, startPos, currentToken.startPos.copy()));
                }
                else if (token.type == TOKENTYPE.STRING)
                {
                    result.registerAdvance();
                    advance();

                    return result.success(new stringNode(token, startPos, currentToken.startPos.copy()));
                }
                else if (token.type == TOKENTYPE.CHARLIST)
                {
                    result.registerAdvance();
                    advance();

                    return result.success(new charListNode(token, startPos, currentToken.startPos.copy()));
                }
                else if (token.type == TOKENTYPE.ID || currentToken.type == TOKENTYPE.QEY)
                {
                    result.registerAdvance();
                    advance();

                    return result.success(new variableAccessNode(token, false, startPos, currentToken.startPos.copy()));
                }
                else if (token.type == TOKENTYPE.LPAREN)
                {
                    result.registerAdvance();
                    advance();

                    node? expression = result.tryRegister(this.expression());
                    if (expression == null || currentToken.type == TOKENTYPE.COMMA)
                    {
                        reverse(result.advanceCount);

                        node arrayExpression = result.register(this.arrayExpression());
                        if (result.error != null) return result;

                        return result.success(arrayExpression);
                    }

                    if (currentToken.type == TOKENTYPE.RPAREN)
                    {
                        result.registerAdvance();
                        advance();

                        expression.startPos = startPos;
                        expression.endPos = currentToken.startPos.copy();
                        return result.success(expression);
                    }

                    return result.failure(new invalidGrammarError("Expected ',' or ')'", currentToken.startPos, currentToken.endPos));
                }
                else if (token.type == TOKENTYPE.LSQUARE)
                {
                    node listExpression = result.register(this.listExpression());
                    if (result.error != null) return result;
                    return result.success(listExpression);
                }
                else if (token.type == TOKENTYPE.LCURLY)
                {
                    node dictionaryExpression = result.register(this.dictionaryExpression());
                    if (result.error != null) return result;
                    return result.success(dictionaryExpression);
                }
                else if (token.matchString(TOKENTYPE.KEY, "global"))
                {
                    result.registerAdvance();
                    advance();

                    if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                        return result.failure(new invalidGrammarError("Expected 'item' or [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                    token varName = currentToken;
                    result.registerAdvance();
                    advance();

                    return result.success(new variableAccessNode(varName, true, startPos, currentToken.startPos.copy()));
                }
                else if (token.matchString(TOKENTYPE.KEY, "if"))
                {
                    node ifExpression = result.register(this.ifExpression());
                    if (result.error != null) return result;
                    return result.success(ifExpression);
                }
                else if (token.matchString(TOKENTYPE.KEY, "count"))
                {
                    node countExpression = result.register(this.countExpression());
                    if (result.error != null) return result;
                    return result.success(countExpression);
                }
                else if (token.matchString(TOKENTYPE.KEY, "while"))
                {
                    node whileExpression = result.register(this.whileExpression());
                    if (result.error != null) return result;
                    return result.success(whileExpression);
                }
                else if (token.matchString(TOKENTYPE.KEY, "try"))
                {
                    node tryExpression = result.register(this.tryExpression());
                    if (result.error != null) return result;
                    return result.success(tryExpression);
                }
                else if (token.matchString(TOKENTYPE.KEY, "function"))
                {
                    node functionExpression = result.register(this.functionExpression());
                    if (result.error != null) return result;
                    return result.success(functionExpression);
                }
                else if (token.matchString(TOKENTYPE.KEY, "special"))
                {
                    node specialExpression = result.register(this.specialExpression());
                    if (result.error != null) return result;
                    return result.success(specialExpression);
                }
                else if (token.matchString(TOKENTYPE.KEY, "object"))
                {
                    node objectExpression = result.register(this.objectExpression());
                    if (result.error != null) return result;
                    return result.success(objectExpression);
                }
                else if (token.matchString(TOKENTYPE.KEY, "include"))
                {
                    node includeExpression = result.register(this.includeExpression());
                    if (result.error != null) return result;
                    return result.success(includeExpression);
                }

                return result.failure(new invalidGrammarError("Expected [INT], [FLOAT], [STRING], [CHARACTER-LIST], [IDENTIFIER], 'if', 'count', 'while', 'try', 'function', 'special', 'object', 'include', '!', '(', '[' or '{'", currentToken.startPos, currentToken.endPos));
            }

            private parseResult arrayExpression()
            {
                parseResult result = new parseResult();
                node[] elementNodes = new node[0];
                position startPos = currentToken.startPos.copy();

                if (currentToken.type != TOKENTYPE.LPAREN)
                    return result.failure(new invalidGrammarError("Expected '('", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                if (currentToken.type == TOKENTYPE.RPAREN)
                {
                    result.registerAdvance();
                    advance();
                }
                else
                {
                    skipNewlines(result);
                    node firstElement = result.register(expression());
                    if (result.error != null)
                        return result.failure(new invalidGrammarError("Expected [INT], [FLOAT], [STRING], [CHARACTER-LIST], [IDENTIFIER], 'if', 'count', 'while', 'try', 'function', 'special', 'object', 'include', 'invert', 'global', 'item', '!', '(', '[', '{', '+', '-', '~' or ')'", currentToken.startPos, currentToken.endPos));

                    List<node> nodeList = new List<node> { firstElement };
                    bool moreElements = true;
                    skipNewlines(result);
                    while (currentToken.type == TOKENTYPE.COMMA)
                    {
                        result.registerAdvance();
                        advance();

                        skipNewlines(result);
                        node? node = result.tryRegister(expression());
                        if (node == null)
                        {
                            reverse(result.reverseCount);
                            moreElements = false;
                            break;
                        }
                        nodeList.Add(node);
                    }
                    skipNewlines(result);

                    if (currentToken.type != TOKENTYPE.RPAREN)
                    {
                        if (moreElements)
                            return result.failure(new invalidGrammarError("Expected ',' or ')'", currentToken.startPos, currentToken.endPos));
                        return result.failure(new invalidGrammarError("Expected [INT], [FLOAT], [STRING], [CHARACTER-LIST], [IDENTIFIER], 'if', 'count', 'while', 'try', 'function', 'special', 'object', 'include', 'invert', 'global', 'item', '!', '(', '[', '{', '+', '-', '~', or ')'", currentToken.startPos, currentToken.endPos));
                    }

                    elementNodes = nodeList.ToArray();
                    result.registerAdvance();
                    advance();
                }

                return result.success(new arrayNode(elementNodes, startPos, currentToken.startPos.copy()));
            }

            private parseResult listExpression()
            {
                parseResult result = new parseResult();
                node[] elementNodes = new node[0];
                position startPos = currentToken.startPos.copy();

                if (currentToken.type != TOKENTYPE.LSQUARE)
                    return result.failure(new invalidGrammarError("Expected '['", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                if (currentToken.type == TOKENTYPE.RSQUARE)
                {
                    result.registerAdvance();
                    advance();
                }
                else
                {
                    skipNewlines(result);
                    node element = result.register(expression());
                    if (result.error != null)
                        return result.failure(new invalidGrammarError("Expected [INT], [FLOAT], [STRING], [CHARACTER-LIST], [IDENTIFIER], 'if', 'count', 'while', 'try', 'function', 'special', 'object', 'include', 'invert', 'global', 'item', '!', '(', '[', '{', '+', '-', '~' or ']'", currentToken.startPos, currentToken.endPos));

                    List<node> nodeList = new List<node> { element };
                    skipNewlines(result);
                    while (currentToken.type == TOKENTYPE.COMMA)
                    {
                        result.registerAdvance();
                        advance();
                        skipNewlines(result);

                        element = result.register(expression());
                        if (result.error != null) return result;
                        nodeList.Add(element);
                    }
                    skipNewlines(result);

                    if (currentToken.type != TOKENTYPE.RSQUARE)
                        return result.failure(new invalidGrammarError("Expected ',' or ']'", currentToken.startPos, currentToken.endPos));

                    elementNodes = nodeList.ToArray();
                    result.registerAdvance();
                    advance();
                }

                return result.success(new listNode(elementNodes, startPos, currentToken.startPos.copy()));
            }

            private parseResult dictionaryExpression()
            {

                parseResult result = new parseResult();
                node[][] nodePairs = new node[0][];
                position startPos = currentToken.startPos.copy();

                if (currentToken.type != TOKENTYPE.LCURLY)
                    return result.failure(new invalidGrammarError("Expected '{'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                if (currentToken.type == TOKENTYPE.RCURLY)
                {
                    result.registerAdvance();
                    advance();
                }
                else
                {
                    skipNewlines(result);

                    node key = result.register(expression());
                    if (result.error != null) return result;

                    if (currentToken.type != TOKENTYPE.COLON)
                        return result.failure(new invalidGrammarError("Expected ':'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    node value = result.register(expression());
                    if (result.error != null) return result;

                    List<node[]> pairs = new List<node[]> { new node[2] { key, value } };
                    skipNewlines(result);

                    while (currentToken.type == TOKENTYPE.COMMA)
                    {
                        result.registerAdvance();
                        advance();

                        skipNewlines(result);
                        key = result.register(expression());
                        if (result.error != null) return result;

                        skipNewlines(result);
                        if (currentToken.type != TOKENTYPE.COLON)
                            return result.failure(new invalidGrammarError("Expected ':'", currentToken.startPos, currentToken.endPos));
                        result.registerAdvance();
                        advance();

                        skipNewlines(result);
                        value = result.register(expression());
                        if (result.error != null) return result;
                        pairs.Add(new node[2] { key, value });
                    }

                    skipNewlines(result);
                    if (currentToken.type != TOKENTYPE.RCURLY)
                        return result.failure(new invalidGrammarError("Expected ',' or '}'", currentToken.startPos, currentToken.endPos));

                    nodePairs = pairs.ToArray();
                    result.registerAdvance();
                    advance();
                }

                return result.success(new dictionaryNode(nodePairs, startPos, currentToken.startPos.copy()));
            }

            private parseResult ifExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                List<node[]> cases = new List<node[]>();
                node? elseCase = null;

                if (!currentToken.matchString(TOKENTYPE.KEY, "if"))
                    return result.failure(new invalidGrammarError("Expected 'if'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node condition = result.register(this.expression());
                if (result.error != null) return result;

                if (!currentToken.matchString(TOKENTYPE.KEY, "do"))
                    return result.failure(new invalidGrammarError("Expected 'do'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    result.registerAdvance();
                    advance();

                    bodyNode = result.register(this.statements());
                    if (result.error != null) return result;
                    cases.Add(new node[2] { condition, bodyNode });

                    if (currentToken.matchString(TOKENTYPE.KEY, "end"))
                    {
                        result.registerAdvance();
                        advance();
                    }
                    else
                    {
                        if (!currentToken.matchString(TOKENTYPE.KEY, "else"))
                            return result.failure(new invalidGrammarError("Expected 'else' or 'end'", currentToken.startPos, currentToken.endPos));

                        while (currentToken.matchString(TOKENTYPE.KEY, "else"))
                        {
                            result.registerAdvance();
                            advance();

                            bool isElseIf = false;
                            if (currentToken.matchString(TOKENTYPE.KEY, "if"))
                            {
                                result.registerAdvance();
                                advance();

                                condition = result.register(this.expression());
                                if (result.error != null) return result;
                                isElseIf = true;
                            }

                            if (!currentToken.matchString(TOKENTYPE.KEY, "do"))
                                return result.failure(new invalidGrammarError("Expected 'do'", currentToken.startPos, currentToken.endPos));
                            result.registerAdvance();
                            advance();

                            node bodyNode_ = result.register(this.statements());
                            if (result.error != null) return result;
                            if (!isElseIf)
                            {
                                elseCase = bodyNode_;
                                break;
                            }
                            else
                            {
                                bodyNode = bodyNode_;
                                cases.Add(new node[2] { condition, bodyNode });
                            }
                        }

                        if (!currentToken.matchString(TOKENTYPE.KEY, "end"))
                        {
                            if (elseCase == null)
                                return result.failure(new invalidGrammarError("Expected 'else' or 'end'", currentToken.startPos, currentToken.endPos));
                            return result.failure(new invalidGrammarError("Expected 'end'", currentToken.startPos, currentToken.endPos));
                        }
                        result.registerAdvance();
                        advance();
                    }

                    return result.success(new ifNode(cases.ToArray(), elseCase, true, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(statement());
                if (result.error != null) return result;
                cases.Add(new node[2] { condition, bodyNode });

                while (currentToken.matchString(TOKENTYPE.KEY, "else"))
                {
                    result.registerAdvance();
                    advance();

                    bool isElseIf = false;
                    if (currentToken.matchString(TOKENTYPE.KEY, "if"))
                    {
                        result.registerAdvance();
                        advance();

                        condition = result.register(this.expression());
                        if (result.error != null) return result;
                        isElseIf = true;
                    }

                    if (!currentToken.matchString(TOKENTYPE.KEY, "do"))
                        return result.failure(new invalidGrammarError("Expected 'do'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    node body = result.register(statement());
                    if (result.error != null) return result;
                    if (!isElseIf)
                    {
                        elseCase = body;
                        break;
                    }
                    else
                    {
                        bodyNode = body;
                        cases.Add(new node[2] { condition, bodyNode });
                    }
                }

                return result.success(new ifNode(cases.ToArray(), elseCase, false, startPos, currentToken.endPos.copy()));
            }

            private parseResult countExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (!currentToken.matchString(TOKENTYPE.KEY, "count"))
                    return result.failure(new invalidGrammarError("Expected 'count'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node? start = null;
                if (currentToken.matchString(TOKENTYPE.KEY, "from"))
                {
                    result.registerAdvance();
                    advance();

                    start = result.register(expression());
                    if (result.error != null) return result;
                }

                if (!currentToken.matchString(TOKENTYPE.KEY, "to"))
                {
                    if (start != null)
                        return result.failure(new invalidGrammarError("Expected 'from'", currentToken.startPos, currentToken.endPos));
                    return result.failure(new invalidGrammarError("Expected 'from' or 'to'", currentToken.startPos, currentToken.endPos));
                }
                result.registerAdvance();
                advance();

                node end = result.register(expression());
                if (result.error != null) return result;

                node? step = null;
                if (currentToken.matchString(TOKENTYPE.KEY, "step"))
                {
                    result.registerAdvance();
                    advance();

                    step = result.register(expression());
                    if (result.error != null) return result;
                }

                token? variableName = null;
                if (currentToken.matchString(TOKENTYPE.KEY, "as"))
                {
                    result.registerAdvance();
                    advance();

                    if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                    variableName = currentToken;
                    result.registerAdvance();
                    advance();
                }

                if (!currentToken.matchString(TOKENTYPE.KEY, "do"))
                {
                    if (step == null && variableName == null)
                        return result.failure(new invalidGrammarError("Expected 'step', 'as' or 'do'", currentToken.startPos, currentToken.endPos));
                    else if (variableName == null)
                        return result.failure(new invalidGrammarError("Expected 'as' or 'do'", currentToken.startPos, currentToken.endPos));
                    return result.failure(new invalidGrammarError("Expected 'do'", currentToken.startPos, currentToken.endPos));
                }
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    result.registerAdvance();
                    advance();

                    bodyNode = result.register(statements());
                    if (result.error != null) return result;

                    if (!currentToken.matchString(TOKENTYPE.KEY, "end"))
                        return result.failure(new invalidGrammarError("Expected 'end'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    return result.success(new countNode(variableName, start, end, step, bodyNode, true, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(statement());
                if (result.error != null) return result;

                return result.success(new countNode(variableName, start, end, step, bodyNode, false, startPos, currentToken.endPos.copy()));
            }

            private parseResult whileExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (!currentToken.matchString(TOKENTYPE.KEY, "while"))
                    return result.failure(new invalidGrammarError("Expected 'while'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node condition = result.register(this.expression());
                if (result.error != null) return result;

                if (!currentToken.matchString(TOKENTYPE.KEY, "do"))
                    return result.failure(new invalidGrammarError("Expected 'do'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    result.registerAdvance();
                    advance();

                    bodyNode = result.register(statements());
                    if (result.error != null) return result;

                    if (!currentToken.matchString(TOKENTYPE.KEY, "end"))
                        return result.failure(new invalidGrammarError("Expected 'end'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    return result.success(new whileNode(condition, bodyNode, true, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(statement());
                if (result.error != null) return result;

                return result.success(new whileNode(condition, bodyNode, false, startPos, currentToken.endPos.copy()));
            }

            private parseResult tryExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();
                List<object?[]> catches = new List<object?[]>();

                if (!currentToken.matchString(TOKENTYPE.KEY, "try"))
                    return result.failure(new invalidGrammarError("Expected 'try'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                if (!currentToken.matchString(TOKENTYPE.KEY, "do"))
                    return result.failure(new invalidGrammarError("Expected 'do'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    result.registerAdvance();
                    advance();

                    bodyNode = result.register(statements());
                    if (result.error != null) return result;

                    int emptyCatches_ = 0;
                    int emptyCatchIndex_ = -1;

                    while (currentToken.matchString(TOKENTYPE.KEY, "error"))
                    {
                        result.registerAdvance();
                        advance();

                        token? error = null;
                        token? varName = null;
                        if (currentToken.type == TOKENTYPE.STRING || currentToken.type == TOKENTYPE.CHARLIST)
                        {
                            error = currentToken;
                            result.registerAdvance();
                            advance();
                        }
                        else if (currentToken.type == TOKENTYPE.ID || currentToken.type == TOKENTYPE.QEY)
                        {
                            error = currentToken;
                            error.type = TOKENTYPE.ID;
                            result.registerAdvance();
                            advance();
                        }

                        if (currentToken.matchString(TOKENTYPE.KEY, "as"))
                        {
                            result.registerAdvance();
                            advance();

                            if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                                return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));

                            varName = currentToken;
                            result.registerAdvance();
                            advance();
                        }

                        if (!currentToken.matchString(TOKENTYPE.KEY, "do"))
                        {
                            if (error == null && varName == null)
                                return result.failure(new invalidGrammarError("Expected [STRING], [CHARACTER-LIST], [IDENTIFIER], 'as' or 'do'", currentToken.startPos, currentToken.endPos));
                            else if (varName == null)
                                return result.failure(new invalidGrammarError("Expected 'as' or 'do'", currentToken.startPos, currentToken.endPos));
                            return result.failure(new invalidGrammarError("Expected 'do'", currentToken.startPos, currentToken.endPos));
                        }
                        result.registerAdvance();
                        advance();

                        node body = result.register(statements());
                        if (result.error != null) return result;

                        if (error == null)
                        {
                            emptyCatches_++;
                            emptyCatchIndex_ = catches.Count;
                        }

                        catches.Add(new object?[3] { error, varName, body });
                    }

                    if (emptyCatches_ > 0)
                    {
                        if (emptyCatches_ > 1)
                            return result.failure(new invalidGrammarError("There cannot be more than one empty 'error' statements", startPos, currentToken.endPos));
                        if (emptyCatchIndex_ != catches.Count - 1)
                            return result.failure(new invalidGrammarError("Empty 'error' statements should always be declared last", startPos, currentToken.endPos));
                    }

                    if (!currentToken.matchString(TOKENTYPE.KEY, "end"))
                        return result.failure(new invalidGrammarError("Expected 'end'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    return result.success(new tryNode(bodyNode, catches.ToArray(), true, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(statement());
                if (result.error != null) return result;

                int emptyCatches = 0;
                int emptyCatchIndex = -1;

                while (currentToken.matchString(TOKENTYPE.KEY, "error"))
                {
                    result.registerAdvance();
                    advance();

                    token? error = null;
                    token? varName = null;
                    if (currentToken.type == TOKENTYPE.STRING || currentToken.type == TOKENTYPE.CHARLIST)
                    {
                        error = currentToken;
                        result.registerAdvance();
                        advance();
                    }
                    else if (currentToken.type == TOKENTYPE.ID || currentToken.type == TOKENTYPE.QEY)
                    {
                        error = currentToken;
                        error.type = TOKENTYPE.ID;
                        result.registerAdvance();
                        advance();
                    }

                    if (currentToken.matchString(TOKENTYPE.KEY, "as"))
                    {
                        result.registerAdvance();
                        advance();

                        if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                            return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));

                        varName = currentToken;
                        result.registerAdvance();
                        advance();
                    }

                    if (!currentToken.matchString(TOKENTYPE.KEY, "do"))
                    {
                        if (error == null && varName == null)
                            return result.failure(new invalidGrammarError("Expected [STRING], [CHARACTER-LIST], [IDENTIFIER], 'as' or 'do'", currentToken.startPos, currentToken.endPos));
                        else if (varName == null)    
                            return result.failure(new invalidGrammarError("Expected 'as' or 'do'", currentToken.startPos, currentToken.endPos));
                        return result.failure(new invalidGrammarError("Expected 'do'", currentToken.startPos, currentToken.endPos));
                    }
                    result.registerAdvance();
                    advance();

                    node expression = result.register(statement());
                    if (result.error != null) return result;

                    if (error == null)
                    {
                        emptyCatches++;
                        emptyCatchIndex = catches.Count;
                    }

                    catches.Add(new object?[3] { error, varName, expression });
                }

                if (emptyCatches > 0)
                {
                    if (emptyCatches > 1)
                        return result.failure(new invalidGrammarError("There cannot be more than one empty 'error' statements", startPos, currentToken.endPos));
                    if (emptyCatchIndex != catches.Count - 1)
                        return result.failure(new invalidGrammarError("Empty 'error' statements should always be declared last", startPos, currentToken.endPos));
                }

                return result.success(new tryNode(bodyNode, catches.ToArray(), false, startPos, currentToken.endPos.copy()));
            }

            private parseResult functionExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (!currentToken.matchString(TOKENTYPE.KEY, "function"))
                    return result.failure(new invalidGrammarError("Expected 'function'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                token? varName = null;
                if (currentToken.type == TOKENTYPE.ID || currentToken.type == TOKENTYPE.QEY)
                {
                    varName = currentToken;
                    result.registerAdvance();
                    advance();
                }

                List<token> argNames = new List<token>();
                if (currentToken.matchString(TOKENTYPE.KEY, "with"))
                {
                    result.registerAdvance();
                    advance();

                    if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                    argNames.Add(currentToken);
                    result.registerAdvance();
                    advance();

                    while (currentToken.type == TOKENTYPE.COMMA)
                    {
                        result.registerAdvance();
                        advance();

                        if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                            return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                        argNames.Add(currentToken);
                        result.registerAdvance();
                        advance();
                    }
                }

                if (!currentToken.matchString(TOKENTYPE.KEY, "do"))
                {
                    if (argNames.Count == 0 && varName == null)
                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER], 'with' or 'do'", currentToken.startPos, currentToken.endPos));
                    else if (argNames.Count == 0)
                        return result.failure(new invalidGrammarError("Expected 'with' or 'do'", currentToken.startPos, currentToken.endPos));
                    return result.failure(new invalidGrammarError("Expected 'do'", currentToken.startPos, currentToken.endPos));
                }
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    bodyNode = result.register(statements());
                    if (result.error != null) return result;

                    if (!currentToken.matchString(TOKENTYPE.KEY, "end"))
                        return result.failure(new invalidGrammarError("Expected 'end'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    return result.success(new functionDefinitionNode(varName, argNames.ToArray(), bodyNode, true, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(expression());
                if (result.error != null) return result;
                return result.success(new functionDefinitionNode(varName, argNames.ToArray(), bodyNode, false, startPos, currentToken.endPos.copy()));
            }

            private parseResult specialExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (!currentToken.matchString(TOKENTYPE.KEY, "special"))
                    return result.failure(new invalidGrammarError("Expected 'special'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                    return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                token varName = currentToken;
                result.registerAdvance();
                advance();

                if (!currentToken.matchString(TOKENTYPE.KEY, "do"))
                    return result.failure(new invalidGrammarError("Expected 'do'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    bodyNode = result.register(statements());
                    if (result.error != null) return result;

                    if (!currentToken.matchString(TOKENTYPE.KEY, "end"))
                        return result.failure(new invalidGrammarError("Expected 'end'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    return result.success(new specialDefinitionNode(varName, bodyNode, true, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(expression());
                if (result.error != null) return result;

                return result.success(new specialDefinitionNode(varName, bodyNode, false, startPos, currentToken.endPos.copy()));
            }

            private parseResult objectExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (!currentToken.matchString(TOKENTYPE.KEY, "object"))
                    return result.failure(new invalidGrammarError("Expected 'object'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                token? inheritFrom = null;
                if (currentToken.type == TOKENTYPE.ID || currentToken.type == TOKENTYPE.QEY)
                {
                    inheritFrom = currentToken;
                    result.registerAdvance();
                    advance();
                }

                token varName;
                if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                {
                    if (inheritFrom == null)
                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                    else
                    {
                        varName = inheritFrom;
                        inheritFrom = null;
                    }
                }
                else
                {
                    varName = currentToken;
                    result.registerAdvance();
                    advance();
                }

                List<token> argNames = new List<token>();
                if (currentToken.matchString(TOKENTYPE.KEY, "with"))
                {
                    result.registerAdvance();
                    advance();

                    if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                    argNames.Add(currentToken);
                    result.registerAdvance();
                    advance();

                    while (currentToken.type == TOKENTYPE.COMMA)
                    {
                        result.registerAdvance();
                        advance();

                        if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                            return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                        argNames.Add(currentToken);
                        result.registerAdvance();
                        advance();
                    }
                }

                if (!currentToken.matchString(TOKENTYPE.KEY, "do"))
                {
                    if (inheritFrom == null && argNames.Count == 0)
                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER], 'with' or 'do'", currentToken.startPos, currentToken.endPos));
                    if (argNames.Count == 0)
                        return result.failure(new invalidGrammarError("Expected 'with' or 'do'", currentToken.startPos, currentToken.endPos));
                    return result.failure(new invalidGrammarError("Expected ',' or 'do'", currentToken.startPos, currentToken.endPos));
                }
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    bodyNode = result.register(statements());
                    if (result.error != null) return result;

                    if (!currentToken.matchString(TOKENTYPE.KEY, "end"))
                        return result.failure(new invalidGrammarError("Expected 'end'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    return result.success(new objectDefinitionNode(varName, inheritFrom, argNames.ToArray(), bodyNode, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(expression());
                if (result.error != null) return result;
                return result.success(new objectDefinitionNode(varName, inheritFrom, argNames.ToArray(), bodyNode, startPos, currentToken.endPos.copy()));
            }

            private parseResult includeExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (!currentToken.matchString(TOKENTYPE.KEY, "include"))
                    return result.failure(new invalidGrammarError("Expected 'include'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                token file;
                if (currentToken.type == TOKENTYPE.STRING || currentToken.type == TOKENTYPE.CHARLIST)
                    file = currentToken;
                else if (currentToken.type == TOKENTYPE.ID || currentToken.type == TOKENTYPE.QEY)
                {
                    file = currentToken;
                    file.type = TOKENTYPE.ID;
                }
                else
                    return result.failure(new invalidGrammarError("Expected [STRING], [CHARACTER-LIST] or [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                token? nickname = null;
                if (currentToken.matchString(TOKENTYPE.KEY, "as"))
                {
                    result.registerAdvance();
                    advance();

                    if (currentToken.type == TOKENTYPE.STRING || currentToken.type == TOKENTYPE.CHARLIST)
                        nickname = currentToken;
                    else if (currentToken.type == TOKENTYPE.ID || currentToken.type == TOKENTYPE.QEY)
                    {
                        nickname = currentToken;
                        nickname.type = TOKENTYPE.ID;
                    }
                    else
                        return result.failure(new invalidGrammarError("Expected [STRING], [CHARACTER-LIST] or [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();
                }

                return result.success(new includeNode(file, nickname, startPos, currentToken.startPos.copy()));
            }

            private parseResult quickSyntaxAtom()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (currentToken.type != TOKENTYPE.NOTEQUAL)
                    return result.failure(new invalidGrammarError("Expected '!'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                token token = currentToken;
                if (token.matchString(TOKENTYPE.QEY, "g"))
                {
                    result.registerAdvance();
                    advance();

                    if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                        return result.failure(new invalidGrammarError("Expected 'd' or [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                    token varName = currentToken;
                    result.registerAdvance();
                    advance();

                    return result.success(new variableAccessNode(varName, true, startPos, currentToken.startPos.copy()));
                }
                else if (token.matchString(TOKENTYPE.QEY, "f"))
                {
                    node ifExpression = result.register(quickIfExpression());
                    if (result.error != null) return result;
                    return result.success(ifExpression);
                }
                else if (token.matchString(TOKENTYPE.QEY, "c"))
                {
                    node countExpression = result.register(quickCountExpression());
                    if (result.error != null) return result;
                    return result.success(countExpression);
                }
                else if (token.matchString(TOKENTYPE.QEY, "w"))
                {
                    node whileExpression = result.register(quickWhileExpression());
                    if (result.error != null) return result;
                    return result.success(whileExpression);
                }
                else if (token.matchString(TOKENTYPE.QEY, "t"))
                {
                    node tryExpression = result.register(quickTryExpression());
                    if (result.error != null) return result;
                    return result.success(tryExpression);
                }
                else if (token.matchString(TOKENTYPE.QEY, "fd"))
                {
                    node functionExpression = result.register(quickFunctionExpression());
                    if (result.error != null) return result;
                    return result.success(functionExpression);
                }
                else if (token.matchString(TOKENTYPE.QEY, "sd"))
                {
                    node specialExpression = result.register(quickSpecialExpression());
                    if (result.error != null) return result;
                    return result.success(specialExpression);
                }
                else if (token.matchString(TOKENTYPE.QEY, "od"))
                {
                    node objectExpression = result.register(quickObjectExpression());
                    if (result.error != null) return result;
                    return result.success(objectExpression);
                }
                else if (token.matchString(TOKENTYPE.QEY, "i"))
                {
                    node includeExpression = result.register(quickIncludeExpression());
                    if (result.error != null) return result;
                    return result.success(includeExpression);
                }

                return result.failure(new invalidGrammarError("Expected 'g', 'd', 'v', 'f', 'c', 'w', 't', 'fd', 'sd', 'od' or 'i'", currentToken.startPos, currentToken.endPos));
            }

            private parseResult quickIfExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                List<node[]> cases = new List<node[]>();
                node? elseCase = null;

                if (!currentToken.matchString(TOKENTYPE.QEY, "f"))
                    return result.failure(new invalidGrammarError("Expected 'f'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node condition = result.register(this.expression());
                if (result.error != null) return result;

                if (currentToken.type != TOKENTYPE.COLON)
                    return result.failure(new invalidGrammarError("Expected ':'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    result.registerAdvance();
                    advance();

                    bodyNode = result.register(this.statements());
                    if (result.error != null) return result;
                    cases.Add(new node[2] { condition, bodyNode });

                    if (currentToken.matchString(TOKENTYPE.QEY, "s"))
                    {
                        result.registerAdvance();
                        advance();
                    }
                    else
                    {
                        if (!currentToken.matchString(TOKENTYPE.QEY, "l") && !currentToken.matchString(TOKENTYPE.QEY, "e"))
                            return result.failure(new invalidGrammarError("Expected 'l', 'e' or 's'", currentToken.startPos, currentToken.endPos));

                        while (currentToken.matchString(TOKENTYPE.QEY, "l"))
                        {
                            result.registerAdvance();
                            advance();

                            condition = result.register(this.expression());
                            if (result.error != null) return result;

                            if (currentToken.type != TOKENTYPE.COLON)
                                return result.failure(new invalidGrammarError("Expected ':'", currentToken.startPos, currentToken.endPos));
                            result.registerAdvance();
                            advance();

                            bodyNode = result.register(this.statements());
                            if (result.error != null) return result;
                            cases.Add(new node[2] { condition, bodyNode });
                        }

                        if (currentToken.matchString(TOKENTYPE.QEY, "e"))
                        {
                            result.registerAdvance();
                            advance();

                            elseCase = result.register(this.statements());
                            if (result.error != null) return result;
                        }

                        if (!currentToken.matchString(TOKENTYPE.QEY, "s"))
                        {
                            if (elseCase == null)
                                return result.failure(new invalidGrammarError("Expected 'e' or 's'", currentToken.startPos, currentToken.endPos));
                            return result.failure(new invalidGrammarError("Expected 's'", currentToken.startPos, currentToken.endPos));
                        }
                        result.registerAdvance();
                        advance();
                    }

                    return result.success(new ifNode(cases.ToArray(), elseCase, true, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(statement());
                if (result.error != null) return result;
                cases.Add(new node[2] { condition, bodyNode });

                while (currentToken.matchString(TOKENTYPE.QEY, "l"))
                {
                    result.registerAdvance();
                    advance();

                    condition = result.register(this.expression());
                    if (result.error != null) return result;

                    if (currentToken.type != TOKENTYPE.COLON)
                        return result.failure(new invalidGrammarError("Expected ':'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    bodyNode = result.register(this.statement());
                    if (result.error != null) return result;
                    cases.Add(new node[2] { condition, bodyNode });
                }

                if (currentToken.matchString(TOKENTYPE.QEY, "e"))
                {
                    result.registerAdvance();
                    advance();

                    elseCase = result.register(this.statement());
                    if (result.error != null) return result;
                }

                return result.success(new ifNode(cases.ToArray(), elseCase, false, startPos, currentToken.endPos.copy()));
            }

            private parseResult quickCountExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (!currentToken.matchString(TOKENTYPE.QEY, "c"))
                    return result.failure(new invalidGrammarError("Expected 'c'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node startOrEnd = result.register(expression());
                if (result.error != null) return result;

                node? start = null;
                node end;

                if (currentToken.type == TOKENTYPE.ARROW)
                {
                    result.registerAdvance();
                    advance();

                    end = result.register(expression());
                    if (result.error != null) return result;
                    start = startOrEnd;
                }
                else
                    end = startOrEnd;

                node? step = null;
                if (currentToken.matchString(TOKENTYPE.QEY, "t"))
                {
                    result.registerAdvance();
                    advance();

                    step = result.register(expression());
                    if (result.error != null) return result;
                }

                token? variableName = null;
                if (currentToken.matchString(TOKENTYPE.QEY, "n"))
                {
                    result.registerAdvance();
                    advance();

                    if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                    variableName = currentToken;
                    result.registerAdvance();
                    advance();
                }

                if (currentToken.type != TOKENTYPE.COLON)
                {
                    if (start == null && step == null && variableName == null)
                        return result.failure(new invalidGrammarError("Expected '->', 't', 'n' or ':'", currentToken.startPos, currentToken.endPos));
                    if (step == null && variableName == null)
                        return result.failure(new invalidGrammarError("Expected 't', 'n' or ':'", currentToken.startPos, currentToken.endPos));
                    else if (variableName == null)
                        return result.failure(new invalidGrammarError("Expected 'n' or ':'", currentToken.startPos, currentToken.endPos));
                    return result.failure(new invalidGrammarError("Expected ':'", currentToken.startPos, currentToken.endPos));
                }
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    result.registerAdvance();
                    advance();

                    bodyNode = result.register(statements());
                    if (result.error != null) return result;

                    if (!currentToken.matchString(TOKENTYPE.QEY, "s"))
                        return result.failure(new invalidGrammarError("Expected 's'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    return result.success(new countNode(variableName, start, end, step, bodyNode, true, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(statement());
                if (result.error != null) return result;

                return result.success(new countNode(variableName, start, end, step, bodyNode, false, startPos, currentToken.endPos.copy()));
            }

            private parseResult quickWhileExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (!currentToken.matchString(TOKENTYPE.QEY, "w"))
                    return result.failure(new invalidGrammarError("Expected 'w'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node condition = result.register(this.expression());
                if (result.error != null) return result;

                if (currentToken.type != TOKENTYPE.COLON)
                    return result.failure(new invalidGrammarError("Expected ':'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    result.registerAdvance();
                    advance();

                    bodyNode = result.register(statements());
                    if (result.error != null) return result;

                    if (!currentToken.matchString(TOKENTYPE.QEY, "s"))
                        return result.failure(new invalidGrammarError("Expected 's'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    return result.success(new whileNode(condition, bodyNode, true, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(statement());
                if (result.error != null) return result;

                return result.success(new whileNode(condition, bodyNode, false, startPos, currentToken.endPos.copy()));
            }

            private parseResult quickTryExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();
                List<object?[]> catches = new List<object?[]>();

                if (!currentToken.matchString(TOKENTYPE.QEY, "t"))
                    return result.failure(new invalidGrammarError("Expected 't'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    result.registerAdvance();
                    advance();

                    bodyNode = result.register(statements());
                    if (result.error != null) return result;

                    int emptyCatches_ = 0;
                    int emptyCatchIndex_ = -1;
                    while (currentToken.matchString(TOKENTYPE.QEY, "e"))
                    {
                        result.registerAdvance();
                        advance();

                        token? error = null;
                        token? varName = null;
                        if (currentToken.type == TOKENTYPE.STRING || currentToken.type == TOKENTYPE.CHARLIST)
                        {
                            error = currentToken;
                            result.registerAdvance();
                            advance();
                        }
                        else if (currentToken.type == TOKENTYPE.ID || currentToken.type == TOKENTYPE.QEY)
                        {
                            error = currentToken;
                            error.type = TOKENTYPE.ID;
                            result.registerAdvance();
                            advance();
                        }

                        if (currentToken.type == TOKENTYPE.ARROW)
                        {
                            result.registerAdvance();
                            advance();

                            if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                                return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));

                            varName = currentToken;
                            result.registerAdvance();
                            advance();
                        }

                        if (currentToken.type != TOKENTYPE.COLON)
                        {
                            if (error == null && varName == null)
                                return result.failure(new invalidGrammarError("Expected [STRING], [CHARACTER-LIST], [IDENTIFIER], '->' or ':'", currentToken.startPos, currentToken.endPos));
                            else if (varName == null)
                                return result.failure(new invalidGrammarError("Expected '->' or ':'", currentToken.startPos, currentToken.endPos));
                            return result.failure(new invalidGrammarError("Expected ':'", currentToken.startPos, currentToken.endPos));
                        }
                        result.registerAdvance();
                        advance();

                        node body = result.register(statements());
                        if (result.error != null) return result;

                        if (error == null)
                        {
                            emptyCatches_++;
                            emptyCatchIndex_ = catches.Count;
                        }

                        catches.Add(new object?[3] { error, varName, body });
                    }

                    if (emptyCatches_ > 0)
                    {
                        if (emptyCatches_ > 1)
                            return result.failure(new invalidGrammarError("There cannot be more than one empty 'error' statements", startPos, currentToken.endPos));
                        if (emptyCatchIndex_ != catches.Count - 1)
                            return result.failure(new invalidGrammarError("Empty 'error' statements should always be declared last", startPos, currentToken.endPos));
                    }

                    if (!currentToken.matchString(TOKENTYPE.QEY, "s"))
                        return result.failure(new invalidGrammarError("Expected 's'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    return result.success(new tryNode(bodyNode, catches.ToArray(), true, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(statement());
                if (result.error != null) return result;

                int emptyCatches = 0;
                int emptyCatchIndex = -1;
                while (currentToken.matchString(TOKENTYPE.QEY, "e"))
                {
                    result.registerAdvance();
                    advance();

                    token? error = null;
                    token? varName = null;
                    if (currentToken.type == TOKENTYPE.STRING || currentToken.type == TOKENTYPE.CHARLIST)
                    {
                        error = currentToken;
                        result.registerAdvance();
                        advance();
                    }
                    else if (currentToken.type == TOKENTYPE.ID || currentToken.type == TOKENTYPE.QEY)
                    {
                        error = currentToken;
                        error.type = TOKENTYPE.ID;
                        result.registerAdvance();
                        advance();
                    }

                    if (currentToken.type == TOKENTYPE.ARROW)
                    {
                        result.registerAdvance();
                        advance();

                        if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                            return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));

                        varName = currentToken;
                        result.registerAdvance();
                        advance();
                    }

                    if (currentToken.type != TOKENTYPE.COLON)
                    {
                        if (error == null && varName == null)
                            return result.failure(new invalidGrammarError("Expected [STRING], [CHARACTER-LIST], [IDENTIFIER], '->' or ':'", currentToken.startPos, currentToken.endPos));
                        else if (varName == null)
                            return result.failure(new invalidGrammarError("Expected '->' or ':'", currentToken.startPos, currentToken.endPos));
                        return result.failure(new invalidGrammarError("Expected ':'", currentToken.startPos, currentToken.endPos));
                    }
                    result.registerAdvance();
                    advance();

                    node expression = result.register(statement());
                    if (result.error != null) return result;

                    if (error == null)
                    {
                        emptyCatches++;
                        emptyCatchIndex = catches.Count;
                    }

                    catches.Add(new object?[3] { error, varName, expression });
                }

                if (emptyCatches > 0)
                {
                    if (emptyCatches > 1)
                        return result.failure(new invalidGrammarError("There cannot be more than one empty 'error' statements", startPos, currentToken.endPos));
                    if (emptyCatchIndex != catches.Count - 1)
                        return result.failure(new invalidGrammarError("Empty 'error' statements should always be declared last", startPos, currentToken.endPos));
                }

                return result.success(new tryNode(bodyNode, catches.ToArray(), false, startPos, currentToken.endPos.copy()));
            }

            private parseResult quickFunctionExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (!currentToken.matchString(TOKENTYPE.QEY, "fd"))
                    return result.failure(new invalidGrammarError("Expected 'fd'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                token? varName = null;
                if (currentToken.type == TOKENTYPE.ID || currentToken.type == TOKENTYPE.QEY)
                {
                    varName = currentToken;
                    result.registerAdvance();
                    advance();
                }

                List<token> argNames = new List<token>();
                if (currentToken.matchString(TOKENTYPE.QEY, "n"))
                {
                    result.registerAdvance();
                    advance();

                    if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                    argNames.Add(currentToken);
                    result.registerAdvance();
                    advance();

                    while (currentToken.type == TOKENTYPE.COMMA)
                    {
                        result.registerAdvance();
                        advance();

                        if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                            return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                        argNames.Add(currentToken);
                        result.registerAdvance();
                        advance();
                    }
                }

                if (currentToken.type != TOKENTYPE.COLON)
                {
                    if (argNames.Count == 0 && varName == null)
                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER], 'n' or ':'", currentToken.startPos, currentToken.endPos));
                    else if (argNames.Count == 0)
                        return result.failure(new invalidGrammarError("Expected 'n' or ':'", currentToken.startPos, currentToken.endPos));
                    return result.failure(new invalidGrammarError("Expected ':'", currentToken.startPos, currentToken.endPos));
                }
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    bodyNode = result.register(statements());
                    if (result.error != null) return result;

                    if (!currentToken.matchString(TOKENTYPE.QEY, "s"))
                        return result.failure(new invalidGrammarError("Expected 's'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    return result.success(new functionDefinitionNode(varName, argNames.ToArray(), bodyNode, true, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(expression());
                if (result.error != null) return result;
                return result.success(new functionDefinitionNode(varName, argNames.ToArray(), bodyNode, false, startPos, currentToken.endPos.copy()));
            }

            private parseResult quickSpecialExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (!currentToken.matchString(TOKENTYPE.QEY, "sd"))
                    return result.failure(new invalidGrammarError("Expected 'sd'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                    return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                token varName = currentToken;
                result.registerAdvance();
                advance();

                if (currentToken.type != TOKENTYPE.COLON)
                    return result.failure(new invalidGrammarError("Expected ':'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    bodyNode = result.register(statements());
                    if (result.error != null) return result;

                    if (!currentToken.matchString(TOKENTYPE.QEY, "s"))
                        return result.failure(new invalidGrammarError("Expected 's'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    return result.success(new specialDefinitionNode(varName, bodyNode, true, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(expression());
                if (result.error != null) return result;

                return result.success(new specialDefinitionNode(varName, bodyNode, false, startPos, currentToken.endPos.copy()));
            }

            private parseResult quickObjectExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (!currentToken.matchString(TOKENTYPE.QEY, "od"))
                    return result.failure(new invalidGrammarError("Expected 'od'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                token? inheritFrom = null;
                if (currentToken.type == TOKENTYPE.ID || currentToken.type == TOKENTYPE.QEY)
                {
                    inheritFrom = currentToken;
                    result.registerAdvance();
                    advance();
                }

                token varName;
                if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                {
                    if (inheritFrom == null)
                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                    else
                    {
                        varName = inheritFrom;
                        inheritFrom = null;
                    }
                }
                else
                {
                    varName = currentToken;
                    result.registerAdvance();
                    advance();
                }

                List<token> argNames = new List<token>();
                if (currentToken.matchString(TOKENTYPE.QEY, "n"))
                {
                    result.registerAdvance();
                    advance();

                    if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                    argNames.Add(currentToken);
                    result.registerAdvance();
                    advance();

                    while (currentToken.type == TOKENTYPE.COMMA)
                    {
                        result.registerAdvance();
                        advance();

                        if (currentToken.type != TOKENTYPE.ID && currentToken.type != TOKENTYPE.QEY)
                            return result.failure(new invalidGrammarError("Expected [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                        argNames.Add(currentToken);
                        result.registerAdvance();
                        advance();
                    }
                }

                if (currentToken.type != TOKENTYPE.COLON)
                {
                    if (inheritFrom == null && argNames.Count == 0)
                        return result.failure(new invalidGrammarError("Expected [IDENTIFIER], 'n' or ':'", currentToken.startPos, currentToken.endPos));
                    if (argNames.Count == 0)
                        return result.failure(new invalidGrammarError("Expected 'n' or ':'", currentToken.startPos, currentToken.endPos));
                    return result.failure(new invalidGrammarError("Expected ',' or ':'", currentToken.startPos, currentToken.endPos));
                }
                result.registerAdvance();
                advance();

                node bodyNode;
                if (currentToken.type == TOKENTYPE.NEWLINE)
                {
                    bodyNode = result.register(statements());
                    if (result.error != null) return result;

                    if (!currentToken.matchString(TOKENTYPE.QEY, "s"))
                        return result.failure(new invalidGrammarError("Expected 's'", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();

                    return result.success(new objectDefinitionNode(varName, inheritFrom, argNames.ToArray(), bodyNode, startPos, currentToken.startPos.copy()));
                }

                bodyNode = result.register(expression());
                if (result.error != null) return result;
                return result.success(new objectDefinitionNode(varName, inheritFrom, argNames.ToArray(), bodyNode, startPos, currentToken.endPos.copy()));
            }

            private parseResult quickIncludeExpression()
            {
                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                if (!currentToken.matchString(TOKENTYPE.QEY, "i"))
                    return result.failure(new invalidGrammarError("Expected 'i'", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                token file;
                if (currentToken.type == TOKENTYPE.STRING || currentToken.type == TOKENTYPE.CHARLIST)
                    file = currentToken;
                else if (currentToken.type == TOKENTYPE.ID || currentToken.type == TOKENTYPE.QEY)
                {
                    file = currentToken;
                    file.type = TOKENTYPE.ID;
                }
                else
                    return result.failure(new invalidGrammarError("Expected [STRING], [CHARACTER-LIST] or [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                result.registerAdvance();
                advance();

                token? nickname = null;
                if (currentToken.matchString(TOKENTYPE.QEY, "n"))
                {
                    result.registerAdvance();
                    advance();

                    if (currentToken.type == TOKENTYPE.STRING || currentToken.type == TOKENTYPE.CHARLIST)
                        nickname = currentToken;
                    else if (currentToken.type == TOKENTYPE.ID || currentToken.type == TOKENTYPE.QEY)
                    {
                        nickname = currentToken;
                        nickname.type = TOKENTYPE.ID;
                    }
                    else
                        return result.failure(new invalidGrammarError("Expected [STRING], [CHARACTER-LIST] or [IDENTIFIER]", currentToken.startPos, currentToken.endPos));
                    result.registerAdvance();
                    advance();
                }

                return result.success(new includeNode(file, nickname, startPos, currentToken.startPos.copy()));
            }

            private struct TypeValuePair
            {
                public TOKENTYPE type;
                public object? value;

                public TypeValuePair(TOKENTYPE type)
                {
                    this.type = type;
                    this.value = null;
                }

                public TypeValuePair(TOKENTYPE type, string? value)
                {
                    this.type = type;
                    this.value = value;
                }
            }

            private parseResult binaryOperation(Func<parseResult> funcA, TypeValuePair[] operations, Func<parseResult>? funcB = null)
            {
                if (funcB == null) funcB = funcA;

                parseResult result = new parseResult();
                position startPos = currentToken.startPos.copy();

                node left = result.register(funcA.Invoke());
                if (result.error != null) return result;

                while (true)
                {
                    bool changed = false;
                    for (int i = 0; i < operations.Length; i++)
                    {
                        if (operations[i].type == currentToken.type && ((operations[i].value != null && currentToken.value.ToString() == operations[i].value.ToString()) || (operations[i].value == null)))
                        {
                            token operatorToken = currentToken;
                            if (operatorToken.type == TOKENTYPE.QEY)
                                operatorToken.type = TOKENTYPE.KEY;

                            result.registerAdvance();
                            advance();

                            node right = result.register(funcB.Invoke());
                            if (result.error != null) return result;

                            left = new binaryOperationNode(left, right, operatorToken, startPos, currentToken.endPos.copy());
                            changed = true;
                        }
                    }

                    if (!changed)
                        break;
                }

                return result.success(left);
            }
        }

        public class interpreter
        {
            public interpreter() { }

            public runtimeResult visit(node node, context context)
            {
                string nodeName = node.GetType().Name;
                string methodName = $"visit_{nodeName}";
                MethodInfo? info = GetType().GetMethod(methodName, (BindingFlags.NonPublic | BindingFlags.Instance));
                if (info != null)
                {
                    object result = info.Invoke(this, new object[] { node, context });
                    return (runtimeResult)result;
                }

                throw new Exception($"No {methodName} method defined!");
            }

            private runtimeResult visit_numberNode(numberNode node, context context)
            {
                if (node.valueToken.type == TOKENTYPE.INT)
                    return new runtimeResult().success(new integer((int)node.valueToken.value).setPosition(node.startPos, node.endPos).setContext(context));
                return new runtimeResult().success(new @float((float)node.valueToken.value).setPosition(node.startPos, node.endPos).setContext(context));
            }

            private runtimeResult visit_stringNode(stringNode node, context context)
            {
                return new runtimeResult().success(new @string(node.valueToken.value.ToString()).setPosition(node.startPos, node.endPos).setContext(context));
            }

            private runtimeResult visit_charListNode(charListNode node, context context)
            {
                return new runtimeResult().success(new character_list(node.valueToken.value.ToString()).setPosition(node.startPos, node.endPos).setContext(context));
            }

            private runtimeResult visit_arrayNode(arrayNode node, context context)
            {
                runtimeResult result = new runtimeResult();
                item[] elements = new item[node.elementNodes.Length];

                for (int i = 0; i < elements.Length; i++)
                {
                    elements[i] = result.register(visit(node.elementNodes[i], context));
                    if (result.shouldReturn()) return result;
                }

                return result.success(new array(elements).setPosition(node.startPos, node.endPos).setContext(context));
            }

            private runtimeResult visit_listNode(listNode node, context context)
            {
                runtimeResult result = new runtimeResult();
                item[] elements = new item[node.elementNodes.Length];

                for (int i = 0; i < elements.Length; i++)
                {
                    elements[i] = result.register(visit(node.elementNodes[i], context));
                    if (result.shouldReturn()) return result;
                }

                return result.success(new list(elements).setPosition(node.startPos, node.endPos).setContext(context));
            }

            private runtimeResult visit_dictionaryNode(dictionaryNode node, context context)
            {
                runtimeResult result = new runtimeResult();
                ItemDictionary dictionary_ = new ItemDictionary();

                for (int i = 0; i < node.nodePairs.Length; i++)
                {
                    item key = result.register(visit(node.nodePairs[i][0], context));
                    if (result.shouldReturn()) return result;
                    
                    item value = result.register(visit(node.nodePairs[i][1], context));
                    if (result.shouldReturn()) return result;

                    dictionary_.Add(key, value, out error? error);
                    if (error != null) return result.failure(error);
                }

                return result.success(new dictionary(dictionary_).setPosition(node.startPos, node.endPos).setContext(context));
            }

            private runtimeResult visit_variableAccessNode(variableAccessNode node, context context)
            {
                runtimeResult result = new runtimeResult();
                string varName = node.valueToken.value.ToString();

                item? variable;
                if (node.isGlobal)
                {
                    variable = globalPredefinedContext.symbolTable.get(varName);

                    if (variable == null)
                    {
                        context parentContext = context;
                        while (parentContext.parent != null && !parentContext.parent.locked)
                            parentContext = parentContext.parent;
                        variable = parentContext.symbolTable.get(varName);
                    }
                }
                else
                    variable = context.symbolTable.get(varName);

                if (variable == null)
                    return result.failure(new runtimeError(node.valueToken.startPos, node.valueToken.endPos, RT_UNDEFINED, $"\"{varName}\" is not defined", context));
                return result.success(variable.copy().setPosition(node.startPos, node.endPos).setContext(context));
            }

            private runtimeResult visit_simpleVariableAssignNode(simpleVariableAssignNode node, context context)
            {
                runtimeResult result = new runtimeResult();
                string varName = node.nameToken.value.ToString();

                item variable = result.register(visit(node.valueNode, context));
                if (result.shouldReturn()) return result;

                if (node.operatorToken != null && node.operatorToken.type != TOKENTYPE.COLON)
                {
                    item? oldVariableValue = context.symbolTable.get(varName);
                    if (oldVariableValue == null)
                        return result.failure(new runtimeError(node.nameToken.startPos, node.nameToken.endPos, RT_UNDEFINED, $"\"{varName}\" is not defined", context));

                    error? err = null;
                    item? newVariable = null;
                    if (node.operatorToken.type == TOKENTYPE.ASSIGNPLUS)
                        newVariable = oldVariableValue.addedTo(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNMINUS)
                        newVariable = oldVariableValue.subbedBy(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNMUL)
                        newVariable = oldVariableValue.multedBy(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNDIV)
                        newVariable = oldVariableValue.divedBy(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNMOD)
                        newVariable = oldVariableValue.modedBy(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNPOW)
                        newVariable = oldVariableValue.powedBy(variable, out err);

                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNBITAND)
                        newVariable = oldVariableValue.bitwiseAndedTo(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNBITOR)
                        newVariable = oldVariableValue.bitwiseOrdTo(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNBITXOR)
                        newVariable = oldVariableValue.bitwiseXOrdTo(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNBITLSHIFT)
                        newVariable = oldVariableValue.bitwiseLeftShiftedTo(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNBITRSHIFT)
                        newVariable = oldVariableValue.bitwiseRightShiftedTo(variable, out err);

                    if (err != null) return result.failure(err);
                    variable = newVariable;
                }

                if (node.isGlobal)
                {
                    context parentContext = context;
                    while (parentContext.parent != null && !parentContext.parent.locked)
                        parentContext = parentContext.parent;
                    parentContext.symbolTable.set(varName, variable);
                }
                else
                    context.symbolTable.set(varName, variable);
                return result.success(variable);
            }

            private runtimeResult visit_objectVariableAssignNode(objectVariableAssignNode node, context context)
            {
                runtimeResult result = new runtimeResult();
                string varName = node.varName.value.ToString();

                item variable = result.register(visit(node.valueNode, context));
                if (result.shouldReturn()) return result;

                binaryOperationNode binOpNode = (binaryOperationNode)node.accessNode;
                item object_ = result.register(visit(binOpNode.leftNode, context));
                if (result.shouldReturn()) return result;

                if (object_ is value)
                    object_ = result.register(object_.execute(new item[0]));
                if (result.shouldReturn()) return result;

                if (node.operatorToken != null && node.operatorToken.type != TOKENTYPE.COLON)
                {
                    item? oldVariableValue = result.register(object_.get(binOpNode.rightNode));
                    if (result.shouldReturn()) return result;

                    error? err = null;
                    item? newVariable = null;
                    if (node.operatorToken.type == TOKENTYPE.ASSIGNPLUS)
                        newVariable = oldVariableValue.addedTo(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNMINUS)
                        newVariable = oldVariableValue.subbedBy(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNMUL)
                        newVariable = oldVariableValue.multedBy(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNDIV)
                        newVariable = oldVariableValue.divedBy(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNMOD)
                        newVariable = oldVariableValue.modedBy(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNPOW)
                        newVariable = oldVariableValue.powedBy(variable, out err);

                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNBITAND)
                        newVariable = oldVariableValue.bitwiseAndedTo(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNBITOR)
                        newVariable = oldVariableValue.bitwiseOrdTo(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNBITXOR)
                        newVariable = oldVariableValue.bitwiseXOrdTo(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNBITLSHIFT)
                        newVariable = oldVariableValue.bitwiseLeftShiftedTo(variable, out err);
                    else if (node.operatorToken.type == TOKENTYPE.ASSIGNBITRSHIFT)
                        newVariable = oldVariableValue.bitwiseRightShiftedTo(variable, out err);

                    if (err != null) return result.failure(err);
                    variable = newVariable;
                }

                if (node.isGlobal)
                    return result.failure(new runtimeError(node.startPos, node.endPos, RT_UNDEFINED, "This type of variable assignment does not support globalization", context));
                else
                {
                    result.register(object_.set(varName, variable));
                    if (result.shouldReturn()) return result;
                }
                return result.success(variable);
            }

            private runtimeResult visit_binaryOperationNode(binaryOperationNode node, context context)
            {
                runtimeResult result = new runtimeResult();
                item left = result.register(visit(node.leftNode, context));
                if (result.shouldReturn()) return result;

                if (node.operatorToken.type == TOKENTYPE.PERIOD)
                {
                    left = left.copy().setPosition(node.startPos, node.endPos);
                    if (left is value)
                        left = result.register(left.execute(new item[0]));
                    if (result.shouldReturn()) return result;

                    item returnValue = result.register(left.get(node.rightNode));
                    if (result.shouldReturn()) return result;

                    return result.success(returnValue.copy().setPosition(node.startPos, node.endPos).setContext(context));
                }

                item right = result.register(visit(node.rightNode, context));
                if (result.shouldReturn()) return result;

                error? err = null;
                item? res = null;
                if (node.operatorToken.type == TOKENTYPE.BITOR)
                    res = left.bitwiseOrdTo(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.BITXOR)
                    res = left.bitwiseXOrdTo(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.BITAND)
                    res = left.bitwiseAndedTo(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.BITLSHIFT)
                    res = left.bitwiseLeftShiftedTo(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.BITRSHIFT)
                    res = left.bitwiseRightShiftedTo(right, out err);

                else if (node.operatorToken.type == TOKENTYPE.PLUS)
                    res = left.addedTo(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.MINUS)
                    res = left.subbedBy(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.MUL)
                    res = left.multedBy(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.DIV)
                    res = left.divedBy(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.MOD)
                    res = left.modedBy(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.POW)
                    res = left.powedBy(right, out err);

                else if (node.operatorToken.type == TOKENTYPE.ISEQUAL)
                    res = left.compareEqual(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.NOTEQUAL)
                    res = left.compareNotEqual(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.LESSTHAN)
                    res = left.compareLessThan(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.GREATERTHAN)
                    res = left.compareGreaterThan(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.LESSTHANOREQUAL)
                    res = left.compareLessThanOrEqual(right, out err);
                else if (node.operatorToken.type == TOKENTYPE.GREATERTHANOREQUAL)
                    res = left.compareGreaterThanOrEqual(right, out err);
                else if (node.operatorToken.matchString(TOKENTYPE.KEY, "and"))
                    res = left.compareAnd(right, out err);
                else if (node.operatorToken.matchString(TOKENTYPE.KEY, "or"))
                    res = left.compareOr(right, out err);

                else if (node.operatorToken.matchString(TOKENTYPE.KEY, "in"))
                    res = left.checkIn(right, out err);

                if (err != null) return result.failure(err);
                return result.success(res.setPosition(node.startPos, node.endPos));
            }

            private runtimeResult visit_unaryOperationNode(unaryOperationNode node, context context)
            {
                runtimeResult result = new runtimeResult();
                item variable = result.register(visit(node.operatedNode, context));
                if (result.shouldReturn()) return result;

                error? err = null;
                item? res = null;
                if (node.operatorToken.type == TOKENTYPE.BITNOT)
                    res = variable.bitwiseNotted(out err);

                else if (node.operatorToken.type == TOKENTYPE.MINUS)
                    res = variable.multedBy(new integer(-1), out err);
                else if (node.operatorToken.type == TOKENTYPE.PLUS)
                    res = variable.multedBy(new integer(1), out err);

                else if (node.operatorToken.matchString(TOKENTYPE.KEY, "invert") || node.operatorToken.matchString(TOKENTYPE.KEY, "v"))
                    res = variable.invert(out err);

                if (err != null) return result.failure(err);
                return result.success(res.setPosition(node.startPos, node.endPos));
            }

            private runtimeResult visit_ifNode(ifNode node, context context)
            {
                runtimeResult result = new runtimeResult();

                for (int i = 0; i < node.cases.Length; i++)
                {
                    item condition = result.register(visit(node.cases[i][0], context));
                    if (result.shouldReturn()) return result;

                    bool boolValue = condition.isTrue(out error? error);
                    if (error != null) return result.failure(error);

                    if (boolValue)
                    {
                        item value = result.register(visit(node.cases[i][1], context));
                        if (result.shouldReturn()) return result;

                        return result.success(node.shouldReturnNull ? new nothing().setPosition(node.startPos, node.endPos).setContext(context) : value);
                    }
                }

                if (node.elseCase != null)
                {
                    item value = result.register(visit(node.elseCase, context));
                    if (result.shouldReturn()) return result;

                    return result.success(node.shouldReturnNull ? new nothing().setPosition(node.startPos, node.endPos).setContext(context) : value);
                }

                return result.success(new nothing().setPosition(node.startPos, node.endPos).setContext(context));
            }

            private runtimeResult visit_countNode(countNode node, context context)
            {
                runtimeResult result = new runtimeResult();
                List<item> elements = new List<item>();
                dynamic i = 0;
                dynamic length = 0;
                dynamic step_ = 1;

                if (node.startValueNode != null)
                {
                    item start = result.register(visit(node.startValueNode, context));
                    if (result.shouldReturn()) return result;

                    if (start is not integer && start is not @float)
                        return result.failure(new runtimeError(start.startPos, start.endPos, RT_TYPE, "Count loop start must be an integer or float", context));

                    if (start is integer)
                        i = ((integer)start).storedValue;
                    else
                        i = ((@float)start).storedValue;
                }

                item end = result.register(visit(node.endValueNode, context));
                if (result.shouldReturn()) return result;

                if (end is not integer && end is not @float)
                    return result.failure(new runtimeError(end.startPos, end.endPos, RT_TYPE, "Count loop end must be an integer or float", context));

                if (end is integer)
                    length = ((integer)end).storedValue;
                else
                    length = ((@float)end).storedValue;

                if (node.stepValueNode != null)
                {
                    item step = result.register(visit(node.stepValueNode, context));
                    if (result.shouldReturn()) return result;

                    if (step is not integer && step is not @float)
                        return result.failure(new runtimeError(step.startPos, step.endPos, RT_TYPE, "Count loop step must be an integer or float", context));

                    if (step is integer)
                        step_ = ((integer)step).storedValue;
                    else
                        step_ = ((@float)step).storedValue;
                }

                string? varName = null;
                if (node.variableNameToken != null)
                    varName = node.variableNameToken.value.ToString();

                for (var j = i; j < length; j += step_)
                {
                    if (varName != null)
                    {
                        if (step_ is float)
                            context.symbolTable.set(varName, new @float(j));
                        else if (step_ is int)
                                context.symbolTable.set(varName, new integer((int)j));
                    }

                    item body = result.register(visit(node.bodyNode, context));
                    if (result.shouldReturn() && !result.loopShouldSkip && !result.loopShouldStop) return result;

                    if (result.loopShouldSkip) continue;
                    if (result.loopShouldStop) break;

                    elements.Add(body);
                }

                return result.success(node.shouldReturnNull ? new nothing().setPosition(node.startPos, node.endPos).setContext(context) : new array(elements.ToArray()).setPosition(node.startPos, node.endPos).setContext(context));
            }

            private runtimeResult visit_whileNode(whileNode node, context context)
            {
                runtimeResult result = new runtimeResult();
                List<item> elements = new List<item>();

                while (true)
                {
                    item condition = result.register(visit(node.conditionNode, context));
                    if (result.shouldReturn()) return result;

                    bool boolValue = condition.isTrue(out error? error);
                    if (error != null) return result.failure(error);
                    if (!boolValue) break;

                    item body = result.register(visit(node.bodyNode, context));
                    if (result.shouldReturn() && !result.loopShouldSkip && !result.loopShouldStop) return result;

                    if (result.loopShouldSkip) continue;
                    if (result.loopShouldStop) break;

                    elements.Add(body);
                }

                return result.success(node.shouldReturnNull ? new nothing().setPosition(node.startPos, node.endPos).setContext(context) : new array(elements.ToArray()).setPosition(node.startPos, node.endPos).setContext(context));
            }

            private runtimeResult visit_tryNode(tryNode node, context context)
            {
                runtimeResult result = new runtimeResult();

                item value = result.register(visit(node.bodyNode, context));
                if (result.shouldReturn() && result.error == null) return result;

                if (result.error != null)
                {
                    string tag = result.error.name;
                    value = new nothing();
                    result.reset();

                    if (node.catches.Length > 0)
                    {
                        for (int i = 0; i < node.catches.Length; i++)
                        {
                            if (node.catches[i][0] == null)
                            {
                                if (node.catches[i][1] != null)
                                    context.symbolTable.set(((token)node.catches[i][1]).value.ToString(), new @string(tag).setPosition(node.startPos, node.endPos).setContext(context));

                                value = result.register(visit((node)node.catches[i][2], context));
                                if (result.shouldReturn()) return result;
                                break;
                            }
                            else
                            {
                                token catchTagToken = (token)node.catches[i][0];
                                string catchTag = string.Empty;
                                if (catchTagToken.type == TOKENTYPE.ID)
                                {
                                    item? tag_ = context.symbolTable.get(catchTagToken.value.ToString());
                                    if (tag_ == null)
                                        return result.failure(new runtimeError(catchTagToken.startPos, catchTagToken.endPos, RT_UNDEFINED, $"\"{catchTagToken.value}\" is not defined", context));
                                    else if (tag_ is not @string)
                                        return result.failure(new runtimeError(catchTagToken.startPos, catchTagToken.endPos, RT_UNDEFINED, $"Error tag must be a string", context));
                                    catchTag = ((@string)tag_).storedValue;
                                }
                                else
                                    catchTag = catchTagToken.value.ToString();

                                if (catchTag == tag || catchTag == RT_DEFAULT)
                                {
                                    if (node.catches[i][1] != null)
                                        context.symbolTable.set(((token)node.catches[i][1]).value.ToString(), new @string(tag).setPosition(node.startPos, node.endPos).setContext(context));

                                    value = result.register(visit((node)node.catches[i][2], context));
                                    if (result.shouldReturn()) return result;
                                    break;
                                }
                            }
                        }
                    }
                }

                return result.success(node.shouldReturnNull ? new nothing().setPosition(node.startPos, node.endPos).setContext(context) : value.setPosition(node.startPos, node.endPos).setContext(context));
            }

            private runtimeResult visit_skipNode(skipNode node, context context)
            {
                return new runtimeResult().skipSuccess();
            }

            private runtimeResult visit_stopNode(stopNode node, context context)
            {
                return new runtimeResult().stopSuccess();
            }

            private runtimeResult visit_functionDefinitionNode(functionDefinitionNode node, context context)
            {
                runtimeResult result = new runtimeResult();

                string? name = (node.nameToken != null) ? node.nameToken.value.ToString() : null;
                string[] argNames = new string[node.argNameTokens.Length];
                for (int i = 0; i < node.argNameTokens.Length; i++)
                    argNames[i] = node.argNameTokens[i].value.ToString();

                item function = new function(name, node.bodyNode, argNames, node.shouldReturnNull).setPosition(node.startPos, node.endPos).setContext(context);
                if (name != null)
                    context.symbolTable.set(name, function);
                return result.success(function);
            }

            private runtimeResult visit_specialDefinitionNode(specialDefinitionNode node, context context)
            {
                runtimeResult result = new runtimeResult();

                string name = node.nameToken.value.ToString();
                if (!SPECIALS.Keys.Contains(name))
                    return result.failure(new runtimeError(node.nameToken.startPos, node.nameToken.endPos, RT_UNDEFINED, $"Undefined special function", context));

                item special = new special(name, node.bodyNode, SPECIALS[name], node.shouldReturnNull).setPosition(node.startPos, node.endPos).setContext(context);
                context.symbolTable.set(name, special);
                
                return result.success(special);
            }

            private runtimeResult visit_objectDefinitionNode(objectDefinitionNode node, context context)
            {
                runtimeResult result = new runtimeResult();

                string name = node.nameToken.value.ToString();
                string[] argNames = new string[node.argNameTokens.Length];
                for (int i = 0; i < node.argNameTokens.Length; i++)
                    argNames[i] = node.argNameTokens[i].value.ToString();

                @class? inheritFrom = null;
                if (node.inheritanceToken != null)
                {
                    item? parent = context.symbolTable.get(node.inheritanceToken.value.ToString());
                    
                    if (parent == null)
                        return result.failure(new runtimeError(node.inheritanceToken.startPos, node.inheritanceToken.endPos, RT_UNDEFINED, $"\"{node.inheritanceToken.value}\" is not defined", context));
                    else if (parent is not @class)
                        return result.failure(new runtimeError(node.inheritanceToken.startPos, node.inheritanceToken.endPos, RT_TYPE, $"\"{node.inheritanceToken.value}\" is not a class", context));
                    inheritFrom = (@class)parent;

                    for (int i = 0; i < inheritFrom.argNames.Length; i++)
                    {
                        if (!argNames.Contains(inheritFrom.argNames[i]))
                            return result.failure(new runtimeError(node.startPos, node.endPos, RT_TYPE, $"Class definition does not contain parent argument \"{inheritFrom.argNames[i]}\"", context));
                    }
                }

                item @object = new @class(name, inheritFrom, node.bodyNode, argNames).setPosition(node.startPos, node.endPos).setContext(context);
                context.symbolTable.set(name, @object);
                return result.success(@object);
            }

            private runtimeResult visit_callNode(callNode node, context context)
            {
                runtimeResult result = new runtimeResult();
                List<item> args = new List<item>();

                item valueToCall = result.register(visit(node.nodeToCall, context));
                if (result.shouldReturn()) return result;

                valueToCall = valueToCall.copy().setPosition(node.startPos, node.endPos);
                for (int i = 0; i < node.argNodes.Length; i++)
                {
                    args.Add(result.register(visit(node.argNodes[i], context)));
                    if (result.shouldReturn()) return result;
                }

                item returnValue = result.register(valueToCall.execute(args.ToArray()));
                if (result.shouldReturn()) return result;

                return result.success(returnValue.copy().setPosition(node.startPos, node.endPos).setContext(context));
            }

            private runtimeResult visit_returnNode(returnNode node, context context)
            {
                runtimeResult result = new runtimeResult();

                item value = new nothing();
                if (node.nodeToReturn != null)
                {
                    value = result.register(visit(node.nodeToReturn, context));
                    if (result.shouldReturn()) return result;
                }

                return result.returnSuccess(value.setPosition(node.startPos, node.endPos).setContext(context));
            }

            private runtimeResult visit_includeNode(includeNode node, context context)
            {
                runtimeResult result = new runtimeResult();

                string file = node.nameToken.value.ToString();

                string? realFilepath = null;
                if (File.Exists(file))
                    realFilepath = file;
                else
                {
                    for (int i = 0; i < LOCALLIBPATHS.Count; i++)
                    {
                        string path = Path.Join(LOCALLIBPATHS[i], file);
                        if (File.Exists(path))
                        {
                            realFilepath = path;
                            break;
                        }
                    }
                }

                if (realFilepath == null)
                    return result.failure(new runtimeError(node.startPos, node.endPos, RT_IO, $"Script \"{file}\" was not found", context));

                string name;
                if (node.nicknameToken != null)
                {
                    if (node.nicknameToken.type == TOKENTYPE.STRING || node.nicknameToken.type == TOKENTYPE.CHARLIST)
                        name = node.nicknameToken.value.ToString();
                    else
                    {
                        item? nickname = context.symbolTable.get(node.nicknameToken.value.ToString());
                        if (nickname == null)
                            return result.failure(new runtimeError(node.nicknameToken.startPos, node.nicknameToken.endPos, RT_UNDEFINED, $"\"{node.nicknameToken.value}\" is not defined", context));
                        else if (nickname is not @string)
                            return result.failure(new runtimeError(node.nicknameToken.startPos, node.nicknameToken.endPos, RT_UNDEFINED, $"Nickname must be a string", context));
                        name = ((@string)nickname).storedValue;
                    }
                }
                else
                {
                    string? filenameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                    name = (filenameWithoutExtension != null) ? filenameWithoutExtension : file;
                }

                string formattedFileName = "";
                for (int i = 0; i < name.Length; i++)
                {
                    if (!ALPHANUM_UNDERSCORE.Contains(name[i]))
                        formattedFileName += '_';
                    else
                        formattedFileName += name[i];
                }

                string? extension = Path.GetExtension(file);

                item value;
                if (extension == ".dll")
                {
                    Assembly DLL;
                    try
                    {
                        DLL = Assembly.LoadFile(file);
                    }
                    catch (Exception exception)
                    {
                        return result.failure(new runtimeError(node.startPos, node.endPos, RT_IO, $"Failed to load script \"{file}\"\n{exception.Message}", context));
                    }

                    try
                    {
                        Type? mainLibClass = DLL.GetType("Main");
                        if (mainLibClass == null)
                            return result.failure(new runtimeError(node.startPos, node.endPos, RT_IO, $"Could not find main library class in script \"{file}\"", context));

                        dynamic? val = Activator.CreateInstance(mainLibClass);

                        if (val is item)
                        {
                            value = result.register(val.setPosition(node.startPos, node.endPos).setContext(context).execute(new item[0]));
                            if (result.shouldReturn()) return result;
                        }
                        else
                            return result.failure(new runtimeError(node.startPos, node.endPos, RT_IO, $"Could not find main library class in script \"{file}\"", context));
                    }
                    catch (Exception exception)
                    {
                        return result.failure(new runtimeError(node.startPos, node.endPos, RT_IO, $"Failed to finish executing script \"{file}\"\n{exception.Message}", context));
                    }
                }
                else
                {
                    string script;
                    try
                    {
                        script = string.Join('\n', File.ReadAllLines(realFilepath));
                    }
                    catch (Exception exception)
                    {
                        return result.failure(new runtimeError(node.startPos, node.endPos, RT_IO, $"Failed to load script \"{file}\"\n{exception.Message}", context));
                    }

                    token[] tokens = new lexer(file, script).compileTokens(out error? error);
                    if (error != null)
                        return result.failure(new runtimeError(node.startPos, node.endPos, RT_RUN, $"Failed to finish executing script \"{file}\"\n\n{error.asString()}", context));

                    parseResult parseResult = new parser(tokens).parse();
                    if (parseResult.error != null)
                        return result.failure(new runtimeError(node.startPos, node.endPos, RT_RUN, $"Failed to finish executing script \"{file}\"\n\n{parseResult.error.asString()}", context));

                    value = result.register(new @class(formattedFileName, null, parseResult.node, new string[0]).setPosition(node.startPos, node.endPos).setContext(context).execute(new item[0]));
                    if (result.shouldReturn()) return result;

                }

                context.symbolTable.set(formattedFileName, value);
                return result.success(new nothing().setPosition(node.startPos, node.endPos).setContext(context));
            }
        }


        public static context globalPredefinedContext = new context("<GLC>", null, null, true);

        public static List<string> LOCALLIBPATHS = new List<string>();

        public ezr()
        {
            symbolTable predefinedSymbolTable = new symbolTable();
            predefinedSymbolTable.set("nothing", new nothing());
            predefinedSymbolTable.set("true", new boolean(true));
            predefinedSymbolTable.set("false", new boolean(false));

            predefinedSymbolTable.set("version__", new @string(VERSION));

            predefinedSymbolTable.set("err_any", new @string(RT_DEFAULT));
            predefinedSymbolTable.set("err_illop", new @string(RT_ILLEGALOP));
            predefinedSymbolTable.set("err_undef", new @string(RT_UNDEFINED));
            predefinedSymbolTable.set("err_key", new @string(RT_KEY));
            predefinedSymbolTable.set("err_index", new @string(RT_INDEX));
            predefinedSymbolTable.set("err_args", new @string(RT_ARGS));
            predefinedSymbolTable.set("err_type", new @string(RT_TYPE));
            predefinedSymbolTable.set("err_math", new @string(RT_MATH));
            predefinedSymbolTable.set("err_run", new @string(RT_RUN));
            predefinedSymbolTable.set("err_io", new @string(RT_IO));

            predefinedSymbolTable.set("show", new builtin_function("show", new string[1] { "message" }));
            predefinedSymbolTable.set("show_error", new builtin_function("show_error", new string[2] { "tag", "message" }));
            predefinedSymbolTable.set("get", new builtin_function("get", new string[1] { "message" }));
            predefinedSymbolTable.set("clear", new builtin_function("clear", new string[0]));
            predefinedSymbolTable.set("hash", new builtin_function("hash", new string[1] { "value" }));
            predefinedSymbolTable.set("type_of", new builtin_function("type_of", new string[1] { "value" }));
            predefinedSymbolTable.set("run", new builtin_function("run", new string[1] { "file" }));

            position pos = new position(0, 0, 0, "<main>", "");
            predefinedSymbolTable.set("file", new @file().setPosition(pos, pos).setContext(globalPredefinedContext).execute(new item[0]).value);
            predefinedSymbolTable.set("folder", new folder().setPosition(pos, pos).setContext(globalPredefinedContext).execute(new item[0]).value);
            predefinedSymbolTable.set("path", new path().setPosition(pos, pos).setContext(globalPredefinedContext).execute(new item[0]).value);

            predefinedSymbolTable.set("integer", new integer_class().setPosition(pos, pos).setContext(globalPredefinedContext).execute(new item[0]).value);
            predefinedSymbolTable.set("float", new float_class().setPosition(pos, pos).setContext(globalPredefinedContext).execute(new item[0]).value);
            predefinedSymbolTable.set("string", new string_class().setPosition(pos, pos).setContext(globalPredefinedContext).execute(new item[0]).value);
            predefinedSymbolTable.set("character_list", new character_list_class().setPosition(pos, pos).setContext(globalPredefinedContext).execute(new item[0]).value);

            predefinedSymbolTable.set("random", new random().setPosition(pos, pos).setContext(globalPredefinedContext).execute(new item[0]).value);

            globalPredefinedContext.symbolTable = predefinedSymbolTable;
        }

        public error? run(string file, string input, context runtimeContext, out item? result)
        {
            result = null;
            string fullFilePath = Path.GetDirectoryName(Path.GetFullPath(file));
            if (fullFilePath != Directory.GetCurrentDirectory())
                LOCALLIBPATHS.Add(fullFilePath);

            lexer lexer = new lexer(file, input);
            token[] tokens = lexer.compileTokens(out error? error);
            if (error != null) return error;

            parser parser = new parser(tokens);
            parseResult abstractSyntaxTree = parser.parse();
            if (abstractSyntaxTree.error != null) return abstractSyntaxTree.error;

            interpreter interpreter = new interpreter();
            runtimeResult result_ = interpreter.visit(abstractSyntaxTree.node, runtimeContext);
            if (result_.error != null) return result_.error;

            result = result_.value;
            return null;
        }
    }
}