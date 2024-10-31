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

        // Index for iterating through the ParameterNames array.
        int parameterIndex = 0;

        // Index for iterating through the arguments.
        int argumentIndex = 0;

        while (argumentIndex < arguments.Length)
        {
            // Get the argument. Reference and object.
            Reference argument = arguments[argumentIndex];
            IEzrObject argumentObject = argument.Object;

            // Update the object's context.
            argumentObject.Update(context, argumentObject.StartPosition, argumentObject.EndPosition);

            // Is the parameterIndex in the bounds of the dictionary?
            bool isValidIndex = parameterIndex < Parameters.Length;

            // Is this a keyword argument? As in, provided like name: value.
            bool isKeywordArgument = !string.IsNullOrEmpty(argument.Name) && !argument.IsRegistered;

            // The name of the argument as in Parameters or the given name.
            string key = isValidIndex ? Parameters[parameterIndex].Name : string.Empty;

            if (isKeywordArgument)
            {
                // If it is a keyword argument, and it exists in Parameters:
                if (Array.FindIndex(Parameters, paramter => paramter.Name == argument.Name) != -1)
                    key = argument.Name; // Choose that as the key.
                else if (extraKeywordArguments is not null) // If it doesn't, and extra keyword arguments are allowed:
                {
                    // Add the argument to the dictionary, and continue onto the next argument.
                    extraKeywordArguments.Update(new EzrString(argument.Name, context, argumentObject.StartPosition, argumentObject.EndPosition), argumentObject, result);

                    argumentIndex++;
                    continue;
                }
                else if (!ignoreExtraArguments) // If extra keyword arguments are not allowed, and ignoreExtraArguments is not set:
                {
                    // Return an error.
                    result.Failure(new EzrUnexpectedArgumentError($"Did not expect argument \"{argument.Name}\"!", _executionContext, argumentObject.StartPosition, argumentObject.EndPosition));
                    return;
                }
                else // Otherwise, if ignoreExtraArguments is set, continue onto the next argument.
                {
                    argumentIndex++;
                    continue;
                }
            }

            // Does the key already exist?
            bool hasKey = isValidIndex && context.IsDefined(key);

            // If parameterIndex is out of bounds, or the key is already defined and the argument is a keyword argument, or if all the arguments have already been set:
            if (!isValidIndex || (hasKey && (isKeywordArgument || parameterIndex + 1 >= Parameters.Length)))
            {
                // Return an error.
                result.Failure(new EzrUnexpectedArgumentError(
                    isKeywordArgument
                        ? $"Did not expect argument \"{key}\", as it is already specified!"
                        : "Did not expect any more arguments!",
                    _executionContext, argumentObject.StartPosition, argumentObject.EndPosition));
                return;
            }

            // Otherwise, if it is not a keyword argument:
            if (!isKeywordArgument)
                parameterIndex++; // Add to parameterIndex.

            // If the argument is already defined, and it's not a keyword argument, then go to the next iteration while staying on the same argument.
            // The parameterIndex will be added to again, and get onto the next required parameter.
            if (hasKey)
                continue;

            // After all that, set it to the context.
            context.Set(null, key, ReferencePool.Get(argumentObject, AccessMod.Private));

            // Add to the argument index.
            argumentIndex++;
        }

        // Go through all the parameters.
        for (int i = parameterIndex; i < Parameters.Length; i++)
        {
            (string parameter, Node parameterNode) = Parameters[i];

            // Check if it is defined:
            if (context.IsDefined(parameter))
                continue; // If so, continue to the next.

            // The accessibility modifiers for the execution; if it is a simple access node, the modifiers should be local-only.
            AccessMod operationAccessibilityModifiers = parameterNode is VariableAccessNode ? AccessMod.LocalScope : AccessMod.None;

            // Otherwise, execute the parameter source code.
            interpreter.VisitNode(parameterNode, context, null, operationAccessibilityModifiers, true);
            if (result.ShouldReturn) // Check for errors and return if necessary.
                return;

            // If the result is empty, i.e. there is no default value:
            if (result.Reference.IsEmpty)
            {
                // Return an error.
                result.Failure(new EzrMissingRequiredArgumentError($"Expected required argument \"{parameter}\"!", _executionContext, StartPosition, EndPosition));
                return;
            }

            // Otherwise, if it is still not defined due to strange assignment practises by the programmer:
            if (!context.IsDefined(parameter))
                context.Set(null, parameter, ReferencePool.Get(result.Reference.Object, AccessMod.Private)); // Set it.
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
