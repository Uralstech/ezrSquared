using EzrSquared.Syntax.Errors;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The representation of an ezr² source code construct. This is the base class of all nodes. Only for inheritance!
/// </summary>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="Node"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="Node"/>.</param>
public abstract class Node(Position startPosition, Position endPosition)
{
    /// <summary>
    /// The starting <see cref="Position"/> of the <see cref="Node"/>.
    /// </summary>
    public Position StartPosition = startPosition;

    /// <summary>
    /// The ending <see cref="Position"/> of the <see cref="Node"/>.
    /// </summary>
    public Position EndPosition = endPosition;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="Node"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(Node)}()";
    }
}

/// <summary>
/// The dummy invalid <see cref="Node"/> structure. For returning instead of <see langword="null"/> if an <see cref="SyntaxError"/> occurs during parsing.
/// </summary>
public class InvalidNode : Node
{
    /// <summary>
    /// The static <see cref="InvalidNode"/> object.
    /// </summary>
    internal static readonly InvalidNode s_invalidNode = new(Position.None, Position.None);

    /// <summary>
    /// Creates a new <see cref="InvalidNode"/> object.
    /// </summary>
    /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="InvalidNode"/>.</param>
    /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="InvalidNode"/>.</param>
    InvalidNode(Position startPosition, Position endPosition) : base(startPosition, endPosition) { }

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="InvalidNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(InvalidNode)}()";
    }
}
