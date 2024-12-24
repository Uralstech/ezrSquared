using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for an if expression.
/// </summary>
/// <param name="Cases">The cases of the if expression.</param>
/// <param name="ElseCase">The body of the else case.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="IfNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="IfNode"/>.</param>
public record IfNode(IReadOnlyCollection<(Node Condition, Node Body)> Cases, Node? ElseCase, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
