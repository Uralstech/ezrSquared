namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for assigning a value to a variable in the context.
/// </summary>
/// <param name="Variable">The variable to be assigned to.</param>
/// <param name="AssignmentOperator">The operation <see cref="TokenType"/>, if not <see cref="TokenType.Colon"/>, between the existing value of <paramref name="Variable"/> and <paramref name="Value"/>. The result of the operation will be assigned to <paramref name="Variable"/>.</param>
/// <param name="Value">The value to be assigned to <see cref="Variable"/>.</param>
/// <param name="AccessibilityModifiers">The accessibility modifiers for the variable assignment operation.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="VariableAssignmentNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="VariableAssignmentNode"/>.</param>
public record VariableAssignmentNode(Node Variable, TokenType AssignmentOperator, Node Value, AccessMod AccessibilityModifiers, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
