namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a for-each expression.
/// </summary>
/// <param name="expression">A check-in expression, where the LHS is the variable to store the iterated values and RHS is the collection to iterate.</param>
/// <param name="body">The body of the loop.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="ForEachNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="ForEachNode"/>.</param>
public class ForEachNode(BinaryOperationNode expression, Node body, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// A check-in expression, where the LHS is the variable to store the iterated values and RHS is the collection to iterate.
    /// </summary>
    public BinaryOperationNode Expression = expression;

    /// <summary>
    /// The body of the count loop.
    /// </summary>
    public Node Body = body;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="ForEachNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(ForEachNode)}({Expression}, {Body})";
    }
}
