global using OptionalExtraArguments = (EzrSquared.Position StartPosition, EzrSquared.Position EndPosition, string Name)?;

using EzrSquared.Runtime.Collections;
using EzrSquared.Runtime.Nodes;
using EzrSquared.Runtime.Types.Collections;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Text;
using EzrSquared.Util;
using System;

namespace EzrSquared.Runtime.Types.Executables;

/// <summary>
/// The base root class of all runtime executables.
/// </summary>
public abstract class EzrRuntimeExecutable : EzrObject
{
    /// <summary>
    /// The name of the executable.
    /// </summary>
    public readonly string ExecutableName;

    /// <summary>
    /// Is the executable anonymous?
    /// </summary>
    public readonly bool IsAnonymous;

    /// <summary>
    /// The source code body of the executable.
    /// </summary>
    public readonly Node Body;

    /// <summary>
    /// The name and source code of the executable's parameters and their default values.
    /// </summary>
    public (string Name, Node Node)[] Parameters { get; internal protected set; }

    /// <summary>
    /// The position in source code and name of the variable for the executable's extra keyword arguments.
    /// </summary>
    public OptionalExtraArguments ExtraKeywordArguments { get; internal protected set; }

    /// <summary>
    /// The position in source code and name of the variable for the executable's extra positional arguments.
    /// </summary>
    public OptionalExtraArguments ExtraPositionalArguments { get; internal protected set; }

    /// <summary>
    /// Creates a new executable object.
    /// </summary>
    /// <param name="name">The name of the executable.</param>
    /// <param name="body">The source code body of the executable.</param>
    /// <param name="parameters">The source code of the executable's parameters and their default values.</param>
    /// <param name="extraKeywordArguments">The position in source code and name of the variable for the executable's extra keyword arguments.</param>
    /// <param name="extraPositionalArguments">The position in source code and name of the variable for the executable's extra positional arguments.</param>
    /// <param name="initializationContext">The internal context, if <see langword="null"/>, creates a new one.</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrRuntimeExecutable(string? name, Node body, (string Name, Node Node)[] parameters, OptionalExtraArguments extraKeywordArguments, OptionalExtraArguments extraPositionalArguments, Context parentContext, Position startPosition, Position endPosition, Context? initializationContext = null) : base(initializationContext, parentContext, startPosition, endPosition)
    {
        Tag = name is not null
            ? $"{Tag}.{name}.{Utils.GetNextUniqueId()}"
            : $"{Tag}.{Utils.GetNextUniqueId()}";
        ExecutableName = name ?? $"<anonymous #{HashTag}>";

        Body = body;
        Parameters = parameters;
        ExtraKeywordArguments = extraKeywordArguments;
        ExtraPositionalArguments = extraPositionalArguments;

        IsAnonymous = string.IsNullOrEmpty(name);
    }

