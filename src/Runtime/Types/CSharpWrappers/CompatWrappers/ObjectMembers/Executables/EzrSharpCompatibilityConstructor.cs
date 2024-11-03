using EzrSquared.Runtime.Types.Core.Errors;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;

/// <summary>
/// Class to automatically wrap C# constructors so that they can be used in ezr².
/// </summary>
/// <param name="constructingTypeWrapper">The wrapper for this C# constructor's class.</param>
/// <param name="sharpConstructor">The constructor to wrap.</param>
/// <param name="parentContext">The context in which this object was created.</param>
/// <param name="startPosition">The starting position of the object.</param>
/// <param name="endPosition">The ending position of the object.</param>
/// <param name="skipValidation">Skip method signature validation?</param>
public class EzrSharpCompatibilityConstructor(EzrSharpCompatibilityType constructingTypeWrapper, ConstructorInfo sharpConstructor, Context parentContext, Position startPosition, Position endPosition, bool skipValidation=false) : EzrSharpCompatibilityExecutable(sharpConstructor, null, parentContext, startPosition, endPosition, skipValidation)
{
    /// <summary>
    /// The wrapper for this C# constructor's class.
    /// </summary>
    public readonly EzrSharpCompatibilityType ConstructingTypeWrapper = constructingTypeWrapper;

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
            object? output = ((ConstructorInfo)Executable).Invoke(mappedArguments);

            if (output is null)
                result.Success(NewNothingConstant());
            else
            {
                IEzrObject wrapper = new EzrSharpCompatibilityObjectInstance(output, ConstructingTypeWrapper.SharpType, result, _executionContext, StartPosition, EndPosition);
                if (result.ShouldReturn)
                    return;

                result.Success(ReferencePool.Get(wrapper));
            }
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
            ? $"<{TypeName} for type \"{ConstructingTypeWrapper.SharpMemberName}\", with \"{string.Join("\", \"", ParameterNames)}\">"
            : $"<{TypeName} for \"{ConstructingTypeWrapper.SharpMemberName}\">";
    }
}
