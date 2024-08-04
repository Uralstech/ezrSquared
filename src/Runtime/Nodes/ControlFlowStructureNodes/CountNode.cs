namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a count expression.
/// </summary>
/// <param name="to">The amount to count to.</param>
/// <param name="from">The amount to count from - optional.</param>
/// <param name="step">The increment of each iteration - optional.</param>
/// <param name="iterationVariable">TThe variable to store the iteration number in - optional.</param>
/// <param name="body">The body of the count loop.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="CountNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="CountNode"/>.</param>
public class CountNode(Node to, Node? from, Node? step, Node? iterationVariable, Node body, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The amount to count to.
    /// </summary>
    public Node To = to;

    /// <summary>
    /// The amount to count from - optional.
    /// </summary>
    public Node? From = from;

    /// <summary>
    /// The increment of each iteration - optional.
    /// </summary>
    public Node? Step = step;

    /// <summary>
    /// The variable to store the iteration number in - optional.
    /// </summary>
    public Node? IterationVariable = iterationVariable;

    /// <summary>
    /// The body of the count loop.
    /// </summary>
    public Node Body = body;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="CountNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        return $"{nameof(CountNode)}({To}, {From?.ToString() ?? "null"}, {Step?.ToString() ?? "null"}, {IterationVariable?.ToString() ?? "null"}, {Body})";
    }
}
