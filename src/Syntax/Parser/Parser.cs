using EzrSquared.Runtime.Nodes;
using EzrSquared.Syntax.Errors;
using System;
using System.Collections.Generic;

namespace EzrSquared.Syntax;

/// <summary>
/// The ezr² Parser. The job of the Parser is to convert the input <see cref="Token"/> objects from the <see cref="Lexer"/> into <see cref="Node"/> objects to be given as the input to the <see cref="Runtime.Interpreter"/>.
/// </summary>
public class Parser
{
    /// <summary>
    /// The <see cref="List{T}"/> of <see cref="Token"/> objects to be parsed.
    /// </summary>
    private readonly List<Token> _tokens;

    /// <summary>
    /// The object that holds the result of the parsing.
    /// </summary>
    private readonly ParseResult _result;

    /// <summary>
    /// The index of the <see cref="Token"/> object currently being parsed in the <see cref="_tokens"/> <see cref="List{T}"/>.
    /// </summary>
    private int _index;

    /// <summary>
    /// The <see cref="Token"/> object currently being parsed at <see cref="_index"/> of <see cref="_tokens"/>.
    /// </summary>
    private Token _currentToken;

    // /// <summary>
    // /// The boolean check for if normal syntax or QuickSyntax is being used.
    // /// </summary>
    // private bool _usingQuickSyntax;

    // /// <summary>
    // /// The boolean check for if QuickSyntax was being used previously in parsing.
    // /// </summary>
    // private bool _wasUsingQuickSyntax;

    /// <summary>
    /// Creates a new <see cref="Parser"/> object.
    /// </summary>
    /// <param name="tokens">The <see cref="List{T}"/> of <see cref="Token"/> objects to be parsed.</param>
    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
        // _usingQuickSyntax = false;
        // _wasUsingQuickSyntax = false;
        _result = new ParseResult();

