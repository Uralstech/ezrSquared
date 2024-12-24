namespace EzrSquared.Runtime.Nodes;

/// <summary>
/// The <see cref="Node"/> structure for a return statement.
/// </summary>
/// <param name="Value">The optional value to be returned.</param>
/// <param name="ReturnLast">Return the last element of <see cref="Value"/>, which should be a list or array.</param>
/// <param name="StartPosition">The starting <see cref="Position"/> of the <see cref="ReturnNode"/>.</param>
/// <param name="EndPosition">The ending <see cref="Position"/> of the <see cref="ReturnNode"/>.</param>
public record ReturnNode(Node? Value, bool ReturnLast, Position StartPosition, Position EndPosition) : Node(StartPosition, EndPosition)
{
}
