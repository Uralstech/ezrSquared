using System.Collections.Generic;

namespace EzrSquared.EzrNodes
{
    using EzrCommon;

    /// <summary>
    /// The representation of an ezrSquared source code construct. This is the base class of all nodes. Only for inheritance!
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// The starting <see cref="Position"/> of the <see cref="Node"/>.
        /// </summary>
        public Position StartPosition;

        /// <summary>
        /// The ending <see cref="Position"/> of the <see cref="Node"/>.
        /// </summary>
        public Position EndPosition;

        /// <summary>
        /// Creates a new <see cref="Node"/> object.
        /// </summary>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="Node"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="Node"/>.</param>
        public Node(Position startPosition, Position endPosition)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
        }

        public override string ToString()
        {
            return "Node()";
        }
    }

    /// <summary>
    /// The dummy invalid <see cref="Node"/> structure. For returning instead of <see langword="null"/> if an <see cref="EzrErrors.Error"/> occurs during parsing.
    /// </summary>
    public class InvalidNode : Node
    {
        /// <summary>
        /// The static <see cref="InvalidNode"/> object.
        /// </summary>
        public static InvalidNode StaticInvalidNode = new InvalidNode(new Position(-1, -1, string.Empty, string.Empty), new Position(0, -1, string.Empty, string.Empty));

        /// <summary>
        /// Creates a new <see cref="InvalidNode"/> object.
        /// </summary>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="InvalidNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="InvalidNode"/>.</param>
        public InvalidNode(Position startPosition, Position endPosition) : base(startPosition, endPosition) { }

        public override string ToString()
        {
            return "InvalidNode()";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure of a simple value like literals and variables.
    /// </summary>
    public class ValueNode : Node
    {
        /// <summary>
        /// The value the <see cref="ValueNode"/> represents.
        /// </summary>
        public Token Value;

        /// <summary>
        /// Creates a new <see cref="ValueNode"/> object.
        /// </summary>
        /// <param name="value">The value the <see cref="ValueNode"/> represents, a <see cref="Token"/> object of the same type as must be used with the <see cref="ValueNodeType"/> <paramref name="type"/>.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="ValueNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="ValueNode"/>.</param>
        public ValueNode(Token value, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"ValueNode({Value.Value})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for an arraylike (array or list).
    /// </summary>
    public class ArrayLikeNode : Node
    {
        /// <summary>
        /// The elements of the arraylike.
        /// </summary>
        public List<Node> Elements;

        /// <summary>
        /// The check for if the <see cref="ArrayLikeNode"/> should create a list instead of an array.
        /// </summary>
        public bool CreateList;

        /// <summary>
        /// Creates a new <see cref="ArrayLikeNode"/> object.
        /// </summary>
        /// <param name="elements">The elements of the arraylike.</param>
        /// <param name="createList">The check for if the <see cref="ArrayLikeNode"/> should create a list instead of an array.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="ArrayLikeNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="ArrayLikeNode"/>.</param>
        public ArrayLikeNode(List<Node> elements, bool createList, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Elements = elements;
            CreateList = createList;
        }

        public override string ToString()
        {
            string[] elements = new string[Elements.Count];
            for (int i = 0; i < Elements.Count; i++)
                elements[i] = Elements[i].ToString();

            return $"ArrayLikeNode([{string.Join(", ", elements)}], {CreateList})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for a dictionary.
    /// </summary>
    public class DictionaryNode : Node
    {
        /// <summary>
        /// The key-value pairs of the dictionary.
        /// </summary>
        public List<Node[]> KeyValuePairs;

        /// <summary>
        /// Creates a new <see cref="DictionaryNode"/> object.
        /// </summary>
        /// <param name="keyValuePairs">The key-value pairs of the dictionary.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="DictionaryNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="DictionaryNode"/>.</param>
        public DictionaryNode(List<Node[]> keyValuePairs, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            KeyValuePairs = keyValuePairs;
        }

        public override string ToString()
        {
            string[] keyValuePairs = new string[KeyValuePairs.Count];
            for (int i = 0; i < KeyValuePairs.Count; i++)
                keyValuePairs[i] = $"{keyValuePairs[i][0]} : {keyValuePairs[i][1]}";
            return $"DictionaryNode([{string.Join(", ", keyValuePairs)}])";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for a binary operation.
    /// </summary>
    public class BinaryOperationNode : Node
    {
        /// <summary>
        /// The first operand of the binary operation.
        /// </summary>
        public Node Left;

        /// <summary>
        /// The second operand of the binary operation.
        /// </summary>
        public Node Right;

        /// <summary>
        /// The operator <see cref="TokenType"/> of the binary operation.
        /// </summary>
        public TokenType Operator;

        /// <summary>
        /// Creates a new <see cref="BinaryOperationNode"/> object.
        /// </summary>
        /// <param name="left">The first operand of the binary operation.</param>
        /// <param name="right">The second operand of the binary operation.</param>
        /// <param name="operator">The operator <see cref="TokenType"/> of the binary operation.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="BinaryOperationNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="BinaryOperationNode"/>.</param>
        public BinaryOperationNode(Node left, Node right, TokenType @operator, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Left = left;
            Right = right;
            Operator = @operator;
        }

        public override string ToString()
        {
            return $"BinaryOperationNode({Left}, {Operator}, {Right})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for a unary operation.
    /// </summary>
    public class UnaryOperationNode : Node
    {
        /// <summary>
        /// The operand of the unary operation.
        /// </summary>
        public Node Operand;

        /// <summary>
        /// The operator <see cref="TokenType"/> of the unary operation.
        /// </summary>
        public TokenType Operator;

        /// <summary>
        /// Creates a new <see cref="UnaryOperationNode"/> object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation.</param>
        /// <param name="operator">The operator <see cref="TokenType"/> of the unary operation.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="UnaryOperationNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="UnaryOperationNode"/>.</param>
        public UnaryOperationNode(Node operand, TokenType @operator, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Operand = operand;
            Operator = @operator;
        }

        public override string ToString()
        {
            return $"UnaryOperationNode({Operand}, {Operator})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for accesing a variable from the context.
    /// </summary>
    public class VariableAccessNode : Node
    {
        /// <summary>
        /// The name of the variable to access.
        /// </summary>
        public Token Name;

        /// <summary>
        /// The check for if the variable access is from the global or local context.
        /// </summary>
        public bool GlobalAccess;

        /// <summary>
        /// Creates a new <see cref="VariableAccessNode"/> object.
        /// </summary>
        /// <param name="name">The name of the variable, a <see cref="Token"/> object of type <see cref="TokenType.Identifier"/>.</param>
        /// <param name="globalAccess">The check for if the variable access is from the global or local context.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="VariableAccessNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="VariableAccessNode"/>.</param>
        public VariableAccessNode(Token name, bool globalAccess, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Name = name;
            GlobalAccess = globalAccess;
        }

        public override string ToString()
        {
            return $"VariableAccessNode({Name.Value}, {GlobalAccess})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for assigning a value to a <see cref="Node"/> variable in the context.
    /// </summary>
    public class NodeVariableAssignmentNode : Node
    {
        /// <summary>
        /// The <see cref="Node"/> variable to be assigned to.
        /// </summary>
        public Node Variable;

        /// <summary>
        /// The operation <see cref="TokenType"/>, if not <see cref="TokenType.Colon"/>, between the existing value of <see cref="Variable"/> and <see cref="Value"/>. The result of the operation will be assigned to <see cref="Variable"/>.
        /// </summary>
        public TokenType AssignmentOperator;

        /// <summary>
        /// The <see cref="Node"/> value to be assigned to <see cref="Variable"/>.
        /// </summary>
        public Node Value;

        /// <summary>
        /// The check for if the variable assignment is to the global or local context. Irrelevant if the variable to be assigned to is in a local context.
        /// </summary>
        public bool GlobalAssignment;

        /// <summary>
        /// Creates a new <see cref="NodeVariableAssignmentNode"/> object.
        /// </summary>
        /// <param name="variable">The <see cref="Node"/> variable to be assigned to.</param>
        /// <param name="assignmentOperator">The operation <see cref="TokenType"/>, if not <see cref="TokenType.Colon"/>, between the existing value of <paramref name="variable"/> and <paramref name="value"/>. The result of the operation will be assigned to <paramref name="variable"/>.</param>
        /// <param name="value">The <see cref="Node"/> value to be assigned to <paramref name="variable"/>.</param>
        /// <param name="globalAssignment">The check for if the variable assignment is to the global or local context. Irrelevant if the variable to be assigned to is in a local context.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="NodeVariableAssignmentNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="NodeVariableAssignmentNode"/>.</param>
        public NodeVariableAssignmentNode(Node variable, TokenType assignmentOperator, Node value, bool globalAssignment, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Variable = variable;
            AssignmentOperator = assignmentOperator;
            Value = value;
            GlobalAssignment = globalAssignment;
        }

        public override string ToString()
        {
            return $"NodeVariableAssignmentNode({Variable}, {AssignmentOperator}, {Value}, {GlobalAssignment})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for assigning a value to a <see cref="Token"/> variable in the context.
    /// </summary>
    public class TokenVariableAssignmentNode : Node
    {
        /// <summary>
        /// The <see cref="Token"/> variable to be assigned to.
        /// </summary>
        public Token Variable;

        /// <summary>
        /// The operation <see cref="TokenType"/>, if not <see cref="TokenType.Colon"/>, between the existing value of <see cref="Variable"/> and <see cref="Value"/>. The result of the operation will be assigned to <see cref="Variable"/>.
        /// </summary>
        public TokenType AssignmentOperator;

        /// <summary>
        /// The <see cref="Node"/> value to be assigned to <see cref="Variable"/>.
        /// </summary>
        public Node Value;

        /// <summary>
        /// The check for if the variable assignment is to the global or local context. Irrelevant if the variable to be assigned to is in a local context.
        /// </summary>
        public bool GlobalAssignment;

        /// <summary>
        /// Creates a new <see cref="TokenVariableAssignmentNode"/> object.
        /// </summary>
        /// <param name="variable">The <see cref="Token"/> variable to be assigned to.</param>
        /// <param name="assignmentOperator">The operation <see cref="TokenType"/>, if not <see cref="TokenType.Colon"/>, between the existing value of <paramref name="variable"/> and <paramref name="value"/>. The result of the operation will be assigned to <paramref name="variable"/>.</param>
        /// <param name="value">The <see cref="Node"/> value to be assigned to <paramref name="variable"/>.</param>
        /// <param name="globalAssignment">The check for if the variable assignment is to the global or local context. Irrelevant if the variable to be assigned to is in a local context.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="TokenVariableAssignmentNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="TokenVariableAssignmentNode"/>.</param>
        public TokenVariableAssignmentNode(Token variable, TokenType assignmentOperator, Node value, bool globalAssignment, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Variable = variable;
            AssignmentOperator = assignmentOperator;
            Value = value;
            GlobalAssignment = globalAssignment;
        }

        public override string ToString()
        {
            return $"TokenVariableAssignmentNode({Variable.Value}, {AssignmentOperator}, {Value}, {GlobalAssignment})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for a function call.
    /// </summary>
    public class CallNode : Node
    {
        /// <summary>
        /// The function to be called.
        /// </summary>
        public Node FunctionToCall;

        /// <summary>
        /// The array of arguments.
        /// </summary>
        public List<Node> Arguments;

        /// <summary>
        /// Creates a new <see cref="CallNode"/> object.
        /// </summary>
        /// <param name="functionToCall">The function to be called.</param>
        /// <param name="arguments">The array of arguments.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="CallNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="CallNode"/>.</param>
        public CallNode(Node functionToCall, List<Node> arguments, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            FunctionToCall = functionToCall;
            Arguments = arguments;
        }

        public override string ToString()
        {
            string[] arguments = new string[Arguments.Count];
            for (int i = 0; i < Arguments.Count; i++)
                arguments[i] = Arguments[i].ToString();

            return $"CallNode({FunctionToCall}, [{string.Join(", ", arguments)}])";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for an if expression.
    /// </summary>
    public class IfNode : Node
    {
        /// <summary>
        /// The cases of the if expression.
        /// </summary>
        public List<Node[]> Cases;

        /// <summary>
        /// The body of the else case.
        /// </summary>
        public Node? ElseCase;

        /// <summary>
        /// Creates a new <see cref="IfNode"/> object.
        /// </summary>
        /// <param name="cases">The cases of the if expression.</param>
        /// <param name="elseCase">The body of the else case.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="IfNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="IfNode"/>.</param>
        public IfNode(List<Node[]> cases, Node? elseCase, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Cases = cases;
            ElseCase = elseCase;
        }

        public override string ToString()
        {
            string?[] cases = new string[Cases.Count];
            for (int i = 0; i < Cases.Count; i++)
                cases[i] = $"({Cases[i][0]}, {Cases[i][1]})";

            return $"IfNode([{string.Join(", ", cases)}], {ElseCase})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for a count expression.
    /// </summary>
    public class CountNode : Node
    {
        /// <summary>
        /// The amount to count to.
        /// </summary>
        public Node To;

        /// <summary>
        /// The amount to count from - optional.
        /// </summary>
        public Node? From;

        /// <summary>
        /// The increment of each iteration - optional.
        /// </summary>
        public Node? Step;

        /// <summary>
        /// The variable to store the iteration in - optional.
        /// </summary>
        public Node? As;

        /// <summary>
        /// The body of the count loop.
        /// </summary>
        public Node Body;

        /// <summary>
        /// Creates a new <see cref="CountNode"/> object.
        /// </summary>
        /// <param name="to">The amount to count to.</param>
        /// <param name="from">The amount to count from - optional.</param>
        /// <param name="step">The increment of each iteration - optional.</param>
        /// <param name="as">The variable to store the iteration in - optional.</param>
        /// <param name="body">The body of the count loop.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="CountNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="CountNode"/>.</param>
        public CountNode(Node to, Node? from, Node? step, Node? @as, Node body, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            To = to;
            From = from;
            Step = step;
            As = @as;
            Body = body;
        }

        public override string ToString()
        {
            return $"CountNode({To}, {From}, {Step}, {As}, {Body})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for an while expression.
    /// </summary>
    public class WhileNode : Node
    {
        /// <summary>
        /// The condition of the while loop.
        /// </summary>
        public Node Condition;

        /// <summary>
        /// The body of the while loop.
        /// </summary>
        public Node Body;

        /// <summary>
        /// Creates a new <see cref="WhileNode"/> object.
        /// </summary>
        /// <param name="condition">The condition of the while loop.</param>
        /// <param name="body">The body of the while loop.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="WhileNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="WhileNode"/>.</param>
        public WhileNode(Node condition, Node body, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Condition = condition;
            Body = body;
        }

        public override string ToString()
        {
            return $"WhileNode({Condition}, {Body})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for a try expression.
    /// </summary>
    public class TryNode : Node
    {
        /// <summary>
        /// The try block.
        /// </summary>
        public Node Block;

        /// <summary>
        /// The error cases of the try expression.
        /// </summary>
        public List<Node?[]> Cases;

        /// <summary>
        /// The (optional) <see cref="Node"/> where the error will be stored and the body of the empty else case.
        /// </summary>
        public Node?[]? EmptyCase;

        /// <summary>
        /// Creates a new <see cref="TryNode"/> object.
        /// </summary>
        /// <param name="block">The try block.</param>
        /// <param name="cases">The error cases of the try expression.</param>
        /// <param name="emptyCase">The (optional) <see cref="Node"/> where the error will be stored and the body of the empty else case.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="TryNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="TryNode"/>.</param>
        public TryNode(Node block, List<Node?[]> cases, Node?[]? emptyCase, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Block = block;
            Cases = cases;
            EmptyCase = emptyCase;
        }

        public override string ToString()
        {
            string?[] cases = new string[Cases.Count];
            for (int i = 0; i < Cases.Count; i++)
                cases[i] = $"({Cases[i][0]}, {Cases[i][1]}, {Cases[i][2]})";

            return (EmptyCase != null) ? $"TryNode({Block}, [{string.Join(", ", cases)}], ({EmptyCase[0]}, {EmptyCase[1]}))" : $"TryNode({Block}, [{string.Join(", ", cases)}], )";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for a function definition.
    /// </summary>
    public class FunctionDefinitionNode : Node
    {
        /// <summary>
        /// The (optional) name of the function.
        /// </summary>
        public Node? Name;

        /// <summary>
        /// The parameters of the function.
        /// </summary>
        public List<Node> Parameters;

        /// <summary>
        /// The body of the function.
        /// </summary>
        public Node Body;

        /// <summary>
        /// Creates a new <see cref="FunctionDefinitionNode"/> object.
        /// </summary>
        /// <param name="name">The (optional) name of the function.</param>
        /// <param name="parameters">The parameters of the function.</param>
        /// <param name="body">The body of the function.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="FunctionDefinitionNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="FunctionDefinitionNode"/>.</param>
        public FunctionDefinitionNode(Node? name, List<Node> parameters, Node body, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }

        public override string ToString()
        {
            string?[] parameters = new string[Parameters.Count];
            for (int i = 0; i < Parameters.Count; i++)
                parameters[i] = Parameters[i].ToString();

            return $"FunctionDefinitionNode({Name}, [{string.Join(", ", parameters)}], {Body})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for an object definition.
    /// </summary>
    public class ObjectDefinitionNode : Node
    {
        /// <summary>
        /// The (optional) name of the object.
        /// </summary>
        public Node? Name;

        /// <summary>
        /// The creation parameters of the object.
        /// </summary>
        public List<Node> Parameters;

        /// <summary>
        /// The parents of the object.
        /// </summary>
        public List<Node> Parents;

        /// <summary>
        /// The body of the object.
        /// </summary>
        public Node Body;

        /// <summary>
        /// Creates a new <see cref="ObjectDefinitionNode"/> object.
        /// </summary>
        /// <param name="name">The (optional) name of the object.</param>
        /// <param name="parameters">The creation parameters of the object.</param>
        /// <param name="parents">The parents of the object.</param>
        /// <param name="body">The body of the object.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="ObjectDefinitionNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="ObjectDefinitionNode"/>.</param>
        public ObjectDefinitionNode(Node? name, List<Node> parameters, List<Node> parents, Node body, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Name = name;
            Parameters = parameters;
            Parents = parents;
            Body = body;
        }

        public override string ToString()
        {
            string?[] parameters = new string[Parameters.Count];
            for (int i = 0; i < Parameters.Count; i++)
                parameters[i] = Parameters[i].ToString();

            string?[] parents = new string[Parents.Count];
            for (int i = 0; i < Parents.Count; i++)
                parents[i] = Parents[i].ToString();

            return $"ObjectDefinitionNode({Name}, [{string.Join(", ", parameters)}], [{string.Join(", ", parents)}], {Body})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for an include expression.
    /// </summary>
    public class IncludeNode : Node
    {
        /// <summary>
        /// The script to include.
        /// </summary>
        Node Script;

        /// <summary>
        /// The (optional) specific sub-structure or object to be included from the script.
        /// </summary>
        Node? SubStructure;

        /// <summary>
        /// Specifies if all contents of the script need to be dumped into the current context.
        /// </summary>
        bool IsDumped;

        /// <summary>
        /// The (optional) nickname of the object to be included.
        /// </summary>
        Node? Nickname;

        /// <summary>
        /// Creates a new <see cref="IncludeNode"/> object.
        /// </summary>
        /// <param name="script">The script to include.</param>
        /// <param name="subStructure">The (optional) specific sub-structure or object to be included from the script.</param>
        /// <param name="isDumped">Specifies if all contents of the script need to be dumped into the current context.</param>
        /// <param name="nickname">The (optional) nickname of the object to be included.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="IncludeNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="IncludeNode"/>.</param>
        public IncludeNode(Node script, Node? subStructure, bool isDumped, Node? nickname, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Script = script;
            SubStructure = subStructure;
            IsDumped = isDumped;
            Nickname = nickname;
        }

        public override string ToString()
        {
            return $"IncludeNode({Script}, {SubStructure}, {IsDumped}, {Nickname})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for a return statement.
    /// </summary>
    public class ReturnNode : Node
    {
        /// <summary>
        /// The optional value to be returned.
        /// </summary>
        public Node? Value;

        /// <summary>
        /// Creates a new <see cref="ReturnNode"/> object.
        /// </summary>
        /// <param name="value">The optional value to be returned.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="ReturnNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="ReturnNode"/>.</param>
        public ReturnNode(Node? value, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"ReturnNode({Value})";
        }
    }

    /// <summary>
    /// The <see cref="Node"/> structure for a statement or expression without any value (used as skip and stop statement nodes).
    /// </summary>
    public class NoValueNode : Node
    {
        /// <summary>
        /// The identifying <see cref="TokenType"/> of the <see cref="NoValueNode"/>.
        /// </summary>
        public TokenType ValueType;

        /// <summary>
        /// Creates a new <see cref="NoValueNode"/> object.
        /// </summary>
        /// <param name="valueType">The identifying <see cref="TokenType"/> of the <see cref="NoValueNode"/>.</param>
        /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="NoValueNode"/>.</param>
        /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="NoValueNode"/>.</param>
        public NoValueNode(TokenType valueType, Position startPosition, Position endPosition) : base(startPosition, endPosition)
        {
            ValueType = valueType;
        }

        public override string ToString()
        {
            return $"NoValueNode({ValueType})";
        }
    }
}
