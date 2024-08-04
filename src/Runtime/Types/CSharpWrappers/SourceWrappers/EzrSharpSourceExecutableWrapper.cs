using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers;
using EzrSquared.Util;
using System;
using System.Collections.Generic;

namespace EzrSquared.Runtime.Types.CSharpWrappers.SourceWrappers;

public abstract class EzrSharpSourceExecutableWrapper : EzrSharpCompatibilityWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp source executable wrapper";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpSourceExecutableWrapper";

    public (string Name, bool IsRequired)[] Parameters;
    public bool HasKeywordArguments;

    public EzrSharpSourceExecutableWrapper(Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        Parameters = [];
    }

    protected internal Dictionary<string, Reference> CheckAndPopulateArguments(Reference[] arguments, RuntimeResult result)
    {
        int calculatedParameterIndex = 0;
        int requiredKeywordArguments = 0;
        int flaggedRequiredArguments = -1;

        Dictionary<string, Reference> argumentReferences = new(arguments.Length);

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
                    return argumentReferences;
                }

                bool found = Array.Find(Parameters, (v) =>
                {
                    parameterIndex++;
                    return v.Name == name;
                }) != default;

                if (found)
                {
                    argumentReferences.Add(name, reference);
                    requiredKeywordArguments++;
                }
                else if (HasKeywordArguments)
                {
                    argumentReferences.Add(name, reference);
                    parameterIndex = -1;
                }
                else
                {
                    result.Failure(new EzrUnexpectedArgumentError($"Did not expect argument \"{name}\"!", _executionContext, reference.Object.StartPosition, reference.Object.EndPosition));
                    return argumentReferences;
                }
            }
            else
            {
            IndexCheck:
                if (Parameters.Length <= calculatedParameterIndex)
                {
                    result.Failure(new EzrUnexpectedArgumentError(
                        requiredKeywordArguments > 0
                            ? $"Only expected {Parameters.Length - requiredKeywordArguments} unnamed argument(s) as {requiredKeywordArguments} required argument(s) has/have been declared as (a) keyword argument(s)!"
                            : $"Only expected {Parameters.Length} unnamed argument(s)!",
                        _executionContext, StartPosition, EndPosition));
                    break;
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
                    flaggedRequiredArguments = Utils.IndexToFlag(parameterIndex);
                else
                    flaggedRequiredArguments |= Utils.IndexToFlag(parameterIndex);
        }

        for (int i = 0; i < Parameters.Length; i++)
        {
            int parameterFlag = Utils.IndexToFlag(i);
            if (Parameters[i].IsRequired && (flaggedRequiredArguments < 0 || (flaggedRequiredArguments & parameterFlag) != parameterFlag))
            {
                result.Failure(new EzrMissingRequiredArgumentError($"Expected required argument \"{Parameters[i].Name}\"!", _executionContext, StartPosition, EndPosition));
                return argumentReferences;
            }
        }

        return argumentReferences;
    }
}
