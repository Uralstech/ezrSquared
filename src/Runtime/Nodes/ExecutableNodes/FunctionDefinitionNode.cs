using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a function definition.
/// </summary>
/// <param name="Name">The (optional) name of the function.</param>
/// <param name="AccessibilityModifiers">The accessibility modifiers for the function definition.</param>
/// <param name="ReturnLast">The check for if the last expression of the function should be returned as its result. Only used in oneliners.</param>
/// <param name="Parameters">The parameters of the function.</param>
/// <param name="ExtraKeywordArguments">The reference to store the extra keyword arguments in.</param>
/// <param name="ExtraPositionalArguments">The reference to store the extra positional arguments in.</param>
/// <param name="Body">The body of the function.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="FunctionDefinitionNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="FunctionDefinitionNode"/>.</param>
public record FunctionDefinitionNode(Node? Name, AccessMod AccessibilityModifiers, bool ReturnLast, IReadOnlyCollection<Node> Parameters, Node? ExtraKeywordArguments, Node? ExtraPositionalArguments, Node Body, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
