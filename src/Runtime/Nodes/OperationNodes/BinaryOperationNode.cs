namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a binary operation.
/// </summary>
/// <param name="Left">The first operand of the binary operation.</param>
/// <param name="Right">The second operand of the binary operation.</param>
/// <param name="Operator">The operator <see cref="TokenType"/> of the binary operation.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="BinaryOperationNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="BinaryOperationNode"/>.</param>
public record BinaryOperationNode(Node Left, Node Right, TokenType Operator, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
