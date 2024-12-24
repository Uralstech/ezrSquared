using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EzrSquared.Runtime.Types.Wrappers.Members.Methods;

/// <summary>
/// Base class for all automatic wrappers which wrap C# executables so that they can be used in ezr².
/// </summary>
public abstract class EzrMethodBaseWrapper<TMethodBase> : EzrWrapper<TMethodBase>
    where TMethodBase : MethodBase
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp runtime executable";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpRuntimeExecutable";

    /// <summary>
    /// Reflection information about the exposed parameters of the executable to wrap.
    /// </summary>
    public readonly (ParameterInfo Info, bool Optional)[] Parameters;

    /// <summary>
    /// The names of the exposed parameters of the executable to wrap, in ezr² (snake_case) format.
    /// </summary>
    public readonly string[] ParameterNames;

    /// <summary>
    /// The attributes of the executable's runtime-provided parameters.
    /// </summary>
    public readonly FeatureParameterAttribute[] RuntimeProvidedParameters;

    /// <summary>
    /// Does this executable accept extra keyword arguments?
    /// </summary>
    public readonly bool AcceptsExtraKeywordArguments;

    /// <summary>
    /// Does this executable accept extra positional arguments?
    /// </summary>
    public readonly bool AcceptsExtraPositionalArguments;

    /// <summary>
    /// Creates a new <see cref="EzrMethodBaseWrapper{TMethodBase}"/>.
    /// </summary>
    /// <param name="sharpMethodBase">The executable to wrap.</param>
    /// <param name="instance">The object which contains the executable, <see langword="null"/> if static.</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    /// <param name="skipValidation">Skip method signature validation?</param>
    public EzrMethodBaseWrapper(TMethodBase sharpMethodBase, object? instance, Context parentContext, Position startPosition, Position endPosition, bool skipValidation) : base(sharpMethodBase, instance, parentContext, startPosition, endPosition)
    {
        Tag = $"{Tag}.{SharpMemberName}.{UIDProvider.Get()}";
        if (!skipValidation)
            WrapMemberAttribute.ValidateMethod(SharpMember, AutoWrapperAttribute is null);

        ParameterInfo[] allParameters = SharpMember.GetParameters();
        List<(ParameterInfo, bool)> exposedParameters = new(allParameters.Length);
        List<string> exposedParameterNames = new(allParameters.Length);

        Lazy<List<FeatureParameterAttribute>> runtimeProvidedParameters = new();

        for (int i = 0; i < allParameters.Length; i++)
        {
            ParameterInfo parameterInfo = allParameters[i];

            ParameterAttribute? autoWrapperAttribute = parameterInfo.GetCustomAttribute<ParameterAttribute>();
            FeatureParameterAttribute? runtimeParamAttribute = parameterInfo.GetCustomAttribute<FeatureParameterAttribute>();

            if (autoWrapperAttribute is not null && runtimeParamAttribute is not null)
                throw new ArgumentException($"Method \"{SharpMember.Name}\" cannot have a parameter with both {nameof(ParameterAttribute)} and {nameof(FeatureParameterAttribute)} attributes ({parameterInfo.Name})!", nameof(sharpMethodBase));
            else if (runtimeParamAttribute is null && runtimeProvidedParameters.IsValueCreated)
                throw new ArgumentException($"Method \"{SharpMember.Name}\" cannot have a normal parameter after {nameof(FeatureParameterAttribute)}-attributed parameters ({parameterInfo.Name})!", nameof(sharpMethodBase));

            if (runtimeParamAttribute is null)
            {
                if (autoWrapperAttribute?.Optional == true
                    && parameterInfo.ParameterType.IsValueType
                    && Nullable.GetUnderlyingType(parameterInfo.ParameterType) is null)
                    throw new ArgumentException($"Method \"{SharpMember.Name}\" contains a parameter declared optional through its {nameof(ParameterAttribute)} which is not nullable ({parameterInfo.Name})!", nameof(sharpMethodBase));

                exposedParameters.Add((parameterInfo, autoWrapperAttribute?.Optional ?? false));
                exposedParameterNames.Add(string.IsNullOrEmpty(autoWrapperAttribute?.Name) ? PascalToSnakeCase(parameterInfo.Name) ?? $"param_{i}" : autoWrapperAttribute.Name);
                continue;
            }

            runtimeParamAttribute.ValidateParameter(parameterInfo);
            runtimeProvidedParameters.Value.Add(runtimeParamAttribute);

            switch (runtimeParamAttribute.Type)
            {
                case Feature.KeywordArguments:
                    AcceptsExtraKeywordArguments = true; break;

                case Feature.PositionalArguments:
                    AcceptsExtraPositionalArguments = true; break;
            }
        }

        Parameters = [.. exposedParameters];
        ParameterNames = [.. exposedParameterNames];
        RuntimeProvidedParameters = runtimeProvidedParameters.IsValueCreated ? [.. runtimeProvidedParameters.Value] : [];
    }

    /// <summary>
    /// Converts an array of arguments from ezr² code to an array of primitive C# objects in the order the executable expects them in.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <param name="interpreter">The interpreter to be used in execution.</param>
    /// <param name="result">Runtime result for carrying errors.</param>
    /// <returns>The array of objects.</returns>
    protected internal object?[] CheckAndPopulateArguments(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        int exposedParametersLength = Parameters.Length;
        int totalParametersLength = exposedParametersLength + RuntimeProvidedParameters.Length;

        object?[] formattedArguments = totalParametersLength == 0 ? [] : new object?[totalParametersLength];
        Array.Fill(formattedArguments, Type.Missing, 0, Parameters.Length);

        Lazy<ExtraKeywordArguments> extraKeywordArguments = new();
        Lazy<ExtraPositionalArguments> extraPositionalArguments = new();

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
                bool isExtraArgument = parameterIndex == -1;

                if (isExtraArgument && !AcceptsExtraKeywordArguments)
                {
                    result.Failure(new EzrUnexpectedArgumentError($"Did not expect argument \"{argumentName}\"!", _executionContext, argumentObject.StartPosition, argumentObject.EndPosition));
                    return [];
                }

                if ((isExtraArgument && extraKeywordArguments.Value.ContainsKey(argumentName))
                    || (!isExtraArgument && !ReferenceEquals(formattedArguments[parameterIndex], Type.Missing)))
                {
                    result.Failure(new EzrIllegalOperationError($"Cannot override already defined argument \"{argumentName}\"!", _executionContext, argumentObject.StartPosition, argumentObject.EndPosition));
                    return [];
                }

                if (isExtraArgument)
                {
                    extraKeywordArguments.Value.Add(argumentName, argumentObject);
                    ReferencePool.TryRelease(argumentReference);
                    continue;
                }

                formattedArguments[parameterIndex] = EzrObjectToCSharp(argumentObject, Parameters[parameterIndex].Info.ParameterType, result);
                if (result.ShouldReturn)
                    return [];

                ReferencePool.TryRelease(argumentReference);
                continue;
            }

            // Handle unnamed parameter.

            // Find the next unfilled parameter, skip already assigned ones.
            for (; nextUnnamedParamIndex < exposedParametersLength && !ReferenceEquals(formattedArguments[nextUnnamedParamIndex], Type.Missing); nextUnnamedParamIndex++)
                continue;

            bool allPositionalArgumentsFilled = nextUnnamedParamIndex >= exposedParametersLength;
            if ((allPositionalArgumentsFilled || Parameters[nextUnnamedParamIndex].Optional) && AcceptsExtraPositionalArguments)
            {
                extraPositionalArguments.Value.Add(argumentObject);
                ReferencePool.TryRelease(argumentReference);
                continue;
            }

            if (allPositionalArgumentsFilled)
            {
                result.Failure(new EzrUnexpectedArgumentError("Did not expect any more unnamed arguments!", _executionContext, argumentObject.StartPosition, argumentObject.EndPosition));
                return [];
            }

            formattedArguments[nextUnnamedParamIndex] = EzrObjectToCSharp(argumentObject, Parameters[nextUnnamedParamIndex].Info.ParameterType, result);
            if (result.ShouldReturn)
                return [];

            nextUnnamedParamIndex++;
            ReferencePool.TryRelease(argumentReference);
        }

        // Check for missing parameters, but skip parameters with default values.
        for (int i = 0; i < exposedParametersLength; i++)
        {
            if (!ReferenceEquals(formattedArguments[i], Type.Missing))
                continue;

            (ParameterInfo parameterInfo, bool isOptional) = Parameters[i];
            if (!parameterInfo.HasDefaultValue && !isOptional)
            {
                result.Failure(new EzrMissingRequiredArgumentError($"Expected required argument \"{ParameterNames[i]}\"!", _executionContext, StartPosition, EndPosition));
                return [];
            }

            formattedArguments[i] = parameterInfo.HasDefaultValue ? parameterInfo.DefaultValue : null;
        }

        // Supply runtime-provided parameters.
        for (int i = 0; i < RuntimeProvidedParameters.Length; i++)
        {
            Feature paramtype = RuntimeProvidedParameters[i].Type;
            formattedArguments[exposedParametersLength + i] = paramtype switch
            {
                Feature.KeywordArguments => extraKeywordArguments.Value,
                Feature.PositionalArguments => extraPositionalArguments.Value,

                Feature.CallerRef => this,
                Feature.ExecutionRef => _executionContext,
                Feature.InterpreterRef => interpreter,
                Feature.ResultRef => result,

                _ => throw new NotImplementedException($"Case for handling runtime-provided parameter type \"{paramtype}\" has not been implemented!")
            };
        }

        return formattedArguments;
    }
}
