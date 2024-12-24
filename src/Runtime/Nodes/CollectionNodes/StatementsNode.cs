using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for holding multiple statements.
/// </summary>
/// <param name="Statements">The statements.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="StatementsNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="StatementsNode"/>.</param>
public record StatementsNode(IReadOnlyCollection<Node> Statements, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
