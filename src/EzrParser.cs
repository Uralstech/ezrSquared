using System.Collections.Generic;
using System;

namespace EzrSquared.EzrParser
{
    using EzrCommon;
    using EzrNodes;
    using EzrErrors;

    /// <summary>
    /// The ezrSquared Parser. The job of the Parser is to convert the input <see cref="Token"/> objects from the <see cref="EzrLexer.Lexer"/> into <see cref="Node"/> objects to be given as the input to the Interpreter (TODO).
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// The <see cref="List{T}"/> of <see cref="Token"/> objects to be parsed.
        /// </summary>
        private List<Token> _tokens;

        /// <summary>
        /// The index of the <see cref="Token"/> object currently being parsed in the <see cref="_tokens"/> <see cref="List{T}"/>.
        /// </summary>
        private int _index;

        /// <summary>
        /// The <see cref="Token"/> object currently being parsed at <see cref="_index"/> of <see cref="_tokens"/>.
        /// </summary>
        private Token _currentToken;

        /// <summary>
        /// The boolean check for if normal syntax or QuickSyntax is being used.
        /// </summary>
        private bool _usingQuickSyntax;

        /// <summary>
        /// The boolean check for if QuickSyntax was being used previously in parsing.
        /// </summary>
        private bool _wasUsingQuickSyntax;

        /// <summary>
        /// The object that holds the result of the parsing.
        /// </summary>
        private ParseResult _result;

        /// <summary>
        /// Creates a new <see cref="Parser"/> object.
        /// </summary>
        /// <param name="tokens">The <see cref="List{T}"/> of <see cref="Token"/> objects to be parsed.</param>
        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _currentToken = Token.Dummy;
            _index = -1;
            _usingQuickSyntax = false;
            _wasUsingQuickSyntax = false;
            _result = new ParseResult();