        _index = 0;
        _currentToken = tokens.Count > _index ? _tokens[_index] : Token.Empty;
    }

    /// <summary>
    /// Advances to the next <see cref="Token"/> object in <see cref="_tokens"/>.
    /// </summary>
    private void Advance(int advanceCount = 1)
    {
        _index += advanceCount;
        _result.AdvanceCount += advanceCount;

        if (_tokens.Count > _index)
            _currentToken = _tokens[_index];
    }

    /// <summary>
    /// Reverses back to the <see cref="Token"/> object at <see cref="_index"/> - <paramref name="reverseCount"/> in <see cref="_tokens"/>.
    /// </summary>
    /// <param name="reverseCount">The number of positions to reverse <see cref="_index"/> in <see cref="_tokens"/>.</param>
    private void Reverse(int reverseCount = 1)
    {
        _index -= reverseCount;
        _result.AdvanceCount -= reverseCount;
        _currentToken = _tokens[_index];
    }

    /// <summary>
    /// Peeks at the previous <see cref="Token"/> object from <see cref="_index"/> in <see cref="_tokens"/>.
    /// </summary>
    /// <returns>The <see cref="Token"/> object.</returns>
    private Token PeekPrevious()
    {
        int previousIndex = _index - 1;
        return previousIndex < 0 ? Token.Empty : _tokens[previousIndex];
    }

    /// <summary>
    /// Peeks at the next <see cref="Token"/> object from <see cref="_index"/> in <see cref="_tokens"/>.
    /// </summary>
    /// <param name="advanceCount">The numbers of places to advance in <see cref="_tokens"/>.</param>
    /// <returns>The <see cref="Token"/> object.</returns>
    private Token PeekNext(int advanceCount = 1)
    {
        int nextIndex = _index + advanceCount;
        return _tokens.Count < nextIndex ? Token.Empty : _tokens[nextIndex];
    }

    // /// <summary>
    // /// Registers the use of QuickSyntax.
    // /// </summary>
    // private void RegisterQuickSyntaxUse()
    // {
    //     _wasUsingQuickSyntax = _usingQuickSyntax;
    //     _usingQuickSyntax = true;
    // }

    //  /// <summary>
    //  /// Unregisters the use of QuickSyntax.
    //  /// </summary>
    // private void UnregisterQuickSyntaxUse()
    // {
    //     _usingQuickSyntax = _wasUsingQuickSyntax;
    // }

    /// <summary>
    /// Parses the <see cref="Token"/> objects in <see cref="_tokens"/>.
    /// </summary>
    public ParseResult Parse()
    {
        ParseStatements();
        if (_result.Error is null && _currentToken.Type != TokenType.EndOfFile)
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Did not expect this!", _currentToken.StartPosition, _currentToken.EndPosition));
        return _result;
    }

    /// <summary>
    /// Tries creating a <see cref="BinaryOperationNode"/>.
    /// </summary>
    /// <param name="left">The function to call for the first operand.</param>
    /// <param name="right">The function to call for the second operand.</param>
    /// <param name="operators">The operator <see cref="TokenType"/> object(s).</param>
    /// <param name="specialCase">
    /// Any special case that needs to be uniquely handled by the "OnCase" <see cref="Action"/>.
    /// <br/><br/>
    /// "OnCase" is called after the token of type "Type" and any new lines have been parsed, and before <paramref name="right"/> has been called.
    /// <br/>
    /// Note that "Type" must be included in <paramref name="operators"/>.
    /// </param>
    private void BinaryOperation(Action left, Action right, TokenType[] operators, (TokenType Type, Action OnCase)? specialCase = null)
    {
        Position startPosition = _currentToken.StartPosition;

        left();
        if (_result.Error is not null)
            return;
        Node leftNode = _result.Node;

        int toReverseTo = _result.AdvanceCount;
        if (_currentToken.Type == TokenType.NewLine)
            Advance();

        while (Array.Exists(operators, @operator => @operator == _currentToken.Type))
        {
            TokenType @operator = _currentToken.Type;
            Advance();

            if (_currentToken.Type == TokenType.NewLine)
                Advance();

            if (specialCase?.Type == @operator)
            {
                specialCase.Value.OnCase();
                if (_result.Error is not null)
                    return;
            }

            right();
            if (_result.Error is not null)
                return;
            Node rightNode = _result.Node;

            leftNode = new BinaryOperationNode(leftNode, rightNode, @operator, startPosition, rightNode.EndPosition);
            toReverseTo = _result.AdvanceCount;

            if (_currentToken.Type == TokenType.NewLine)
                Advance();
        }

        Reverse(_result.AdvanceCount - toReverseTo);
        _result.Success(leftNode);
    }

    /// <summary>
    /// Tries parsing a 'statements' structure.
    /// </summary>
    private void ParseStatements()
    {
        List<Node> statements = [];
        Position startPosition = _currentToken.StartPosition;

        if (_currentToken.Type == TokenType.NewLine)
            Advance();

        ParseStatement();
        if (_result.Error is not null)
            return;

        statements.Add(_result.Node);
        while (true)
        {
            // NOTE: Qeyword 'l' for 'else if' is deprecated, use Qeyword 'e' instead in format:
            // 'e [check]: [statement(s)]'

            if (_currentToken.Type == TokenType.NewLine)
                Advance();
            else
                break;

            if (_currentToken.Type is TokenType.EndOfFile
                or TokenType.KeywordEnd
                or TokenType.KeywordElse
                or TokenType.KeywordCatch)
                // || (_usingQuickSyntax
                //     && (_currentToken.Type == TokenType.QeywordL
                //     || _currentToken.Type == TokenType.QeywordE
                //     || _currentToken.Type == TokenType.QeywordS)))
                break;

            ParseStatement();
            if (_result.Error is not null)
                return;

            statements.Add(_result.Node);
        }

        _result.Success(new ArrayLikeNode(statements, false, startPosition, statements[^1].EndPosition));
    }

    /// <summary>
    /// Tries parsing a 'statement' structure.
    /// </summary>
    private void ParseStatement()
    {
        Position startPosition = _currentToken.StartPosition;
        if (_currentToken.Type == TokenType.KeywordReturn)
        {
            Position possibleEndPosition = _currentToken.EndPosition;
            Advance();

            if (_currentToken.Type is TokenType.NewLine
                or TokenType.EndOfFile
                or TokenType.KeywordEnd
                or TokenType.KeywordElse
                or TokenType.KeywordCatch)
            // || (_usingQuickSyntax
            //     && (_currentToken.Type == TokenType.QeywordL
            //     || _currentToken.Type == TokenType.QeywordE
            //     || _currentToken.Type == TokenType.QeywordS)))
            {
                _result.Success(new ReturnNode(null, false, startPosition, possibleEndPosition));
                return;
            }

            bool returnLast = false;
            if (_currentToken.Type == TokenType.KeywordLast)
            {
                Advance();
                returnLast = true;
            }

            ParseExpression();
            if (_result.Error is not null)
                return;

            Node expression = _result.Node;
            _result.Success(new ReturnNode(expression, returnLast, startPosition, expression.EndPosition));
            return;
        }
        else if (_currentToken.Type is TokenType.KeywordSkip or TokenType.KeywordStop)
        {
            Position endPosition = _currentToken.EndPosition;
            TokenType currentTokenType = _currentToken.Type;
            Advance();

            _result.Success(new NoValueNode(currentTokenType, startPosition, endPosition));
            return;
        }

        ParseExpression();
        if (_result.Error is not null)
            _result.Failure(4, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a statement!", _currentToken.StartPosition, _currentToken.EndPosition));
    }

    /// <summary>
    /// Tries parsing an 'expression' structure.
    /// </summary>
    /// <param name="itemKeywordRequired">In cases where the variable assignment expression requires the 'item' keyword, set this to <see langword="true"/>.</param>
    private void ParseExpression(bool itemKeywordRequired = false)
    {
        Position startPosition = _currentToken.StartPosition;

        AccessMod accessibilityModifiers = AccessMod.None;
        bool usedItemKeyword = false;
        int startingAdvanceCount = _result.AdvanceCount;

        if (_currentToken.Type == TokenType.KeywordGlobal)
        {
            accessibilityModifiers |= AccessMod.Global;
            Advance();
        }

        if (_currentToken.Type == TokenType.KeywordPrivate)
        {
            if ((accessibilityModifiers & AccessMod.Global) == AccessMod.Global)
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "A variable, function or class cannot be declared 'global' and 'private' at the same time! It must be either 'global' , which means it is accessible to any code, or 'private' , which means it is only accessible to the current context.", startPosition, _currentToken.EndPosition));
                return;
            }

            accessibilityModifiers |= AccessMod.Private;
            Advance();
        }

        if (_currentToken.Type == TokenType.KeywordStatic)
        {
            accessibilityModifiers |= AccessMod.Static;
            Advance();
        }

        if (_currentToken.Type == TokenType.KeywordConstant)
        {
            accessibilityModifiers |= AccessMod.Constant;
            Advance();
        }

        if (_currentToken.Type == TokenType.KeywordItem)
        {
            usedItemKeyword = true;
            Advance();
        }
        else if (itemKeywordRequired)
        {
            ParseQuickExpression(true);
            if (_result.Error is not null)
                _result.Failure(4, new SyntaxError(SyntaxError.InvalidGrammar, "Expected an expression!", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }

        if (_currentToken.Type == TokenType.Identifier || _currentToken.TypeGroup == TokenTypeGroup.Qeyword || _currentToken.Type == TokenType.LeftParenthesis)
        {
            ParseJunction();
            if (_result.Error is not null)
                return;

            Node variable = _result.Node;
            if (_currentToken.TypeGroup == TokenTypeGroup.AssignmentSymbol)
            {
                TokenType assignmentOperator = _currentToken.Type;
                Advance();

                ParseExpression();
                if (_result.Error is not null)
                    return;

                Node value = _result.Node;
                _result.Success(new VariableAssignmentNode(variable, assignmentOperator, value, accessibilityModifiers, startPosition, value.EndPosition));
                return;
            }
            else if (usedItemKeyword || ((accessibilityModifiers & AccessMod.PrivateConstant) != 0))
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected an assignment symbol! The assignment symbol seperates the variable name and value, and declares how to handle any existing values in the variable, in a variable assignment expression. A few examples of assignment symbols are: (':') - normal assignment, (':+') - adds existing value in variable to new value, assigns the result, (':*') - multiplies existing value with new value, assigns the result, and (':&') - does a bitwise and operation between the existing and new value, assigns the result. There are equivalent symbols for all binary mathematical and bitwise operations.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }
            else
                Reverse(_result.AdvanceCount - startingAdvanceCount);
        }
        else if (usedItemKeyword)
        {
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a variable name! The variable name is where the value will be assigned.", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }
        else
            Reverse(_result.AdvanceCount - startingAdvanceCount);

        ParseQuickExpression();
        if (_result.Error is not null)
            _result.Failure(4, new SyntaxError(SyntaxError.InvalidGrammar, "Expected an expression!", _currentToken.StartPosition, _currentToken.EndPosition));
    }

    /// <summary>
    /// Tries parsing a 'quick-expression' structure.
    /// </summary>
    private void ParseQuickExpression(bool itemKeywordRequired = false)
    {
        Position startPosition = _currentToken.StartPosition;
        int startingAdvanceCount = _result.AdvanceCount;

        if (_currentToken.Type == TokenType.ExclamationMark)
        {
            Advance();

            AccessMod accessibilityModifiers = AccessMod.None;
            bool usedItemKeyword = false;
            if (_currentToken.Type == TokenType.QeywordG)
            {
                accessibilityModifiers |= AccessMod.Global;
                Advance();
            }

            if (_currentToken.Type == TokenType.QeywordP)
            {
                if ((accessibilityModifiers & AccessMod.Global) == AccessMod.Global)
                {
                    Position errorStartPosition = startPosition.Copy();
                    errorStartPosition.Advance();

                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "A variable, function or class cannot be declared 'global' and 'private' at the same time! It must be either 'global' , which means it is accessible to any code, or 'private' , which means it is only accessible to the current context.", errorStartPosition, _currentToken.EndPosition));
                    return;
                }

                accessibilityModifiers |= AccessMod.Private;
                Advance();
            }

            if (_currentToken.Type == TokenType.QeywordSb)
            {
                accessibilityModifiers |= AccessMod.Static;
                Advance();
            }

            if (_currentToken.Type == TokenType.QeywordC)
            {
                accessibilityModifiers |= AccessMod.Constant;
                Advance();
            }

            if (_currentToken.Type == TokenType.QeywordD)
            {
                usedItemKeyword = true;
                Advance();
            }
            else if (itemKeywordRequired)
            {
                ParseJunction();
                if (_result.Error is not null)
                    _result.Failure(4, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a QuickSyntax expression!", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            if (_currentToken.Type == TokenType.Identifier || _currentToken.TypeGroup == TokenTypeGroup.Qeyword || _currentToken.Type == TokenType.LeftParenthesis)
            {
                ParseJunction();
                if (_result.Error is not null)
                    return;

                Node variable = _result.Node;
                if (_currentToken.TypeGroup == TokenTypeGroup.AssignmentSymbol)
                {
                    TokenType assignmentOperator = _currentToken.Type;
                    Advance();

                    ParseExpression();
                    if (_result.Error is not null)
                        return;

                    Node value = _result.Node;
                    _result.Success(new VariableAssignmentNode(variable, assignmentOperator, value, accessibilityModifiers, startPosition, value.EndPosition));
                    return;
                }
                else if (usedItemKeyword || ((accessibilityModifiers & AccessMod.PrivateConstant) != 0))
                {
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected an assignment symbol! The assignment symbol seperates the variable name and value, and declares how to handle any existing values in the variable, in a variable assignment expression. A few examples of assignment symbols are: (':') - normal assignment, (':+') - adds existing value in variable to new value, assigns the result, (':*') - multiplies existing value with new value, assigns the result, and (':&') - does a bitwise and operation between the existing and new value, assigns the result. There are equivalent symbols for all binary mathematical and bitwise operations.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }
                else
                    Reverse(_result.AdvanceCount - startingAdvanceCount);
            }
            else if (usedItemKeyword)
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a variable name! The variable name is where the value will be assigned.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }
            else
                Reverse(_result.AdvanceCount - startingAdvanceCount);
        }

        ParseJunction();
        if (_result.Error is not null)
            _result.Failure(4, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a QuickSyntax expression!", _currentToken.StartPosition, _currentToken.EndPosition));
    }

    /// <summary>
    /// Tries parsing a 'junction' structure.
    /// </summary>
    private void ParseJunction()
    {
        BinaryOperation(ParseInversion, ParseInversion, [TokenType.KeywordAnd, TokenType.KeywordOr]);
    }

    /// <summary>
    /// Tries parsing an 'inversion' structure.
    /// </summary>
    private void ParseInversion()
    {
        Position startPosition = _currentToken.StartPosition;
        int startingAdvanceCount = _result.AdvanceCount;

        if (_currentToken.Type is TokenType.KeywordInvert or TokenType.KeywordNot)
        {
            TokenType @operator = _currentToken.Type;
            Advance();

            ParseInversion();
            if (_result.Error is not null)
                return;

            Node operand = _result.Node;
            _result.Success(new UnaryOperationNode(operand, @operator, startPosition, operand.EndPosition));
            return;
        }
        else if (_currentToken.Type == TokenType.ExclamationMark)
        {
            Advance();

            if (_currentToken.Type == TokenType.QeywordV)
            {
                TokenType @operator = _currentToken.Type;
                Advance();

                ParseInversion();
                if (_result.Error is not null)
                    return;

                Node operand = _result.Node;
                _result.Success(new UnaryOperationNode(operand, @operator, startPosition, operand.EndPosition));
                return;
            }
            else
                Reverse(_result.AdvanceCount - startingAdvanceCount);
        }

        ParseContainsCheck();
        if (_result.Error is not null)
            _result.Failure(4, new SyntaxError(SyntaxError.InvalidGrammar, "Expected an inversion expression!", _currentToken.StartPosition, _currentToken.EndPosition));
    }

    /// <summary>
    /// Tries parsing a 'contains-check' structure.
    /// </summary>
    private void ParseContainsCheck()
    {
        BinaryOperation(ParseComparison, ParseComparison,
            [
                TokenType.KeywordIn,
                TokenType.KeywordNot,
            ],
            (TokenType.KeywordNot, () =>
            {
                if (_currentToken.Type != TokenType.KeywordIn)
                {
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'in' keyword! The 'in' keyword is the second part of a check-not-in operation.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }

                Advance();
                if (_currentToken.Type == TokenType.NewLine)
                    Advance();
            }
        ));
    }

    /// <summary>
    /// Tries parsing a 'comparison' structure.
    /// </summary>
    private void ParseComparison()
    {
        BinaryOperation(ParseBitwiseOr, ParseBitwiseOr,
            [
                TokenType.EqualSign,
                TokenType.ExclamationMark,
                TokenType.LessThanSign,
                TokenType.GreaterThanSign,
                TokenType.LessThanOrEqual,
                TokenType.GreaterThanOrEqual,
            ]);
    }

    /// <summary>
    /// Tries parsing a 'bitwise-or' structure.
    /// </summary>
    private void ParseBitwiseOr()
    {
        BinaryOperation(ParseBitwiseXOr, ParseBitwiseXOr, [TokenType.VerticalBar]);
    }

    /// <summary>
    /// Tries parsing a 'bitwise-xor' structure.
    /// </summary>
    private void ParseBitwiseXOr()
    {
        BinaryOperation(ParseBitwiseAnd, ParseBitwiseAnd, [TokenType.Backslash]);
    }

    /// <summary>
    /// Tries parsing a 'bitwise-and' structure.
    /// </summary>
    private void ParseBitwiseAnd()
    {
        BinaryOperation(ParseBitwiseShift, ParseBitwiseShift, [TokenType.Ampersand]);
    }

    /// <summary>
    /// Tries creating a 'bitwise-shift' structure.
    /// </summary>
    private void ParseBitwiseShift()
    {
        BinaryOperation(ParseArithmeticExpression, ParseArithmeticExpression, [TokenType.BitwiseLeftShift, TokenType.BitwiseRightShift]);
    }

    /// <summary>
    /// Tries parsing an 'arithmetic-expression' structure.
    /// </summary>
    private void ParseArithmeticExpression()
    {
        BinaryOperation(ParseTerm, ParseTerm, [TokenType.Plus, TokenType.HyphenMinus]);
    }

    /// <summary>
    /// Tries parsing a 'term' structure.
    /// </summary>
    private void ParseTerm()
    {
        BinaryOperation(ParseFactor, ParseFactor, [TokenType.Asterisk, TokenType.Slash, TokenType.PercentSign]);
    }

    /// <summary>
    /// Tries parsing a 'factor' structure.
    /// </summary>
    private void ParseFactor()
    {
        Position startPosition = _currentToken.StartPosition;

        TokenType @operator = _currentToken.Type;
        if (@operator is TokenType.Plus or TokenType.HyphenMinus or TokenType.Tilde)
        {
            Advance();

            ParseFactor();
            if (_result.Error is not null)
                return;

            Node operand = _result.Node;
            _result.Success(new UnaryOperationNode(operand, @operator, startPosition, operand.EndPosition));
            return;
        }

        ParsePower();
    }

    /// <summary>
    /// Tries parsing a 'power' structure.
    /// </summary>
    private void ParsePower()
    {
        BinaryOperation(ParseObjectAttributeAccess, ParseObjectAttributeAccess, [TokenType.Caret]);
    }

    /// <summary>
    /// Tries parsing an 'object-attribute-access' structure.
    /// </summary>
    private void ParseObjectAttributeAccess()
    {
        BinaryOperation(ParseCall, ParseCall, [TokenType.Period]);
    }

    /// <summary>
    /// Tries parsing a 'call' structure.
    /// </summary>
    private void ParseCall()
    {
        Position startPosition = _currentToken.StartPosition;

        ParseAtom();
        if (_result.Error is not null)
            return;

        if (_currentToken.Type != TokenType.LeftParenthesis)
            return;

        Node node = _result.Node;
        Advance();

        Token possibleErrorToken = _currentToken;
        if (_currentToken.Type == TokenType.NewLine)
            Advance();

        Position endPosition;
        List<Node> arguments = [];
        if (_currentToken.Type == TokenType.RightParenthesis)
        {
            endPosition = _currentToken.EndPosition;
            Advance();
        }
        else
        {
            ParseExpression();
            if (_result.Error is not null)
            {
                _result.Failure(10, new StackedSyntaxError(_result.Error, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a right-parenthesis symbol! The function/object call expression must end with a right-parenthesis.", possibleErrorToken.StartPosition, possibleErrorToken.EndPosition)));
                return;
            }

            arguments.Add(_result.Node);
            if (_currentToken.Type == TokenType.NewLine)
                Advance();

            while (_currentToken.Type == TokenType.Comma)
            {
                possibleErrorToken = _currentToken;
                Advance();

                if (_currentToken.Type == TokenType.NewLine)
                    Advance();

                ParseExpression();
                if (_result.Error is not null)
                {
                    _result.Failure(10, new StackedSyntaxError(_result.Error, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a right-parenthesis symbol! The function/object call expression must end with a right-parenthesis.", possibleErrorToken.StartPosition, possibleErrorToken.EndPosition)));
                    return;
                }
                arguments.Add(_result.Node);

                if (_currentToken.Type == TokenType.NewLine)
                    Advance();
            }

            if (_currentToken.Type != TokenType.RightParenthesis)
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a comma or right-parenthesis symbol! Commas are used to seperate the arguments of the function/object call expression, and the right-parenthesis is used to end it.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            endPosition = _currentToken.EndPosition;
            Advance();
        }

        _result.Success(new CallNode(node, arguments, startPosition, endPosition));
    }

    /// <summary>
    /// Tries parsing a 'atom' structure.
    /// </summary>
    private void ParseAtom()
    {
        Token startToken = _currentToken;
        switch (startToken.Type)
        {
            case TokenType.KeywordGlobal:
                Token nextToken = PeekNext();
                int peek = 1;

                AccessMod accessibilityModifiers = AccessMod.Global;
                bool readOnly = false;

                if (nextToken.Type == TokenType.KeywordStatic)
                {
                    nextToken = PeekNext(++peek);
                    accessibilityModifiers |= AccessMod.Static;
                }

                if (nextToken.Type == TokenType.Identifier || nextToken.TypeGroup == TokenTypeGroup.Qeyword)
                {
                    Advance(accessibilityModifiers == AccessMod.GlobalStatic ? 2 : 1);

                    Token variable = _currentToken;
                    Position endPosition = _currentToken.EndPosition;
                    Advance();

                    _result.Success(new VariableAccessNode(variable, accessibilityModifiers, startToken.StartPosition, endPosition));
                    return;
                }

                if (nextToken.Type == TokenType.KeywordConstant)
                {
                    nextToken = PeekNext(++peek);
                    accessibilityModifiers |= AccessMod.Constant;
                }

                if (nextToken.Type == TokenType.KeywordReadonly)
                {
                    nextToken = PeekNext(++peek);
                    readOnly = true;
                }

                if (!readOnly && nextToken.Type == TokenType.KeywordFunction)
                {
                    ParseFunctionDefinitionExpression();
                    return;
                }
                else if (nextToken.Type == TokenType.KeywordObject)
                {
                    ParseClassDefinitionExpression();
                    return;
                }

                bool isConstant = (accessibilityModifiers & AccessMod.Constant) == AccessMod.Constant;
                string onConstant = isConstant ? "constant " : string.Empty;
                string onVariable = isConstant ? string.Empty : "variable ";

                string onStatic = (accessibilityModifiers & AccessMod.Static) == AccessMod.Static ? "static " : string.Empty;

                string errorMessage = (readOnly, isConstant) switch
                {
                    (true, _) => $"Expected the 'object' keyword! In this case, the 'object' keyword is used to declare a global {onStatic}{onConstant}read-only class.",
                    (false, true) => $"Expected a variable assignment expression or the 'function' or 'object' keywords! In this case, a variable assignment expression will assign a global {onStatic}{onConstant}{onVariable}and the 'function' or 'object' keywords will declare a global {onStatic}{onConstant}function or class, respectively.",
                    (false, false) => $"Expected a variable assignment expression, variable access expression or the 'function' or 'object' keywords! In this case, a variable assignment expression will assign a global {onStatic}variable, a variable access expression references a global {onStatic}variable and the 'function' or 'object' keywords will declare a global {onStatic}function or class, respectively.",
                };

                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, errorMessage, nextToken.StartPosition, nextToken.EndPosition));
                break;
            case TokenType.KeywordPrivate:
                nextToken = PeekNext();
                peek = 1;

                accessibilityModifiers = AccessMod.Private;
                readOnly = false;

                if (nextToken.Type == TokenType.KeywordStatic)
                {
                    nextToken = PeekNext(++peek);
                    accessibilityModifiers |= AccessMod.Static;
                }

                if (nextToken.Type == TokenType.KeywordConstant)
                {
                    nextToken = PeekNext(++peek);
                    accessibilityModifiers |= AccessMod.Constant;
                }

                if (nextToken.Type == TokenType.KeywordReadonly)
                {
                    nextToken = PeekNext(++peek);
                    readOnly = true;
                }

                if (!readOnly && nextToken.Type == TokenType.KeywordFunction)
                {
                    ParseFunctionDefinitionExpression();
                    return;
                }
                else if (nextToken.Type == TokenType.KeywordObject)
                {
                    ParseClassDefinitionExpression();
                    return;
                }

                isConstant = (accessibilityModifiers & AccessMod.Constant) == AccessMod.Constant;
                onConstant = isConstant ? "constant " : string.Empty;
                onVariable = isConstant ? string.Empty : "variable ";

                onStatic = (accessibilityModifiers & AccessMod.Static) == AccessMod.Static ? "static " : string.Empty;

                errorMessage = readOnly
                    ? $"Expected the 'object' keyword! In this case, the 'object' keyword is used to declare a private {onStatic}{onConstant}read-only class."
                    : $"Expected a variable assignment expression or the 'function' or 'object' keywords! In this case, a variable assignment expression will assign a private {onStatic}{onConstant}{onVariable}and the 'function' or 'object' keywords will declare a private {onStatic}{onConstant}function or class, respectively.";

                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, errorMessage, nextToken.StartPosition, nextToken.EndPosition));
                break;
            case TokenType.KeywordStatic:
                nextToken = PeekNext();
                peek = 1;

                accessibilityModifiers = AccessMod.Static;
                readOnly = false;

                if (nextToken.Type == TokenType.Identifier || nextToken.TypeGroup == TokenTypeGroup.Qeyword)
                {
                    Advance();

                    Token variable = _currentToken;
                    Position endPosition = _currentToken.EndPosition;
                    Advance();

                    _result.Success(new VariableAccessNode(variable, accessibilityModifiers, startToken.StartPosition, endPosition));
                    return;
                }

                if (nextToken.Type == TokenType.KeywordConstant)
                {
                    nextToken = PeekNext(++peek);
                    accessibilityModifiers |= AccessMod.Constant;
                }

                if (nextToken.Type == TokenType.KeywordReadonly)
                {
                    nextToken = PeekNext(++peek);
                    readOnly = true;
                }

                if (!readOnly && nextToken.Type == TokenType.KeywordFunction)
                {
                    ParseFunctionDefinitionExpression();
                    return;
                }
                else if (nextToken.Type == TokenType.KeywordObject)
                {
                    ParseClassDefinitionExpression();
                    return;
                }

                isConstant = (accessibilityModifiers & AccessMod.Constant) == AccessMod.Constant;
                onConstant = isConstant ? "constant " : string.Empty;
                onVariable = isConstant ? string.Empty : "variable ";

                errorMessage = (readOnly, isConstant) switch
                {
                    (true, _) => $"Expected the 'object' keyword! In this case, the 'object' keyword is used to declare a static {onConstant}read-only class.",
                    (false, true) => $"Expected a variable assignment expression or the 'function' or 'object' keywords! In this case, a variable assignment expression will assign a static {onConstant}{onVariable}and the 'function' or 'object' keywords will declare a static {onConstant}function or class, respectively.",
                    (false, false) => $"Expected a variable assignment expression, variable access expression or the 'function' or 'object' keywords! In this case, a variable assignment expression will assign a static variable, a variable access expression references a static variable and the 'function' or 'object' keywords will declare a static function or class, respectively.",
                };

                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, errorMessage, nextToken.StartPosition, nextToken.EndPosition));
                break;
            case TokenType.KeywordConstant:
                nextToken = PeekNext();

                readOnly = false;
                if (nextToken.Type == TokenType.KeywordReadonly)
                {
                    nextToken = PeekNext(2);
                    readOnly = true;
                }

                if (!readOnly && nextToken.Type == TokenType.KeywordFunction)
                {
                    ParseFunctionDefinitionExpression();
                    return;
                }
                else if (nextToken.Type == TokenType.KeywordObject)
                {
                    ParseClassDefinitionExpression();
                    return;
                }

                errorMessage = readOnly
                    ? "Expected the 'object' keyword! In this case, the 'object' keyword is used to declare a constant read-only class."
                    : "Expected a variable assignment expression or the 'function' or 'object' keywords! In this case, a variable assignment expression will assign a constant, while the 'function' or 'object' keywords will declare a constant function or class, respectively.";

                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, errorMessage, nextToken.StartPosition, nextToken.EndPosition));
                break;
            case TokenType.KeywordReadonly:
                nextToken = PeekNext();

                if (nextToken.Type != TokenType.KeywordObject)
                {
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'object' keyword! In this case, the 'object' keyword is used to declare a read-only class.", nextToken.StartPosition, nextToken.EndPosition));
                    return;
                }

                ParseClassDefinitionExpression();
                break;
            case TokenType.LeftParenthesis:
                ParseArrayOrParentheticalExpression();
                break;
            case TokenType.LeftSquareBracket:
                ParseList();
                break;
            case TokenType.LeftCurlyBracket:
                ParseDictionary();
                break;
            case TokenType.KeywordIf:
                ParseIfExpression();
                break;
            case TokenType.KeywordCount:
                ParseCountExpression();
                break;
            case TokenType.KeywordFor:
                ParseForEachExpression();
                break;
            case TokenType.KeywordWhile:
                ParseWhileExpression();
                break;
            case TokenType.KeywordTry:
                ParseTryExpression();
                break;
            case TokenType.KeywordFunction:
                ParseFunctionDefinitionExpression();
                break;
            case TokenType.KeywordObject:
                ParseClassDefinitionExpression();
                break;
            case TokenType.KeywordInclude:
                ParseIncludeExpression();
                break;
            case TokenType.KeywordDefine:
                ParseDefineBlockExpression();
                break;

