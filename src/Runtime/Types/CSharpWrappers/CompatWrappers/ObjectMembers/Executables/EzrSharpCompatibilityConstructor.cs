using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.Attributes;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

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

        WrappedMemberAttribute? autoWrapperAttribute = constructingType.GetCustomAttribute<WrappedMemberAttribute>();
        ConstructingTypeName = !string.IsNullOrEmpty(autoWrapperAttribute?.Name) ? autoWrapperAttribute.Name : PascalToSnakeCase(constructingType.Name)!;
    }

    /// <inheritdoc/>
    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        object?[] mappedArguments = CheckAndPopulateArguments(arguments, interpreter, result);
        if (result.ShouldReturn)
            return;

        try
        {
            object? output = SharpMember.Invoke(mappedArguments);
            if (result.ShouldReturn)
                return;

            CSharpToEzrObject(output, ConstructingType, result);
        }
        catch (Exception error)
        {
            result.Failure(new EzrWrapperExecutionError(error.InnerException?.Message ?? error.Message, Context, StartPosition, EndPosition));
        }
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        bool hasParameters = ParameterNames.Length > 0;
        StringBuilder builder = new($"<{TypeName} for \"{ConstructingTypeName}\"");

        if (hasParameters)
            builder.Append($", with \"{string.Join("\", \"", ParameterNames)}\"");

        if (AcceptsExtraKeywordArguments)
        {
            builder.Append(hasParameters && AcceptsExtraPositionalArguments ? ", " : AcceptsExtraPositionalArguments || !hasParameters ? ", with " : " and ");
            builder.Append("extra keyword arguments");
        }

        if (AcceptsExtraPositionalArguments)
        {
            builder.Append(hasParameters || AcceptsExtraKeywordArguments ? " and " : ", with ");
            builder.Append("extra positional arguments");
        }

        return builder.Append('>').ToString();
    }
}
