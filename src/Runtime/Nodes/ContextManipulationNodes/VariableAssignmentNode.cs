namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for assigning a value to a variable in the context.
/// </summary>
/// <param name="variable">The variable to be assigned to.</param>
/// <param name="assignmentOperator">The operation <see cref="TokenType"/>, if not <see cref="TokenType.Colon"/>, between the existing value of <paramref name="variable"/> and <paramref name="value"/>. The result of the operation will be assigned to <paramref name="variable"/>.</param>
/// <param name="value">The value to be assigned to <see cref="Variable"/>.</param>
/// <param name="accessibilityModifiers">The accessibility modifiers for the variable assignment operation.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="VariableAssignmentNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="VariableAssignmentNode"/>.</param>
public class VariableAssignmentNode(Node variable, TokenType assignmentOperator, Node value, AccessMod accessibilityModifiers, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The variable to be assigned to.
    /// </summary>
    public Node Variable = variable;

    /// <summary>
    /// The operation <see cref="TokenType"/>, if not <see cref="TokenType.Colon"/>, between the existing value of <see cref="Variable"/> and <see cref="Value"/>. The result of the operation will be assigned to <see cref="Variable"/>.
    /// </summary>
    public TokenType AssignmentOperator = assignmentOperator;

    /// <summary>
    /// The value to be assigned to <see cref="Variable"/>.
    /// </summary>
    public Node Value = value;

    /// <summary>
    /// The accessibility modifiers for the variable assignment operation.
    /// </summary>
    public AccessMod AccessibilityModifiers = accessibilityModifiers;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="VariableAssignmentNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(VariableAssignmentNode)}({Variable}, {AssignmentOperator}, {Value}, {AccessibilityModifiers})";
    }
}
