using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;

public abstract class EzrSharpCompatibilityExecutable : EzrSharpCompatibilityWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp runtime executable";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpRuntimeExecutable";

    public readonly ParameterInfo[] Parameters;
    public readonly string[] ParameterNames;
    public readonly MethodBase Executable;
    public readonly object? Instance;
    public readonly string SharpRuntimeExecutableName;

    public EzrSharpCompatibilityExecutable(string name, MethodBase sharpMember, object? instance, Context context, Position startPosition, Position endPosition) : base(context, startPosition, endPosition)
    {
        SharpRuntimeExecutableName = name;
        Tag = $"{Tag}.{SharpRuntimeExecutableName}.{Utils.GetNextUniqueId()}";
        
        Executable = sharpMember;
        Parameters = Executable.GetParameters();
        ParameterNames = Array.ConvertAll(Parameters, p => Utils.PascalToSnakeCase(p.Name ?? string.Empty));
        Instance = instance;
    }

    public EzrSharpCompatibilityExecutable(MethodBase sharpMember, object? instance, Context context, Position startPosition, Position endPosition)
        : this(Utils.PascalToSnakeCase(sharpMember.Name), sharpMember, instance, context, startPosition, endPosition) { }

    protected internal Dictionary<string, IEzrObject> ArgumentsArrayToDictionary(Reference[] arguments, RuntimeResult result)
    {
        Dictionary<string, IEzrObject> formattedArguments = new(arguments.Length);

        int index = 0;
        int requiredKeywordArguments = 0;
        for (int i = 0; i < arguments.Length; i++)
        {
            Reference reference = arguments[i];
            if (!string.IsNullOrEmpty(reference.Name) && !reference.IsRegistered)
            {
                if (formattedArguments.ContainsKey(reference.Name))
                {
                    result.Failure(new EzrIllegalOperationError($"Cannot override already defined argument \"{reference.Name}\"!", _executionContext, reference.Object.StartPosition, reference.Object.EndPosition));
                    break;
                }

                formattedArguments[reference.Name] = reference.Object;
                if (Array.IndexOf(ParameterNames, reference.Name) >= 0)
                    requiredKeywordArguments++;
            }
            else
            {
            IndexCheck:
                if (ParameterNames.Length <= index)
                {
                    result.Failure(new EzrUnexpectedArgumentError(
                        requiredKeywordArguments > 0
                            ? $"Only expected {ParameterNames.Length - requiredKeywordArguments} unnamed argument(s) as {requiredKeywordArguments} required argument(s) has/have been declared as (a) keyword argument(s)!"
                            : $"Only expected {ParameterNames.Length} unnamed argument(s)!",
                        _executionContext, StartPosition, EndPosition));
                    break;
                }

                string argumentName = ParameterNames[index];
                if (!formattedArguments.ContainsKey(argumentName))
                {
                    formattedArguments[argumentName] = reference.Object;
                    index++;
                }
                else
                {
                    index++;
                    goto IndexCheck;
                }
            }
        }

        return formattedArguments;
    }

    protected internal object?[] CheckAndPopulateArguments(Dictionary<string, IEzrObject> arguments, RuntimeResult result)
    {
        object?[] formattedArguments = new object?[Parameters.Length];
        Array.Fill(formattedArguments, Type.Missing);

        for (int i = 0; i < Parameters.Length; i++)
        {
            ParameterInfo parameter = Parameters[i];
            if (!string.IsNullOrEmpty(parameter.Name) && arguments.TryGetValue(ParameterNames[i], out IEzrObject? argument))
            {
                object? primitiveArgument = EzrObjectToPrimitive(argument, Type.GetTypeCode(parameter.ParameterType), result);
                if (result.ShouldReturn)
                    return [];

                if (primitiveArgument?.GetType() != parameter.ParameterType)
                {
                    result.Failure(new EzrUnexpectedTypeError($"CSharp argument \"{ParameterNames[i]}\" expected value of CSharp type \"{parameter.ParameterType.Name}\", but got object of type \"{argument.TypeName}\"!", Context, argument.StartPosition, argument.EndPosition));
                    return [];
                }

                formattedArguments[i] = primitiveArgument;
                arguments.Remove(ParameterNames[i]);
            }
            else if (parameter.HasDefaultValue)
                formattedArguments[i] = parameter.DefaultValue;
            else
            {
                result.Failure(new EzrMissingRequiredArgumentError($"Expected required argument \"{ParameterNames[i]}\"!", _executionContext, StartPosition, EndPosition));
                return [];
            }
        }

        if (arguments.Count > 0)
        {
            Dictionary<string, IEzrObject>.Enumerator argumentsEnumerator = arguments.GetEnumerator();
            argumentsEnumerator.MoveNext();

            KeyValuePair<string, IEzrObject> first = argumentsEnumerator.Current;
            result.Failure(new EzrUnexpectedArgumentError($"Did not expect argument \"{first.Key}\"!", _executionContext, first.Value.StartPosition, first.Value.EndPosition));
        }

        return formattedArguments;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, Executable);
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return other is EzrSharpCompatibilityExecutable executable
            && executable.Executable == Executable
            && executable.Instance?.GetHashCode() == Instance?.GetHashCode()
            && other.HashTag == HashTag;
    }
}
