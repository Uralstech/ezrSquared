using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.WrapperAttributes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;

/// <summary>
/// Class to automatically wrap C# constructors so that they can be used in ezr².
/// </summary>
public class EzrSharpCompatibilityConstructor : EzrSharpCompatibilityExecutable<ConstructorInfo>
{
    /// <summary>
    /// This C# constructor's class.
    /// </summary>
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods
        | DynamicallyAccessedMemberTypes.NonPublicMethods
        | DynamicallyAccessedMemberTypes.PublicProperties
        | DynamicallyAccessedMemberTypes.NonPublicProperties
        | DynamicallyAccessedMemberTypes.PublicFields
        | DynamicallyAccessedMemberTypes.NonPublicFields)]
    public readonly Type ConstructingType;

    /// <summary>
    /// The name of the constructing type in snake_case.
    /// </summary>
    public readonly string ConstructingTypeName;

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityConstructor"/>.
    /// </summary>
    /// <param name="constructingType">This C# constructor's class.</param>
    /// <param name="sharpConstructor">The constructor to wrap.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    /// <param name="skipValidation">Skip method signature validation?</param>
    public EzrSharpCompatibilityConstructor(
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods
            | DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.NonPublicProperties
            | DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.NonPublicFields)]
        Type constructingType,

        ConstructorInfo sharpConstructor, Context parentContext, Position startPosition, Position endPosition, bool skipValidation = false) : base(sharpConstructor, null, parentContext, startPosition, endPosition, skipValidation)
    {
        ConstructingType = constructingType;

        SharpAutoWrapperAttribute? autoWrapperAttribute = constructingType.GetCustomAttribute<SharpAutoWrapperAttribute>();
        ConstructingTypeName = !string.IsNullOrEmpty(autoWrapperAttribute?.Name) ? autoWrapperAttribute.Name : PascalToSnakeCase(constructingType.Name)!;
    }

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
            object? output = SharpMember.Invoke(mappedArguments);

            if (output is null)
                result.Success(NewNothingConstant());
            else
            {
                IEzrObject wrapper = new EzrSharpCompatibilityObjectInstance(output, ConstructingType, _executionContext, StartPosition, EndPosition);
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
