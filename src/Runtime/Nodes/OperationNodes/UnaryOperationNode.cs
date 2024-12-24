namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a unary operation.
/// </summary>
/// <param name="Operand">The operand of the unary operation.</param>
/// <param name="Operator">The operator <see cref="TokenType"/> of the unary operation.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="UnaryOperationNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="UnaryOperationNode"/>.</param>
public record UnaryOperationNode(Node Operand, TokenType Operator, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
