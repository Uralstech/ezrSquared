using EzrSquared.Runtime.WrapperAttributes;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.SourceWrappers;

public partial class EzrSharpSourceFunctionWrapper : EzrSharpSourceExecutableWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp source function wrapper";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpSourceFunctionWrapper";

    public readonly EzrSharpSourceWrappableMethod SharpFunction;
    public readonly string SharpFunctionName;

    public EzrSharpSourceFunctionWrapper(EzrSharpSourceWrappableMethod function, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        SharpFunction = function;
        SharpMethodWrapperAttribute attribute = SharpFunction.Method.GetCustomAttribute<SharpMethodWrapperAttribute>(true) ?? throw new ArgumentException($"No \"{nameof(SharpMethodWrapperAttribute)}\" attribute found!", nameof(function));

        Exception? parameterException = SharpMethodWrapperAttribute.ValidateMethodParameters(SharpFunction.Method);
        if (parameterException is not null)
            throw parameterException;

        if (string.IsNullOrEmpty(attribute.Name))
            throw new ArgumentException($"ezrSquared compliant name not provided in {nameof(SharpMethodWrapperAttribute)} of function!", nameof(function));

        int requiredParameters = attribute.RequiredParameters.Length;
        Parameters = new (string Name, bool IsRequired)[attribute.RequiredParameters.Length + attribute.OptionalParameters.Length];

        for (int j = 0; j < requiredParameters; j++)
            Parameters[j] = new(attribute.RequiredParameters[j], true);

        for (int j = 0; j < attribute.OptionalParameters.Length; j++)
            Parameters[j + requiredParameters] = new(attribute.OptionalParameters[j], false);

        HasKeywordArguments = attribute.HasKeywordArguments;
        SharpFunctionName = attribute.Name;
        Tag = $"{Tag}.{SharpFunctionName}.{Utils.GetNextUniqueId()}";
    }

    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        Dictionary<string, Reference> argumentReferences = CheckAndPopulateArguments(arguments, result);
        if (result.ShouldReturn)
            return;

        SharpFunction.Invoke(new SharpMethodParameters
        (
            argumentReferences,
            _executionContext,
            _creationContext,
            Context,
            StartPosition,
            EndPosition,
            interpreter,
            result
        ));
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, SharpFunction);
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return (other as EzrSharpSourceFunctionWrapper)?.SharpFunction == SharpFunction && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"<{TypeName} \"{SharpFunctionName}\">";
    }
}
