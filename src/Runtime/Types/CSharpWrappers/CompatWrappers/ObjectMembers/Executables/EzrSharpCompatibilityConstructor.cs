using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;

public class EzrSharpCompatibilityConstructor : EzrSharpCompatibilityExecutable
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public readonly Type ConstructingType;
    public readonly string ConstructingTypeName;
    public readonly ConstructorInfo SharpConstructor;

    public EzrSharpCompatibilityConstructor(ConstructorInfo sharpConstructor,

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
        Type constructType,

        Context context, Position startPosition, Position endPosition) : base(sharpConstructor, null, context, startPosition, endPosition)
    {
        SharpConstructor = sharpConstructor;
        ConstructingType = constructType;
        ConstructingTypeName = Utils.PascalToSnakeCase(ConstructingType.Name);
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
            object? output = SharpConstructor.Invoke(mappedArguments);

            if (output is null)
                result.Success(NewNothingConstant());
            else
            {
                IEzrObject wrapper = new EzrSharpCompatibilityObjectInstance(output, ConstructingType, result, _executionContext, StartPosition, EndPosition);
                if (result.ShouldReturn)
                    return;

                result.Success(ReferencePool.Get(wrapper));
            }
        }
        catch (Exception error)
        {
            result.Failure(new EzrWrapperExecutionError(error.Message, Context, StartPosition, EndPosition));
            return;
        }
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return (other as EzrSharpCompatibilityConstructor)?.SharpConstructor == SharpConstructor && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, SharpConstructor);
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return ParameterNames.Length > 0
            ? $"<{TypeName} for type \"{ConstructingTypeName}\", with \"{string.Join("\", \"", ParameterNames)}\">"
            : $"<{TypeName} for \"{ConstructingTypeName}\">";
    }
}
