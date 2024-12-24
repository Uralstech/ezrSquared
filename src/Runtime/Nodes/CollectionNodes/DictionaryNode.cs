using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a dictionary.
/// </summary>
/// <param name="KeyValuePairs">The key-value pairs of the dictionary.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="DictionaryNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="DictionaryNode"/>.</param>
public record DictionaryNode(IReadOnlyCollection<(Node Key, Node Value)> KeyValuePairs, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
