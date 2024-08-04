namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a define block.
/// </summary>
/// <param name="body">The body of the block.</param>
/// <param name="accessibilityModifiers">The accessibility modifiers for the define block.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="DefineBlockNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="DefineBlockNode"/>.</param>
public class DefineBlockNode(Node body, AccessMod accessibilityModifiers, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The body of the block.
    /// </summary>
    public Node Body = body;

    /// <summary>
    /// The accessibility modifiers of the define block.
    /// </summary>
    public AccessMod AccessibilityModifiers = accessibilityModifiers;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="DefineBlockNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(DefineBlockNode)}({Body}, {AccessibilityModifiers})";
    }
}
