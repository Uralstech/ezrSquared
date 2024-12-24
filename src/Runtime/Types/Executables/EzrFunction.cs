using EzrSquared.Runtime.Nodes;

namespace EzrSquared.Runtime.Types.Executables;

/// <summary>
/// The function type object.
/// </summary>
public class EzrFunction : EzrRuntimeExecutable, IEzrMutableObject
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "function";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.RuntimeDefinedFunction";

    /// <summary>
    /// Should the function return its last expression as the result?
    /// </summary>
    public readonly bool ReturnLast;

    /// <summary>
    /// The context used to run the method's code in.
    /// </summary>
    /// <remarks>
    /// Do not use this, it's cleared after the method is run.
    /// </remarks>
    private readonly Context _runtimeMethodContext;

    /// <param name="name">The name of the executable.</param>
    /// <param name="body">The source code body of the executable.</param>
    /// <param name="parameters">The source code of the executable's parameters and their default values.</param>
    /// <param name="extraKeywordArguments">The position in source code and name of the variable for the executable's extra keyword arguments.</param>
    /// <param name="extraPositionalArguments">The position in source code and name of the variable for the executable's extra positional arguments.</param>
    /// <param name="returnLast">Should the function return its last expression as the result?</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrFunction(string? name, Node body, (string Name, Node Node)[] parameters, OptionalExtraArguments extraKeywordArguments, OptionalExtraArguments extraPositionalArguments, bool returnLast, Context parentContext, Position startPosition, Position endPosition) : base(name, body, parameters, extraKeywordArguments, extraPositionalArguments, parentContext, startPosition, endPosition)
    {
        ReturnLast = returnLast;
        _runtimeMethodContext = new($"<{TypeName} \"{ExecutableName}\">", false, StartPosition, CreationContext, CreationContext.StaticContext);
    }

    /// <summary>
    /// Executes the current function.
    /// </summary>
    /// <param name="arguments">The arguments of the execution.</param>
    /// <param name="interpreter">The interpreter to be used in execution.</param>
    /// <param name="result">Runtime result for carrying the result and any errors.</param>
    /// <param name="ignoreExtraArguments">Should the function ignore extra arguments?</param>
    public void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result, bool ignoreExtraArguments)
    {
        CheckAndPopulateArguments(arguments, _runtimeMethodContext, interpreter, result, ignoreExtraArguments);
        if (result.ShouldReturn)
            return;

        interpreter.VisitNode(Body, _runtimeMethodContext, null, AccessMod.None);
        if (result.ShouldReturnFunction)
            return;

        if (result.Reference.IsEmpty)
        {
            result.Success(NewNothingConstant());
            _runtimeMethodContext.Release();
            return;
        }

        if (result.Reference.RegisteredContext?.Id == _runtimeMethodContext.Id)
            result.Reference.UpdateRegisteredContext(Context, _runtimeMethodContext);
        else if (result.Reference.Object.CreationContext.Id == _runtimeMethodContext.Id)
            result.Reference.Object.UpdateCreationContext(_runtimeMethodContext);

        result.Success(result.Reference);
        _runtimeMethodContext.Release();
    }

    /// <inheritdoc/>
    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        Execute(arguments, interpreter, result, false);
    }

    /// <inheritdoc/>
    public IMutable<IEzrMutableObject>? DeepCopy(RuntimeResult result)
    {
        return new EzrFunction(IsAnonymous ? null : ExecutableName, Body, Parameters, ExtraKeywordArguments, ExtraPositionalArguments, ReturnLast, CreationContext, StartPosition, EndPosition);
    }
}
