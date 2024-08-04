namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a binary operation.
/// </summary>
/// <param name="left">The first operand of the binary operation.</param>
/// <param name="right">The second operand of the binary operation.</param>
/// <param name="operator">The operator <see cref="TokenType"/> of the binary operation.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="BinaryOperationNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="BinaryOperationNode"/>.</param>
public class BinaryOperationNode(Node left, Node right, TokenType @operator, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The first operand of the binary operation.
    /// </summary>
    public Node Left = left;

    /// <summary>
    /// The second operand of the binary operation.
    /// </summary>
    public Node Right = right;

    /// <summary>
    /// The operator <see cref="TokenType"/> of the binary operation.
    /// </summary>
    public TokenType Operator = @operator;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="BinaryOperationNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(BinaryOperationNode)}({Left}, {Operator}, {Right})";
    }
}