#pragma warning disable CS0618
            case TokenType.KeywordSpecial:
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "The \"special function\" structure has been removed from ezr². Please use normal functions with dedicated names for operator overloading. Check [DOCUMENTATION LINK HERE] for more info.", _currentToken.StartPosition, _currentToken.EndPosition));
                break;
#pragma warning restore CS0618

            default:
                if (startToken.TypeGroup == TokenTypeGroup.Value)
                {
                    Advance();

                    _result.Success(new ValueNode(startToken, startToken.StartPosition, startToken.EndPosition));
                    return;
                }
                else if (startToken.Type == TokenType.Identifier || startToken.TypeGroup == TokenTypeGroup.Qeyword)
                {
                    Advance();

                    _result.Success(new VariableAccessNode(startToken, AccessMod.None, startToken.StartPosition, startToken.EndPosition));
                    return;
                }

                _result.Failure(4, new SyntaxError(SyntaxError.InvalidGrammar, "Expected an integer, float, string, character, character list, identifier, 'if' expression, 'count' expression, 'while' expression...", _currentToken.StartPosition, _currentToken.EndPosition));
                break;
        }
    }

    /// <summary>
    /// Tries parsing an array, an <see cref="ArrayLikeNode"/> with <see cref="ArrayLikeNode.CreateList"/> set to <see langword="false"/> OR a parenthetical expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.LeftParenthesis"/>.
    /// </summary>
    private void ParseArrayOrParentheticalExpression()
    {
        Position startPosition = _currentToken.StartPosition;
        Advance();

        Token possibleErrorToken = _currentToken;
        List<Node> elements = [];
        bool isArray = false;

        if (_currentToken.Type == TokenType.NewLine)
            Advance();

        Position endPosition;
        if (_currentToken.Type == TokenType.RightParenthesis)
        {
            endPosition = _currentToken.EndPosition;
            Advance();
        }
        else
        {
            ParseExpression();
            if (_result.Error is not null)
            {
                _result.Failure(10, new StackedSyntaxError(_result.Error, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a right-parenthesis symbol! An array/parenthetical expression must end with a right-parenthesis.", possibleErrorToken.StartPosition, possibleErrorToken.EndPosition)));
                return;
            }

            elements.Add(_result.Node);

            if (_currentToken.Type == TokenType.NewLine)
                Advance();

            while (_currentToken.Type == TokenType.Comma)
            {
                isArray = true;
                possibleErrorToken = _currentToken;
                Advance();

                if (_currentToken.Type == TokenType.NewLine)
                    Advance();

                if (_currentToken.Type == TokenType.RightParenthesis && elements.Count == 1)
                    break;

                ParseExpression();
                if (_result.Error is not null)
                {
                    _result.Failure(10, new StackedSyntaxError(_result.Error, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a right-parenthesis symbol! An array must end with a right-parenthesis.", possibleErrorToken.StartPosition, possibleErrorToken.EndPosition)));
                    return;
                }

                elements.Add(_result.Node);
                if (_currentToken.Type == TokenType.NewLine)
                    Advance();
            }

            if (_currentToken.Type != TokenType.RightParenthesis)
            {
                if (isArray)
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a comma or a right-parenthesis symbol! Commas seperate the elements of the array, while the right-parenthesis ends it.", _currentToken.StartPosition, _currentToken.EndPosition));
                else
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a comma or a right-parenthesis symbol! The comma is used to create an array and seperate its elements, while the right-parenthesis declares the end of a parenthetical expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            endPosition = _currentToken.EndPosition;
            Advance();
        }

        if (!isArray && elements.Count > 0)
        {
            _result.Node.StartPosition = startPosition;
            _result.Node.EndPosition = endPosition;
        }
        else
            _result.Success(new ArrayLikeNode(elements, false, startPosition, endPosition));
    }

    /// <summary>
    /// Tries parsing a list, an <see cref="ArrayLikeNode"/> with <see cref="ArrayLikeNode.CreateList"/> set to <see langword="true"/>. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.LeftSquareBracket"/>.
    /// </summary>
    private void ParseList()
    {
        Position startPosition = _currentToken.StartPosition;
        Advance();

        Token possibleErrorToken = _currentToken;

        if (_currentToken.Type == TokenType.NewLine)
            Advance();

        Position endPosition;
        List<Node> elements = [];
        if (_currentToken.Type == TokenType.RightSquareBracket)
        {
            endPosition = _currentToken.EndPosition;
            Advance();
        }
        else
        {
            ParseExpression();
            if (_result.Error is not null)
            {
                _result.Failure(10, new StackedSyntaxError(_result.Error, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a right-square-bracket symbol! A list expression must end with a right-square-bracket.", possibleErrorToken.StartPosition, possibleErrorToken.EndPosition)));
                return;
            }
            elements.Add(_result.Node);

            if (_currentToken.Type == TokenType.NewLine)
                Advance();

            while (_currentToken.Type == TokenType.Comma)
            {
                possibleErrorToken = _currentToken;
                Advance();

                if (_currentToken.Type == TokenType.NewLine)
                    Advance();

                ParseExpression();
                if (_result.Error is not null)
                {
                    _result.Failure(10, new StackedSyntaxError(_result.Error, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a right-square-bracket symbol! A list expression must end with a right-square-bracket.", possibleErrorToken.StartPosition, possibleErrorToken.EndPosition)));
                    return;
                }
                elements.Add(_result.Node);

                if (_currentToken.Type == TokenType.NewLine)
                    Advance();
            }

            if (_currentToken.Type != TokenType.RightSquareBracket)
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a comma or a right-square-bracket symbol! Commas are used to seperate elements in the list, while the right-square-bracket ends it.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            endPosition = _currentToken.EndPosition;
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
        Advance();

        Token possibleErrorToken = _currentToken;
        List<(Node Key, Node Value)> pairs = [];

        if (_currentToken.Type == TokenType.NewLine)
            Advance();

        Position endPosition;
        if (_currentToken.Type == TokenType.RightCurlyBracket)
        {
            endPosition = _currentToken.EndPosition;
            Advance();
        }
        else
        {
            ParseExpression(true);
            if (_result.Error is not null)
            {
                _result.Failure(10, new StackedSyntaxError(_result.Error, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a right-curly-bracket symbol! A dictionary expression must end with a right-curly-bracket.", possibleErrorToken.StartPosition, possibleErrorToken.EndPosition)));
                return;
            }

            Node left = _result.Node;
            if (_currentToken.Type == TokenType.NewLine)
                Advance();

            if (_currentToken.Type != TokenType.Colon)
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a colon symbol! The colon is the seperator between a key and its value in a dictionary.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            Advance();

            if (_currentToken.Type == TokenType.NewLine)
                Advance();

            ParseExpression();
            if (_result.Error is not null)
                return;

            Node right = _result.Node;
            pairs.Add(new(left, right));
            if (_currentToken.Type == TokenType.NewLine)
                Advance();

            while (_currentToken.Type == TokenType.Comma)
            {
                possibleErrorToken = _currentToken;
                Advance();

                if (_currentToken.Type == TokenType.NewLine)
                    Advance();

                ParseExpression(true);
                if (_result.Error is not null)
                {
                    _result.Failure(10, new StackedSyntaxError(_result.Error, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a right-curly-bracket symbol! A dictionary expression must end with a right-curly-bracket.", possibleErrorToken.StartPosition, possibleErrorToken.EndPosition)));
                    return;
                }

                left = _result.Node;
                if (_currentToken.Type == TokenType.NewLine)
                    Advance();

                if (_currentToken.Type != TokenType.Colon)
                {
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a colon symbol! The colon is the seperator between a key and its value in a dictionary.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }

                Advance();

                if (_currentToken.Type == TokenType.NewLine)
                    Advance();

                ParseExpression();
                if (_result.Error is not null)
                    return;

                right = _result.Node;
                pairs.Add(new(left, right));
                if (_currentToken.Type == TokenType.NewLine)
                    Advance();
            }

            if (_currentToken.Type != TokenType.RightCurlyBracket)
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a comma or right-curly-bracket symbol! Commas are used to seperate key-value pairs in the dictionary, and the right-curly-bracket declares its end.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            endPosition = _currentToken.EndPosition;
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
        Advance();

        List<(Node Condition, Node Body)> cases = [];
        Node? elseCase = null;

        ParseExpression();
        if (_result.Error is not null)
            return;

        Node condition = _result.Node;
        if (_currentToken.Type != TokenType.KeywordDo)
        {
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }

        Advance();

        Node body;
        if (_currentToken.Type == TokenType.NewLine)
        {
            Advance();

            ParseStatements();
            if (_result.Error is not null)
                return;

            body = _result.Node;
            Position endPosition;

            cases.Add(new(condition, body));
            if (_currentToken.Type == TokenType.KeywordEnd)
            {
                endPosition = _currentToken.EndPosition;
                Advance();
            }
            else if (_currentToken.Type == TokenType.KeywordElse)
            {
                while (_currentToken.Type == TokenType.KeywordElse)
                {
                    Advance();

                    if (_currentToken.Type == TokenType.KeywordIf)
                    {
                        if (elseCase is not null)
                        {
                            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "The \"else-if\" expression cannot be declared before the \"else\" expression! You cannot have \"else\" expressions before or in-between \"else if\" expressions.", PeekPrevious().StartPosition, _currentToken.EndPosition));
                            return;
                        }

                        Advance();

                        ParseExpression();
                        if (_result.Error is not null)
                            return;

                        condition = _result.Node;
                        if (_currentToken.Type != TokenType.KeywordDo)
                        {
                            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"else if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                            return;
                        }

                        Advance();

                        ParseStatements();
                        if (_result.Error is not null)
                            return;

                        body = _result.Node;
                        cases.Add(new(condition, body));
                    }
                    else if (_currentToken.Type != TokenType.KeywordDo)
                    {
                        _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'if' or 'do' keywords! The 'if' keyword declares the start of an \"else if\" expression, and the 'do' keyword declares the start of the body of an \"else\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }
                    else
                    {
                        if (elseCase is not null)
                        {
                            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "There should only be one \"else\" expression! You cannot have multiple \"else\" expressions in an \"if\" expression.", PeekPrevious().StartPosition, _currentToken.EndPosition));
                            return;
                        }

                        Advance();

                        ParseStatements();
                        if (_result.Error is not null)
                            return;

                        elseCase = _result.Node;
                    }
                }

                if (_currentToken.Type != TokenType.KeywordEnd)
                {
                    if (elseCase is null)
                        _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'else' or 'end' keywords! The 'else' keyword defines the start of an \"else\" or \"else if\" expression, and the 'end' keyword declares the end of the whole \"if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    else
                        _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'end' keyword! The 'end' keyword declares the end of the whole \"if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }

                endPosition = _currentToken.EndPosition;
                Advance();
            }
            else
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'else' or 'end' keywords! The 'else' keyword defines the start of an \"else\" or \"else if\" expression, and the 'end' keyword declares the end of the whole \"if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            _result.Success(new IfNode(cases, elseCase, startPosition, endPosition));
            return;
        }

        ParseStatement();
        if (_result.Error is not null)
            return;

        body = _result.Node;
        cases.Add(new(condition, body));

        while (_currentToken.Type == TokenType.KeywordElse)
        {
            Advance();

            if (_currentToken.Type == TokenType.KeywordIf)
            {
                if (elseCase is not null)
                {
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "The \"else-if\" expression cannot be declared before the \"else\" expression! You cannot have \"else\" expressions before or in-between \"else if\" expressions.", PeekPrevious().StartPosition, _currentToken.EndPosition));
                    return;
                }

                Advance();

                ParseExpression();
                if (_result.Error is not null)
                    return;

                condition = _result.Node;
                if (_currentToken.Type != TokenType.KeywordDo)
                {
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"else if\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }

                Advance();

                ParseStatement();
                if (_result.Error is not null)
                    return;

                body = _result.Node;
                cases.Add(new(condition, body));
            }
            else if (_currentToken.Type != TokenType.KeywordDo)
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'if' or 'do' keywords! The 'if' keyword declares the start of an \"else if\" expression, and the 'do' keyword declares the start of the body of an \"else\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }
            else
            {
                if (elseCase is not null)
                {
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "There should only be one \"else\" expression! You cannot have multiple \"else\" expressions in an \"if\" expression.", PeekPrevious().StartPosition, _currentToken.EndPosition));
                    return;
                }

                Advance();

                ParseStatement();
                if (_result.Error is not null)
                    return;

                elseCase = _result.Node;
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
        Advance();

        Node to = InvalidNode.s_invalidNode;
        Node? from = null;
        Node? step = null;
        Node? iterationVariable = null;

        if (_currentToken.Type == TokenType.KeywordFrom)
        {
            Advance();
            ParseExpression();
            if (_result.Error is not null)
                return;

            from = _result.Node;
        }

        if (_currentToken.Type != TokenType.KeywordTo)
        {
            if (from is null)
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'to' or 'from' keyword! The 'to' keyword and the following expression is the amount to count to in the count loop, and the optional 'from' keyword and the following expression is the amount to count from.", _currentToken.StartPosition, _currentToken.EndPosition));
            else
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'to' keyword! The 'to' keyword and the following expression is the amount to count to in the count loop.", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }

        Advance();
        ParseExpression();
        if (_result.Error is not null)
            return;

        to = _result.Node;
        if (_currentToken.Type == TokenType.KeywordStep)
        {
            Advance();

            ParseExpression();
            if (_result.Error is not null)
                return;

            step = _result.Node;
        }

        if (_currentToken.Type == TokenType.KeywordAs)
        {
            Advance();
            ParseExpression();
            if (_result.Error is not null)
                return;

            iterationVariable = _result.Node;
        }

        if (_currentToken.Type != TokenType.KeywordDo)
        {
            if (step is null && iterationVariable is null)
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'do', 'step' or 'as' keyword! The 'do' keyword declares the start of the body of the count loop, the optional 'step' keyword and the following expression is the increment, and the optional 'as' keyword and the following expression is where the iterations are stored.", _currentToken.StartPosition, _currentToken.EndPosition));
            else if (iterationVariable is null)
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'do' or 'as' keyword! The 'do' keyword declares the start of the body of the count loop, and the optional 'as' keyword and the following expression is where the iterations are stored.", _currentToken.StartPosition, _currentToken.EndPosition));
            else
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'do' keyword! The 'do' keyword declares the start of the body of the count loop.", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }

        Advance();

        Node body;
        Position endPosition;
        if (_currentToken.Type == TokenType.NewLine)
        {
            Advance();

            ParseStatements();
            if (_result.Error is not null)
                return;

            body = _result.Node;
            if (_currentToken.Type != TokenType.KeywordEnd)
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'end' keyword! The 'end' keyword declares the end of the count loop.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            endPosition = _currentToken.EndPosition;
            Advance();
        }
        else
        {
            ParseStatement();
            if (_result.Error is not null)
                return;

            body = _result.Node;
            endPosition = body.EndPosition;
        }

        _result.Success(new CountNode(to, from, step, iterationVariable, body, startPosition, endPosition));
    }

    /// <summary>
    /// Tries parsing a for-each expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordFor"/>.
    /// </summary>
    private void ParseForEachExpression()
    {
        Position startPosition = _currentToken.StartPosition;
        Advance();

        if (_currentToken.Type != TokenType.KeywordEach)
        {
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'each' keyword! The 'each' keyword is essential for forming a \"for each\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }

        Advance();
        ParseExpression();
        if (_result.Error is not null)
            return;

        if (_result.Node is not BinaryOperationNode expression || expression.Operator != TokenType.KeywordIn)
        {
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a binary expression in the format \"[iteration variable] in [iterable collection]\".", _result.Node.StartPosition, _result.Node.EndPosition));
            return;
        }

        if (_currentToken.Type != TokenType.KeywordDo)
        {
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"for each\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }

        Advance();

        Node body;
        Position endPosition;
        if (_currentToken.Type == TokenType.NewLine)
        {
            ParseStatements();
            if (_result.Error is not null)
                return;
            else if (_currentToken.Type != TokenType.KeywordEnd)
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'end' keyword! The 'end' keyword declares the end of the whole \"for each\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            body = _result.Node;
            endPosition = _currentToken.EndPosition;
            Advance();
        }
        else
        {
            ParseStatement();
            if (_result.Error is not null)
                return;

            body = _result.Node;
            endPosition = PeekPrevious().EndPosition;
        }

        _result.Success(new ForEachNode(expression, body, startPosition, endPosition));
    }

    /// <summary>
    /// Tries parsing a while expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordWhile"/>.
    /// </summary>
    private void ParseWhileExpression()
    {
        Position startPosition = _currentToken.StartPosition;
        Advance();

        ParseExpression();
        if (_result.Error is not null)
            return;

        Node condition = _result.Node;
        if (_currentToken.Type != TokenType.KeywordDo)
        {
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'do' keyword! The 'do' keyword declares the start of the body of the while loop.", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }

        Advance();

        Node body;
        Position endPosition;
        if (_currentToken.Type == TokenType.NewLine)
        {
            Advance();

            ParseStatements();
            if (_result.Error is not null)
                return;

            body = _result.Node;
            if (_currentToken.Type != TokenType.KeywordEnd)
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'end' keyword! The 'end' keyword declares the end of the while loop.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            endPosition = _currentToken.EndPosition;
            Advance();
        }
        else
        {
            ParseStatement();
            if (_result.Error is not null)
                return;

            body = _result.Node;
            endPosition = body.EndPosition;
        }

        _result.Success(new WhileNode(condition, body, startPosition, endPosition));
    }

    /// <summary>
    /// Tries parsing a try expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordTry"/>.
    /// </summary>
    private void ParseTryExpression()
    {
        Position startPosition = _currentToken.StartPosition;
        Advance();

        Node block;
        List<(Node ErrorType, Node? Variable, Node Body)> cases = [];
        (Node? Variable, Node Body)? emptyCase = null;

        if (_currentToken.Type != TokenType.KeywordDo)
        {
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"try\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }

        Advance();

        Node body;
        if (_currentToken.Type == TokenType.NewLine)
        {
            Advance();

            ParseStatements();
            if (_result.Error is not null)
                return;

            block = _result.Node;
            Position endPosition;
            if (_currentToken.Type == TokenType.KeywordEnd)
            {
                endPosition = _currentToken.EndPosition;
                Advance();
            }
            else if (_currentToken.Type == TokenType.KeywordCatch)
            {
                while (_currentToken.Type == TokenType.KeywordCatch)
                {
                    Advance();

                    Node? error = null;
                    bool isErrorNull = true;

                ErrorExpressionEvaluation:
                    bool isAsKeyword = _currentToken.Type == TokenType.KeywordAs;
                    if (_currentToken.Type == TokenType.KeywordDo || isAsKeyword)
                    {
                        if (emptyCase is not null)
                        {
                            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "There should only be one empty \"catch\" expression! You cannot have multiple empty \"catch\" expressions in a \"try\" expression.", PeekPrevious().StartPosition, _currentToken.EndPosition));
                            return;
                        }

                        Node? @as = null;
                        if (isAsKeyword)
                        {
                            Advance();
                            ParseExpression();
                            if (_result.Error is not null)
                                return;

                            @as = _result.Node;
                            if (_currentToken.Type != TokenType.KeywordDo)
                            {
                                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"catch\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                                return;
                            }
                        }

                        Advance();

                        ParseStatements();
                        if (_result.Error is not null)
                            return;

                        body = _result.Node;
                        if (error is not null)
                            cases.Add(new(error, @as, body));
                        else
                            emptyCase = new(@as, body);
                    }
                    else if (isErrorNull && _currentToken.Type != TokenType.EndOfFile && _currentToken.Type != TokenType.NewLine)
                    {
                        if (emptyCase is not null)
                        {
                            Token previous = PeekPrevious();
                            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "There can't be any \"catch\" expressions after an empty \"catch\" expression!", previous.StartPosition, previous.EndPosition));
                            return;
                        }

                        ParseExpression();
                        if (_result.Error is not null)
                            return;

                        error = _result.Node;
                        isErrorNull = false;
                        goto ErrorExpressionEvaluation;
                    }
                    else if (isErrorNull)
                    {
                        _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected an expression or the 'as' or 'do' keywords! An expression after the 'catch' keyword defines what error(s) will lead to the \"catch\" expression, the 'as' keyword and the following expression declares where the error will be stored and the 'do' keyword declares the start of the body of the \"catch\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }
                    else
                    {
                        _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'as' or 'do' keywords! The 'as' keyword and the following expression tells the interpreter where the error will be stored and the 'do' keyword declares the start of the body of the \"catch\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }
                }

                if (_currentToken.Type != TokenType.KeywordEnd)
                {
                    if (emptyCase is null)
                        _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'catch' or 'end' keywords! The 'catch' keyword defines the start of an \"catch\" expression, and the 'end' keyword declares the end of the whole \"try\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    else
                        _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'end' keyword! The 'end' keyword declares the end of the whole \"try\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }

                endPosition = _currentToken.EndPosition;
                Advance();
            }
            else
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'catch' or 'end' keywords! The 'catch' keyword defines the start of an \"catch\" expression, and the 'end' keyword declares the end of the whole \"try\" expression. Note: In newer versions of ezr², 'error' has been replaced by 'catch' and is now recognized as an identifier. Check [DOCUMENTATION LINK HERE] for more info.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }

            _result.Success(new TryNode(block, cases, emptyCase, startPosition, endPosition));
            return;
        }

        ParseStatement();
        if (_result.Error is not null)
            return;

        block = _result.Node;
        while (_currentToken.Type == TokenType.KeywordCatch)
        {
            Advance();

            Node? error = null;
            bool isErrorNull = true;

        ErrorExpressionEvaluation:
            bool isAsKeyword = _currentToken.Type == TokenType.KeywordAs;
            if (_currentToken.Type == TokenType.KeywordDo || isAsKeyword)
            {
                if (emptyCase is not null)
                {
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "There should only be one empty \"catch\" expression! You cannot have multiple empty \"catch\" expressions in a \"try\" expression.", PeekPrevious().StartPosition, _currentToken.EndPosition));
                    return;
                }

                Node? @as = null;
                if (isAsKeyword)
                {
                    Advance();
                    ParseExpression();
                    if (_result.Error is not null)
                        return;

                    @as = _result.Node;
                    if (_currentToken.Type != TokenType.KeywordDo)
                    {
                        _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'do' keyword! The 'do' keyword declares the start of the body of the \"catch\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                        return;
                    }
                }

                Advance();

                ParseStatement();
                if (_result.Error is not null)
                    return;

                body = _result.Node;
                if (error is not null)
                    cases.Add(new(error, @as, body));
                else
                    emptyCase = new(@as, body);
            }
            else if (isErrorNull && _currentToken.Type != TokenType.EndOfFile && _currentToken.Type != TokenType.NewLine)
            {
                if (emptyCase is not null)
                {
                    Token previous = PeekPrevious();
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "There can't be any \"catch\" expressions after an empty \"catch\" expression!", previous.StartPosition, previous.EndPosition));
                    return;
                }

                ParseExpression();
                if (_result.Error is not null)
                    return;

                error = _result.Node;
                isErrorNull = false;
                goto ErrorExpressionEvaluation;
            }
            else if (isErrorNull)
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected an expression or the 'as' or 'do' keywords! An expression after the 'catch' keyword defines what error(s) will lead to the \"catch\" expression, the 'as' keyword and the following expression declares where the error will be stored and the 'do' keyword declares the start of the body of the \"catch\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }
            else
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'as' or 'do' keywords! The 'as' keyword and the following expression tells the interpreter where the error will be stored and the 'do' keyword declares the start of the body of the \"catch\" expression.", _currentToken.StartPosition, _currentToken.EndPosition));
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
        AccessMod accessibilityModifiers = AccessMod.None;
        bool returnLast = false;

        if (_currentToken.Type == TokenType.KeywordGlobal)
        {
            accessibilityModifiers |= AccessMod.Global;
            Advance();
        }
        else if (_currentToken.Type == TokenType.KeywordPrivate)
        {
            accessibilityModifiers |= AccessMod.Private;
            Advance();
        }

        if (_currentToken.Type == TokenType.KeywordStatic)
        {
            accessibilityModifiers |= AccessMod.Static;
            Advance();
        }

        if (_currentToken.Type == TokenType.KeywordConstant)
        {
            accessibilityModifiers |= AccessMod.Constant;
            Advance();
        }

        Advance();
        Node? name = null;
        List<Node> parameters = [];
        Node? extraKeywordArguments = null;
        Node? extraPositionalArguments = null;
        Node body;

        Position endPosition;
        bool isNameNull = true;

    FunctionDefinitionEvaluation:
        bool isWithKeyword = _currentToken.Type == TokenType.KeywordWith;
        if (_currentToken.Type == TokenType.KeywordDo || isWithKeyword)
        {
            if (isWithKeyword)
            {
                do
                {
                    Position paramStartPosition = _currentToken.StartPosition;
                    Advance();

                    if (_currentToken.Type == TokenType.NewLine)
                        Advance();

                    if (_currentToken.Type == TokenType.KeywordMore)
                    {
                        Advance();

                        bool isNamedExtraParamters = false;
                        if (_currentToken.Type == TokenType.KeywordNamed)
                        {
                            isNamedExtraParamters = true;
                            Advance();
                        }

                        if (_currentToken.Type != TokenType.KeywordAs)
                        {
                            if (isNamedExtraParamters)
                                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'as' keyword! The 'as' keyword comes in-between the 'named' keyword and an expression when declaring extra keyword parameters for an object.", _currentToken.StartPosition, _currentToken.EndPosition));
                            else
                                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'as' or 'named' keywords! The 'named' keyword starts the definition of extra keyword parameters and the 'as' keyword comes in-between the 'from' or 'named' keyword and an expression when declaring extra positional or keyword parameters for an object.", _currentToken.StartPosition, _currentToken.EndPosition));

                            return;
                        }

                        Advance();
                        ParseExpression(true);
                        if (_result.Error is not null)
                            return;

                        if (isNamedExtraParamters && extraKeywordArguments is not null)
                        {
                            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Only one extra keyword parameter dictionary can be defined per function!", paramStartPosition, _currentToken.EndPosition));
                            return;
                        }
                        else if (!isNamedExtraParamters && extraPositionalArguments is not null)
                        {
                            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Only one extra positional parameter list can be defined per function!", paramStartPosition, _currentToken.EndPosition));
                            return;
                        }

                        if (isNamedExtraParamters)
                            extraKeywordArguments = _result.Node;
                        else
                            extraPositionalArguments = _result.Node;

                        if (_currentToken.Type == TokenType.NewLine)
                            Advance();

                        continue;
                    }
                    else if (extraKeywordArguments is not null || extraPositionalArguments is not null)
                    {
                        _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Normal mixed parameters must come before extra positional/keyword parameter list/dictionary definitions.", paramStartPosition, _currentToken.EndPosition));
                        return;
                    }

                    ParseExpression();
                    if (_result.Error is not null)
                        return;
                    parameters.Add(_result.Node);

                    if (_currentToken.Type == TokenType.NewLine)
                        Advance();
                } while (_currentToken.Type == TokenType.Comma);

                if (_currentToken.Type != TokenType.KeywordDo)
                {
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a comma symbol or the 'do' keyword! The comma symbol and the following expression defines another parameter and the 'do' keyword declares the start of the body of the \"function\" definition.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }
            }

            if (name is null && accessibilityModifiers != AccessMod.None)
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "The accessibility of an anonymous function cannot be declared in the definition! Anonymous functions are meant to be stored in variables or constants, where the accessibility is determined.", startPosition, _currentToken.EndPosition));
                return;
            }

            Advance();

            if (_currentToken.Type == TokenType.NewLine)
            {
                Advance();

                ParseStatements();
                if (_result.Error is not null)
                    return;

                body = _result.Node;
                if (_currentToken.Type != TokenType.KeywordEnd)
                {
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'end' keyword! The 'end' keyword declares the end of the whole \"function\" definition.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }

                endPosition = _currentToken.EndPosition;
                Advance();
            }
            else
            {
                ParseStatement();
                if (_result.Error is not null)
                    return;

                body = _result.Node;
                endPosition = body.EndPosition;
                returnLast = true;
            }
        }
        else if (isNameNull && _currentToken.Type != TokenType.EndOfFile && _currentToken.Type != TokenType.NewLine)
        {
            ParseExpression();
            if (_result.Error is not null)
                return;

            name = _result.Node;
            isNameNull = false;
            goto FunctionDefinitionEvaluation;
        }
        else if (isNameNull)
        {
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected an expression or the 'with' or 'do' keywords! An expression after the 'function' keyword defines the name, the 'with' keyword and the following expression(s, seperated by commas) declare(s) the paramenters and the 'do' keyword declares the start of the body of the \"function\".", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }
        else
        {
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'with' or 'do' keywords! The 'with' keyword and the following expression(s, seperated by commas) declare(s) the paramenters and the 'do' keyword declares the start of the body of the \"function\".", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }

        _result.Success(new FunctionDefinitionNode(name, accessibilityModifiers, returnLast, parameters, extraKeywordArguments, extraPositionalArguments, body, startPosition, endPosition));
    }

    /// <summary>
    /// Tries parsing an class definition expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordObject"/>.
    /// </summary>
    private void ParseClassDefinitionExpression()
    {
        Position startPosition = _currentToken.StartPosition;
        AccessMod accessibilityModifiers = AccessMod.None;
        bool readOnly = false;

        if (_currentToken.Type == TokenType.KeywordGlobal)
        {
            accessibilityModifiers |= AccessMod.Global;
            Advance();
        }
        else if (_currentToken.Type == TokenType.KeywordPrivate)
        {
            accessibilityModifiers |= AccessMod.Private;
            Advance();
        }

        if (_currentToken.Type == TokenType.KeywordStatic)
        {
            accessibilityModifiers |= AccessMod.Static;
            Advance();
        }

        if (_currentToken.Type == TokenType.KeywordConstant)
        {
            accessibilityModifiers |= AccessMod.Constant;
            Advance();
        }

        if (_currentToken.Type == TokenType.KeywordReadonly)
        {
            readOnly = true;
            Advance();
        }

        Advance();
        Node? name = null;
        List<Node> parents = [];
        Node body;

        Position endPosition;

        if (_currentToken.Type is not TokenType.KeywordFrom and not TokenType.KeywordDo and not TokenType.EndOfFile and not TokenType.NewLine)
        {
            ParseExpression();
            name = _result.Node;
            if (_result.Error is not null)
                return;
        }
        else if (accessibilityModifiers != AccessMod.None)
        {
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "The accessibility of an anonymous class cannot be declared in the definition! Anonymous classes are meant to be stored in variables or constants, where the accessibility is determined.", startPosition, _currentToken.EndPosition));
            return;
        }

        if (_currentToken.Type == TokenType.KeywordFrom)
        {
            do
            {
                Advance();
                if (_currentToken.Type == TokenType.NewLine)
                    Advance();

                ParseExpression();
                if (_result.Error is not null)
                    return;
                parents.Add(_result.Node);

                if (_currentToken.Type == TokenType.NewLine)
                    Advance();
            }
            while (_currentToken.Type == TokenType.Comma);

            if (_currentToken.Type != TokenType.KeywordDo)
            {
                _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected a comma symbol or the 'do' keyword! The comma symbol and the following expression defines another parent and the 'do' keyword declares the start of the body of the class definition.", _currentToken.StartPosition, _currentToken.EndPosition));
                return;
            }
        }

        if (_currentToken.Type == TokenType.KeywordDo)
        {
            Advance();

            if (_currentToken.Type == TokenType.NewLine)
            {
                Advance();

                ParseStatements();
                body = _result.Node;
                if (_result.Error is not null)
                    return;

                if (_currentToken.Type != TokenType.KeywordEnd)
                {
                    _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'end' keyword! The 'end' keyword declares the end of the whole class definition.", _currentToken.StartPosition, _currentToken.EndPosition));
                    return;
                }

                endPosition = _currentToken.EndPosition;
                Advance();
            }
            else
            {
                ParseStatement();
                if (_result.Error is not null)
                    return;

                body = _result.Node;
                endPosition = body.EndPosition;
            }

            _result.Success(new ClassDefinitionNode(name, accessibilityModifiers, readOnly, parents, body, startPosition, endPosition));
            return;
        }

        _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar,
            (name is null && parents.Count == 0)
            ? "Expected an expression or the 'from' or 'do' keywords! An expression after the 'object' keyword defines the name, the 'from' keyword and the following expression(s, seperated by commas) define(s) the parents and the 'do' keyword declares the start of the body of the class."
            : (parents.Count == 0)
                ? "Expected the 'from' or 'do' keywords! The 'from' keyword and the following expression(s, seperated by commas) define(s) the parents and the 'do' keyword declares the start of the body of the class."
                : "Expected the 'do' keyword! The 'do' keyword declares the start of the body of the class.",
            _currentToken.StartPosition, _currentToken.EndPosition));
    }

    /// <summary>
    /// Tries parsing an include expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordInclude"/>.
    /// </summary>
    private void ParseIncludeExpression()
    {
        Position startPosition = _currentToken.StartPosition;
        Advance();

        Node? subStructure = null;
        Node? nickname = null;
        bool isDumped = false;
        Node script;

        if (_currentToken.Type is not TokenType.KeywordAll and not TokenType.Comma)
        {
            ParseExpression();
            subStructure = _result.Node;
            if (_result.Error is not null)
                return;
        }
        else
        {
            isDumped = true;
            Advance();
        }

        if (_currentToken.Type == TokenType.KeywordFrom)
        {
            Advance();

            ParseExpression();
            script = _result.Node;
            if (_result.Error is not null)
                return;
        }
        else if (subStructure is not null)
        {
            script = subStructure;
            subStructure = null;
        }
        else
        {
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'from' keyword! If a specific object being included from a script (when the object's name is provided after the 'include' keyword) or if the whole script is added to the script (using the 'all' keyword or a comma symbol after the 'include' keyword), the 'from' keyword followed by an expression declaring the script's name or path must be provided.", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }

        if (_currentToken.Type == TokenType.KeywordAs)
        {
            Advance();

            ParseExpression();
            nickname = _result.Node;
            if (_result.Error is not null)
                return;
        }

        _result.Success(new IncludeNode(script, subStructure, isDumped, nickname, startPosition, PeekPrevious().EndPosition));
    }

    /// <summary>
    /// Tries parsing a define block expression. Starts from <see cref="_currentToken"/>, which should be of <see cref="TokenType"/> <see cref="TokenType.KeywordDefine"/>.
    /// </summary>
    private void ParseDefineBlockExpression()
    {
        Position startPosition = _currentToken.StartPosition;
        AccessMod accessibilityModifiers = AccessMod.None;

        Advance();
        if (_currentToken.Type == TokenType.KeywordGlobal)
        {
            accessibilityModifiers |= AccessMod.Global;
            Advance();
        }
        else if (_currentToken.Type == TokenType.KeywordPrivate)
        {
            accessibilityModifiers |= AccessMod.Private;
            Advance();
        }

        if (_currentToken.Type == TokenType.KeywordStatic)
        {
            accessibilityModifiers |= AccessMod.Static;
            Advance();
        }

        if (_currentToken.Type == TokenType.KeywordConstant)
        {
            accessibilityModifiers |= AccessMod.Constant;
            Advance();
        }

        if (accessibilityModifiers == AccessMod.None)
        {
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'private', 'global', 'static' or 'constant' keywords! The \"define block\" needs at least one accessibility modifier. The 'private' or 'global' keywords declare all variables, constants, functions and classes defined in the block as private or global - not both, the 'static' or 'constant' keywords declare them as statically bound or constants - can be both.", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }

        ParseStatements();
        if (_result.Error is not null)
            return;

        Node body = _result.Node;
        if (_currentToken.Type != TokenType.KeywordEnd)
        {
            _result.Failure(10, new SyntaxError(SyntaxError.InvalidGrammar, "Expected the 'end' keyword! The 'end' keyword declares the end of the whole \"define block\" definition.", _currentToken.StartPosition, _currentToken.EndPosition));
            return;
        }

        Position endPosition = _currentToken.EndPosition;
        Advance();

        _result.Success(new DefineBlockNode(body, accessibilityModifiers, startPosition, endPosition));
    }
}
