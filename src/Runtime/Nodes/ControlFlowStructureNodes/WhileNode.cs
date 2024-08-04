namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for an while expression.
/// </summary>
/// <param name="condition">The condition of the while loop.</param>
/// <param name="body">The body of the while loop.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="WhileNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="WhileNode"/>.</param>
public class WhileNode(Node condition, Node body, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The condition of the while loop.
    /// </summary>
    public Node Condition = condition;

    /// <summary>
    /// The body of the while loop.
    /// </summary>
    public Node Body = body;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="WhileNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(WhileNode)}({Condition}, {Body})";
    }
}
