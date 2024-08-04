namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a statement or expression without any value (used as skip and stop statement nodes).
/// </summary>
/// <param name="valueType">The identifying <see cref="TokenType"/> of the <see cref="NoValueNode"/>.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="NoValueNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="NoValueNode"/>.</param>
public class NoValueNode(TokenType valueType, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The identifying <see cref="TokenType"/> of the <see cref="NoValueNode"/>.
    /// </summary>
    public TokenType ValueType = valueType;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="NoValueNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(NoValueNode)}({ValueType})";
    }
}
