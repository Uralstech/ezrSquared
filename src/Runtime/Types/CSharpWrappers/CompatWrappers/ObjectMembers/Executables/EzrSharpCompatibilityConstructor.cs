using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;

/// <summary>
/// Class to automatically wrap C# constructors so that they can be used in ezr².
/// </summary>
public class EzrSharpCompatibilityConstructor : EzrSharpCompatibilityExecutable
{
    /// <summary>
    /// Reflection information about the type which is constructed by the wrapped constructor.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public readonly Type ConstructingType;

    /// <summary>
    /// The name of the constructor's constructing type, in ezr² format (snake_case).
    /// </summary>
    public readonly string ConstructingTypeName;

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityConstructor"/>.
    /// </summary>
    /// <param name="constructTypeName">The name of the constructor's constructing type, in ezr² format (snake_case).</param>
    /// <param name="sharpConstructor">The constructor to wrap.</param>
    /// <param name="constructType">The type which is constructed by the constructor.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrSharpCompatibilityConstructor(
        string constructTypeName,
        ConstructorInfo sharpConstructor,

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
        Type constructType,

        Context parentContext, Position startPosition, Position endPosition) : base(sharpConstructor, null, parentContext, startPosition, endPosition)
    {
        ConstructingType = constructType;
        ConstructingTypeName = constructTypeName;
    }

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityConstructor"/>. Infers the constructing type's name by converting it to snake_case.
    /// </summary>
    /// <param name="sharpConstructor">The constructor to wrap.</param>
    /// <param name="constructType">The type which is constructed by the constructor.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrSharpCompatibilityConstructor(
        ConstructorInfo sharpConstructor,

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
        Type constructType,

        Context parentContext, Position startPosition, Position endPosition)
    : this(Utils.PascalToSnakeCase(constructType.Name), sharpConstructor, constructType, parentContext, startPosition, endPosition) { }

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
                IEzrObject wrapper = new EzrSharpCompatibilityObjectInstance(ConstructingTypeName, output, ConstructingType, result, _executionContext, StartPosition, EndPosition);
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
            ? $"<{TypeName} for type \"{ConstructingTypeName}\", with \"{string.Join("\", \"", ParameterNames)}\">"
            : $"<{TypeName} for \"{ConstructingTypeName}\">";
    }
}
