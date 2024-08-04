using System.Collections.Generic;

namespace EzrSquared.Runtime.WrapperAttributes;

/// <summary>
/// Class for the paramters of a wrapped C# method.
/// </summary>
/// <param name="argumentReferences">See <see cref="ArgumentReferences"/>.</param>
/// <param name="executionContext">See <see cref="ExecutionContext"/>.</param>
/// <param name="creationContext">See <see cref="CreationContext"/>.</param>
/// <param name="methodContext">See <see cref="MethodContext"/>.</param>
/// <param name="startPosition">See <see cref="StartPosition"/>.</param>
/// <param name="endPosition">See <see cref="EndPosition"/>.</param>
/// <param name="interpreter">See <see cref="Interpreter"/>.</param>
/// <param name="result">See <see cref="Result"/>.</param>
public class SharpMethodParameters(
    Dictionary<string, Reference> argumentReferences,
    Context executionContext,
    Context creationContext,
    Context methodContext,
    Position startPosition,
    Position endPosition,
    Interpreter interpreter,
    RuntimeResult result)
{
    /// <summary>
    /// The references to the actual ezr² arguments.
    /// </summary>
    public Dictionary<string, Reference> ArgumentReferences = argumentReferences;

    /// <summary>
    /// The context under which the method is being executed.
    /// </summary>
    public Context ExecutionContext = executionContext;

    /// <summary>
    /// The context under which the method wrapper was created.
    /// </summary>
    public Context CreationContext = creationContext;

    /// <summary>
    /// The context of the method wrapper itself.
    /// </summary>
    public Context MethodContext = methodContext;

    /// <summary>
    /// The starting position of the method wrapper object.
    /// </summary>
    public Position StartPosition = startPosition;

    /// <summary>
    /// The ending position of the method wrapper object.
    /// </summary>
    public Position EndPosition = endPosition;

    /// <summary>
    /// The interpreter executing the method.
    /// </summary>
    public Interpreter Interpreter = interpreter;

    /// <summary>
    /// The runtime result to return values or to throw errors.
    /// </summary>
    public RuntimeResult Result = result;
}
