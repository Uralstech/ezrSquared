using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for an arraylike (array or list).
/// </summary>
/// <param name="elements">The elements of the arraylike.</param>
/// <param name="createList">The check for if the <see cref="ArrayLikeNode"/> should create a list instead of an array.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="ArrayLikeNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="ArrayLikeNode"/>.</param>
public class ArrayLikeNode(List<Node> elements, bool createList, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The elements of the arraylike.
    /// </summary>
    public List<Node> Elements = elements;

    /// <summary>
    /// The check for if the <see cref="ArrayLikeNode"/> should create a list instead of an array.
    /// </summary>
    public bool CreateList = createList;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="ArrayLikeNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        string[] elements = new string[Elements.Count];
        for (int i = 0; i < Elements.Count; i++)
            elements[i] = Elements[i].ToString();

        return $"{nameof(ArrayLikeNode)}([{string.Join(", ", elements)}], {CreateList})";
    }
}
