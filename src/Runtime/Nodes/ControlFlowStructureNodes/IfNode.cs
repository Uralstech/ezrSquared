using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for an if expression.
/// </summary>
/// <param name="cases">The cases of the if expression.</param>
/// <param name="elseCase">The body of the else case.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="IfNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="IfNode"/>.</param>
public class IfNode(List<(Node Condition, Node Body)> cases, Node? elseCase, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The cases of the if expression.
    /// </summary>
    public List<(Node Condition, Node Body)> Cases = cases;

    /// <summary>
    /// The body of the else case.
    /// </summary>
    public Node? ElseCase = elseCase;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="IfNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        string?[] cases = new string[Cases.Count];
        for (int i = 0; i < Cases.Count; i++)
            cases[i] = $"({Cases[i].Condition}, {Cases[i].Body})";

        return $"{nameof(IfNode)}([{string.Join(", ", cases)}], {ElseCase?.ToString() ?? "null"})";
    }
}
