using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.WrapperAttributes;
using EzrSquared.Util;
using System;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;

/// <summary>
/// Base class for all automatic wrappers which wrap C# executables so that they can be used in ezr².
/// </summary>
public abstract class EzrSharpCompatibilityExecutable<TMethodBase> : EzrSharpCompatibilityWrapper<TMethodBase>
    where TMethodBase : MethodBase
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp runtime executable";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpRuntimeExecutable";

    /// <summary>
    /// Reflection information about the parameters of the executable to wrap.
    /// </summary>
    public readonly ParameterInfo[] Parameters;

    /// <summary>
    /// The names of the parameters of the executable to wrap, in ezr² (snake_case) format.
    /// </summary>
    public readonly string[] ParameterNames;

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityExecutable{TMethodBase}"/>.
    /// </summary>
    /// <param name="sharpMethodBase">The executable to wrap.</param>
    /// <param name="instance">The object which contains the executable, <see langword="null"/> if static.</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    /// <param name="skipValidation">Skip method signature validation?</param>
    public EzrSharpCompatibilityExecutable(TMethodBase sharpMethodBase, object? instance, Context parentContext, Position startPosition, Position endPosition, bool skipValidation) : base(sharpMethodBase, instance, parentContext, startPosition, endPosition)
    {
        Tag = $"{Tag}.{SharpMemberName}.{UIDProvider.Get()}";

        Parameters = SharpMember.GetParameters();
        ParameterNames = new string[Parameters.Length];

        for (int i = 0; i < Parameters.Length; i++)
        {
            ParameterInfo parameter = Parameters[i];
            string? definedName = parameter.GetCustomAttribute<SharpAutoWrapperAttribute>()?.Name;

            ParameterNames[i] = string.IsNullOrEmpty(definedName) ? PascalToSnakeCase(parameter.Name) ?? $"param_{i}" : definedName;
        }

        if (!skipValidation)
            SharpAutoWrapperAttribute.ValidateMethod(SharpMember, AutoWrapperAttribute is null);
    }

    /// <summary>
    /// Converts an array of arguments from ezr² code to an array of primitive C# objects in the order the executable expects them in.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <param name="result">Runtime result for carrying errors.</param>
    /// <returns>The array of objects.</returns>
    protected internal object?[] CheckAndPopulateArguments(Reference[] arguments, RuntimeResult result)
    {
        int parametersLength = Parameters.Length;

        object?[] formattedArguments = parametersLength == 0 ? [] : new object?[parametersLength];
        Array.Fill(formattedArguments, Type.Missing);

        int nextUnnamedParamIndex = 0; // Track the index for unnamed params.
        for (int argIndex = 0; argIndex < arguments.Length; argIndex++)
        {
            Reference argumentReference = arguments[argIndex];
            string? argumentName = argumentReference.Name;
            IEzrObject argumentObject = argumentReference.Object;

            if (!string.IsNullOrEmpty(argumentName) && !argumentReference.IsRegistered)
            {
                // Handle named parameter.
                int parameterIndex = Array.IndexOf(ParameterNames, argumentName);
                if (parameterIndex == -1)
                {
                    result.Failure(new EzrUnexpectedArgumentError($"Did not expect argument \"{argumentName}\"!", _executionContext, argumentObject.StartPosition, argumentObject.EndPosition));
                    return [];
                }
                
                if (!ReferenceEquals(formattedArguments[parameterIndex], Type.Missing))
                {
                    result.Failure(new EzrIllegalOperationError($"Cannot override already defined argument \"{argumentName}\"!", _executionContext, argumentObject.StartPosition, argumentObject.EndPosition));
                    return [];
                }

                formattedArguments[parameterIndex] = EzrObjectToCSharp(argumentObject, Parameters[parameterIndex].ParameterType, result);
                if (result.ShouldReturn)
                    return [];

                ReferencePool.TryRelease(argumentReference);
                continue;
            }

            // Handle unnamed parameter.

            // Find the next unfilled parameter, skip already assigned ones.
            for (; nextUnnamedParamIndex < parametersLength && !ReferenceEquals(formattedArguments[nextUnnamedParamIndex], Type.Missing); nextUnnamedParamIndex++)
                continue;

            if (nextUnnamedParamIndex >= parametersLength)
            {
                result.Failure(new EzrUnexpectedArgumentError("Did not expect any more unnamed arguments!", _executionContext, argumentObject.StartPosition, argumentObject.EndPosition));
                return [];
            }

            formattedArguments[nextUnnamedParamIndex] = EzrObjectToCSharp(argumentObject, Parameters[nextUnnamedParamIndex].ParameterType, result);
            if (result.ShouldReturn)
                return [];
            
            nextUnnamedParamIndex++;
            ReferencePool.TryRelease(argumentReference);
        }

        // Check for missing parameters, but skip parameters with default values.
        for (int i = 0; i < parametersLength; i++)
        {
            if (!ReferenceEquals(formattedArguments[i], Type.Missing))
                continue;

            if (!Parameters[i].HasDefaultValue)
            {
                result.Failure(new EzrMissingRequiredArgumentError($"Expected required argument \"{ParameterNames[i]}\"!", _executionContext, StartPosition, EndPosition));
                return [];
            }

            formattedArguments[i] = Parameters[i].DefaultValue;
        }

        return formattedArguments;
    }
}
