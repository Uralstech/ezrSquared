using EzrSquared.Syntax.Errors;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The representation of an ezr² source code construct. This is the base class of all nodes. Only for inheritance!
/// </summary>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="Node"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="Node"/>.</param>
public abstract record Node(Position StartPosition, Position EndPosition)
{
}

/// <summary>
/// The dummy invalid <see cref="Node"/> structure. For returning instead of <see langword="null"/> if an <see cref="EzrSyntaxError"/> occurs during parsing.
/// </summary>
public record InvalidNode : Node
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
}
