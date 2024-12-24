namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for an while expression.
/// </summary>
/// <param name="Condition">The condition of the while loop.</param>
/// <param name="Body">The body of the while loop.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="WhileNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="WhileNode"/>.</param>
public record WhileNode(Node Condition, Node Body, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
