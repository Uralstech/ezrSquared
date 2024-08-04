using EzrSquared.Runtime.Nodes;
using EzrSquared.Syntax.Errors;

namespace EzrSquared.Syntax;

/// <summary>
/// The type of the object that is returned as the result of parsing done by the <see cref="Parser"/>.
/// </summary>
public class ParseResult
{
    /// <summary>
    /// The <see cref="SyntaxError"/> that occurred while parsing, if any.
    /// </summary>
    public SyntaxError? Error = null;

    /// <summary>
    /// The <see cref="Runtime.Nodes.Node"/> which is the result of the parsing.
    /// </summary>
    public Node Node = InvalidNode.s_invalidNode;

    /// <summary>
    /// The amount of times the <see cref="Parser"/> advanced.
    /// </summary>
    public int AdvanceCount = 0;

    /// <summary>
    /// The priority of the error held in the <see cref="ParseResult"/>.
    /// </summary>
    private int _errorPriority = 0;

    /// <summary>
    /// Sets <see cref="Node"/> as the result of successful parsing.
    /// </summary>
    /// <param name="node">The <see cref="Runtime.Nodes.Node"/> result of the parsing.</param>
    /// <returns>The same <see cref="ParseResult"/> object.</returns>
    public void Success(Node node)
    {
        Node = node;
    }

    /// <summary>
    /// Sets <see cref="Error"/> as the result of failed parsing.
    /// </summary>
    /// <remarks>
    /// If <paramref name="priority"/> is greater than or equal to the <see cref="_errorPriority"/>
    /// then <paramref name="error"/> will override <see cref="Error"/>.
    /// </remarks>
    /// <param name="priority">The priority/fatality of the failure.</param>
    /// <param name="error">The <see cref="SyntaxError"/> that occurred in parsing.</param>
    /// <returns>The same <see cref="ParseResult"/> object.</returns>
    public void Failure(int priority, SyntaxError error)
    {
        if (Error is null || _errorPriority < priority)
        {
            _errorPriority = priority;
            Error = error;
        }
    }
}
