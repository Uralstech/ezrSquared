namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for an include expression.
/// </summary>
/// <param name="Script">The script to include.</param>
/// <param name="SubStructure">The (optional) specific sub-structure or object to be included from the script.</param>
/// <param name="IsDumped">Specifies if all contents of the script need to be dumped into the current context.</param>
/// <param name="Nickname">The (optional) nickname of the object to be included.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="IncludeNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="IncludeNode"/>.</param>
public record IncludeNode(Node Script, Node? SubStructure, bool IsDumped, Node? Nickname, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
