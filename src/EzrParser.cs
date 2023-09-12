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
        /// Creates a new <see cref="Parser"/> object.
        /// </summary>
        /// <param name="tokens">The <see cref="List{T}"/> of <see cref="Token"/> objects to be parsed.</param>
        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _currentToken = Token.GetInvalidToken(new Position(0, 0, string.Empty, string.Empty), null);
            _index = -1;
            _usingQuickSyntax = false;
            _wasUsingQuickSyntax = false;

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
            else
                _currentToken = Token.GetInvalidToken(_currentToken.StartPosition, _currentToken.EndPosition);
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
            else
                _currentToken = Token.GetInvalidToken(_currentToken.StartPosition, _currentToken.EndPosition);
        }

        /// <summary>
        /// Peeks at the previous <see cref="Token"/> object from <see cref="_index"/> in <see cref="_tokens"/>.
        /// </summary>
        /// <returns>The <see cref="Token"/> object.</returns>
        private Token PeekPrevious()
        {
            int previousIndex = _index - 1;
            if (previousIndex < 0 || previousIndex >= _tokens.Count)
                _currentToken = Token.GetInvalidToken(_currentToken.StartPosition, _currentToken.EndPosition);
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
        /// <param name="result">The <see cref="ParseResult"/> object to register the advancements.</param>
        /// <returns>The number of advancements.</returns>
        private int SkipNewLines(ParseResult result)
        {
            int newLineCount = 0;
            while (_currentToken.Type == TokenType.NewLine)
            {
                result.RegisterAdvance();
                Advance();

                newLineCount++;
            }

            return newLineCount;
        }

        /// <summary>
        /// Parses <see cref="_tokens"/>.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        public ParseResult Parse()
        {
            ParseResult result = Statements();
            if (result.Error == null && _currentToken.Type != TokenType.EndOfFile)
                return result.Failure(10, new InvalidGrammarError("Did not expect this!", _currentToken.StartPosition, _currentToken.EndPosition));
            return result;
        }

        /// <summary>
        /// Tries creating a <see cref="BinaryOperationNode"/>.
        /// </summary>
        /// <param name="left">The function to call for the first operand.</param>
        /// <param name="right">The function to call for the second operand. If <see langword="null"/>, defaults to <paramref name="left"/>.</param>
        /// <param name="operators">The operator <see cref="TokenType"/> object(s).</param>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult BinaryOperation(Func<ParseResult> left, Func<ParseResult>? right, params TokenType[] operators)
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;

            if (right == null)
                right = left;

            Node leftNode = result.Register(left());
            if (result.Error != null)
                return result;

            int toReverseTo = result.AdvanceCount;

            SkipNewLines(result);
            while (Array.IndexOf(operators, _currentToken.Type) != -1)
            {
                TokenType @operator = _currentToken.Type;
                result.RegisterAdvance();
                Advance();

                SkipNewLines(result);
                Node rightNode = result.Register(right());
                if (result.Error != null)
                    return result;

                leftNode = new BinaryOperationNode(leftNode, rightNode, @operator, startPosition, _currentToken.EndPosition);
                toReverseTo = result.AdvanceCount;
                SkipNewLines(result);
            }


            int reverseCount = result.AdvanceCount - toReverseTo;
            Reverse(reverseCount);
            result.Reverse(reverseCount);
            return result.Success(leftNode);
        }

        /// <summary>
        /// Tries creating a 'statements' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult Statements()
        {
            ParseResult result = new ParseResult();
            List<Node> statements = new List<Node>();
            Position startPosition = _currentToken.StartPosition;

            SkipNewLines(result);

            Node statement = result.Register(Statement());
            if (result.Error != null)
                return result;
            statements.Add(statement);

            while (true)
            {
                // NOTE: Qeyword 'l' for 'else if' is deprecated, use Qeyword 'e' instead in format:
                // 'e [check]: [statement(s)]'

                int newLineCount = SkipNewLines(result);
                if (newLineCount == 0 || _currentToken.Type == TokenType.EndOfFile
                    || _currentToken.Type == TokenType.KeywordEnd
                    || _currentToken.Type == TokenType.KeywordElse
                    || _currentToken.Type == TokenType.KeywordError
                    || (_usingQuickSyntax
                        && (_currentToken.Type == TokenType.QeywordL
                        || _currentToken.Type == TokenType.QeywordE
                        || _currentToken.Type == TokenType.QeywordS)))
                    break;

                statement = result.Register(Statement());
                if (result.Error != null)
                    return result;
                statements.Add(statement);
            }

            return result.Success(new ArrayLikeNode(statements, false, startPosition, _currentToken.EndPosition));
        }

        /// <summary>
        /// Tries creating a 'statement' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult Statement()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;

            Node expression;
            if (_currentToken.Type == TokenType.KeywordReturn)
            {
                Position possibleEndPosition = _currentToken.EndPosition;
                result.RegisterAdvance();
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
                    return result.Success(new ReturnNode(null, startPosition, possibleEndPosition));

                expression = result.Register(Expression());
                if (result.Error != null)
                    return result;

                return result.Success(new ReturnNode(expression, startPosition, _currentToken.EndPosition));
            }
            else if (_currentToken.Type == TokenType.KeywordSkip || _currentToken.Type == TokenType.KeywordStop)
            {
                Position endPosition = _currentToken.EndPosition;
                TokenType currentTokenType = _currentToken.Type;
                result.RegisterAdvance();
                Advance();

                return result.Success(new NoValueNode(currentTokenType, startPosition, endPosition));
            }

            expression = result.Register(Expression());
            if (result.Error != null)
                return result.Failure(4, new InvalidGrammarError("Expected a statement!", _currentToken.StartPosition, _currentToken.EndPosition));
            return result.Success(expression);
        }

        /// <summary>
        /// Tries creating a 'expression' structure.
        /// </summary>
        /// <param name="itemKeywordRequired">In cases where the variable assignment expression requires the 'item' keyword, set this to <see langword="true"/>.</param>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult Expression(bool itemKeywordRequired = false)
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;

            bool globalAssignment = false;
            bool usedItemKeyword = false;

            if (_currentToken.Type == TokenType.KeywordGlobal)
            {
                globalAssignment = true;
                result.RegisterAdvance();
                Advance();
            }
            
            if (_currentToken.Type == TokenType.KeywordItem)
            {
                usedItemKeyword = true;
                result.RegisterAdvance();
                Advance();
            }
            else if (itemKeywordRequired)
            {
                Node node_ = result.Register(QuickExpression());
                if (result.Error != null)
                    return result.Failure(4, new InvalidGrammarError("Expected an expression!", _currentToken.StartPosition, _currentToken.EndPosition));
                return result.Success(node_);
            }

            if (_currentToken.Type == TokenType.Identifier || _currentToken.TypeGroup == TokenTypeGroup.Qeyword)
            {
                Token possibleVariable = _currentToken;
                result.RegisterAdvance();
                Advance();

                if (_currentToken.TypeGroup != TokenTypeGroup.AssignmentSymbol)
                {
                    Reverse();
                    result.Reverse();

                    Node variable = result.Register(Junction());
                    if (result.Error != null)
                        return result;

                    if (_currentToken.TypeGroup == TokenTypeGroup.AssignmentSymbol)
                    {
                        TokenType assignmentOperator = _currentToken.Type;
                        result.RegisterAdvance();
                        Advance();

                        Node value = result.Register(Expression());
                        if (result.Error != null)
                            return result;

                        return result.Success(new NodeVariableAssignmentNode(variable, assignmentOperator, value, globalAssignment, startPosition, _currentToken.EndPosition));
                    }
                    else if (usedItemKeyword)
                        return result.Failure(10, new InvalidGrammarError("Expected an assignment symbol! The assignment symbol seperates the variable name and value, and declares how to handle any existing values in the variable, in a variable assignment expression. A few examples of assignment symbols are: (':') - normal assignment, (':+') - adds existing value in variable to new value, assigns the result, (':*') - multiplies existing value with new value, assigns the result, and (':&') - does a bitwise and operation between the existing and new value, assigns the result. There are equivalent symbols for all binary mathematical and bitwise operations.", _currentToken.StartPosition, _currentToken.EndPosition));
                    else
                    {
                        Reverse(result.AdvanceCount);
                        result.Reset();
                    }
                }
                else
                {
                    TokenType assignmentOperator = _currentToken.Type;
                    result.RegisterAdvance();
                    Advance();

                    Node value = result.Register(Expression());
                    if (result.Error != null)
                        return result;

                    return result.Success(new TokenVariableAssignmentNode(possibleVariable, assignmentOperator, value, globalAssignment, startPosition, _currentToken.EndPosition));
                }
            }
            else
            {
                if (usedItemKeyword)
                    return result.Failure(10, new InvalidGrammarError("Expected a variable name! The variable name is what will be assigned the value in a variable assignment expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                else
                {
                    Reverse(result.AdvanceCount);
                    result.Reset();
                }
            }

            Node node = result.Register(QuickExpression());
            if (result.Error != null)
                return result.Failure(4, new InvalidGrammarError("Expected an expression!", _currentToken.StartPosition, _currentToken.EndPosition));
            return result.Success(node);
        }

        /// <summary>
        /// Tries creating a 'quick-expression' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult QuickExpression()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;

            if (_currentToken.Type == TokenType.ExclamationMark)
            {
                result.RegisterAdvance();
                Advance();

                bool globalAssignment = false;
                bool usedItemKeyword = false;
                if (_currentToken.Type == TokenType.QeywordG)
                {
                    globalAssignment = true;
                    result.RegisterAdvance();
                    Advance();
                }

                if (_currentToken.Type == TokenType.QeywordD)
                {
                    usedItemKeyword = true;
                    result.RegisterAdvance();
                    Advance();
                }

                if (_currentToken.Type == TokenType.Identifier || _currentToken.TypeGroup == TokenTypeGroup.Qeyword)
                {
                    Token possibleVariable = _currentToken;
                    result.RegisterAdvance();
                    Advance();

                    if (_currentToken.TypeGroup != TokenTypeGroup.AssignmentSymbol)
                    {
                        Reverse();
                        result.Reverse();

                        Node variable = result.Register(Junction());
                        if (result.Error != null)
                            return result;

                        if (_currentToken.TypeGroup == TokenTypeGroup.AssignmentSymbol)
                        {
                            TokenType assignmentOperator = _currentToken.Type;
                            result.RegisterAdvance();
                            Advance();

                            Node value = result.Register(Expression());
                            if (result.Error != null)
                                return result;

                            return result.Success(new NodeVariableAssignmentNode(variable, assignmentOperator, value, globalAssignment, startPosition, _currentToken.EndPosition));
                        }
                        else if (usedItemKeyword)
                            return result.Failure(10, new InvalidGrammarError("Expected an assignment symbol! The assignment symbol seperates the variable name and value, and declares how to handle any existing values in the variable, in a variable assignment expression. A few examples of assignment symbols are: (':') - normal assignment, (':+') - adds existing value in variable to new value, assigns the result, (':*') - multiplies existing value with new value, assigns the result, and (':&') - does a bitwise and operation between the existing and new value, assigns the result. There are equivalent symbols for all binary mathematical and bitwise operations.", _currentToken.StartPosition, _currentToken.EndPosition));
                        else
                        {
                            Reverse(result.AdvanceCount);
                            result.Reset();
                        }
                    }
                    else
                    {
                        TokenType assignmentOperator = _currentToken.Type;
                        result.RegisterAdvance();
                        Advance();

                        Node value = result.Register(Expression());
                        if (result.Error != null)
                            return result;

                        return result.Success(new TokenVariableAssignmentNode(possibleVariable, assignmentOperator, value, globalAssignment, startPosition, _currentToken.EndPosition));
                    }
                }
                else
                {
                    if (usedItemKeyword)
                        return result.Failure(10, new InvalidGrammarError("Expected a variable name! The variable name is what will be assigned the value in a variable assignment expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    else
                    {
                        Reverse(result.AdvanceCount);
                        result.Reset();
                    }
                }
            }

            Node node = result.Register(Junction());
            if (result.Error != null)
                return result.Failure(4, new InvalidGrammarError("Expected a QuickSyntax expression!", _currentToken.StartPosition, _currentToken.EndPosition));
            return result.Success(node);
        }

        /// <summary>
        /// Tries creating a 'junction' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult Junction()
        {
            return BinaryOperation(Inversion, null, TokenType.KeywordAnd, TokenType.KeywordOr);
        }

        /// <summary>
        /// Tries creating a 'inversion' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult Inversion()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;

            if (_currentToken.Type == TokenType.ExclamationMark)
            {
                result.RegisterAdvance();
                Advance();

                if (_currentToken.Type == TokenType.QeywordV)
                {
                    TokenType @operator = _currentToken.Type;
                    result.RegisterAdvance();
                    Advance();

                    Node operand = result.Register(Expression());
                    if (result.Error != null)
                        return result;

                    return result.Success(new UnaryOperationNode(operand, @operator, startPosition, _currentToken.EndPosition));
                }
                else
                {
                    Reverse(result.AdvanceCount);
                    result.Reset();
                }
            }
            else if (_currentToken.Type == TokenType.KeywordInvert)
            {
                TokenType @operator = _currentToken.Type;
                result.RegisterAdvance();
                Advance();

                Node operand = result.Register(Expression());
                if (result.Error != null)
                    return result;

                return result.Success(new UnaryOperationNode(operand, @operator, startPosition, _currentToken.EndPosition));
            }

            Node node = result.Register(Comparison());
            if (result.Error != null)
                return result.Failure(4, new InvalidGrammarError("Expected an inversion expression!", _currentToken.StartPosition, _currentToken.EndPosition));
            return result.Success(node);
        }

        /// <summary>
        /// Tries creating a 'comparison' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult Comparison()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;

            Node left = result.Register(BitwiseOr());
            if (result.Error != null)
                return result;
            
            int toReverseTo = result.AdvanceCount;

            SkipNewLines(result);
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
                result.RegisterAdvance();
                Advance();

                SkipNewLines(result);
                if (@operator == TokenType.KeywordNot)
                {
                    if (_currentToken.Type != TokenType.KeywordIn)
                        return result.Failure(10, new InvalidGrammarError("Expected the 'in' keyword! The 'in' keyword is the second part of a check-not-in operation.", _currentToken.StartPosition, _currentToken.EndPosition));
                    result.RegisterAdvance();
                    Advance();
                }

                Node rightNode = result.Register(BitwiseOr());
                if (result.Error != null)
                    return result;

                left = new BinaryOperationNode(left, rightNode, @operator, startPosition, _currentToken.EndPosition);
                toReverseTo = result.AdvanceCount;
                SkipNewLines(result);
            }


            int reverseCount = result.AdvanceCount - toReverseTo;
            Reverse(reverseCount);
            result.Reverse(reverseCount);
            return result.Success(left);
        }

        /// <summary>
        /// Tries creating a 'bitwise-or' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult BitwiseOr()
        {
            return BinaryOperation(BitwiseXOr, null, TokenType.VerticalBar);
        }

        /// <summary>
        /// Tries creating a 'bitwise-xor' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult BitwiseXOr()
        {
            return BinaryOperation(BitwiseAnd, null, TokenType.Backslash);
        }

        /// <summary>
        /// Tries creating a 'bitwise-and' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult BitwiseAnd()
        {
            return BinaryOperation(BitwiseShift, null, TokenType.Ampersand);
        }

        /// <summary>
        /// Tries creating a 'bitwise-shift' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult BitwiseShift()
        {
            return BinaryOperation(ArithmeticExpression, null, TokenType.BitwiseLeftShift, TokenType.BitwiseRightShift);
        }

        /// <summary>
        /// Tries creating a 'arithmetic-expression' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult ArithmeticExpression()
        {
            return BinaryOperation(Term, null, TokenType.Plus, TokenType.HyphenMinus);
        }

        /// <summary>
        /// Tries creating a 'term' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult Term()
        {
            return BinaryOperation(Factor, null, TokenType.Asterisk, TokenType.Slash, TokenType.PercentSign);
        }

        /// <summary>
        /// Tries creating a 'factor' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult Factor()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;

            TokenType @operator = _currentToken.Type;
            if (@operator == TokenType.Plus || @operator == TokenType.HyphenMinus || @operator == TokenType.Tilde)
            {
                result.RegisterAdvance();
                Advance();

                Node operand = result.Register(Factor());
                if (result.Error != null)
                    return result;

                return result.Success(new UnaryOperationNode(operand, @operator, startPosition, _currentToken.EndPosition));
            }

            return Power();
        }

        /// <summary>
        /// Tries creating a 'power' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult Power()
        {
            return BinaryOperation(ObjectAttributeAccess, null, TokenType.Caret);
        }

        /// <summary>
        /// Tries creating a 'object-attribute-access' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult ObjectAttributeAccess()
        {
            return BinaryOperation(Call, ObjectAttributeAccess, TokenType.Period);
        }

        /// <summary>
        /// Tries creating a 'call' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult Call()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;

            Node node = result.Register(Atom());
            if (result.Error != null)
                return result;

            if(_currentToken.Type == TokenType.LeftParenthesis)
            {
                result.RegisterAdvance();
                Advance();

                Position possibleErrorStartPosition = _currentToken.StartPosition;

                SkipNewLines(result);
                Position endPosition;
                List<Node> arguments = new List<Node>();
                if (_currentToken.Type == TokenType.RightParenthesis)
                {
                    endPosition = _currentToken.EndPosition;
                    result.RegisterAdvance();
                    Advance();
                }
                else
                {

                    arguments.Add(result.Register(Expression()));
                    if (result.Error != null)
                        return result.Failure(10, new StackedError(result.Error, new InvalidGrammarError("Expected a right-parenthesis symbol! The function/object call expression must end with a right-parenthesis.", possibleErrorStartPosition, _currentToken.EndPosition)));
                        // return result.Failure(5, new InvalidGrammarError("Expected an expression or right-parenthesis symbol! The right-parenthesis ends the function/object call expression.", _currentToken.StartPosition, _currentToken.EndPosition));

                    SkipNewLines(result);
                    while (_currentToken.Type == TokenType.Comma)
                    {
                        possibleErrorStartPosition = _currentToken.StartPosition;
                        result.RegisterAdvance();
                        Advance();

                        SkipNewLines(result);
                        arguments.Add(result.Register(Expression()));
                        if (result.Error != null)
                            return result.Failure(10, new StackedError(result.Error, new InvalidGrammarError("Expected a right-parenthesis symbol! The function/object call expression must end with a right-parenthesis.", possibleErrorStartPosition, _currentToken.EndPosition)));
                        SkipNewLines(result);
                    }

                    if (_currentToken.Type != TokenType.RightParenthesis)
                        return result.Failure(10, new InvalidGrammarError("Expected a comma or right-parenthesis symbol! Commas are used to seperate the arguments of the function/object call expression, and the right-parenthesis is used to end it.", _currentToken.StartPosition, _currentToken.EndPosition));
                    endPosition = _currentToken.EndPosition;
                    result.RegisterAdvance();
                    Advance();
                }

                return result.Success(new CallNode(node, arguments, startPosition, endPosition));
            }
            return result.Success(node);
        }

        /// <summary>
        /// Tries creating a 'atom' structure.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult Atom()
        {
            ParseResult result = new ParseResult();

            Token startToken = _currentToken;
            if(startToken.TypeGroup == TokenTypeGroup.Value)
            {
                result.RegisterAdvance();
                Advance();

                return result.Success(new ValueNode(startToken, startToken.StartPosition, startToken.EndPosition));
            }
            else if (startToken.Type == TokenType.Identifier || startToken.TypeGroup == TokenTypeGroup.Qeyword)
            {
                result.RegisterAdvance();
                Advance();

                return result.Success(new VariableAccessNode(startToken, false, startToken.StartPosition, startToken.EndPosition));
            }
            else
            {
                switch (startToken.Type)
                {
                    case TokenType.KeywordGlobal:
                        result.RegisterAdvance();
                        Advance();

                        if (_currentToken.Type == TokenType.Identifier || _currentToken.TypeGroup == TokenTypeGroup.Qeyword)
                        {
                            Token variable = _currentToken;
                            Position endPosition = _currentToken.EndPosition;
                            result.RegisterAdvance();
                            Advance();

                            return result.Success(new VariableAccessNode(variable, true, startToken.StartPosition, endPosition));
                        }
                        return result.Failure(10, new InvalidGrammarError("Expected a variable name or the 'item' keyword! The variable name is used to access a global variable in a global variable access expression and the 'item' keyword is used to assign to a global variable in a global variable assignment expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    case TokenType.LeftParenthesis:
                        Node arrayNode = result.Register(ArrayStructure());
                        if (result.Error != null)
                            return result;
                        return result.Success(arrayNode);
                    case TokenType.LeftSquareBracket:
                        Node encapsulatedListNode = result.Register(ListStructure());
                        if (result.Error != null)
                            return result;
                        return result.Success(encapsulatedListNode);
                    case TokenType.LeftCurlyBracket:
                        Node dictionaryNode = result.Register(DictionaryStructure());
                        if (result.Error != null)
                            return result;
                        return result.Success(dictionaryNode);
                    case TokenType.KeywordIf:
                        Node ifNode = result.Register(IfStructure());
                        if (result.Error != null)
                            return result;
                        return result.Success(ifNode);
                    case TokenType.KeywordCount:
                        Node countNode = result.Register(CountStructure());
                        if (result.Error != null)
                            return result;
                        return result.Success(countNode);
                    case TokenType.KeywordWhile:
                        Node whileNode = result.Register(WhileStructure());
                        if (result.Error != null)
                            return result;
                        return result.Success(whileNode);
                    case TokenType.KeywordTry:
                        Node tryNode = result.Register(TryStructure());
                        if (result.Error != null)
                            return result;
                        return result.Success(tryNode);
                    case TokenType.KeywordFunction:
                        Node functionDefinitionNode = result.Register(FunctionDefinitionStructure());
                        if (result.Error != null)
                            return result;
                        return result.Success(functionDefinitionNode);
                    case TokenType.KeywordObject:
                        Node objectDefinitionNode = result.Register(ObjectDefinitionStructure());
                        if (result.Error != null)
                            return result;
                        return result.Success(objectDefinitionNode);
                    case TokenType.KeywordInclude:
                        Node includeNode = result.Register(IncludeStructure());
                        if (result.Error != null)
                            return result;
                        return result.Success(includeNode);
                    default:
                        return result.Failure(4, new InvalidGrammarError("Expected an integer, float, string, character, character list, identifier, 'if' expression, 'count' expression, 'while' expression and so on.", _currentToken.StartPosition, _currentToken.EndPosition));
                }
            }
        }

        /// <summary>
        /// Creates the structure of an array, an <see cref="ArrayLikeNode"/> with <see cref="ArrayLikeNode.CreateList"/> set to <see langword="false"/> OR a parenthetical expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.LeftParenthesis"/>.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult ArrayStructure()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;
            result.RegisterAdvance();
            Advance();

            Position possibleErrorStartPosition = _currentToken.StartPosition;
            List<Node> elements = new List<Node>();
            bool isArray = false;

            SkipNewLines(result);
            Position endPosition;
            if (_currentToken.Type == TokenType.RightParenthesis)
            {
                endPosition = _currentToken.EndPosition;
                result.RegisterAdvance();
                Advance();
            }
            else
            {
                elements.Add(result.Register(Expression()));
                if (result.Error != null)
                    return result.Failure(10, new StackedError(result.Error, new InvalidGrammarError("Expected a right-parenthesis symbol! An array/parenthetical expression must end with a right-parenthesis.", possibleErrorStartPosition, _currentToken.EndPosition)));
                // return result.Failure(5, new InvalidGrammarError("Expected an expression or right-parenthesis symbol! The right-parenthesis closes off an empty array.", _currentToken.StartPosition, _currentToken.EndPosition));

                SkipNewLines(result);
                while (_currentToken.Type == TokenType.Comma)
                {
                    isArray = true;
                    possibleErrorStartPosition = _currentToken.StartPosition;
                    result.RegisterAdvance();
                    Advance();

                    SkipNewLines(result);
                    if (_currentToken.Type == TokenType.RightParenthesis && elements.Count == 1)
                        break;

                    Node element = result.Register(Expression());
                    if (result.Error != null)
                    {
                        if (elements.Count > 1)
                            return result.Failure(10, new StackedError(result.Error, new InvalidGrammarError("Expected a right-parenthesis symbol! An array must end with a right-parenthesis.", possibleErrorStartPosition, _currentToken.EndPosition)));

                        Position errorStartPosition = possibleErrorStartPosition.Copy();
                        errorStartPosition.Advance();
                        return result.Failure(10, new StackedError(result.Error, new InvalidGrammarError("Expected a right-parenthesis symbol! An array must end with a right-parenthesis.", errorStartPosition, _currentToken.EndPosition)));
                    }

                    elements.Add(element);
                    SkipNewLines(result);
                }

                if (_currentToken.Type != TokenType.RightParenthesis)
                {
                    if (isArray)
                        return result.Failure(10, new InvalidGrammarError("Expected a comma or a right-parenthesis symbol! Commas seperate the elements of the array, while the right-parenthesis ends it.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return result.Failure(10, new InvalidGrammarError("Expected a comma or a right-parenthesis symbol! The comma is used to create an array and seperate its elements, while the right-parenthesis declares the end of a parenthetical expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                }
                
                endPosition = _currentToken.EndPosition;
                result.RegisterAdvance();
                Advance();
            }

            if (!isArray && elements.Count > 0)
                return result.Success(elements[0]);
            return result.Success(new ArrayLikeNode(elements, false, startPosition, endPosition));
        }

        /// <summary>
        /// Creates the structure of a list, an <see cref="ArrayLikeNode"/> with <see cref="ArrayLikeNode.CreateList"/> set to <see langword="true"/>. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.LeftSquareBracket"/>.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult ListStructure()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;
            result.RegisterAdvance();
            Advance();

            Position possibleErrorStartPosition = _currentToken.StartPosition;

            SkipNewLines(result);
            Position endPosition;
            List<Node> elements = new List<Node>();
            if (_currentToken.Type == TokenType.RightSquareBracket)
            {
                endPosition = _currentToken.EndPosition;
                result.RegisterAdvance();
                Advance();
            }
            else
            {
                elements.Add(result.Register(Expression()));
                if (result.Error != null)
                    return result.Failure(10, new StackedError(result.Error, new InvalidGrammarError("Expected a right-square-bracket symbol! A list expression must end with a right-square-bracket.", possibleErrorStartPosition, _currentToken.EndPosition)));
                // return result.Failure(5, new InvalidGrammarError("Expected an expression or right-square-bracket symbol! The right-square-bracket ends the list expression.", _currentToken.StartPosition, _currentToken.EndPosition));

                SkipNewLines(result);
                while (_currentToken.Type == TokenType.Comma)
                {
                    possibleErrorStartPosition = _currentToken.StartPosition;
                    result.RegisterAdvance();
                    Advance();

                    SkipNewLines(result);
                    elements.Add(result.Register(Expression()));
                    if (result.Error != null)
                        return result.Failure(10, new StackedError(result.Error, new InvalidGrammarError("Expected a right-square-bracket symbol! A list expression must end with a right-square-bracket.", possibleErrorStartPosition, _currentToken.EndPosition)));
                    SkipNewLines(result);
                }

                if (_currentToken.Type != TokenType.RightSquareBracket)
                    return result.Failure(10, new InvalidGrammarError("Expected a comma or a right-square-bracket symbol! Commas are used to seperate elements in the list, while the right-square-bracket ends it.", _currentToken.StartPosition, _currentToken.EndPosition));
                endPosition = _currentToken.EndPosition;
                result.RegisterAdvance();
                Advance();
            }

            return result.Success(new ArrayLikeNode(elements, true, startPosition, endPosition));
        }

        /// <summary>
        /// Creates the structure of a dictionary. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.LeftCurlyBracket"/>.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult DictionaryStructure()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;
            result.RegisterAdvance();
            Advance();

            Position possibleErrorStartPosition = _currentToken.StartPosition;
            List<Node[]> pairs = new List<Node[]>();

            SkipNewLines(result);
            Position endPosition;
            if (_currentToken.Type == TokenType.RightCurlyBracket)
            {
                endPosition = _currentToken.EndPosition;
                result.RegisterAdvance();
                Advance();
            }
            else
            {
                Node left = result.Register(Expression(true));
                if (result.Error != null)
                    return result.Failure(10, new StackedError(result.Error, new InvalidGrammarError("Expected a right-curly-bracket symbol! A dictionary expression must end with a right-curly-bracket.", possibleErrorStartPosition, _currentToken.EndPosition)));
                // return result.Failure(5, new InvalidGrammarError("Expected an expression or a right-curly-bracket symbol! The right-curly-bracket declares the end of the dictionary.", _currentToken.StartPosition, _currentToken.EndPosition));

                SkipNewLines(result);
                
                // NOTE: Breaking change - assignment symbols will break this.
                if (_currentToken.Type != TokenType.Colon)
                    return result.Failure(10, new InvalidGrammarError("Expected a colon symbol! The colon is the seperator between a key and its value in a dictionary.", _currentToken.StartPosition, _currentToken.EndPosition));
                result.RegisterAdvance();
                Advance();

                SkipNewLines(result);
                Node right = result.Register(Expression());
                if (result.Error != null)
                    return result;

                pairs.Add(new Node[2] { left, right });
                SkipNewLines(result);

                while (_currentToken.Type == TokenType.Comma)
                {
                    possibleErrorStartPosition = _currentToken.StartPosition;
                    result.RegisterAdvance();
                    Advance();

                    SkipNewLines(result);
                    left = result.Register(Expression(true));
                    if (result.Error != null)
                        return result.Failure(10, new StackedError(result.Error, new InvalidGrammarError("Expected a right-curly-bracket symbol! A dictionary expression must end with a right-curly-bracket.", possibleErrorStartPosition, _currentToken.EndPosition)));

                    SkipNewLines(result);
                    if (_currentToken.Type != TokenType.Colon)
                        return result.Failure(10, new InvalidGrammarError("Expected a colon symbol! The colon is the seperator between a key and its value in a dictionary.", _currentToken.StartPosition, _currentToken.EndPosition));
                    result.RegisterAdvance();
                    Advance();

                    SkipNewLines(result);
                    right = result.Register(Expression());
                    if (result.Error != null)
                        return result;

                    pairs.Add(new Node[2] { left, right });
                    SkipNewLines(result);
                }

                if (_currentToken.Type != TokenType.RightCurlyBracket)
                    return result.Failure(10, new InvalidGrammarError("Expected a comma or right-curly-bracket symbol! Commas are used to seperate key-value pairs in the dictionary, and the right-curly-bracket declares its end.", _currentToken.StartPosition, _currentToken.EndPosition));
                endPosition = _currentToken.EndPosition;
                result.RegisterAdvance();
                Advance();
            }

            return result.Success(new DictionaryNode(pairs, startPosition, endPosition));
        }

        /// <summary>
        /// Creates the structure of an if expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordIf"/>.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult IfStructure()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;
            result.RegisterAdvance();
            Advance();

            List<Node[]> cases = new List<Node[]>();
            Node? elseCase = null;

            Node condition = result.Register(Expression());
            if (result.Error != null)
                return result;

            if (_currentToken.Type != TokenType.KeywordDo)
                return result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
            result.RegisterAdvance();
            Advance();

            Node body;
            if (_currentToken.Type == TokenType.NewLine)
            {
                result.RegisterAdvance();
                Advance();

                body = result.Register(Statements());
                if (result.Error != null)
                    return result;

                Position endPosition;
                cases.Add(new Node[2] { condition, body });
                if (_currentToken.Type == TokenType.KeywordEnd)
                {
                    endPosition = _currentToken.EndPosition;
                    result.RegisterAdvance();
                    Advance();
                }
                else if (_currentToken.Type == TokenType.KeywordElse)
                {
                    while (_currentToken.Type == TokenType.KeywordElse)
                    {
                        result.RegisterAdvance();
                        Advance();

                        if (_currentToken.Type == TokenType.KeywordIf)
                        {
                            if (elseCase != null)
                                return result.Failure(10, new InvalidGrammarError("The \"else-if\" expression cannot be declared before the \"else\" expression! You cannot have \"else\" expressions before or in-between \"else if\" expressions.", PeekPrevious().StartPosition, _currentToken.EndPosition));

                            result.RegisterAdvance();
                            Advance();

                            condition = result.Register(Expression());
                            if (result.Error != null)
                                return result;

                            if (_currentToken.Type != TokenType.KeywordDo)
                                return result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"else if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                            result.RegisterAdvance();
                            Advance();

                            body = result.Register(Statements());
                            if (result.Error != null)
                                return result;

                            cases.Add(new Node[2] { condition, body });
                        }
                        else if (_currentToken.Type != TokenType.KeywordDo)
                            return result.Failure(10, new InvalidGrammarError("Expected the 'if' or 'do' keywords! The 'if' keyword declares the start of an \"else if\" expression, and the 'do' keyword declares the start of the body of an \"else\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        else
                        {
                            if (elseCase != null)
                                return result.Failure(10, new InvalidGrammarError("There should only be one \"else\" expression! You cannot have multiple \"else\" expressions in an \"if\" expression.", PeekPrevious().StartPosition, _currentToken.EndPosition));

                            result.RegisterAdvance();
                            Advance();

                            elseCase = result.Register(Statements());
                            if (result.Error != null)
                                return result;
                        }
                    }

                    if (_currentToken.Type != TokenType.KeywordEnd)
                    {
                        if (elseCase == null)
                            return result.Failure(10, new InvalidGrammarError("Expected the 'else' or 'end' keywords! The 'else' keyword defines the start of an \"else\" or \"else if\" expression, and the 'end' keyword declares the end of the whole \"if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return result.Failure(10, new InvalidGrammarError("Expected the 'end' keyword! The 'end' keyword declares the end of the whole \"if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    }

                    endPosition = _currentToken.EndPosition;
                    result.RegisterAdvance();
                    Advance();
                }
                else
                    return result.Failure(10, new InvalidGrammarError("Expected the 'else' or 'end' keywords! The 'else' keyword defines the start of an \"else\" or \"else if\" expression, and the 'end' keyword declares the end of the whole \"if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));

                return result.Success(new IfNode(cases, elseCase, startPosition, endPosition));
            }

            body = result.Register(Statement());
            if (result.Error != null)
                return result;

            cases.Add(new Node[2] { condition, body });
            while (_currentToken.Type == TokenType.KeywordElse)
            {
                result.RegisterAdvance();
                Advance();

                if (_currentToken.Type == TokenType.KeywordIf)
                {
                    if (elseCase != null)
                        return result.Failure(10, new InvalidGrammarError("The \"else-if\" expression cannot be declared before the \"else\" expression! You cannot have \"else\" expressions before or in-between \"else if\" expressions.", PeekPrevious().StartPosition, _currentToken.EndPosition));

                    result.RegisterAdvance();
                    Advance();

                    condition = result.Register(Expression());
                    if (result.Error != null)
                        return result;

                    if (_currentToken.Type != TokenType.KeywordDo)
                        return result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"else if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    result.RegisterAdvance();
                    Advance();

                    body = result.Register(Statement());
                    if (result.Error != null)
                        return result;

                    cases.Add(new Node[2] { condition, body });
                }
                else if (_currentToken.Type != TokenType.KeywordDo)
                    return result.Failure(10, new InvalidGrammarError("Expected the 'if' or 'do' keywords! The 'if' keyword declares the start of an \"else if\" expression, and the 'do' keyword declares the start of the body of an \"else\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                else
                {
                    if (elseCase != null)
                        return result.Failure(10, new InvalidGrammarError("There should only be one \"else\" expression! You cannot have multiple \"else\" expressions in an \"if\" expression.", PeekPrevious().StartPosition, _currentToken.EndPosition));

                    result.RegisterAdvance();
                    Advance();

                    elseCase = result.Register(Statement());
                    if (result.Error != null)
                        return result;
                }
            }

            return result.Success(new IfNode(cases, elseCase, startPosition, PeekPrevious().EndPosition));
        }

        /// <summary>
        /// Creates the structure of a count expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordCount"/>.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult CountStructure()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;
            result.RegisterAdvance();
            Advance();

            Node to = InvalidNode.StaticInvalidNode;
            Node? from = null;
            Node? step = null;
            Node? @as = null;

            if (_currentToken.Type == TokenType.KeywordFrom)
            {
                result.RegisterAdvance();
                Advance();

                from = result.Register(Expression());
                if (result.Error != null)
                    return result;
            }

            if (_currentToken.Type != TokenType.KeywordTo)
            {
                if (from == null)
                    return result.Failure(10, new InvalidGrammarError("Expected the 'to' or 'from' keyword! The 'to' keyword and the following expression is the amount to count to in the count loop, and the optional 'from' keyword and the following expression is the amount to count from.", _currentToken.StartPosition, _currentToken.EndPosition));
                return result.Failure(10, new InvalidGrammarError("Expected the 'to' keyword! The 'to' keyword and the following expression is the amount to count to in the count loop.", _currentToken.StartPosition, _currentToken.EndPosition));
            }

            result.RegisterAdvance();
            Advance();

            to = result.Register(Expression());
            if (result.Error != null)
                return result;

            if (_currentToken.Type == TokenType.KeywordStep)
            {
                result.RegisterAdvance();
                Advance();

                step = result.Register(Expression());
                if (result.Error != null)
                    return result;
            }

            if (_currentToken.Type == TokenType.KeywordAs)
            {
                result.RegisterAdvance();
                Advance();

                @as = result.Register(Expression());
                if (result.Error != null)
                    return result;
            }

            if (_currentToken.Type != TokenType.KeywordDo)
            {
                if (step == null && @as == null)
                    return result.Failure(10, new InvalidGrammarError("Expected the 'do', 'step' or 'as' keyword! The 'do' keyword declares the start of the body of the count loop, the optional 'step' keyword and the following expression is the increment, and the optional 'as' keyword and the following expression is where the iterations are stored.", _currentToken.StartPosition, _currentToken.EndPosition));
                else if (@as == null)    
                    return result.Failure(10, new InvalidGrammarError("Expected the 'do' or 'as' keyword! The 'do' keyword declares the start of the body of the count loop, and the optional 'as' keyword and the following expression is where the iterations are stored.", _currentToken.StartPosition, _currentToken.EndPosition));
                return result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the count loop.", _currentToken.StartPosition, _currentToken.EndPosition));
            }

            result.RegisterAdvance();
            Advance();

            Node body;
            Position endPosition;
            if (_currentToken.Type == TokenType.NewLine)
            {
                result.RegisterAdvance();
                Advance();

                body = result.Register(Statements());
                if (result.Error != null)
                    return result;

                if (_currentToken.Type != TokenType.KeywordEnd)
                    return result.Failure(10, new InvalidGrammarError("Expected the 'end' keyword! The 'end' keyword declares the end of the count loop.", _currentToken.StartPosition, _currentToken.EndPosition));

                endPosition = _currentToken.EndPosition;
                result.RegisterAdvance();
                Advance();
            }
            else
            {
                body = result.Register(Statement());
                if (result.Error != null)
                    return result;

                endPosition = PeekPrevious().EndPosition;
            }

            return result.Success(new CountNode(to, from, step, @as, body, startPosition, endPosition));
        }

        /// <summary>
        /// Creates the structure of a while expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordWhile"/>.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult WhileStructure()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;
            result.RegisterAdvance();
            Advance();

            Node condition = result.Register(Expression());
            if (result.Error != null)
                return result;

            if (_currentToken.Type != TokenType.KeywordDo)
                return result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the while loop.", _currentToken.StartPosition, _currentToken.EndPosition));
            result.RegisterAdvance();
            Advance();

            Node body;
            Position endPosition;
            if (_currentToken.Type == TokenType.NewLine)
            {
                result.RegisterAdvance();
                Advance();

                body = result.Register(Statements());
                if (result.Error != null)
                    return result;

                if (_currentToken.Type != TokenType.KeywordEnd)
                    return result.Failure(10, new InvalidGrammarError("Expected the 'end' keyword! The 'end' keyword declares the end of the while loop.", _currentToken.StartPosition, _currentToken.EndPosition));

                endPosition = _currentToken.EndPosition;
                result.RegisterAdvance();
                Advance();
            }
            else
            {
                body = result.Register(Statement());
                if (result.Error != null)
                    return result;

                endPosition = PeekPrevious().EndPosition;
            }

            return result.Success(new WhileNode(condition, body, startPosition, endPosition));
        }

        /// <summary>
        /// Creates the structure of a try expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordTry"/>.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult TryStructure()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;
            result.RegisterAdvance();
            Advance();

            Node block;
            List<Node?[]> cases = new List<Node?[]>();
            Node?[]? emptyCase = null;

            if (_currentToken.Type != TokenType.KeywordDo)
                return result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"try\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
            result.RegisterAdvance();
            Advance();

            Node body;
            if (_currentToken.Type == TokenType.NewLine)
            {
                result.RegisterAdvance();
                Advance();

                block = result.Register(Statements());
                if (result.Error != null)
                    return result;

                Position endPosition;
                if (_currentToken.Type == TokenType.KeywordEnd)
                {
                    endPosition = _currentToken.EndPosition;
                    result.RegisterAdvance();
                    Advance();
                }
                else if (_currentToken.Type == TokenType.KeywordError)
                {
                    while (_currentToken.Type == TokenType.KeywordError)
                    {
                        result.RegisterAdvance();
                        Advance();

                        Node? error = null;
                        bool isErrorNull = true;

                    ErrorExpressionEvaluation:
                        bool isAsKeyword = _currentToken.Type == TokenType.KeywordAs;
                        if (_currentToken.Type == TokenType.KeywordDo || isAsKeyword)
                        {
                            if (emptyCase != null)
                                return result.Failure(10, new InvalidGrammarError("There should only be one empty \"error\" expression! You cannot have multiple \"error\" expressions in a \"try\" expression.", PeekPrevious().StartPosition, _currentToken.EndPosition));

                            Node? @as = null;
                            if (isAsKeyword)
                            {
                                result.RegisterAdvance();
                                Advance();

                                @as = result.Register(Expression());
                                if (result.Error != null)
                                    return result;

                                if (_currentToken.Type != TokenType.KeywordDo)
                                    return result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"error\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                            }

                            result.RegisterAdvance();
                            Advance();

                            body = result.Register(Statements());
                            if (result.Error != null)
                                return result;

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
                                return result.Failure(10, new InvalidGrammarError("There can't be any \"error\" expressions after an empty \"error\" expression!", previous.StartPosition, previous.EndPosition));
                            }

                            error = result.Register(Expression());
                            if (result.Error != null)
                                return result;

                            isErrorNull = false;
                            goto ErrorExpressionEvaluation;
                        }
                        else if (isErrorNull)
                            return result.Failure(10, new InvalidGrammarError("Expected an expression or the 'as' or 'do' keywords! An expression after the 'error' keyword defines what error(s) will lead to the \"error\" expression, the 'as' keyword and the following expression declares where the error will be stored and the 'do' keyword declares the start of the body of the \"error\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        else
                            return result.Failure(10, new InvalidGrammarError("Expected the 'as' or 'do' keywords! The 'as' keyword and the following expression tells the interpreter where the error will be stored and the 'do' keyword declares the start of the body of the \"error\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    }

                    if (_currentToken.Type != TokenType.KeywordEnd)
                    {
                        if (emptyCase == null)
                            return result.Failure(10, new InvalidGrammarError("Expected the 'error' or 'end' keywords! The 'error' keyword defines the start of an \"error\" expression, and the 'end' keyword declares the end of the whole \"try\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return result.Failure(10, new InvalidGrammarError("Expected the 'end' keyword! The 'end' keyword declares the end of the whole \"try\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    }

                    endPosition = _currentToken.EndPosition;
                    result.RegisterAdvance();
                    Advance();
                }
                else
                    return result.Failure(10, new InvalidGrammarError("Expected the 'error' or 'end' keywords! The 'error' keyword defines the start of an \"error\" expression, and the 'end' keyword declares the end of the whole \"try\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));

                return result.Success(new TryNode(block, cases, emptyCase, startPosition, endPosition));
            }

            block = result.Register(Statement());
            if (result.Error != null)
                return result;

            while (_currentToken.Type == TokenType.KeywordError)
            {
                result.RegisterAdvance();
                Advance();

                Node? error = null;
                bool isErrorNull = true;

            ErrorExpressionEvaluation:
                bool isAsKeyword = _currentToken.Type == TokenType.KeywordAs;
                if (_currentToken.Type == TokenType.KeywordDo || isAsKeyword)
                {
                    if (emptyCase != null)
                        return result.Failure(10, new InvalidGrammarError("There should only be one empty \"error\" expression! You cannot have multiple \"error\" expressions in a \"try\" expression.", PeekPrevious().StartPosition, _currentToken.EndPosition));

                    Node? @as = null;
                    if (isAsKeyword)
                    {
                        result.RegisterAdvance();
                        Advance();

                        @as = result.Register(Expression());
                        if (result.Error != null)
                            return result;

                        if (_currentToken.Type != TokenType.KeywordDo)
                            return result.Failure(10, new InvalidGrammarError("Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"error\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    }

                    result.RegisterAdvance();
                    Advance();

                    body = result.Register(Statement());
                    if (result.Error != null)
                        return result;

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
                        return result.Failure(10, new InvalidGrammarError("There can't be any \"error\" expressions after an empty \"error\" expression!", previous.StartPosition, previous.EndPosition));
                    }

                    error = result.Register(Expression());
                    if (result.Error != null)
                        return result;

                    isErrorNull = false;
                    goto ErrorExpressionEvaluation;
                }
                else if (isErrorNull)
                    return result.Failure(10, new InvalidGrammarError("Expected an expression or the 'as' or 'do' keywords! An expression after the 'error' keyword defines what error(s) will lead to the \"error\" expression, the 'as' keyword and the following expression declares where the error will be stored and the 'do' keyword declares the start of the body of the \"error\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                else    
                    return result.Failure(10, new InvalidGrammarError("Expected the 'as' or 'do' keywords! The 'as' keyword and the following expression tells the interpreter where the error will be stored and the 'do' keyword declares the start of the body of the \"error\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
            }

            return result.Success(new TryNode(block, cases, emptyCase, startPosition, PeekPrevious().EndPosition));
        }

        /// <summary>
        /// Creates the structure of a function definition expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordFunction"/>.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult FunctionDefinitionStructure()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;
            result.RegisterAdvance();
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
                    result.RegisterAdvance();
                    Advance();

                    SkipNewLines(result);
                    parameters.Add(result.Register(Expression()));
                    if (result.Error != null)
                        return result;

                    SkipNewLines(result);
                    while (_currentToken.Type == TokenType.Comma)
                    {
                        result.RegisterAdvance();
                        Advance();

                        SkipNewLines(result);
                        parameters.Add(result.Register(Expression()));
                        if (result.Error != null)
                            return result;

                        SkipNewLines(result);
                    }

                    if (_currentToken.Type != TokenType.KeywordDo)
                        return result.Failure(10, new InvalidGrammarError("Expected a comma symbol or the 'do' keyword! The comma symbol and the following expression defines another parameter and the 'do' keyword declares the start of the body of the \"function\" definition.", _currentToken.StartPosition, _currentToken.EndPosition));
                }

                result.RegisterAdvance();
                Advance();

                if (_currentToken.Type == TokenType.NewLine)
                {
                    result.RegisterAdvance();
                    Advance();

                    body = result.Register(Statements());
                    if (result.Error != null)
                        return result;

                    if (_currentToken.Type != TokenType.KeywordEnd)
                        return result.Failure(10, new InvalidGrammarError("Expected the 'end' keyword! The 'end' keyword declares the end of the whole \"function\" definition.", _currentToken.StartPosition, _currentToken.EndPosition));

                    endPosition = _currentToken.EndPosition;
                    result.RegisterAdvance();
                    Advance();
                }
                else
                {
                    body = result.Register(Statement());
                    if (result.Error != null)
                        return result;

                    endPosition = PeekPrevious().EndPosition;
                }
            }
            else if (isNameNull && _currentToken.Type != TokenType.EndOfFile && _currentToken.Type != TokenType.NewLine)
            {
                name = result.Register(Expression());
                if (result.Error != null)
                    return result;

                isNameNull = false;
                goto FunctionDefinitionEvaluation;
            }
            else if (isNameNull)
                return result.Failure(10, new InvalidGrammarError("Expected an expression or the 'with' or 'do' keywords! An expression after the 'function' keyword defines the name, the 'with' keyword and the following expression(s, seperated by commas) declare(s) the paramenters and the 'do' keyword declares the start of the body of the \"function\".", _currentToken.StartPosition, _currentToken.EndPosition));
            else
                return result.Failure(10, new InvalidGrammarError("Expected the 'with' or 'do' keywords! The 'with' keyword and the following expression(s, seperated by commas) declare(s) the paramenters and the 'do' keyword declares the start of the body of the \"function\".", _currentToken.StartPosition, _currentToken.EndPosition));

            return result.Success(new FunctionDefinitionNode(name, parameters, body, startPosition, endPosition));
        }

        // 'special' expressions are gone.

        /// <summary>
        /// Creates the structure of an object definition expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordObject"/>.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult ObjectDefinitionStructure()
        {
            // The parents of the object are now defined after the 'from' keyword like an array. The 'from' keyword must come before the 'do' keyword and after the 'with' keyword.

            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;
            result.RegisterAdvance();
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
                    result.RegisterAdvance();
                    Advance();

                    SkipNewLines(result);
                    parameters.Add(result.Register(Expression()));
                    if (result.Error != null)
                        return result;

                    SkipNewLines(result);
                    while (_currentToken.Type == TokenType.Comma)
                    {
                        result.RegisterAdvance();
                        Advance();

                        SkipNewLines(result);
                        parameters.Add(result.Register(Expression()));
                        if (result.Error != null)
                            return result;

                        SkipNewLines(result);
                    }

                    isFromKeyword = _currentToken.Type == TokenType.KeywordFrom;
                    if (_currentToken.Type != TokenType.KeywordDo && !isFromKeyword)
                        return result.Failure(10, new InvalidGrammarError("Expected a comma symbol or the 'from' or 'do' keywords! The comma symbol and the following expression defines another parameter, the 'from' keyword and the following expression(s, seperated by commas) define(s) the parents of the \"object\" and the 'do' keyword declares the start of the body of the \"object\" definition.", _currentToken.StartPosition, _currentToken.EndPosition));
                }

                if (isFromKeyword)
                {
                    result.RegisterAdvance();
                    Advance();

                    SkipNewLines(result);
                    parents.Add(result.Register(Expression()));
                    if (result.Error != null)
                        return result;

                    SkipNewLines(result);
                    while (_currentToken.Type == TokenType.Comma)
                    {
                        result.RegisterAdvance();
                        Advance();

                        SkipNewLines(result);
                        parents.Add(result.Register(Expression()));
                        if (result.Error != null)
                            return result;

                        SkipNewLines(result);
                    }

                    if (_currentToken.Type != TokenType.KeywordDo)
                        return result.Failure(10, new InvalidGrammarError("Expected a comma symbol or the 'do' keyword! The comma symbol and the following expression defines another parent and the 'do' keyword declares the start of the body of the \"object\" definition.", _currentToken.StartPosition, _currentToken.EndPosition));
                }

                result.RegisterAdvance();
                Advance();

                if (_currentToken.Type == TokenType.NewLine)
                {
                    result.RegisterAdvance();
                    Advance();

                    body = result.Register(Statements());
                    if (result.Error != null)
                        return result;

                    if (_currentToken.Type != TokenType.KeywordEnd)
                        return result.Failure(10, new InvalidGrammarError("Expected the 'end' keyword! The 'end' keyword declares the end of the whole \"object\" definition.", _currentToken.StartPosition, _currentToken.EndPosition));

                    endPosition = _currentToken.EndPosition;
                    result.RegisterAdvance();
                    Advance();
                }
                else
                {
                    body = result.Register(Statement());
                    if (result.Error != null)
                        return result;

                    endPosition = PeekPrevious().EndPosition;
                }
            }
            else if (isNameNull && _currentToken.Type != TokenType.EndOfFile && _currentToken.Type != TokenType.NewLine)
            {
                name = result.Register(Expression());
                if (result.Error != null)
                    return result;

                isNameNull = false;
                goto ObjectDefinitionEvaluation;
            }
            else if (isNameNull)
                return result.Failure(10, new InvalidGrammarError("Expected an expression or the 'with', 'from' or 'do' keywords! An expression after the 'object' keyword defines the name, the 'with' keyword and the following expression(s, seperated by commas) declare(s) the paramenters, the 'from' keyword and the following expression(s, seperated by commas) define(s) the parents of the \"object\" and the 'do' keyword declares the start of the body of the \"object\".", _currentToken.StartPosition, _currentToken.EndPosition));
            else
                return result.Failure(10, new InvalidGrammarError("Expected the 'with', 'from' or 'do' keywords! The 'with' keyword and the following expression(s, seperated by commas) declare(s) the paramenters, the 'from' keyword and the following expression(s, seperated by commas) define(s) the parents of the \"object\" and the 'do' keyword declares the start of the body of the \"object\".", _currentToken.StartPosition, _currentToken.EndPosition));

            return result.Success(new ObjectDefinitionNode(name, parameters, parents, body, startPosition, endPosition));
        }

        /// <summary>
        /// Creates the structure of an include expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordInclude"/>.
        /// </summary>
        /// <returns>The <see cref="ParseResult"/> object.</returns>
        private ParseResult IncludeStructure()
        {
            ParseResult result = new ParseResult();
            Position startPosition = _currentToken.StartPosition;
            result.RegisterAdvance();
            Advance();

            Node? subStructure = null;
            Node? nickname = null;
            bool isDumped = false;
            Node script;

            if (_currentToken.Type != TokenType.KeywordAll && _currentToken.Type != TokenType.Comma)
            {
                subStructure = result.Register(Expression());
                if (result.Error != null)
                    return result;
            }
            else
            {
                isDumped = true;
                result.RegisterAdvance();
                Advance();
            }

            if (_currentToken.Type == TokenType.KeywordFrom)
            {
                result.RegisterAdvance();
                Advance();

                script = result.Register(Expression());
                if (result.Error != null)
                    return result;
            }
            else if (subStructure != null)
            {
                script = subStructure;
                subStructure = null;
            }
            else
                return result.Failure(10, new InvalidGrammarError("Expected the 'from' keyword! If a specific object being included from a script (when the object's name is provided after the 'include' keyword) or if the whole script is added to the script (using the 'all' keyword or a comma symbol after the 'include' keyword), the 'from' keyword followed by an expression declaring the script's name or path must be provided.", _currentToken.StartPosition, _currentToken.EndPosition));
        
            if (_currentToken.Type == TokenType.KeywordAs)
            {
                result.RegisterAdvance();
                Advance();

                nickname = result.Register(Expression());
                if (result.Error != null)
                    return result;
            }

            return result.Success(new IncludeNode(script, subStructure, isDumped, nickname, startPosition, PeekPrevious().EndPosition));
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
        /// The amount of times the <see cref="Parser"/> will have to reverse if an <see cref="EzrErrors.Error"/> occurred in a <see cref="TryRegister(ParseResult, out Node)"/> call.
        /// </summary>
        public int ReverseCount;

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
            ReverseCount = 0;
            ErrorPriority = 0;
        }

        /// <summary>
        /// Resets the <see cref="ParseResult"/> object to initialized state.
        /// </summary>
        public void Reset()
        {
            Error = null;
            Node = InvalidNode.StaticInvalidNode;
            AdvanceCount = 0;
            ReverseCount = 0;
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
        /// Registers the <see cref="ParseResult"/> from a parsing function. 
        /// </summary>
        /// <param name="result">The result of the parsing function.</param>
        /// <returns>The <see cref="EzrNodes.Node"/> object of <paramref name="result"/>.</returns>
        public Node Register(ParseResult result)
        {
            AdvanceCount += result.AdvanceCount;
            if (result.Error != null)
            {
                ErrorPriority = result.ErrorPriority;
                Error = result.Error;
            }

            return result.Node;
        }

        /// <summary>
        /// Tries registering the <see cref="ParseResult"/> from a parsing function.
        /// </summary>
        /// <param name="result">The result of the parsing function.</param>
        /// <param name="node">The <see cref="EzrNodes.Node"/> object of <paramref name="result"/>, <see langword="null"/> if an <see cref="EzrErrors.Error"/> occurred.</param>
        /// <returns><see langword="true"/> if <paramref name="result"/> has no <see cref="EzrErrors.Error"/>; otherwise <see langword="false"/>.</returns>
        public bool TryRegister(ParseResult result, out Node node)
        {
            if (result.Error != null)
            {
                ReverseCount = result.AdvanceCount;
                node = InvalidNode.StaticInvalidNode;
                return false;
            }

            AdvanceCount += result.AdvanceCount;
            node = result.Node;
            return true;
        }

        /// <summary>
        /// Sets <see cref="Node"/> as the result of successful parsing.
        /// </summary>
        /// <param name="node">The <see cref="EzrNodes.Node"/> result of the parsing.</param>
        /// <returns>The same <see cref="ParseResult"/> object.</returns>
        public ParseResult Success(Node node)
        {
            Node = node;
            return this;
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
        public ParseResult Failure(int priority, Error error)
        {
            if (Error == null || ErrorPriority <= priority || AdvanceCount == 0)
            {
                ErrorPriority = priority;
                Error = error;
            }

            return this;
        }
    }
}
