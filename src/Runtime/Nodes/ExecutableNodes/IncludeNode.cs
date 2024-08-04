namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for an include expression.
/// </summary>
/// <param name="script">The script to include.</param>
/// <param name="subStructure">The (optional) specific sub-structure or object to be included from the script.</param>
/// <param name="isDumped">Specifies if all contents of the script need to be dumped into the current context.</param>
/// <param name="nickname">The (optional) nickname of the object to be included.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="IncludeNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="IncludeNode"/>.</param>
public class IncludeNode(Node script, Node? subStructure, bool isDumped, Node? nickname, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The script to include.
    /// </summary>
    public readonly Node Script = script;

    /// <summary>
    /// The (optional) specific sub-structure or object to be included from the script.
    /// </summary>
    public readonly Node? SubStructure = subStructure;

    /// <summary>
    /// Specifies if all contents of the script need to be dumped into the current context.
    /// </summary>
    public readonly bool IsDumped = isDumped;

    /// <summary>
    /// The (optional) nickname of the object to be included.
    /// </summary>
    public readonly Node? Nickname = nickname;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="IncludeNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(IncludeNode)}({Script}, {SubStructure?.ToString() ?? "null"}, {IsDumped}, {Nickname?.ToString() ?? "null"})";
    }
}
