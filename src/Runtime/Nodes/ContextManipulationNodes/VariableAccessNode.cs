namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for accesing a variable from the context.
/// </summary>
/// <param name="name">The name of the variable, a <see cref="Token"/> object of type <see cref="TokenType.Identifier"/>.</param>
/// <param name="accessibilityModifiers">The accessibility modifiers for the variable access operation.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="VariableAccessNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="VariableAccessNode"/>.</param>
public class VariableAccessNode(Token name, AccessMod accessibilityModifiers, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The name of the variable to access.
    /// </summary>
    public Token Name = name;

    /// <summary>
    /// The accessibility modifiers for the variable access operation.
    /// </summary>
    public AccessMod AccessibilityModifiers = accessibilityModifiers;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="VariableAccessNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(VariableAccessNode)}({Name}, {AccessibilityModifiers})";
    }
}
