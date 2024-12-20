using EzrSquared.Runtime.Collections;
using EzrSquared.Runtime.Types.Collections;
using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Text;
using EzrSquared.Runtime.Types.Wrappers;
using EzrSquared.Runtime.Types.Wrappers.Members.Methods;
using System;
using System.Collections.Generic;
using System.Text;

namespace EzrSquared.Runtime.Types.Builtins;

/// <summary>
/// All built-in functions in ezr².
/// </summary>
public static class EzrBuiltinFunctions
{
    /// <summary>
    /// Basic console print function. Implements <see cref="Console.WriteLine()"/>.
    /// </summary>
    /// <remarks>
    /// ezr² errors:
    /// <see cref="EzrMissingRequiredArgumentError"/> if no messages are provided.
    /// </remarks>
    /// <param name="lineEnd">Optional line end character(s) to use instead of <see cref="Environment.NewLine"/>.</param>
    /// <param name="separator">Optional separator to separate each message to be printed.</param>
    /// <param name="messages">The message(s) to display on the console.</param>
    /// <param name="wrapper">Reference to the caller.</param>
    /// <param name="result">Reference to the current <see cref="RuntimeResult"/>.</param>
    /// <param name="executionContext">Reference to the current execution context.</param>
    [WrapMember]
    public static void Show(
        [Parameter(true)] string lineEnd,
        [Parameter(true)] string separator,
        [FeatureParameter(Feature.PositionalArguments)] ExtraPositionalArguments messages,
        [FeatureParameter(Feature.CallerRef)] IEzrObject wrapper,
        [FeatureParameter(Feature.ResultRef)] RuntimeResult result,
        [FeatureParameter(Feature.ExecutionRef)] Context executionContext)
    {
        if (messages.Count == 0)
        {

            result.Failure(new EzrMissingRequiredArgumentError("At least one message must be provided!", executionContext, wrapper.StartPosition, wrapper.EndPosition));
            return;
        }

        separator ??= ", ";
        StringBuilder messageBuilder = new();
        int messagesCount = messages.Count;

        for (int i = 0; i < messagesCount; i++)
        {
            string messagePart = messages[i].ToPureString(result);
            if (result.ShouldReturn)
                return;

            messageBuilder.Append(messagePart);
            if (i < messagesCount - 1)
                messageBuilder.Append(separator);
        }

        Console.Write(messageBuilder.Append(lineEnd ?? Environment.NewLine).ToString());
    }

    /// <summary>
    /// Basic error throwing function. Implements <see cref="RuntimeResult.Failure(IEzrRuntimeError)"/>.
    /// </summary>
    /// <param name="error">The error to throw.</param>
    /// <param name="result">Reference to the current <see cref="RuntimeResult"/>.</param>
    [WrapMember]
    public static void ThrowError(IEzrRuntimeError error, [FeatureParameter(Feature.ResultRef)] RuntimeResult result)
    {
        result.Failure(error);
    }

    /// <summary>
    /// Basic console input function. Implements <see cref="Console.ReadLine()"/>.
    /// </summary>
    /// <param name="message">The message to display on the console before waiting for user input.</param>
    /// <param name="result">Reference to the current <see cref="RuntimeResult"/>.</param>
    /// <returns>The user's input.</returns>
    [WrapMember]
    public static string Get([Parameter(true)] IEzrObject? message, [FeatureParameter(Feature.ResultRef)] RuntimeResult result)
    {
        if (message is not null)
        {
            string messageStr = message.ToPureString(result);
            if (result.ShouldReturn)
                return string.Empty;

            Console.Write(messageStr);
        }

        return Console.ReadLine() ?? string.Empty;
    }

    /// <summary>
    /// Basic console clear function. Implements <see cref="Console.Clear()"/>.
    /// </summary>
    [WrapMember]
    public static void Clear()
    {
        Console.Clear();
    }

    /// <summary>
    /// Assertion function to assert conditions.
    /// </summary>
    /// <remarks>
    /// ezr² errors:
    /// <see cref="EzrAssertionError"/> if the condition is not met.
    /// </remarks>
    /// <param name="condition">The condition to assert.</param>
    /// <param name="wrapper">Reference to the caller.</param>
    /// <param name="result">Reference to the current <see cref="RuntimeResult"/>.</param>
    /// <param name="executionContext">Reference to the current execution context.</param>
    [WrapMember]
    public static void Assert(IEzrObject condition,
        [FeatureParameter(Feature.CallerRef)] IEzrObject wrapper,
        [FeatureParameter(Feature.ResultRef)] RuntimeResult result,
        [FeatureParameter(Feature.ExecutionRef)] Context executionContext)
    {
        bool conditionResult = condition.EvaluateBoolean(result);
        if (!result.ShouldReturn && !conditionResult)
            result.Failure(new EzrAssertionError(executionContext, wrapper.StartPosition, wrapper.EndPosition));
    }

