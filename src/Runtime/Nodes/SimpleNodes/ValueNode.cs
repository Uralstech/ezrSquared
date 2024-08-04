namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure of a simple value like literals and variables.
/// </summary>
/// <param name="value">The <see cref="Token"/> value the <see cref="ValueNode"/> represents.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="ValueNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="ValueNode"/>.</param>
public class ValueNode(Token value, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The value the <see cref="ValueNode"/> represents.
    /// </summary>
    public Token Value = value;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="ValueNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(ValueNode)}({Value})";
    }
}