    /// <summary>
    /// Checks if the given array of arguments conform to the executables parameters, and populates them in the given context.
    /// </summary>
    /// <param name="arguments">The arguments to check.</param>
    /// <param name="context">The context to populate</param>
    /// <param name="interpreter">The interpreter for executing parameter default values.</param>
    /// <param name="result">Runtime result for carrying any errors.</param>
    /// <param name="ignoreExtraArguments">Should the checker ignore extra arguments?</param>
    protected internal void CheckAndPopulateArguments(Reference[] arguments, Context context, Interpreter interpreter, RuntimeResult result, bool ignoreExtraArguments)
    {
        // Create dictionary or list to store extra arguments, if allowed.
        RuntimeEzrObjectDictionary? extraKeywordArguments = ExtraKeywordArguments.HasValue ? new() : null;
        RuntimeEzrObjectList? extraPositionalArguments = ExtraPositionalArguments.HasValue ? new() : null;

        int currentIndexThroughParameters = 0;
        for (int i = 0; i < arguments.Length; i++)
        {
            // Get the argument. Reference and object.
            Reference argument = arguments[i];
            IEzrObject argumentObject = argument.Object;

            // Update the argument's context.
            argumentObject.Update(context, argumentObject.StartPosition, argumentObject.EndPosition);

            // Is the argument a keyword argument? As in, defined as name: value.
            bool isKeywordArgument = !string.IsNullOrEmpty(argument.Name) && !argument.IsRegistered;

            // The name of the argument as in Parameters or the given name.
            string parameterName = currentIndexThroughParameters < Parameters.Length ? Parameters[currentIndexThroughParameters].Name : string.Empty;

            // Handle keyword arguments.
            if (isKeywordArgument)
            {
                string keywordArgumentName = argument.Name;

                // Check if there is a parameter with the same name as the keyword argument:
                if (keywordArgumentName == parameterName || Array.Exists(Parameters, param => keywordArgumentName == param.Name))
                    parameterName = keywordArgumentName; // If so, fine.
                else if (extraKeywordArguments is not null) // Otherwise, if extra keyword arguments (EKAs) are allowed:
                {
                    // Add it to the EKA dictionary.
                    extraKeywordArguments.Update(new EzrString(keywordArgumentName, context, argumentObject.StartPosition, argumentObject.EndPosition), argumentObject, result);

                    // Continue onto the next argument.
                    ReferencePool.TryRelease(argument);
                    continue;
                }
                else if (!ignoreExtraArguments) // Otherwise still, if extra arguments should not be ignored:
                {
                    // Throw an error.
                    result.Failure(new EzrUnexpectedArgumentError($"Did not expect argument \"{argument.Name}\"!", _executionContext, argumentObject.StartPosition, argumentObject.EndPosition));
                    return;
                }
                else // Otherwise, skip this argument.
                {
                    ReferencePool.TryRelease(argument);
                    continue;
                }
            }

            // Has the current parameter been defined?
            bool parameterAlreadyDefined = context.IsDefined(parameterName);

            // If the argument is not a kwarg and there are no more parameters to define and EPAs are allowed:
            if (!isKeywordArgument && currentIndexThroughParameters >= Parameters.Length && extraPositionalArguments is not null)
            {
                // Create a new reference.
                Reference newReference = ReferencePool.Get(argumentObject);
                newReference.UpdateRegister(true);

                // And add it to the EPA list.
                extraPositionalArguments.Add(newReference);
                ReferencePool.TryRelease(argument);

                continue;
            }
            else if ((parameterAlreadyDefined && isKeywordArgument) || currentIndexThroughParameters >= Parameters.Length)
            {
                // Return an error.
                result.Failure(new EzrUnexpectedArgumentError(
                    isKeywordArgument
                        ? $"Did not expect argument \"{parameterName}\", as it is already specified!"
                        : "Did not expect any more positional arguments!",
                    _executionContext, argumentObject.StartPosition, argumentObject.EndPosition));

                return;
            }

            // Otherwise, if this is / corresponds to a positional argument previously undefined:
            if (!isKeywordArgument || (Parameters[currentIndexThroughParameters].Name == parameterName))
                currentIndexThroughParameters++; // Add to currentIndexThroughParameters!

            // After all that, set the argument to the context.
            context.Set(null, parameterName, ReferencePool.Get(argumentObject, AccessMod.Private));
            ReferencePool.TryRelease(argument);
        }

        // Go through all the leftover parameters.
        for (int i = currentIndexThroughParameters; i < Parameters.Length; i++)
        {
            (string parameterName, Node parameterCode) = Parameters[i];

            // Check if it is defined as a kwarg:
            if (context.IsDefined(parameterName))
                continue; // If so, continue to the next.

            // The accessibility modifiers for the execution.
            AccessMod operationAccessibilityModifiers = AccessMod.None;
            if (parameterCode is VariableAccessNode vaNode)
            {
                operationAccessibilityModifiers = (vaNode.AccessibilityModifiers & AccessMod.Global) != AccessMod.Global
                                                    ? vaNode.AccessibilityModifiers |= AccessMod.LocalScope
                                                    : vaNode.AccessibilityModifiers;
            }

            // Execute the parameter source code!
            interpreter.VisitNode(parameterCode, context, null, operationAccessibilityModifiers, true);
            if (result.ShouldReturn) // Check for errors and return if necessary.
                return;

            // If the result is empty, i.e. there is no default value:
            if (result.Reference.IsEmpty)
            {
                // Return an error.
                result.Failure(new EzrMissingRequiredArgumentError($"Expected required argument \"{parameterName}\"!", _executionContext, StartPosition, EndPosition));
                return;
            }

            // Otherwise, if it is still not defined due to strange assignment practises by the programmer:
            if (!context.IsDefined(parameterName))
                context.Set(null, parameterName, ReferencePool.Get(result.Reference.Object, AccessMod.Private)); // Set it.
        }

        // If the extra keyword arguments dictionary should be set:
        if (ExtraKeywordArguments.HasValue)
            context.Set(null, ExtraKeywordArguments.Value.Name, ReferencePool.Get(new EzrDictionary(extraKeywordArguments!, context, ExtraKeywordArguments.Value.StartPosition, ExtraKeywordArguments.Value.EndPosition), AccessMod.Private)); // Set it.

        // If the extra positional arguments list should be set:
        if (ExtraPositionalArguments.HasValue)
            context.Set(null, ExtraPositionalArguments.Value.Name, ReferencePool.Get(new EzrList(extraPositionalArguments!, context, ExtraPositionalArguments.Value.StartPosition, ExtraPositionalArguments.Value.EndPosition), AccessMod.Private)); // Set it.
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

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashTag;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        static string ToEnabledOrDisabled(bool enabled)
        {
            return enabled ? "enabled" : "disabled";
        }

        string[] parameterNames = Array.ConvertAll(Parameters, parameter => parameter.Name);
        return (Parameters.Length > 0, IsAnonymous) switch
        {
            (true, true) => $"<{TypeName} {ExecutableName}, with \"{string.Join("\", \"", parameterNames)}\", extra keyword arguments {ToEnabledOrDisabled(ExtraKeywordArguments.HasValue)} and extra positional arguments {ToEnabledOrDisabled(ExtraPositionalArguments.HasValue)}>",
            (true, false) => $"<{TypeName} \"{ExecutableName}\", with \"{string.Join("\", \"", parameterNames)}\", extra keyword arguments {ToEnabledOrDisabled(ExtraKeywordArguments.HasValue)} and extra positional arguments {ToEnabledOrDisabled(ExtraPositionalArguments.HasValue)}>",

            (false, true) => $"<{TypeName} {ExecutableName} with extra keyword arguments {ToEnabledOrDisabled(ExtraKeywordArguments.HasValue)} and extra positional arguments {ToEnabledOrDisabled(ExtraPositionalArguments.HasValue)}>",
            (false, false) => $"<{TypeName} \"{ExecutableName}\" with extra keyword arguments {ToEnabledOrDisabled(ExtraKeywordArguments.HasValue)} and extra positional arguments {ToEnabledOrDisabled(ExtraPositionalArguments.HasValue)}>",
        };
    }
}
