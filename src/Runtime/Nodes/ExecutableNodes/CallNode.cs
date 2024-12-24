using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a function call.
/// </summary>
/// <param name="Receiver">The function/object to be called.</param>
/// <param name="Arguments">The array of arguments.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="CallNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="CallNode"/>.</param>
public record CallNode(Node Receiver, IReadOnlyCollection<Node> Arguments, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
