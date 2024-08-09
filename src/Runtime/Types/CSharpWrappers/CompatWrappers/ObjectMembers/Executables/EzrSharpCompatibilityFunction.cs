using EzrSquared.Runtime.Types.Core.Errors;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;

/// <summary>
/// Class to automatically wrap C# methods so that they can be used in ezr².
/// </summary>
public class EzrSharpCompatibilityFunction : EzrSharpCompatibilityExecutable
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp function";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpFunction";

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityFunction"/>. Infers the name by converting the member's name to snake_case.
    /// </summary>
    /// <param name="name">The name of the method to wrap, in ezr² format (snake_case).</param>
    /// <param name="sharpFunction">The method to wrap.</param>
    /// <param name="instance">The object which contains the method, <see langword="null"/> if static.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrSharpCompatibilityFunction(string name, MethodInfo sharpFunction, object? instance, Context parentContext, Position startPosition, Position endPosition)
        : base(name, sharpFunction, instance, parentContext, startPosition, endPosition) { }

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityFunction"/>.
    /// </summary>
    /// <param name="sharpFunction">The method to wrap.</param>
    /// <param name="instance">The object which contains the method, <see langword="null"/> if static.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrSharpCompatibilityFunction(MethodInfo sharpFunction, object? instance, Context parentContext, Position startPosition, Position endPosition)
        : base(sharpFunction, instance, parentContext, startPosition, endPosition) { }

    /// <inheritdoc/>
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
            object? output = Executable.Invoke(Instance, mappedArguments);

            if (output is null)
                result.Success(NewNothingConstant());
            else
                PrimitiveToEzrObject(output, output.GetType(), result);
        }
        catch (Exception error)
        {
            result.Failure(new EzrWrapperExecutionError(error.InnerException?.Message ?? error.Message, Context, StartPosition, EndPosition));
        }
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return ParameterNames.Length > 0
            ? $"<{TypeName} \"{SharpRuntimeExecutableName}\", with \"{string.Join("\", \"", ParameterNames)}\">"
            : $"<{TypeName} \"{SharpRuntimeExecutableName}\">";
    }
}
