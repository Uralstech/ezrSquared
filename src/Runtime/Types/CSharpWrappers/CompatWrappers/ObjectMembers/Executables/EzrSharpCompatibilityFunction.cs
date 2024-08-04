using EzrSquared.Runtime.Types.Core.Errors;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;

public class EzrSharpCompatibilityFunction : EzrSharpCompatibilityExecutable
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp function";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpFunction";

    public readonly MethodInfo SharpFunction;

    public EzrSharpCompatibilityFunction(MethodInfo sharpFunction, object? instance, Context context, Position startPosition, Position endPosition) : base(sharpFunction, instance, context, startPosition, endPosition)
    {
        SharpFunction = sharpFunction;
    }

    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        Dictionary<string, IEzrObject> formattedArguments = ArgumentsArrayToDictionary(arguments, result);
        if (result.ShouldReturn)
            return;

        object?[] mappedArguments = CheckAndPopulateArguments(formattedArguments, result);
        if (result.ShouldReturn)
            return;

        try
        {
            object? output = SharpFunction.Invoke(Instance, mappedArguments);

            if (output is null)
                result.Success(NewNothingConstant());
            else
                PrimitiveToEzrObject(output, Type.GetTypeCode(output.GetType()), result);
        }
        catch (Exception error)
        {
            result.Failure(new EzrWrapperExecutionError(error.Message, Context, StartPosition, EndPosition));
            return;
        }
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, SharpFunction);
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return other is EzrSharpCompatibilityFunction function
            && function.SharpFunction == SharpFunction
            && function.Instance?.GetHashCode() == Instance?.GetHashCode()
            && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return ParameterNames.Length > 0
            ? $"<{TypeName} \"{SharpRuntimeExecutableName}\", with \"{string.Join("\", \"", ParameterNames)}\">"
            : $"<{TypeName} \"{SharpRuntimeExecutableName}\">";
    }
}
