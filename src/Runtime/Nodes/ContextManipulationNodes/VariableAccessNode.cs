namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for accesing a variable from the context.
/// </summary>
/// <param name="Name">The name of the variable, a <see cref="Token"/> object of type <see cref="TokenType.Identifier"/>.</param>
/// <param name="AccessibilityModifiers">The accessibility modifiers for the variable access operation.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="VariableAccessNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="VariableAccessNode"/>.</param>
public record VariableAccessNode(Token Name, AccessMod AccessibilityModifiers, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
