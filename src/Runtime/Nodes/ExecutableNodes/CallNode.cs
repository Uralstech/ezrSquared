using System.Collections.Generic;
namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a function call.
/// </summary>
/// <param name="receiver">The function/object to be called.</param>
/// <param name="arguments">The array of arguments.</param>
/// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="CallNode"/>.</param>
/// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="CallNode"/>.</param>
public class CallNode(Node receiver, List<Node> arguments, Position startPosition, Position endPosition) : Node(startPosition, endPosition)
{
    /// <summary>
    /// The function/object to be called.
    /// </summary>
    public Node Receiver = receiver;

    /// <summary>
    /// The array of arguments.
    /// </summary>
    public List<Node> Arguments = arguments;

    /// <summary>
    /// Creates a <see cref="string"/> representation of the <see cref="CallNode"/>.<br/>
    /// All fields excluding the start and end positions are included in the representation, like "ExampleNode(Property1, Property2)".
    /// </summary>
    /// <returns>The <see cref="string"/> representation.</returns>
    public override string ToString()
    {
        string[] arguments = new string[Arguments.Count];
        for (int i = 0; i < Arguments.Count; i++)
            arguments[i] = Arguments[i].ToString();

        return $"{nameof(CallNode)}({Receiver}, [{string.Join(", ", arguments)}])";
    }
}
