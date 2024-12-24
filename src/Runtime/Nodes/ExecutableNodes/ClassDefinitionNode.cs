using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a class definition.
/// </summary>
/// <param name="Name">The (optional) name of the class.</param>
/// <param name="AccessibilityModifiers">The accessibility modifiers for the class definition.</param>
/// <param name="Readonly">The check for if the class should be declared read-only.</param>
/// <param name="Parents">The parents of the class.</param>
/// <param name="Body">The body of the class.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="ClassDefinitionNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="ClassDefinitionNode"/>.</param>
public record ClassDefinitionNode(Node? Name, AccessMod AccessibilityModifiers, bool Readonly, IReadOnlyCollection<Node> Parents, Node Body, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
