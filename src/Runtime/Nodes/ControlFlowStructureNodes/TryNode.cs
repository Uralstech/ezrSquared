using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a try expression.
/// </summary>
/// <param name="Block">The try block.</param>
/// <param name="Cases">The error cases of the try expression.</param>
/// <param name="EmptyCase">The (optional) <see cref="Node"/> where the error will be stored and the body of the empty else case.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="TryNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="TryNode"/>.</param>
public record TryNode(Node Block, IReadOnlyCollection<(Node ErrorType, Node? Variable, Node Body)> Cases, (Node? Variable, Node Body)? EmptyCase, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
