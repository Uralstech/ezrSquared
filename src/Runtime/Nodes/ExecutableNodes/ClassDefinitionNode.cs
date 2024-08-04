using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a class definition.
/// </summary>
/// <param name="name">The (optional) name of the class.</param>
/// <param name="accessibilityModifiers">The accessibility modifiers for the class definition.</param>
/// <param name="readonly">The check for if the class should be declared read-only.</param>
/// <param name="parents">The parents of the class.</param>
/// <param name="body">The body of the class.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="ClassDefinitionNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="ClassDefinitionNode"/>.</param>
public class ClassDefinitionNode(Node? name, AccessMod accessibilityModifiers, bool @readonly, List<Node> parents, Node body, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The name of the class. May be <see langword="null"/>.
    /// </summary>
    public Node? Name = name;

    /// <summary>
    /// The accessibility modifiers for the class definition.
    /// </summary>
    public AccessMod AccessibilityModifiers = accessibilityModifiers;

    /// <summary>
    /// The check for if the class should be declared read-only.
    /// </summary>
    public bool Readonly = @readonly;

    /// <summary>
    /// The parents of the class.
    /// </summary>
    public List<Node> Parents = parents;

    /// <summary>
    /// The body of the class.
    /// </summary>
    public Node Body = body;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="ClassDefinitionNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        string?[] parents = new string[Parents.Count];
        for (int i = 0; i < Parents.Count; i++)
            parents[i] = Parents[i].ToString();

        return $"{nameof(ClassDefinitionNode)}({Name?.ToString() ?? "null"}, {AccessibilityModifiers}, {Readonly}, [{string.Join(", ", parents)}], {Body})";
    }
}
