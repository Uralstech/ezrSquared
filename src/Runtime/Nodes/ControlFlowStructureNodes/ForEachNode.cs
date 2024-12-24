namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a for-each expression.
/// </summary>
/// <param name="Expression">A check-in expression, where the LHS is the variable to store the iterated values and RHS is the collection to iterate.</param>
/// <param name="Body">The body of the loop.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="ForEachNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="ForEachNode"/>.</param>
public record ForEachNode(BinaryOperationNode Expression, Node Body, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
