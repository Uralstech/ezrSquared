namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a statement or expression without any value (used as skip and stop statement nodes).
/// </summary>
/// <param name="ValueType">The identifying <see cref="TokenType"/> of the <see cref="NoValueNode"/>.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="NoValueNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="NoValueNode"/>.</param>
public record NoValueNode(TokenType ValueType, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
