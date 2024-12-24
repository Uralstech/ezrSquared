namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a define block.
/// </summary>
/// <param name="Body">The body of the block.</param>
/// <param name="AccessibilityModifiers">The accessibility modifiers for the define block.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="DefineBlockNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="DefineBlockNode"/>.</param>
public record DefineBlockNode(Node Body, AccessMod AccessibilityModifiers, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
