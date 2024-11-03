global using WrapperArgumentPopulationResult = (System.Collections.Generic.Dictionary<string, EzrSquared.Runtime.Reference> Arguments, System.Collections.Generic.List<EzrSquared.Runtime.Reference>? ExtraPositionalArguments);

using EzrSquared.Runtime.Types.Core.Errors;
using System;
using System.Collections.Generic;
using System.Text;

namespace EzrSquared.Runtime.Types.CSharpWrappers.SourceWrappers;

/// <summary>
/// Parent of all wrapper classes which wrap executable members written in C# so that they can be used in ezr².
/// </summary>
/// <param name="parentContext">The context in which this object was created.</param>
/// <param name="startPosition">The starting position of the object.</param>
/// <param name="endPosition">The ending position of the object.</param>
public abstract class EzrSharpSourceExecutableWrapper(Context parentContext, Position startPosition, Position endPosition) : EzrObject(parentContext, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp source executable wrapper";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpSourceExecutableWrapper";

    /// <summary>
    /// The name of the executable's parameters and if they are required.
    /// </summary>
    public (string Name, bool IsRequired)[] Parameters = [];

    /// <summary>
    /// Does the executable accept extra keyword arguments?
    /// </summary>
    public bool HasExtraKeywordArguments;

    /// <summary>
    /// Does the executable accept extra positional arguments?
    /// </summary>
    public bool HasExtraPositionalArguments;

    /// <summary>
    /// Converts a string from PascalCase to lowecase plain text, seperated by spaces.
    /// </summary>
    /// <param name="text">The text to convert in PascalCase.</param>
    /// <returns>The converted text in lowecase plain text.</returns>
    internal protected static string PascalCaseToLowerCasePlainText(string text)
    {
        StringBuilder result = new();
        result.Append(char.ToLowerInvariant(text[0]));

        for (int i = 1; i < text.Length; ++i)
        {
            char c = text[i];
            if (char.IsUpper(c))
                result.Append(' ').Append(char.ToLowerInvariant(c));
            else
                result.Append(c);
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts an index to a power of two, so that the index can be used like an enum flag.
    /// </summary>
    /// <remarks>
    /// This function is used to check if all arguments have been provided to <see cref="Runtime.Types.CSharpWrappers.SourceWrappers.EzrSharpSourceExecutableWrapper"/> and <see cref="Runtime.Types.Executables.EzrRuntimeExecutable"/> types.<br/>
    /// The indices of the given arguments are converted to powers of two and bitwise-ored together, then bitwise-anded with the index of a defined parameter which is also converted to a power of two.<br/>
    /// Finally, if the result is the same as the power of two of the parameter's index, this tells the interpreter that the particular required parameter has been provided.
    /// </remarks>
    /// <param name="index">The index to be converted.</param>
    /// <returns>The power of two.</returns>
    protected internal static int IndexToFlag(int index)
    {
        return (index++ < 3) ? index : (4 * index) - 8;
    }

    /// <summary>
    /// Checks and populates the arguments given by the user's ezr² code into a dictionary.
    /// </summary>
    /// <param name="arguments">The array of arguments.</param>
    /// <param name="result">Runtime result for carrying any errors.</param>
    /// <returns>The dictionary of all the arguments.</returns>
    protected internal WrapperArgumentPopulationResult CheckAndPopulateArguments(Reference[] arguments, RuntimeResult result)
    {
        int totalRequiredArguments = 0;
        foreach ((string Name, bool IsRequired) in Parameters)
        {
            if (IsRequired)
                totalRequiredArguments++;
        }

        int calculatedParameterIndex = 0;
        int requiredKeywordArguments = 0;
        int flaggedRequiredArguments = -1;

        Dictionary<string, Reference> argumentReferences = new(arguments.Length);
        List<Reference>? extraPositionalArgumentReferences = HasExtraPositionalArguments ? new() : null;

        for (int i = 0; i < arguments.Length; i++)
        {
            string name;
            int parameterIndex = 0;
            Reference reference = arguments[i];

            if (!string.IsNullOrEmpty(reference.Name) && !reference.IsRegistered && !reference.IsEmpty)
            {
                name = reference.Name;
                if (argumentReferences.ContainsKey(name))
                {
                    result.Failure(new EzrIllegalOperationError($"Cannot override already defined argument \"{name}\"!", _executionContext, reference.Object.StartPosition, reference.Object.EndPosition));
                    return (argumentReferences, extraPositionalArgumentReferences);
                }

                parameterIndex = Array.FindIndex(Parameters, (param) => param.Name == name);
                if (parameterIndex > -1)
                {
                    argumentReferences.Add(name, reference);
                    requiredKeywordArguments++;
                }
                else if (HasExtraKeywordArguments)
                {
                    argumentReferences.Add(name, reference);
                    parameterIndex = -1;
                }
                else
                {
                    result.Failure(new EzrUnexpectedArgumentError($"Did not expect argument \"{name}\"!", _executionContext, reference.Object.StartPosition, reference.Object.EndPosition));
                    return (argumentReferences, extraPositionalArgumentReferences);
                }
            }
            else
            {
            IndexCheck:
                if (Parameters.Length <= calculatedParameterIndex && !HasExtraPositionalArguments)
                {
                    result.Failure(new EzrUnexpectedArgumentError(
                        requiredKeywordArguments > 0
                            ? $"Only expected {Parameters.Length - requiredKeywordArguments} unnamed argument(s) as {requiredKeywordArguments} required argument(s) has/have been declared as (a) keyword argument(s)!"
                            : $"Only expected {Parameters.Length} unnamed argument(s)!",
                        _executionContext, StartPosition, EndPosition));
                    break;
                }
                else if (totalRequiredArguments <= calculatedParameterIndex && HasExtraPositionalArguments)
                {
                    extraPositionalArgumentReferences!.Add(reference);
                    continue;
                }

                string argumentName = Parameters[calculatedParameterIndex].Name;
                if (!argumentReferences.ContainsKey(argumentName))
                {
                    name = argumentName;
                    parameterIndex = calculatedParameterIndex;

                    argumentReferences.Add(name, reference);
                    calculatedParameterIndex++;
                }
                else
                {
                    calculatedParameterIndex++;
                    goto IndexCheck;
                }
            }

            if (parameterIndex > -1)
                if (flaggedRequiredArguments < 0)
                    flaggedRequiredArguments = IndexToFlag(parameterIndex);
                else
                    flaggedRequiredArguments |= IndexToFlag(parameterIndex);
        }

        for (int i = 0; i < Parameters.Length; i++)
        {
            int parameterFlag = IndexToFlag(i);
            if (Parameters[i].IsRequired && (flaggedRequiredArguments < 0 || (flaggedRequiredArguments & parameterFlag) != parameterFlag))
            {
                result.Failure(new EzrMissingRequiredArgumentError($"Expected required argument \"{Parameters[i].Name}\"!", _executionContext, StartPosition, EndPosition));
                return (argumentReferences, extraPositionalArgumentReferences);
            }
        }

        return (argumentReferences, extraPositionalArgumentReferences);
    }

    /// <inheritdoc/>
    public override void ComparisonEqual(IEzrObject other, RuntimeResult result)
    {
        bool equal = StrictEquals(other, result);
        if (result.ShouldReturn)
            return;

        result.Success(NewBooleanConstant(equal));
    }

    /// <inheritdoc/>
    public override void ComparisonNotEqual(IEzrObject other, RuntimeResult result)
    {
        bool equal = StrictEquals(other, result);
        if (result.ShouldReturn)
            return;

        result.Success(NewBooleanConstant(!equal));
    }

    /// <inheritdoc/>
    public override bool EvaluateBoolean(RuntimeResult result)
    {
        return true;
    }
}