    /// <summary>
    /// Basic hash function. Implements <see cref="IEzrObject.ComputeHashCode(RuntimeResult)"/>.
    /// </summary>
    /// <param name="toHash">The object to hash.</param>
    /// <param name="result">Reference to the current <see cref="RuntimeResult"/>.</param>
    /// <returns>The hash of the object.</returns>
    [WrapMember]
    public static int Hash(IEzrObject toHash, [FeatureParameter(Feature.ResultRef)] RuntimeResult result)
    {
        return toHash.ComputeHashCode(result);
    }

    /// <summary>
    /// Basic <see langword="typeof"/>-like function. Uses <see cref="IEzrObject.Tag"/>.
    /// </summary>
    /// <param name="toCheck">The object to check the type of.</param>
    /// <returns>The tag of the object.</returns>
    [WrapMember]
    public static string TypeOf(IEzrObject toCheck)
    {
        return toCheck.Tag;
    }

    /// <summary>
    /// Gets the plain text name of the type of an object. Uses <see cref="IEzrObject.TypeName"/>.
    /// </summary>
    /// <param name="toCheck">The object to check the type-name of.</param>
    /// <returns>The type name of the object.</returns>
    [WrapMember]
    public static string TypeNameOf(IEzrObject toCheck)
    {
        return toCheck.TypeName;
    }

    /// <summary>
    /// Gets the hash ID of the type of an object. Uses <see cref="IEzrObject.HashTag"/>.
    /// </summary>
    /// <param name="toCheck">The object to check the type hash of.</param>
    /// <returns>The hashed tag of the object.</returns>
    [WrapMember]
    public static int TypeHashOf(IEzrObject toCheck)
    {
        return toCheck.HashTag;
    }

    /// <summary>
    /// Creates a copy of an <see cref="IEzrMutableObject"/>. Uses <see cref="IMutable{T}.DeepCopy(RuntimeResult)"/>.
    /// </summary>
    /// <param name="toCopy">The object to copy.</param>
    /// <param name="result">Reference to the current <see cref="RuntimeResult"/>.</param>
    /// <returns>The copy of the object.</returns>
    [WrapMember]
    public static IEzrObject Copy(IEzrMutableObject toCopy, [FeatureParameter(Feature.ResultRef)] RuntimeResult result)
    {
        return (IEzrObject?)toCopy.DeepCopy(result) ?? EzrConstants.Nothing;
    }

    /// <summary>
    /// Wraps the given ezr² object so that the runtime has access to its raw C# object.
    /// </summary>
    /// <param name="toGet">The object to wrap.</param>
    /// <param name="wrapper">Reference to the caller.</param>
    /// <param name="executionContext">Reference to the current execution context.</param>
    /// <returns>The wrapped object.</returns>
    [WrapMember]
    public static EzrObjectWrapper GetRaw(IEzrObject toGet,
        [FeatureParameter(Feature.CallerRef)] IEzrObject wrapper,
        [FeatureParameter(Feature.ExecutionRef)] Context executionContext)
    {
        return new EzrObjectWrapper(toGet, toGet.GetType(), executionContext, wrapper.StartPosition, wrapper.EndPosition);
    }

    /// <summary>
    /// Returns an <see cref="EzrDictionary"/> of the references (as &lt;name, object&gt;) contained in the <see cref="Context"/> of the given object.
    /// </summary>
    /// <param name="toGet">The object to get the context of.</param>
    /// <param name="wrapper">Reference to the caller.</param>
    /// <param name="result">Reference to the current <see cref="RuntimeResult"/>.</param>
    /// <param name="executionContext">Reference to the current execution context.</param>
    /// <returns>The content of the object's context.</returns>
    [WrapMember]
    public static IEzrObject GetContext(IEzrObject toGet,
        [FeatureParameter(Feature.CallerRef)] IEzrObject wrapper,
        [FeatureParameter(Feature.ResultRef)] RuntimeResult result,
        [FeatureParameter(Feature.ExecutionRef)] Context executionContext)
    {
        RuntimeEzrObjectDictionary context = new();
        foreach (KeyValuePair<string, Reference> pair in toGet.Context)
        {
            if (pair.Value.AccessibilityModifiers.HasFlag(AccessMod.Private))
                continue;

            context.Update(new EzrString(pair.Key, executionContext, wrapper.StartPosition, wrapper.EndPosition), pair.Value.Object, result);
            if (result.ShouldReturn)
                return EzrConstants.Nothing;
        }

        return new EzrDictionary(context, executionContext, wrapper.StartPosition, wrapper.EndPosition);
    }
}
