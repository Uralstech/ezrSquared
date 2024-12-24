namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure of a simple value like literals and variables.
/// </summary>
/// <param name="Value">The <see cref="Token"/> value the <see cref="ValueNode"/> represents.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="ValueNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="ValueNode"/>.</param>
public record ValueNode(Token Value, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