            Advance();
        }

        /// <summary>
        /// Advances to the next <see cref="Token"/> object in <see cref="_tokens"/>.
        /// </summary>
        private void Advance()
        {
            _index++;

            if (_index >= 0 && _index < _tokens.Count)
                _currentToken = _tokens[_index];
        }

        /// <summary>
        /// Reverses back to the <see cref="Token"/> object at <see cref="_index"/> - <paramref name="reverseCount"/> in <see cref="_tokens"/>.
        /// </summary>
        /// <param name="reverseCount">The number of positions to reverse <see cref="_index"/> in <see cref="_tokens"/>.</param>
        private void Reverse(int reverseCount = 1)
        {
            _index -= reverseCount;

            if (_index >= 0 && _index < _tokens.Count)
                _currentToken = _tokens[_index];
        }

        /// <summary>
        /// Peeks at the previous <see cref="Token"/> object from <see cref="_index"/> in <see cref="_tokens"/>.
        /// </summary>
        /// <returns>The <see cref="Token"/> object.</returns>
        private Token PeekPrevious()
        {
            int previousIndex = _index - 1;
            if (previousIndex < 0)
                return Token.Dummy;

            return _tokens[previousIndex];
        }

        /// <summary>
        /// Registers the use of QuickSyntax.
        /// </summary>
        private void RegisterQuickSyntaxUse()
        {
            _wasUsingQuickSyntax = _usingQuickSyntax;
            _usingQuickSyntax = true;
        }

        /// <summary>
        /// Unregisters the use of QuickSyntax.
        /// </summary>
        private void UnregisterQuickSyntaxUse()
        {
            _usingQuickSyntax = _wasUsingQuickSyntax;
        }

        /// <summary>
        /// Advances through <see cref="TokenType.NewLine"/> <see cref="Token"/> objects until a <see cref="Token"/> object of any other <see cref="TokenType"/> has been reached.
        /// </summary>
        private void SkipNewLines()
        {
            if (_currentToken.Type != TokenType.NewLine)
                return;

            while (_currentToken.Type == TokenType.NewLine)
            {
                _result.RegisterAdvance();
                Advance();
            }
        }

        /// <summary>
        /// Parses the <see cref="Token"/> objects in <see cref="_tokens"/>.
        /// </summary>
        public ParseResult Parse()
        {
            ParseStatements();
            if (_result.Error == null && _currentToken.Type != TokenType.EndOfFile)
                _result.Failure(10, new InvalidGrammarError("Did not expect this!", _currentToken.StartPosition, _currentToken.EndPosition));
            return _result;
        }

        /// <summary>
        /// Tries creating a <see cref="BinaryOperationNode"/>.
        /// </summary>
        /// <param name="left">The function to call for the first operand.</param>
        /// <param name="right">The function to call for the second operand.</param>
        /// <param name="operators">The operator <see cref="TokenType"/> object(s).</param>
        private void BinaryOperation(Action left, Action right, params TokenType[] operators)
        {
            Position startPosition = _currentToken.StartPosition;

            left();
            Node leftNode = _result.Node;
            if (_result.Error != null)
                return;

            int toReverseTo = _result.AdvanceCount;

            SkipNewLines();
            while (Array.IndexOf(operators, _currentToken.Type) != -1)
            {
                TokenType @operator = _currentToken.Type;
                _result.RegisterAdvance();
                Advance();

                SkipNewLines();
                
                right();
                Node rightNode = _result.Node;
                if (_result.Error != null)
                    return;

                leftNode = new BinaryOperationNode(leftNode, rightNode, @operator, startPosition, _currentToken.EndPosition);
                toReverseTo = _result.AdvanceCount;
                SkipNewLines();
            }


            int reverseCount = _result.AdvanceCount - toReverseTo;
            Reverse(reverseCount);
            _result.Reverse(reverseCount);
            _result.Success(leftNode);
        }

        /// <summary>
        /// Tries parsing a 'statements' structure.
        /// </summary>
        private void ParseStatements()
        {
            List<Node> statements = new List<Node>();
            Position startPosition = _currentToken.StartPosition;

            SkipNewLines();

            ParseStatement();
            Node statement = _result.Node;
            if (_result.Error != null)
                return;
            statements.Add(statement);

            while (true)
            {
                // NOTE: Qeyword 'l' for 'else if' is deprecated, use Qeyword 'e' instead in format:
                // 'e [check]: [statement(s)]'

                int skipStart = _result.AdvanceCount;
                SkipNewLines();
                if (_result.AdvanceCount - skipStart == 0 || _currentToken.Type == TokenType.EndOfFile
                    || _currentToken.Type == TokenType.KeywordEnd
                    || _currentToken.Type == TokenType.KeywordElse
                    || _currentToken.Type == TokenType.KeywordError
                    || (_usingQuickSyntax
                        && (_currentToken.Type == TokenType.QeywordL
                        || _currentToken.Type == TokenType.QeywordE
                        || _currentToken.Type == TokenType.QeywordS)))
                    break;

                ParseStatement();
                statement = _result.Node;
                if (_result.Error != null)
                    return;
                statements.Add(statement);
            }

            _result.Success(new ArrayLikeNode(statements, false, startPosition, _currentToken.EndPosition));
        }

        /// <summary>
        /// Tries parsing a 'statement' structure.
        /// </summary>
        private void ParseStatement()
        {
            Position startPosition = _currentToken.StartPosition;

            Node expression;
            if (_currentToken.Type == TokenType.KeywordReturn)
            {
                Position possibleEndPosition = _currentToken.EndPosition;
                _result.RegisterAdvance();
                Advance();

                if (_currentToken.Type == TokenType.NewLine
                    || _currentToken.Type == TokenType.EndOfFile
                    || _currentToken.Type == TokenType.KeywordEnd
                    || _currentToken.Type == TokenType.KeywordElse
                    || _currentToken.Type == TokenType.KeywordError
                    || (_usingQuickSyntax
                        && (_currentToken.Type == TokenType.QeywordL
                        || _currentToken.Type == TokenType.QeywordE
                        || _currentToken.Type == TokenType.QeywordS)))
                {
                    _result.Success(new ReturnNode(null, startPosition, possibleEndPosition));
                    return;
                }

                ParseExpression();
                expression = _result.Node;
                if (_result.Error != null)
                    return;

                _result.Success(new ReturnNode(expression, startPosition, _currentToken.EndPosition));
                return;
            }
            else if (_currentToken.Type == TokenType.KeywordSkip || _currentToken.Type == TokenType.KeywordStop)
            {
                Position endPosition = _currentToken.EndPosition;
                TokenType currentTokenType = _currentToken.Type;
                _result.RegisterAdvance();
                Advance();

                _result.Success(new NoValueNode(currentTokenType, startPosition, endPosition));
                return;
            }

            ParseExpression();
            expression = _result.Node;
            if (_result.Error != null)
                _result.Failure(4, new InvalidGrammarError("Expected a statement!", _currentToken.StartPosition, _currentToken.EndPosition));
            else
                _result.Success(expression);
        }

        /// <summary>
        /// Tries parsing an 'expression' structure.
        /// </summary>
        /// <param name="itemKeywordRequired">In cases where the variable assignment expression requires the 'item' keyword, set this to <see langword="true"/>.</param>
        private void ParseExpression(bool itemKeywordRequired = false)
        {
            Position startPosition = _currentToken.StartPosition;

            bool globalAssignment = false;
            bool usedItemKeyword = false;
            int startingAdvanceCount = _result.AdvanceCount;

            if (_currentToken.Type == TokenType.KeywordGlobal)
            {
                globalAssignment = true;
                _result.RegisterAdvance();
                Advance();
            }
            
            if (_currentToken.Type == TokenType.KeywordItem)
            {
                usedItemKeyword = true;
                _result.RegisterAdvance();
                Advance();
            }
            else if (itemKeywordRequired)
            {
                ParseQuickExpression();
                Node node_ = _result.Node;
                if (_result.Error != null)
                    _result.Failure(4, new InvalidGrammarError("Expected an expression!", _currentToken.StartPosition, _currentToken.EndPosition));
                else
                    _result.Success(node_);
                return;
            }

            if (_currentToken.Type == TokenType.Identifier || _currentToken.TypeGroup == TokenTypeGroup.Qeyword)
            {
                Token possibleVariable = _currentToken;
                _result.RegisterAdvance();
                Advance();

                if (_currentToken.TypeGroup != TokenTypeGroup.AssignmentSymbol)
                {
                    Reverse();
                    _result.Reverse();

                    ParseJunction();
                    Node variable = _result.Node;
                    if (_result.Error != null)
                        return;

                    if (_currentToken.TypeGroup == TokenTypeGroup.AssignmentSymbol)
                    {
                        TokenType assignmentOperator = _currentToken.Type;
                        _result.RegisterAdvance();
                        Advance();

                        ParseExpression();
                        Node value = _result.Node;
                        if (_result.Error != null)
                            return;

                        _result.Success(new NodeVariableAssignmentNode(variable, assignmentOperator, value, globalAssignment, startPosition, _currentToken.EndPosition));
                        return;
                    }
                    else if (usedItemKeyword)
                    {
                        _result.Failure(10, new InvalidGrammarError("Expected an assignment symbol! The assignment symbol seperates the variable name and value, and declares how to handle any existing values in the variable, in a variable assignment expression. A few examples of assignment symbols are: (':') - normal assignment, (':+') - adds existing value in variable to new value, assigns the result, (':*') - multiplies existing value with new value, assigns the result, and (':&') - does a bitwise and operation between the existing and new value, assigns the result. There are equivalent symbols for all binary mathematical and bitwise operations.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }
                    else
                    {
                        int reverseCount = _result.AdvanceCount - startingAdvanceCount;
                        Reverse(reverseCount);
                        _result.Reverse(reverseCount);
                    }
                }
                else
                {
                    TokenType assignmentOperator = _currentToken.Type;
                    _result.RegisterAdvance();
                    Advance();

                    ParseExpression();
                    Node value = _result.Node;
                    if (_result.Error != null)
                        return;

                    _result.Success(new TokenVariableAssignmentNode(possibleVariable, assignmentOperator, value, globalAssignment, startPosition, _currentToken.EndPosition));
                    return;
                }
            }
            else if (usedItemKeyword)
            {
                _result.Failure(10, new InvalidGrammarError("Expected a variable name! The variable name is what will be assigned the value in a variable assignment expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }
            else
            {
                int reverseCount = _result.AdvanceCount - startingAdvanceCount;
                Reverse(reverseCount);
                _result.Reverse(reverseCount);
            }

            ParseQuickExpression();
            Node node = _result.Node;
            if (_result.Error != null)
                _result.Failure(4, new InvalidGrammarError("Expected an expression!", _currentToken.StartPosition, _currentToken.EndPosition));
            else
                _result.Success(node);
        }

        /// <summary>
        /// Tries parsing a 'quick-expression' structure.
        /// </summary>
        private void ParseQuickExpression()
        {
            Position startPosition = _currentToken.StartPosition;
            int startingAdvanceCount = _result.AdvanceCount;

            if (_currentToken.Type == TokenType.ExclamationMark)
            {
                _result.RegisterAdvance();
                Advance();

                bool globalAssignment = false;
                bool usedItemKeyword = false;
                if (_currentToken.Type == TokenType.QeywordG)
                {
                    globalAssignment = true;
                    _result.RegisterAdvance();
                    Advance();
                }

                if (_currentToken.Type == TokenType.QeywordD)
                {
                    usedItemKeyword = true;
                    _result.RegisterAdvance();
                    Advance();
                }

                if (_currentToken.Type == TokenType.Identifier || _currentToken.TypeGroup == TokenTypeGroup.Qeyword)
                {
                    Token possibleVariable = _currentToken;
                    _result.RegisterAdvance();
                    Advance();

                    if (_currentToken.TypeGroup != TokenTypeGroup.AssignmentSymbol)
                    {
                        Reverse();
                        _result.Reverse();

                        ParseJunction();
                        Node variable = _result.Node;
                        if (_result.Error != null)
                            return;

                        if (_currentToken.TypeGroup == TokenTypeGroup.AssignmentSymbol)
                        {
                            TokenType assignmentOperator = _currentToken.Type;
                            _result.RegisterAdvance();
                            Advance();

                            ParseExpression();
                            Node value = _result.Node;
                            if (_result.Error != null)
                                return;

                            _result.Success(new NodeVariableAssignmentNode(variable, assignmentOperator, value, globalAssignment, startPosition, _currentToken.EndPosition));
                            return;
                        }
                        else if (usedItemKeyword)
                        {
                            _result.Failure(10, new InvalidGrammarError("Expected an assignment symbol! The assignment symbol seperates the variable name and value, and declares how to handle any existing values in the variable, in a variable assignment expression. A few examples of assignment symbols are: (':') - normal assignment, (':+') - adds existing value in variable to new value, assigns the result, (':*') - multiplies existing value with new value, assigns the result, and (':&') - does a bitwise and operation between the existing and new value, assigns the result. There are equivalent symbols for all binary mathematical and bitwise operations.", _currentToken.StartPosition, _currentToken.EndPosition));
                            return;
                        }
                        else
                        {
                            int reverseCount = _result.AdvanceCount - startingAdvanceCount;
                            Reverse(reverseCount);
                            _result.Reverse(reverseCount);
                        }
                    }
                    else
                    {
                        TokenType assignmentOperator = _currentToken.Type;
                        _result.RegisterAdvance();
                        Advance();

                        ParseExpression();
                        Node value = _result.Node;
                        if (_result.Error != null)
                            return;

                        _result.Success(new TokenVariableAssignmentNode(possibleVariable, assignmentOperator, value, globalAssignment, startPosition, _currentToken.EndPosition));
                        return;
                    }
                }
                else if (usedItemKeyword)
                {
                    _result.Failure(10, new InvalidGrammarError("Expected a variable name! The variable name is what will be assigned the value in a variable assignment expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }
                else
                {
                    int reverseCount = _result.AdvanceCount - startingAdvanceCount;
                    Reverse(reverseCount);
                    _result.Reverse(reverseCount);
                }
            }

            ParseJunction();
            Node node = _result.Node;
            if (_result.Error != null)
                _result.Failure(4, new InvalidGrammarError("Expected a QuickSyntax expression!", _currentToken.StartPosition, _currentToken.EndPosition));
            else
                _result.Success(node);
        }

        /// <summary>
        /// Tries parsing a 'junction' structure.
        /// </summary>
        private void ParseJunction()
        {
            BinaryOperation(ParseInversion, ParseInversion, TokenType.KeywordAnd, TokenType.KeywordOr);
        }

        /// <summary>
        /// Tries parsing an 'inversion' structure.
        /// </summary>
        private void ParseInversion()
        {
            Position startPosition = _currentToken.StartPosition;
            int startingAdvanceCount = _result.AdvanceCount;

            if (_currentToken.Type == TokenType.ExclamationMark)
            {
                _result.RegisterAdvance();
                Advance();

                if (_currentToken.Type == TokenType.QeywordV)
                {
                    TokenType @operator = _currentToken.Type;
                    _result.RegisterAdvance();
                    Advance();

                    ParseExpression();
                    Node operand = _result.Node;
                    if (_result.Error != null)
                        return;

                    _result.Success(new UnaryOperationNode(operand, @operator, startPosition, _currentToken.EndPosition));
                    return;
                }
                else
                {
                    int reverseCount = _result.AdvanceCount - startingAdvanceCount;
                    Reverse(reverseCount);
                    _result.Reverse(reverseCount);
                }
            }
            else if (_currentToken.Type == TokenType.KeywordInvert)
            {
                TokenType @operator = _currentToken.Type;
                _result.RegisterAdvance();
                Advance();

                ParseExpression();
                Node operand = _result.Node;
                if (_result.Error != null)
                    return;

                _result.Success(new UnaryOperationNode(operand, @operator, startPosition, _currentToken.EndPosition));
                return;
            }

            ParseComparison();
            Node node = _result.Node;
            if (_result.Error != null)
                _result.Failure(4, new InvalidGrammarError("Expected an inversion expression!", _currentToken.StartPosition, _currentToken.EndPosition));
            else
                _result.Success(node);
        }

        /// <summary>
        /// Tries parsing a 'comparison' structure.
        /// </summary>
        private void ParseComparison()
        {
            Position startPosition = _currentToken.StartPosition;

            ParseBitwiseOr();
            Node left = _result.Node;
            if (_result.Error != null)
                return;
            
            int toReverseTo = _result.AdvanceCount;

            SkipNewLines();
            while (_currentToken.Type == TokenType.EqualSign
                || _currentToken.Type == TokenType.ExclamationMark
                || _currentToken.Type == TokenType.LessThanSign
                || _currentToken.Type == TokenType.GreaterThanSign
                || _currentToken.Type == TokenType.LessThanOrEqual
                || _currentToken.Type == TokenType.GreaterThanOrEqual
                || _currentToken.Type == TokenType.KeywordNot
                || _currentToken.Type == TokenType.KeywordIn)
            {
                TokenType @operator = _currentToken.Type;
                _result.RegisterAdvance();
                Advance();

                SkipNewLines();
                if (@operator == TokenType.KeywordNot)
                {
                    if (_currentToken.Type != TokenType.KeywordIn)
                    {
                        _result.Failure(10, new InvalidGrammarError("Expected the 'in' keyword! The 'in' keyword is the second part of a check-not-in operation.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }
                    _result.RegisterAdvance();
                    Advance();
                }

                ParseBitwiseOr();
                Node right = _result.Node;
                if (_result.Error != null)
                    return;

                left = new BinaryOperationNode(left, right, @operator, startPosition, _currentToken.EndPosition);
                toReverseTo = _result.AdvanceCount;
                SkipNewLines();
            }


            int reverseCount = _result.AdvanceCount - toReverseTo;
            Reverse(reverseCount);
            _result.Reverse(reverseCount);
            _result.Success(left);
        }

        /// <summary>
        /// Tries parsing a 'bitwise-or' structure.
        /// </summary>
        private void ParseBitwiseOr()
        {
            BinaryOperation(ParseBitwiseXOr, ParseBitwiseXOr, TokenType.VerticalBar);
        }

        /// <summary>
        /// Tries parsing a 'bitwise-xor' structure.
        /// </summary>
        private void ParseBitwiseXOr()
        {
            BinaryOperation(ParseBitwiseAnd, ParseBitwiseAnd, TokenType.Backslash);
        }

        /// <summary>
        /// Tries parsing a 'bitwise-and' structure.
        /// </summary>
        private void ParseBitwiseAnd()
        {
            BinaryOperation(ParseBitwiseShift, ParseBitwiseShift, TokenType.Ampersand);
        }

        /// <summary>
        /// Tries creating a 'bitwise-shift' structure.
        /// </summary>
        private void ParseBitwiseShift()
        {
            BinaryOperation(ParseArithmeticExpression, ParseArithmeticExpression, TokenType.BitwiseLeftShift, TokenType.BitwiseRightShift);
        }

        /// <summary>
        /// Tries parsing an 'arithmetic-expression' structure.
        /// </summary>
        private void ParseArithmeticExpression()
        {
            BinaryOperation(ParseTerm, ParseTerm, TokenType.Plus, TokenType.HyphenMinus);
        }

        /// <summary>
        /// Tries parsing a 'term' structure.
        /// </summary>
        private void ParseTerm()
        {
            BinaryOperation(ParseFactor, ParseFactor, TokenType.Asterisk, TokenType.Slash, TokenType.PercentSign);
        }

        /// <summary>
        /// Tries parsing a 'factor' structure.
        /// </summary>
        private void ParseFactor()
        {
            Position startPosition = _currentToken.StartPosition;

            TokenType @operator = _currentToken.Type;
            if (@operator == TokenType.Plus || @operator == TokenType.HyphenMinus || @operator == TokenType.Tilde)
            {
                _result.RegisterAdvance();
                Advance();

                ParseFactor();
                Node operand = _result.Node;
                if (_result.Error != null)
                    return;

                _result.Success(new UnaryOperationNode(operand, @operator, startPosition, _currentToken.EndPosition));
                return;
            }

            ParsePower();
        }

        /// <summary>
        /// Tries parsing a 'power' structure.
        /// </summary>
        private void ParsePower()
        {
            BinaryOperation(ParseObjectAttributeAccess, ParseObjectAttributeAccess, TokenType.Caret);
        }

        /// <summary>
        /// Tries parsing an 'object-attribute-access' structure.
        /// </summary>
        private void ParseObjectAttributeAccess()
        {
            BinaryOperation(ParseCall, ParseObjectAttributeAccess, TokenType.Period);
        }

        /// <summary>
        /// Tries parsing a 'call' structure.
        /// </summary>
        private void ParseCall()
        {
            Position startPosition = _currentToken.StartPosition;

            ParseAtom();
            Node node = _result.Node;
            if (_result.Error != null)
                return;

            if(_currentToken.Type == TokenType.LeftParenthesis)
            {
                _result.RegisterAdvance();
                Advance();

                Position possibleErrorStartPosition = _currentToken.StartPosition;

                SkipNewLines();
                Position endPosition;
                List<Node> arguments = new List<Node>();
                if (_currentToken.Type == TokenType.RightParenthesis)
                {
                    endPosition = _currentToken.EndPosition;
                    _result.RegisterAdvance();
                    Advance();
                }
                else
                {
                    ParseExpression();
                    arguments.Add(_result.Node);
                    if (_result.Error != null)
                    {
                        _result.Failure(10, new StackedError(_result.Error, new InvalidGrammarError("Expected a right-parenthesis symbol! The function/object call expression must end with a right-parenthesis.", possibleErrorStartPosition, _currentToken.EndPosition)));
                        return;
                    }

                    SkipNewLines();
                    while (_currentToken.Type == TokenType.Comma)
                    {
                        possibleErrorStartPosition = _currentToken.StartPosition;
                        _result.RegisterAdvance();
                        Advance();

                        SkipNewLines();
                        ParseExpression();
                        arguments.Add(_result.Node);
                        if (_result.Error != null)
                        {
                            _result.Failure(10, new StackedError(_result.Error, new InvalidGrammarError("Expected a right-parenthesis symbol! The function/object call expression must end with a right-parenthesis.", possibleErrorStartPosition, _currentToken.EndPosition)));
                            return;
                        }
                        SkipNewLines();
                    }

                    if (_currentToken.Type != TokenType.RightParenthesis)
                    {
                        _result.Failure(10, new InvalidGrammarError("Expected a comma or right-parenthesis symbol! Commas are used to seperate the arguments of the function/object call expression, and the right-parenthesis is used to end it.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }
                    endPosition = _currentToken.EndPosition;
                    _result.RegisterAdvance();
                    Advance();
                }

                _result.Success(new CallNode(node, arguments, startPosition, endPosition));
                return;
            }
            
            _result.Success(node);
        }

        /// <summary>
        /// Tries parsing a 'atom' structure.
        /// </summary>
        private void ParseAtom()
        {

            Token startToken = _currentToken;
            if(startToken.TypeGroup == TokenTypeGroup.Value)
            {
                _result.RegisterAdvance();
                Advance();

                _result.Success(new ValueNode(startToken, startToken.StartPosition, startToken.EndPosition));
                return;
            }
            else if (startToken.Type == TokenType.Identifier || startToken.TypeGroup == TokenTypeGroup.Qeyword)
            {
                _result.RegisterAdvance();
                Advance();

                _result.Success(new VariableAccessNode(startToken, false, startToken.StartPosition, startToken.EndPosition));
                return;
            }
            else
            {
                switch (startToken.Type)
                {
                    case TokenType.KeywordGlobal:
                        _result.RegisterAdvance();
                        Advance();

                        if (_currentToken.Type == TokenType.Identifier || _currentToken.TypeGroup == TokenTypeGroup.Qeyword)
                        {
                            Token variable = _currentToken;
                            Position endPosition = _currentToken.EndPosition;
                            _result.RegisterAdvance();
                            Advance();

                            _result.Success(new VariableAccessNode(variable, true, startToken.StartPosition, endPosition));
                            return;
                        }
                        _result.Failure(10, new InvalidGrammarError("Expected a variable name or the 'item' keyword! The variable name is used to access a global variable in a global variable access expression and the 'item' keyword is used to assign to a global variable in a global variable assignment expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        break;
                    case TokenType.LeftParenthesis:
                        ParseArrayOrParentheticalExpression();
                        Node arrayNode = _result.Node;
                        if (_result.Error != null)
                            return;
                        _result.Success(arrayNode);
                        break;
                    case TokenType.LeftSquareBracket:
                        ParseList();
                        Node encapsulatedListNode = _result.Node;
                        if (_result.Error != null)
                            return;
                        _result.Success(encapsulatedListNode);
                        break;
                    case TokenType.LeftCurlyBracket:
                        ParseDictionary();
                        Node dictionaryNode = _result.Node;
                        if (_result.Error != null)
                            return;
                        _result.Success(dictionaryNode);
                        break;
                    case TokenType.KeywordIf:
                        ParseIfExpression();
                        Node ifNode = _result.Node;
                        if (_result.Error != null)
                            return;
                        _result.Success(ifNode);
                        break;
                    case TokenType.KeywordCount:
                        ParseCountExpression();
                        Node countNode = _result.Node;
                        if (_result.Error != null)
                            return;
                        _result.Success(countNode);
                        break;
                    case TokenType.KeywordWhile:
                        ParseWhileExpression();
                        Node whileNode = _result.Node;
                        if (_result.Error != null)
                            return;
                        _result.Success(whileNode);
                        break;
                    case TokenType.KeywordTry:
                        ParseTryExpression();
                        Node tryNode = _result.Node;
                        if (_result.Error != null)
                            return;
                        _result.Success(tryNode);
                        break;
                    case TokenType.KeywordFunction:
                        ParseFunctionDefinitionExpression();
                        Node functionDefinitionNode = _result.Node;
                        if (_result.Error != null)
                            return;
                        _result.Success(functionDefinitionNode);
                        break;
                    case TokenType.KeywordObject:
                        ParseObjectDefinitionExpression();
                        Node objectDefinitionNode = _result.Node;
                        if (_result.Error != null)
                            return;
                        _result.Success(objectDefinitionNode);
                        break;
                    case TokenType.KeywordInclude:
                        ParseIncludeExpression();
                        Node includeNode = _result.Node;
                        if (_result.Error != null)
                            return;
                        _result.Success(includeNode);
                        break;
                    default:
                        _result.Failure(4, new InvalidGrammarError("Expected an integer, float, string, character, character list, identifier, 'if' expression, 'count' expression, 'while' expression and so on.", _currentToken.StartPosition, _currentToken.EndPosition));
                        break;
                }
            }
        }

        /// <summary>
        /// Tries parsing an array, an <see cref="ArrayLikeNode"/> with <see cref="ArrayLikeNode.CreateList"/> set to <see langword="false"/> OR a parenthetical expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.LeftParenthesis"/>.
        /// </summary>
        private void ParseArrayOrParentheticalExpression()
        {
            Position startPosition = _currentToken.StartPosition;
            _result.RegisterAdvance();
            Advance();

            Position possibleErrorStartPosition = _currentToken.StartPosition;
            List<Node> elements = new List<Node>();
            bool isArray = false;

            SkipNewLines();
            Position endPosition;
            if (_currentToken.Type == TokenType.RightParenthesis)
            {
                endPosition = _currentToken.EndPosition;
                _result.RegisterAdvance();
                Advance();
            }
            else
            {
                ParseExpression();
                elements.Add(_result.Node);
                if (_result.Error != null)
                {
                    _result.Failure(10, new StackedError(_result.Error, new InvalidGrammarError("Expected a right-parenthesis symbol! An array/parenthetical expression must end with a right-parenthesis.", possibleErrorStartPosition, _currentToken.EndPosition)));
                    return;
                }

                SkipNewLines();
                while (_currentToken.Type == TokenType.Comma)
                {
                    isArray = true;
                    possibleErrorStartPosition = _currentToken.StartPosition;
                    _result.RegisterAdvance();
                    Advance();

                    SkipNewLines();
                    if (_currentToken.Type == TokenType.RightParenthesis && elements.Count == 1)
                        break;

                    ParseExpression();
                    if (_result.Error != null)
                    {
                        if (elements.Count > 1)
                        {
                            _result.Failure(10, new StackedError(_result.Error, new InvalidGrammarError("Expected a right-parenthesis symbol! An array must end with a right-parenthesis.", possibleErrorStartPosition, _currentToken.EndPosition)));
                            return;
                        }

                        Position errorStartPosition = possibleErrorStartPosition.Copy();
                        errorStartPosition.Advance();
                        _result.Failure(10, new StackedError(_result.Error, new InvalidGrammarError("Expected a right-parenthesis symbol! An array must end with a right-parenthesis.", errorStartPosition, _currentToken.EndPosition)));
                    }

                    elements.Add(_result.Node);
                    SkipNewLines();
                }

                if (_currentToken.Type != TokenType.RightParenthesis)
                {
                    if (isArray)
                        _result.Failure(10, new InvalidGrammarError("Expected a comma or a right-parenthesis symbol! Commas seperate the elements of the array, while the right-parenthesis ends it.", _currentToken.StartPosition, _currentToken.EndPosition));
                    else
                        _result.Failure(10, new InvalidGrammarError("Expected a comma or a right-parenthesis symbol! The comma is used to create an array and seperate its elements, while the right-parenthesis declares the end of a parenthetical expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }
                
                endPosition = _currentToken.EndPosition;
                _result.RegisterAdvance();
                Advance();
            }

            if (!isArray && elements.Count > 0)
                _result.Success(elements[0]);
            else
                _result.Success(new ArrayLikeNode(elements, false, startPosition, endPosition));
        }

        /// <summary>
        /// Tries parsing a list, an <see cref="ArrayLikeNode"/> with <see cref="ArrayLikeNode.CreateList"/> set to <see langword="true"/>. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.LeftSquareBracket"/>.
        /// </summary>
        private void ParseList()
        {
            Position startPosition = _currentToken.StartPosition;
            _result.RegisterAdvance();
            Advance();

            Position possibleErrorStartPosition = _currentToken.StartPosition;

            SkipNewLines();
            Position endPosition;
            List<Node> elements = new List<Node>();
            if (_currentToken.Type == TokenType.RightSquareBracket)
            {
                endPosition = _currentToken.EndPosition;
                _result.RegisterAdvance();
                Advance();
            }
            else
            {
                ParseExpression();
                elements.Add(_result.Node);
                if (_result.Error != null)
                {
                    _result.Failure(10, new StackedError(_result.Error, new InvalidGrammarError("Expected a right-square-bracket symbol! A list expression must end with a right-square-bracket.", possibleErrorStartPosition, _currentToken.EndPosition)));
                    return;
                }

                SkipNewLines();
                while (_currentToken.Type == TokenType.Comma)
                {
                    possibleErrorStartPosition = _currentToken.StartPosition;
                    _result.RegisterAdvance();
                    Advance();

                    SkipNewLines();

                    ParseExpression();
                    elements.Add(_result.Node);
                    if (_result.Error != null)
                    {
                        _result.Failure(10, new StackedError(_result.Error, new InvalidGrammarError("Expected a right-square-bracket symbol! A list expression must end with a right-square-bracket.", possibleErrorStartPosition, _currentToken.EndPosition)));
                        return;
                    }
                    SkipNewLines();
                }

                if (_currentToken.Type != TokenType.RightSquareBracket)
                {
                    _result.Failure(10, new InvalidGrammarError("Expected a comma or a right-square-bracket symbol! Commas are used to seperate elements in the list, while the right-square-bracket ends it.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }
                endPosition = _currentToken.EndPosition;
                _result.RegisterAdvance();
                Advance();
            }

            _result.Success(new ArrayLikeNode(elements, true, startPosition, endPosition));
        }

        /// <summary>
        /// Tries parsing a dictionary. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.LeftCurlyBracket"/>.
        /// </summary>
        private void ParseDictionary()
        {
            Position startPosition = _currentToken.StartPosition;
            _result.RegisterAdvance();
            Advance();

            Position possibleErrorStartPosition = _currentToken.StartPosition;
            List<Node[]> pairs = new List<Node[]>();

            SkipNewLines();
            Position endPosition;
            if (_currentToken.Type == TokenType.RightCurlyBracket)
            {
                endPosition = _currentToken.EndPosition;
                _result.RegisterAdvance();
                Advance();
            }
            else
            {
                ParseExpression(true);
                Node left = _result.Node;
                if (_result.Error != null)
                {
                    _result.Failure(10, new StackedError(_result.Error, new InvalidGrammarError("Expected a right-curly-bracket symbol! A dictionary expression must end with a right-curly-bracket.", possibleErrorStartPosition, _currentToken.EndPosition)));
                    return;
                }

                SkipNewLines();
                
                if (_currentToken.Type != TokenType.Colon)
                {
                    _result.Failure(10, new InvalidGrammarError("Expected a colon symbol! The colon is the seperator between a key and its value in a dictionary.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }
                _result.RegisterAdvance();
                Advance();

                SkipNewLines();

                ParseExpression();
                Node right = _result.Node;
                if (_result.Error != null)
                    return;

                pairs.Add(new Node[2] { left, right });
                SkipNewLines();

                while (_currentToken.Type == TokenType.Comma)
                {
                    possibleErrorStartPosition = _currentToken.StartPosition;
                    _result.RegisterAdvance();
                    Advance();

                    SkipNewLines();

                    ParseExpression(true);
                    left = _result.Node;
                    if (_result.Error != null)
                    {
                        _result.Failure(10, new StackedError(_result.Error, new InvalidGrammarError("Expected a right-curly-bracket symbol! A dictionary expression must end with a right-curly-bracket.", possibleErrorStartPosition, _currentToken.EndPosition)));
                        return;
                    }

                    SkipNewLines();
                    if (_currentToken.Type != TokenType.Colon)
                    {
                        _result.Failure(10, new InvalidGrammarError("Expected a colon symbol! The colon is the seperator between a key and its value in a dictionary.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }

                    _result.RegisterAdvance();
                    Advance();

                    SkipNewLines();

                    ParseExpression();
                    right = _result.Node;
                    if (_result.Error != null)
                        return;

                    pairs.Add(new Node[2] { left, right });
                    SkipNewLines();
                }

                if (_currentToken.Type != TokenType.RightCurlyBracket)
                {
                    _result.Failure(10, new InvalidGrammarError("Expected a comma or right-curly-bracket symbol! Commas are used to seperate key-value pairs in the dictionary, and the right-curly-bracket declares its end.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }

                endPosition = _currentToken.EndPosition;
                _result.RegisterAdvance();
                Advance();
            }

            _result.Success(new DictionaryNode(pairs, startPosition, endPosition));
        }

        /// <summary>
        /// Tries parsing an if expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordIf"/>.
        /// </summary>
        private void ParseIfExpression()
        {
            Position startPosition = _currentToken.StartPosition;
            _result.RegisterAdvance();
            Advance();

            List<Node[]> cases = new List<Node[]>();
            Node? elseCase = null;

            ParseExpression();
            Node condition = _result.Node;
            if (_result.Error != null)
                return;

            if (_currentToken.Type != TokenType.KeywordDo)
            {
                _result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            _result.RegisterAdvance();
            Advance();

            Node body;
            if (_currentToken.Type == TokenType.NewLine)
            {
                _result.RegisterAdvance();
                Advance();

                ParseStatements();
                body = _result.Node;
                if (_result.Error != null)
                    return;

                Position endPosition;
                cases.Add(new Node[2] { condition, body });
                if (_currentToken.Type == TokenType.KeywordEnd)
                {
                    endPosition = _currentToken.EndPosition;
                    _result.RegisterAdvance();
                    Advance();
                }
                else if (_currentToken.Type == TokenType.KeywordElse)
                {
                    while (_currentToken.Type == TokenType.KeywordElse)
                    {
                        _result.RegisterAdvance();
                        Advance();

                        if (_currentToken.Type == TokenType.KeywordIf)
                        {
                            if (elseCase != null)
                            {
                                _result.Failure(10, new InvalidGrammarError("The \"else-if\" expression cannot be declared before the \"else\" expression! You cannot have \"else\" expressions before or in-between \"else if\" expressions.", PeekPrevious().StartPosition, _currentToken.EndPosition));
                                return;
                            }

                            _result.RegisterAdvance();
                            Advance();

                            ParseExpression();
                            condition = _result.Node;
                            if (_result.Error != null)
                                return;

                            if (_currentToken.Type != TokenType.KeywordDo)
                            {
                                _result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"else if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                                return;
                            }

                            _result.RegisterAdvance();
                            Advance();

                            ParseStatements();
                            body = _result.Node;
                            if (_result.Error != null)
                                return;

                            cases.Add(new Node[2] { condition, body });
                        }
                        else if (_currentToken.Type != TokenType.KeywordDo)
                        {
                            _result.Failure(10, new InvalidGrammarError("Expected the 'if' or 'do' keywords! The 'if' keyword declares the start of an \"else if\" expression, and the 'do' keyword declares the start of the body of an \"else\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                            return;
                        }
                        else
                        {
                            if (elseCase != null)
                            {
                                _result.Failure(10, new InvalidGrammarError("There should only be one \"else\" expression! You cannot have multiple \"else\" expressions in an \"if\" expression.", PeekPrevious().StartPosition, _currentToken.EndPosition));
                                return;
                            }

                            _result.RegisterAdvance();
                            Advance();

                            ParseStatements();
                            elseCase = _result.Node;
                            if (_result.Error != null)
                                return;
                        }
                    }

                    if (_currentToken.Type != TokenType.KeywordEnd)
                    {
                        if (elseCase == null)
                            _result.Failure(10, new InvalidGrammarError("Expected the 'else' or 'end' keywords! The 'else' keyword defines the start of an \"else\" or \"else if\" expression, and the 'end' keyword declares the end of the whole \"if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        else
                            _result.Failure(10, new InvalidGrammarError("Expected the 'end' keyword! The 'end' keyword declares the end of the whole \"if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }

                    endPosition = _currentToken.EndPosition;
                    _result.RegisterAdvance();
                    Advance();
                }
                else
                {
                    _result.Failure(10, new InvalidGrammarError("Expected the 'else' or 'end' keywords! The 'else' keyword defines the start of an \"else\" or \"else if\" expression, and the 'end' keyword declares the end of the whole \"if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }

                _result.Success(new IfNode(cases, elseCase, startPosition, endPosition));
                return;
            }

            ParseStatement();
            body = _result.Node;
            if (_result.Error != null)
                return;

            cases.Add(new Node[2] { condition, body });
            while (_currentToken.Type == TokenType.KeywordElse)
            {
                _result.RegisterAdvance();
                Advance();

                if (_currentToken.Type == TokenType.KeywordIf)
                {
                    if (elseCase != null)
                    {
                        _result.Failure(10, new InvalidGrammarError("The \"else-if\" expression cannot be declared before the \"else\" expression! You cannot have \"else\" expressions before or in-between \"else if\" expressions.", PeekPrevious().StartPosition, _currentToken.EndPosition));
                        return;
                    }

                    _result.RegisterAdvance();
                    Advance();

                    ParseExpression();
                    condition = _result.Node;
                    if (_result.Error != null)
                        return;

                    if (_currentToken.Type != TokenType.KeywordDo)
                    {
                        _result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"else if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }

                    _result.RegisterAdvance();
                    Advance();

                    ParseStatement();
                    body = _result.Node;
                    if (_result.Error != null)
                        return;

                    cases.Add(new Node[2] { condition, body });
                }
                else if (_currentToken.Type != TokenType.KeywordDo)
                {
                    _result.Failure(10, new InvalidGrammarError("Expected the 'if' or 'do' keywords! The 'if' keyword declares the start of an \"else if\" expression, and the 'do' keyword declares the start of the body of an \"else\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }
                else
                {
                    if (elseCase != null)
                    {
                        _result.Failure(10, new InvalidGrammarError("There should only be one \"else\" expression! You cannot have multiple \"else\" expressions in an \"if\" expression.", PeekPrevious().StartPosition, _currentToken.EndPosition));
                        return;
                    }

                    _result.RegisterAdvance();
                    Advance();

                    ParseStatement();
                    elseCase = _result.Node;
                    if (_result.Error != null)
                        return;
                }
            }

            _result.Success(new IfNode(cases, elseCase, startPosition, PeekPrevious().EndPosition));
        }

        /// <summary>
        /// Tries parsing a count expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordCount"/>.
        /// </summary>
        private void ParseCountExpression()
        {
            Position startPosition = _currentToken.StartPosition;
            _result.RegisterAdvance();
            Advance();

            Node to = InvalidNode.StaticInvalidNode;
            Node? from = null;
            Node? step = null;
            Node? @as = null;

            if (_currentToken.Type == TokenType.KeywordFrom)
            {
                _result.RegisterAdvance();
                Advance();

                ParseExpression();
                from = _result.Node;
                if (_result.Error != null)
                    return;
            }

            if (_currentToken.Type != TokenType.KeywordTo)
            {
                if (from == null)
                    _result.Failure(10, new InvalidGrammarError("Expected the 'to' or 'from' keyword! The 'to' keyword and the following expression is the amount to count to in the count loop, and the optional 'from' keyword and the following expression is the amount to count from.", _currentToken.StartPosition, _currentToken.EndPosition));
                else
                    _result.Failure(10, new InvalidGrammarError("Expected the 'to' keyword! The 'to' keyword and the following expression is the amount to count to in the count loop.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            _result.RegisterAdvance();
            Advance();

            ParseExpression();
            to = _result.Node;
            if (_result.Error != null)
                return;

            if (_currentToken.Type == TokenType.KeywordStep)
            {
                _result.RegisterAdvance();
                Advance();

                ParseExpression();
                step = _result.Node;
                if (_result.Error != null)
                    return;
            }

            if (_currentToken.Type == TokenType.KeywordAs)
            {
                _result.RegisterAdvance();
                Advance();

                ParseExpression();
                @as = _result.Node;
                if (_result.Error != null)
                    return;
            }

            if (_currentToken.Type != TokenType.KeywordDo)
            {
                if (step == null && @as == null)
                    _result.Failure(10, new InvalidGrammarError("Expected the 'do', 'step' or 'as' keyword! The 'do' keyword declares the start of the body of the count loop, the optional 'step' keyword and the following expression is the increment, and the optional 'as' keyword and the following expression is where the iterations are stored.", _currentToken.StartPosition, _currentToken.EndPosition));
                else if (@as == null)    
                    _result.Failure(10, new InvalidGrammarError("Expected the 'do' or 'as' keyword! The 'do' keyword declares the start of the body of the count loop, and the optional 'as' keyword and the following expression is where the iterations are stored.", _currentToken.StartPosition, _currentToken.EndPosition));
                else
                    _result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the count loop.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            _result.RegisterAdvance();
            Advance();

            Node body;
            Position endPosition;
            if (_currentToken.Type == TokenType.NewLine)
            {
                _result.RegisterAdvance();
                Advance();

                ParseStatements();
                body = _result.Node;
                if (_result.Error != null)
                    return;

                if (_currentToken.Type != TokenType.KeywordEnd)
                {
                    _result.Failure(10, new InvalidGrammarError("Expected the 'end' keyword! The 'end' keyword declares the end of the count loop.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }

                endPosition = _currentToken.EndPosition;
                _result.RegisterAdvance();
                Advance();
            }
            else
            {
                ParseStatement();
                body = _result.Node;
                if (_result.Error != null)
                    return;

                endPosition = PeekPrevious().EndPosition;
            }

            _result.Success(new CountNode(to, from, step, @as, body, startPosition, endPosition));
        }

        /// <summary>
        /// Tries parsing a while expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordWhile"/>.
        /// </summary>
        private void ParseWhileExpression()
        {
            Position startPosition = _currentToken.StartPosition;
            _result.RegisterAdvance();
            Advance();

            ParseExpression();
            Node condition = _result.Node;
            if (_result.Error != null)
                return;

            if (_currentToken.Type != TokenType.KeywordDo)
            {
                _result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the while loop.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }
            _result.RegisterAdvance();
            Advance();

            Node body;
            Position endPosition;
            if (_currentToken.Type == TokenType.NewLine)
            {
                _result.RegisterAdvance();
                Advance();

                ParseStatements();
                body = _result.Node;
                if (_result.Error != null)
                    return;

                if (_currentToken.Type != TokenType.KeywordEnd)
                {
                    _result.Failure(10, new InvalidGrammarError("Expected the 'end' keyword! The 'end' keyword declares the end of the while loop.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }

                endPosition = _currentToken.EndPosition;
                _result.RegisterAdvance();
                Advance();
            }
            else
            {
                ParseStatement();
                body = _result.Node;
                if (_result.Error != null)
                    return;

                endPosition = PeekPrevious().EndPosition;
            }

            _result.Success(new WhileNode(condition, body, startPosition, endPosition));
        }

        /// <summary>
        /// Tries parsing a try expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordTry"/>.
        /// </summary>
        private void ParseTryExpression()
        {
            Position startPosition = _currentToken.StartPosition;
            _result.RegisterAdvance();
            Advance();

            Node block;
            List<Node?[]> cases = new List<Node?[]>();
            Node?[]? emptyCase = null;

            if (_currentToken.Type != TokenType.KeywordDo)
            {
                _result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"try\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            _result.RegisterAdvance();
            Advance();

            Node body;
            if (_currentToken.Type == TokenType.NewLine)
            {
                _result.RegisterAdvance();
                Advance();

                ParseStatements();
                block = _result.Node;
                if (_result.Error != null)
                    return;

                Position endPosition;
                if (_currentToken.Type == TokenType.KeywordEnd)
                {
                    endPosition = _currentToken.EndPosition;
                    _result.RegisterAdvance();
                    Advance();
                }
                else if (_currentToken.Type == TokenType.KeywordError)
                {
                    while (_currentToken.Type == TokenType.KeywordError)
                    {
                        _result.RegisterAdvance();
                        Advance();

                        Node? error = null;
                        bool isErrorNull = true;

                    ErrorExpressionEvaluation:
                        bool isAsKeyword = _currentToken.Type == TokenType.KeywordAs;
                        if (_currentToken.Type == TokenType.KeywordDo || isAsKeyword)
                        {
                            if (emptyCase != null)
                            {
                                _result.Failure(10, new InvalidGrammarError("There should only be one empty \"error\" expression! You cannot have multiple \"error\" expressions in a \"try\" expression.", PeekPrevious().StartPosition, _currentToken.EndPosition));
                                return;
                            }

                            Node? @as = null;
                            if (isAsKeyword)
                            {
                                _result.RegisterAdvance();
                                Advance();

                                ParseExpression();
                                @as = _result.Node;
                                if (_result.Error != null)
                                    return;

                                if (_currentToken.Type != TokenType.KeywordDo)
                                {
                                    _result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"error\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                                    return;
                                }
                            }

                            _result.RegisterAdvance();
                            Advance();

                            ParseStatements();
                            body = _result.Node;
                            if (_result.Error != null)
                                return;

                            if (error != null)
                                cases.Add(new Node?[3] { error, @as, body });
                            else
                                emptyCase = new Node?[2] { @as, body };
                        }
                        else if (isErrorNull && _currentToken.Type != TokenType.EndOfFile && _currentToken.Type != TokenType.NewLine)
                        {
                            if (emptyCase != null)
                            {
                                Token previous = PeekPrevious();
                                _result.Failure(10, new InvalidGrammarError("There can't be any \"error\" expressions after an empty \"error\" expression!", previous.StartPosition, previous.EndPosition));
                                return;
                            }

                            ParseExpression();
                            error = _result.Node;
                            if (_result.Error != null)
                                return;

                            isErrorNull = false;
                            goto ErrorExpressionEvaluation;
                        }
                        else if (isErrorNull)
                        {
                            _result.Failure(10, new InvalidGrammarError("Expected an expression or the 'as' or 'do' keywords! An expression after the 'error' keyword defines what error(s) will lead to the \"error\" expression, the 'as' keyword and the following expression declares where the error will be stored and the 'do' keyword declares the start of the body of the \"error\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                            return;
                        }
                        else
                        {
                            _result.Failure(10, new InvalidGrammarError("Expected the 'as' or 'do' keywords! The 'as' keyword and the following expression tells the interpreter where the error will be stored and the 'do' keyword declares the start of the body of the \"error\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                            return;
                        }
                    }

                    if (_currentToken.Type != TokenType.KeywordEnd)
                    {
                        if (emptyCase == null)
                            _result.Failure(10, new InvalidGrammarError("Expected the 'error' or 'end' keywords! The 'error' keyword defines the start of an \"error\" expression, and the 'end' keyword declares the end of the whole \"try\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        else
                            _result.Failure(10, new InvalidGrammarError("Expected the 'end' keyword! The 'end' keyword declares the end of the whole \"try\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }

                    endPosition = _currentToken.EndPosition;
                    _result.RegisterAdvance();
                    Advance();
                }
                else
                {
                    _result.Failure(10, new InvalidGrammarError("Expected the 'error' or 'end' keywords! The 'error' keyword defines the start of an \"error\" expression, and the 'end' keyword declares the end of the whole \"try\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }

                _result.Success(new TryNode(block, cases, emptyCase, startPosition, endPosition));
                return;
            }

            ParseStatement();
            block = _result.Node;
            if (_result.Error != null)
                return;

            while (_currentToken.Type == TokenType.KeywordError)
            {
                _result.RegisterAdvance();
                Advance();

                Node? error = null;
                bool isErrorNull = true;

            ErrorExpressionEvaluation:
                bool isAsKeyword = _currentToken.Type == TokenType.KeywordAs;
                if (_currentToken.Type == TokenType.KeywordDo || isAsKeyword)
                {
                    if (emptyCase != null)
                    {
                        _result.Failure(10, new InvalidGrammarError("There should only be one empty \"error\" expression! You cannot have multiple \"error\" expressions in a \"try\" expression.", PeekPrevious().StartPosition, _currentToken.EndPosition));
                        return;
                    }

                    Node? @as = null;
                    if (isAsKeyword)
                    {
                        _result.RegisterAdvance();
                        Advance();

                        ParseExpression();
                        @as = _result.Node;
                        if (_result.Error != null)
                            return;

                        if (_currentToken.Type != TokenType.KeywordDo)
                        {
                            _result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"error\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                            return;
                        }
                    }

                    _result.RegisterAdvance();
                    Advance();

                    ParseStatement();
                    body = _result.Node;
                    if (_result.Error != null)
                        return;

                    if (error != null)
                        cases.Add(new Node?[3] { error, @as, body });
                    else
                        emptyCase = new Node?[2] { @as, body };
                }
                else if (isErrorNull && _currentToken.Type != TokenType.EndOfFile && _currentToken.Type != TokenType.NewLine)
                {
                    if (emptyCase != null)
                    {
                        Token previous = PeekPrevious();
                        _result.Failure(10, new InvalidGrammarError("There can't be any \"error\" expressions after an empty \"error\" expression!", previous.StartPosition, previous.EndPosition));
                        return;
                    }

                    ParseExpression();
                    error = _result.Node;
                    if (_result.Error != null)
                        return;

                    isErrorNull = false;
                    goto ErrorExpressionEvaluation;
                }
                else if (isErrorNull)
                {
                    _result.Failure(10, new InvalidGrammarError("Expected an expression or the 'as' or 'do' keywords! An expression after the 'error' keyword defines what error(s) will lead to the \"error\" expression, the 'as' keyword and the following expression declares where the error will be stored and the 'do' keyword declares the start of the body of the \"error\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }
                else
                {
                    _result.Failure(10, new InvalidGrammarError("Expected the 'as' or 'do' keywords! The 'as' keyword and the following expression tells the interpreter where the error will be stored and the 'do' keyword declares the start of the body of the \"error\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }
            }

            _result.Success(new TryNode(block, cases, emptyCase, startPosition, PeekPrevious().EndPosition));
        }

        /// <summary>
        /// Tries parsing a function definition expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordFunction"/>.
        /// </summary>
        private void ParseFunctionDefinitionExpression()
        {
            Position startPosition = _currentToken.StartPosition;
            _result.RegisterAdvance();
            Advance();

            Node? name = null;
            List<Node> parameters = new List<Node>();
            Node body;

            Position endPosition;
            bool isNameNull = true;

        FunctionDefinitionEvaluation:
            bool isWithKeyword = _currentToken.Type == TokenType.KeywordWith;
            if (_currentToken.Type == TokenType.KeywordDo || isWithKeyword)
            {
                if (isWithKeyword)
                {
                    _result.RegisterAdvance();
                    Advance();

                    SkipNewLines();

                    ParseExpression();
                    parameters.Add(_result.Node);
                    if (_result.Error != null)
                        return;

                    SkipNewLines();
                    while (_currentToken.Type == TokenType.Comma)
                    {
                        _result.RegisterAdvance();
                        Advance();

                        SkipNewLines();

                        ParseExpression();
                        parameters.Add(_result.Node);
                        if (_result.Error != null)
                            return;

                        SkipNewLines();
                    }

                    if (_currentToken.Type != TokenType.KeywordDo)
                    {
                        _result.Failure(10, new InvalidGrammarError("Expected a comma symbol or the 'do' keyword! The comma symbol and the following expression defines another parameter and the 'do' keyword declares the start of the body of the \"function\" definition.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }
                }

                _result.RegisterAdvance();
                Advance();

                if (_currentToken.Type == TokenType.NewLine)
                {
                    _result.RegisterAdvance();
                    Advance();

                    ParseStatements();
                    body = _result.Node;
                    if (_result.Error != null)
                        return;

                    if (_currentToken.Type != TokenType.KeywordEnd)
                    {
                        _result.Failure(10, new InvalidGrammarError("Expected the 'end' keyword! The 'end' keyword declares the end of the whole \"function\" definition.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }

                    endPosition = _currentToken.EndPosition;
                    _result.RegisterAdvance();
                    Advance();
                }
                else
                {
                    ParseStatement();
                    body = _result.Node;
                    if (_result.Error != null)
                        return;

                    endPosition = PeekPrevious().EndPosition;
                }
            }
            else if (isNameNull && _currentToken.Type != TokenType.EndOfFile && _currentToken.Type != TokenType.NewLine)
            {
                ParseExpression();
                name = _result.Node;
                if (_result.Error != null)
                    return;

                isNameNull = false;
                goto FunctionDefinitionEvaluation;
            }
            else if (isNameNull)
            {
                _result.Failure(10, new InvalidGrammarError("Expected an expression or the 'with' or 'do' keywords! An expression after the 'function' keyword defines the name, the 'with' keyword and the following expression(s, seperated by commas) declare(s) the paramenters and the 'do' keyword declares the start of the body of the \"function\".", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }
            else
            {
                _result.Failure(10, new InvalidGrammarError("Expected the 'with' or 'do' keywords! The 'with' keyword and the following expression(s, seperated by commas) declare(s) the paramenters and the 'do' keyword declares the start of the body of the \"function\".", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            _result.Success(new FunctionDefinitionNode(name, parameters, body, startPosition, endPosition));
        }

        // 'special' expressions are gone.

        /// <summary>
        /// Tries parsing an object definition expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordObject"/>.
        /// </summary>
        private void ParseObjectDefinitionExpression()
        {
            // The parents of the object are now defined after the 'from' keyword like an array. The 'from' keyword must come before the 'do' keyword and after the 'with' keyword.

            Position startPosition = _currentToken.StartPosition;
            _result.RegisterAdvance();
            Advance();

            Node? name = null;
            List<Node> parameters = new List<Node>();
            List<Node> parents = new List<Node>();
            Node body;

            Position endPosition;
            bool isNameNull = true;

        ObjectDefinitionEvaluation:
            bool isWithKeyword = _currentToken.Type == TokenType.KeywordWith;
            bool isFromKeyword = _currentToken.Type == TokenType.KeywordFrom;
            if (_currentToken.Type == TokenType.KeywordDo || isWithKeyword || isFromKeyword)
            {
                if (isWithKeyword)
                {
                    _result.RegisterAdvance();
                    Advance();

                    SkipNewLines();

                    ParseExpression();
                    parameters.Add(_result.Node);
                    if (_result.Error != null)
                        return;

                    SkipNewLines();
                    while (_currentToken.Type == TokenType.Comma)
                    {
                        _result.RegisterAdvance();
                        Advance();

                        SkipNewLines();

                        ParseExpression();
                        parameters.Add(_result.Node);
                        if (_result.Error != null)
                            return;

                        SkipNewLines();
                    }

                    isFromKeyword = _currentToken.Type == TokenType.KeywordFrom;
                    if (_currentToken.Type != TokenType.KeywordDo && !isFromKeyword)
                    {
                        _result.Failure(10, new InvalidGrammarError("Expected a comma symbol or the 'from' or 'do' keywords! The comma symbol and the following expression defines another parameter, the 'from' keyword and the following expression(s, seperated by commas) define(s) the parents of the \"object\" and the 'do' keyword declares the start of the body of the \"object\" definition.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }
                }

                if (isFromKeyword)
                {
                    _result.RegisterAdvance();
                    Advance();

                    SkipNewLines();

                    ParseExpression();
                    parents.Add(_result.Node);
                    if (_result.Error != null)
                        return;

                    SkipNewLines();
                    while (_currentToken.Type == TokenType.Comma)
                    {
                        _result.RegisterAdvance();
                        Advance();

                        SkipNewLines();

                        ParseExpression();
                        parents.Add(_result.Node);
                        if (_result.Error != null)
                            return;

                        SkipNewLines();
                    }

                    if (_currentToken.Type != TokenType.KeywordDo)
                    {
                        _result.Failure(10, new InvalidGrammarError("Expected a comma symbol or the 'do' keyword! The comma symbol and the following expression defines another parent and the 'do' keyword declares the start of the body of the \"object\" definition.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }
                }

                _result.RegisterAdvance();
                Advance();

                if (_currentToken.Type == TokenType.NewLine)
                {
                    _result.RegisterAdvance();
                    Advance();

                    ParseStatements();
                    body = _result.Node;
                    if (_result.Error != null)
                        return;

                    if (_currentToken.Type != TokenType.KeywordEnd)
                    {
                        _result.Failure(10, new InvalidGrammarError("Expected the 'end' keyword! The 'end' keyword declares the end of the whole \"object\" definition.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }

                    endPosition = _currentToken.EndPosition;
                    _result.RegisterAdvance();
                    Advance();
                }
                else
                {
                    ParseStatement();
                    body = _result.Node;
                    if (_result.Error != null)
                        return;

                    endPosition = PeekPrevious().EndPosition;
                }
            }
            else if (isNameNull && _currentToken.Type != TokenType.EndOfFile && _currentToken.Type != TokenType.NewLine)
            {
                ParseExpression();
                name = _result.Node;
                if (_result.Error != null)
                    return;

                isNameNull = false;
                goto ObjectDefinitionEvaluation;
            }
            else if (isNameNull)
            {
                _result.Failure(10, new InvalidGrammarError("Expected an expression or the 'with', 'from' or 'do' keywords! An expression after the 'object' keyword defines the name, the 'with' keyword and the following expression(s, seperated by commas) declare(s) the paramenters, the 'from' keyword and the following expression(s, seperated by commas) define(s) the parents of the \"object\" and the 'do' keyword declares the start of the body of the \"object\".", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }
            else
            {
                _result.Failure(10, new InvalidGrammarError("Expected the 'with', 'from' or 'do' keywords! The 'with' keyword and the following expression(s, seperated by commas) declare(s) the paramenters, the 'from' keyword and the following expression(s, seperated by commas) define(s) the parents of the \"object\" and the 'do' keyword declares the start of the body of the \"object\".", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            _result.Success(new ObjectDefinitionNode(name, parameters, parents, body, startPosition, endPosition));
        }

        /// <summary>
        /// Tries parsing an include expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordInclude"/>.
        /// </summary>
        private void ParseIncludeExpression()
        {
            Position startPosition = _currentToken.StartPosition;
            _result.RegisterAdvance();
            Advance();

            Node? subStructure = null;
            Node? nickname = null;
            bool isDumped = false;
            Node script;

            if (_currentToken.Type != TokenType.KeywordAll && _currentToken.Type != TokenType.Comma)
            {
                ParseExpression();
                subStructure = _result.Node;
                if (_result.Error != null)
                    return;
            }
            else
            {
                isDumped = true;
                _result.RegisterAdvance();
                Advance();
            }

            if (_currentToken.Type == TokenType.KeywordFrom)
            {
                _result.RegisterAdvance();
                Advance();

                ParseExpression();
                script = _result.Node;
                if (_result.Error != null)
                    return;
            }
            else if (subStructure != null)
            {
                script = subStructure;
                subStructure = null;
            }
            else
            {
                _result.Failure(10, new InvalidGrammarError("Expected the 'from' keyword! If a specific object being included from a script (when the object's name is provided after the 'include' keyword) or if the whole script is added to the script (using the 'all' keyword or a comma symbol after the 'include' keyword), the 'from' keyword followed by an expression declaring the script's name or path must be provided.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }
        
            if (_currentToken.Type == TokenType.KeywordAs)
            {
                _result.RegisterAdvance();
                Advance();

                ParseExpression();
                nickname = _result.Node;
                if (_result.Error != null)
                    return;
            }

            _result.Success(new IncludeNode(script, subStructure, isDumped, nickname, startPosition, PeekPrevious().EndPosition));
        }
    }

    /// <summary>
    /// The type of the object that is returned as the result of parsing done by the <see cref="Parser"/>.
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// The <see cref="EzrErrors.Error"/> that occurred while parsing, if any.
        /// </summary>
        public Error? Error;

        /// <summary>
        /// The <see cref="EzrNodes.Node"/> which is the result of the parsing.
        /// </summary>
        public Node Node;

        /// <summary>
        /// The amount of times the <see cref="Parser"/> advanced.
        /// </summary>
        public int AdvanceCount;

        /// <summary>
        /// The priority of the error held in the <see cref="ParseResult"/>.
        /// </summary>
        private int ErrorPriority;

        /// <summary>
        /// Creates a new <see cref="ParseResult"/> object.
        /// </summary>
        public ParseResult()
        {
            Error = null;
            Node = InvalidNode.StaticInvalidNode;
            AdvanceCount = 0;
            ErrorPriority = 0;
        }

        /// <summary>
        /// Registers an advancement by the <see cref="Parser"/>.
        /// </summary>
        public void RegisterAdvance()
        {
            AdvanceCount++;
        }

        /// <summary>
        /// Reverses <paramref name="reverseCount"/> number of advancements.
        /// </summary>
        /// <param name="reverseCount">The number of advancements to reverse <see cref="AdvanceCount"/> by.</param>
        public void Reverse(int reverseCount = 1)
        {
            AdvanceCount -= reverseCount;
        }

        /// <summary>
        /// Sets <see cref="Node"/> as the result of successful parsing.
        /// </summary>
        /// <param name="node">The <see cref="EzrNodes.Node"/> result of the parsing.</param>
        /// <returns>The same <see cref="ParseResult"/> object.</returns>
        public void Success(Node node)
        {
            Node = node;
        }

        /// <summary>
        /// Sets <see cref="Error"/> as the result of failed parsing.
        /// </summary>
        /// <remarks>
        /// If the <see cref="Error.Priority"/> of <paramref name="error"/> is greater than or equal to the <see cref="Error.Priority"/>
        /// of <see cref="Error"/>, then <paramref name="error"/> will override <see cref="Error"/>.
        /// </remarks>
        /// <param name="priority">The priority/fatality of the failure.</param>
        /// <param name="error">The <see cref="EzrErrors.Error"/> that occurred in parsing.</param>
        /// <returns>The same <see cref="ParseResult"/> object.</returns>
        public void Failure(int priority, Error error)
        {
            if (Error == null || ErrorPriority <= priority || AdvanceCount == 0)
            {
                ErrorPriority = priority;
                Error = error;
            }
        }
    }
}
