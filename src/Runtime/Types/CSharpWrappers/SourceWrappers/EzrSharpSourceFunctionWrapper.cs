using EzrSquared.Runtime.WrapperAttributes;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.SourceWrappers;

/// <summary>
/// A class to wrap methods written in C# so that they can be used in ezr².
/// </summary>
public partial class EzrSharpSourceFunctionWrapper : EzrSharpSourceExecutableWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp source function wrapper";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpSourceFunctionWrapper";

    /// <summary>
    /// The wrapped function.
    /// </summary>
    public readonly EzrSharpSourceWrappableMethod SharpFunction;

    /// <summary>
    /// The name of the function, in snake_case.
    /// </summary>
    public readonly string SharpFunctionName;

    /// <summary>
    /// Creates a new <see cref="EzrSharpSourceFunctionWrapper"/> from a function's <see cref="MethodInfo"/>.
    /// </summary>
    /// <param name="function">The method to wrap.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrSharpSourceFunctionWrapper(MethodInfo function, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        Exception? parameterException = SharpMethodWrapperAttribute.ValidateMethodParameters(function);
        if (parameterException is not null)
            throw parameterException;

        SharpFunction = (EzrSharpSourceWrappableMethod)function.CreateDelegate(typeof(EzrSharpSourceWrappableMethod));
        (SharpFunctionName, SharpMethodWrapperAttribute attribute) = GetFunctionInfo(function);
        
        AddParameters(attribute);
    }

    /// <summary>
    /// Creates a new <see cref="EzrSharpSourceFunctionWrapper"/> from a function.
    /// </summary>
    /// <param name="function">The method to wrap.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrSharpSourceFunctionWrapper(EzrSharpSourceWrappableMethod function, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        SharpFunction = function;
        (SharpFunctionName, SharpMethodWrapperAttribute attribute) = GetFunctionInfo(function.Method);

        AddParameters(attribute);
    }

    /// <summary>
    /// Gets the function's ezr² (snake_case) name and wrapper attribute.
    /// </summary>
    /// <param name="function">The function's <see cref="MethodInfo"/>.</param>
    /// <returns>The function's ezr² name and wrapper attribute</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the <see cref="SharpMethodWrapperAttribute"/> attribute was not found in the function
    /// or if the name given in the function's <see cref="SharpMethodWrapperAttribute"/> is empty.
    /// </exception>
    private (string, SharpMethodWrapperAttribute) GetFunctionInfo(MethodInfo function)
    {
        SharpMethodWrapperAttribute attribute = function.GetCustomAttribute<SharpMethodWrapperAttribute>(true)
            ?? throw new ArgumentException($"No \"{nameof(SharpMethodWrapperAttribute)}\" attribute found in function \"{function.Name}\"!", nameof(function));
        
        if (string.IsNullOrEmpty(attribute.Name))
            throw new ArgumentException($"Name not provided in {nameof(SharpMethodWrapperAttribute)} of function \"{function.Name}\"!", nameof(function));
        
        Tag = $"{Tag}.{attribute.Name}.{Utils.GetNextUniqueId()}";
        return (attribute.Name, attribute);
    }

    /// <summary>
    /// Adds the parameters of the function to be wrapped to the object.
    /// </summary>
    /// <param name="attribute">The attribute of the function containing the details for optional, required and extra keyword parameters.</param>
    private void AddParameters(SharpMethodWrapperAttribute attribute)
    {
        int requiredParameters = attribute.RequiredParameters.Length;
        Parameters = new (string Name, bool IsRequired)[attribute.RequiredParameters.Length + attribute.OptionalParameters.Length];

        for (int j = 0; j < requiredParameters; j++)
            Parameters[j] = new(attribute.RequiredParameters[j], true);

        for (int j = 0; j < attribute.OptionalParameters.Length; j++)
            Parameters[j + requiredParameters] = new(attribute.OptionalParameters[j], false);

        HasKeywordArguments = attribute.HasKeywordArguments;
    }

    /// <inheritdoc/>
    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        Dictionary<string, Reference> argumentReferences = CheckAndPopulateArguments(arguments, result);
        if (result.ShouldReturn)
            return;

        SharpFunction.Invoke(new SharpMethodParameters
        (
            argumentReferences,
            _executionContext,
            CreationContext,
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
