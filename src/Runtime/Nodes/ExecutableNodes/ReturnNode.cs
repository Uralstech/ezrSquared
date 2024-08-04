namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a return statement.
/// </summary>
/// <param name="value">The optional value to be returned.</param>
/// <param name="returnLast">Return the last element of <see cref="Value"/>, which should be a list or array.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="ReturnNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="ReturnNode"/>.</param>
public class ReturnNode(Node? value, bool returnLast, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The optional value to be returned.
    /// </summary>
    public Node? Value = value;

    /// <summary>
    /// Return the last element of <see cref="Value"/>, which should be a list or array.
    /// </summary>
    public bool ReturnLast = returnLast;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="ReturnNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(ReturnNode)}({Value?.ToString() ?? "null"})";
    }
}
