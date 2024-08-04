namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a unary operation.
/// </summary>
/// <param name="operand">The operand of the unary operation.</param>
/// <param name="operator">The operator <see cref="TokenType"/> of the unary operation.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="UnaryOperationNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="UnaryOperationNode"/>.</param>
public class UnaryOperationNode(Node operand, TokenType @operator, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The operand of the unary operation.
    /// </summary>
    public Node Operand = operand;

    /// <summary>
    /// The operator <see cref="TokenType"/> of the unary operation.
    /// </summary>
    public TokenType Operator = @operator;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="UnaryOperationNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(UnaryOperationNode)}({Operand}, {Operator})";
    }
}
