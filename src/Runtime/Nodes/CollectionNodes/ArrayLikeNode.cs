using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for an arraylike (array or list).
/// </summary>
/// <param name="Elements">The elements of the arraylike.</param>
/// <param name="CreateList">The check for if the <see cref="ArrayLikeNode"/> should create a list instead of an array.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="ArrayLikeNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="ArrayLikeNode"/>.</param>
public record ArrayLikeNode(IReadOnlyCollection<Node> Elements, bool CreateList, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
