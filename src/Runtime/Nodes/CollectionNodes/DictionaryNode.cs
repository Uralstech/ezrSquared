using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a dictionary.
/// </summary>
/// <param name="keyValuePairs">The key-value pairs of the dictionary.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="DictionaryNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="DictionaryNode"/>.</param>
public class DictionaryNode(List<(Node Key, Node Value)> keyValuePairs, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The key-value pairs of the dictionary.
    /// </summary>
    public List<(Node Key, Node Value)> KeyValuePairs = keyValuePairs;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="DictionaryNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        string[] keyValuePairs = new string[KeyValuePairs.Count];
        for (int i = 0; i < KeyValuePairs.Count; i++)
            keyValuePairs[i] = $"{KeyValuePairs[i].Key} : {KeyValuePairs[i].Value}";

        return $"{nameof(DictionaryNode)}([{string.Join(", ", keyValuePairs)}])";
    }
}
