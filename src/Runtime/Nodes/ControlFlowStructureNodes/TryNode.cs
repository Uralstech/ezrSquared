using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a try expression.
/// </summary>
/// <param name="block">The try block.</param>
/// <param name="cases">The error cases of the try expression.</param>
/// <param name="emptyCase">The (optional) <see cref="Node"/> where the error will be stored and the body of the empty else case.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="TryNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="TryNode"/>.</param>
public class TryNode(Node block, List<(Node ErrorType, Node? Variable, Node Body)> cases, (Node? Variable, Node Body)? emptyCase, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The try block.
    /// </summary>
    public Node Block = block;

    /// <summary>
    /// The error cases of the try expression.
    /// </summary>
    public List<(Node ErrorType, Node? Variable, Node Body)> Cases = cases;

    /// <summary>
    /// The (optional) <see cref="Node"/> where the error will be stored and the body of the empty else case.
    /// </summary>
    public (Node? Variable, Node Body)? EmptyCase = emptyCase;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="TryNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        string?[] cases = new string[Cases.Count];
        for (int i = 0; i < Cases.Count; i++)
            cases[i] = $"({Cases[i].ErrorType}, {Cases[i].Variable?.ToString() ?? "null"}, {Cases[i].Body})";

        return (EmptyCase is not null)
            ? $"{nameof(TryNode)}({Block}, [{string.Join(", ", cases)}], ({EmptyCase?.Variable?.ToString() ?? "null"}, {EmptyCase?.Body}))"
            : $"{nameof(TryNode)}({Block}, [{string.Join(", ", cases)}])";
    }
}
