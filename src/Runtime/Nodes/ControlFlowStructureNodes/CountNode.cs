namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a count expression.
/// </summary>
/// <param name="To">The amount to count to.</param>
/// <param name="From">The amount to count from - optional.</param>
/// <param name="Step">The increment of each iteration - optional.</param>
/// <param name="IterationVariable">The variable to store the iteration number in - optional.</param>
/// <param name="Body">The body of the count loop.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="CountNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="CountNode"/>.</param>
public record CountNode(Node To, Node? From, Node? Step, Node? IterationVariable, Node Body, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
