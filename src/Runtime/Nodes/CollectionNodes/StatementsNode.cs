using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for holding multiple statements.
/// </summary>
/// <param name="statements">The statements.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="StatementsNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="StatementsNode"/>.</param>
public class StatementsNode(List<Node> statements, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The statements being held by the node.
    /// </summary>
    public List<Node> Statements = statements;

    /// <inheritdoc/>
    public override string ToString()
    {
        string[] elements = new string[Statements.Count];
        for (int i = 0; i < Statements.Count; i++)
            elements[i] = Statements[i].ToString();

        return $"{nameof(StatementsNode)}([{string.Join(", ", elements)}])";
    }
}
