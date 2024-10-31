using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a function definition.
/// </summary>
/// <param name="name">The (optional) name of the function.</param>
/// <param name="accessibilityModifiers">The accessibility modifiers for the function definition.</param>
/// <param name="returnLast">The check for if the last expression of the function should be returned as its result. Only used in oneliners.</param>
/// <param name="parameters">The parameters of the function.</param>
/// <param name="extraKeywordArguments">The reference to store the extra keyword arguments in.</param>
/// <param name="extraPositionalArguments">The reference to store the extra positional arguments in.</param>
/// <param name="body">The body of the function.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="FunctionDefinitionNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="FunctionDefinitionNode"/>.</param>
public class FunctionDefinitionNode(Node? name, AccessMod accessibilityModifiers, bool returnLast, List<Node> parameters, Node? extraKeywordArguments, Node? extraPositionalArguments, Node body, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The (optional) name of the function.
    /// </summary>
    public Node? Name = name;

    /// <summary>
    /// The accessibility modifiers for the function definition.
    /// </summary>
    public AccessMod AccessibilityModifiers = accessibilityModifiers;

    /// <summary>
    /// The check for if the last expression of the function should be returned as its result.
    /// Only used in oneliners.
    /// </summary>
    public bool ReturnLast = returnLast;

    /// <summary>
    /// The parameters of the function.
    /// </summary>
    public List<Node> Parameters = parameters;

    /// <summary>
    /// The reference to store the extra keyword arguments in.
    /// </summary>
    public Node? ExtraKeywordArguments = extraKeywordArguments;

    /// <summary>
    /// The reference to store the extra positional arguments in.
    /// </summary>
    public Node? ExtraPositionalArguments = extraPositionalArguments;

    /// <summary>
    /// The body of the function.
    /// </summary>
    public Node Body = body;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="FunctionDefinitionNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        string?[] parameters = new string[Parameters.Count];
        for (int i = 0; i < Parameters.Count; i++)
            parameters[i] = Parameters[i].ToString();

        return $"{nameof(FunctionDefinitionNode)}({Name?.ToString() ?? "null"}, {AccessibilityModifiers}, {ReturnLast}, [{string.Join(", ", parameters)}], {ExtraKeywordArguments?.ToString() ?? "null"}, {ExtraPositionalArguments?.ToString() ?? "null"}, {Body})";
    }
}
