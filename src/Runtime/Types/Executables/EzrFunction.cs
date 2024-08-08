using EzrSquared.Runtime.Nodes;

namespace EzrSquared.Runtime.Types.Executables;

/// <summary>
/// The function type object.
/// </summary>
/// <param name="name">The name of the executable.</param>
/// <param name="body">The source code body of the executable.</param>
/// <param name="parameters">The source code of the executable's parameters and their default values.</param>
/// <param name="keywordArguments">The position in source code and name of the variable for the executable's extra keyword arguments.</param>
/// <param name="returnLast">Should the function return its last expression as the result?</param>
/// <param name="parentContext">The parent context.</param>
/// <param name="startPosition">The starting position of the object.</param>
/// <param name="endPosition">The ending position of the object.</param>
public class EzrFunction(string? name, Node body, (string Name, Node Node)[] parameters, (Position StartPosition, Position EndPosition, string Name)? keywordArguments, bool returnLast, Context parentContext, Position startPosition, Position endPosition) : EzrRuntimeExecutable(name, body, parameters, keywordArguments, parentContext, startPosition, endPosition), IEzrMutableObject
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "function";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.RuntimeDefinedFunction";

    /// <summary>
    /// Should the function return its last expression as the result?
    /// </summary>
    public readonly bool ReturnLast = returnLast;

    /// <summary>
    /// Executes the current function.
    /// </summary>
    /// <param name="arguments">The arguments of the execution.</param>
    /// <param name="interpreter">The interpreter to be used in execution.</param>
    /// <param name="result">Runtime result for carrying any errors.</param>
    /// <param name="ignoreExtraArguments">Should the function ignore extra arguments?</param>
    public void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result, bool ignoreExtraArguments)
    {
        Context newContext = new($"<{TypeName} \"{ExecutableName}\">", false, StartPosition, CreationContext, CreationContext.StaticContext);
        CheckAndPopulateArguments(arguments, newContext, interpreter, result, ignoreExtraArguments);
        if (result.ShouldReturn)
            return;

        interpreter.VisitNode(Body, newContext, null, AccessMod.None);
        if (result.ShouldReturnFunction)
        {
            Context.Release();
            return;
        }

        if (result.Reference.IsEmpty)
            result.Success(NewNothingConstant());
        else if (result.Reference.RegisteredContext?.Id == newContext.Id)
            result.Success(ReferencePool.Get(result.Reference.Object, AccessMod.PrivateConstant));
        else
            result.Success(result.Reference);

        newContext.Release();
    }

    /// <inheritdoc/>
    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        Execute(arguments, interpreter, result, false);
    }

    /// <inheritdoc/>
    public IMutable<IEzrMutableObject>? DeepCopy(RuntimeResult result)
    {
        return new EzrFunction(IsAnonymous ? null : ExecutableName, Body, Parameters, KeywordArguments, ReturnLast, CreationContext, StartPosition, EndPosition);
    }
}
