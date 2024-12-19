using EzrSquared.Runtime.Collections;
using EzrSquared.Runtime.Types.Collections;
using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using EzrSquared.Runtime.Types.Core.Text;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.Attributes;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables.Attributes;
using EzrSquared.Runtime.WrapperAttributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EzrSquared.Runtime.Types.CSharpWrappers.Builtins;

/// <summary>
/// All built-in functions in ezr².
/// </summary>
public static class EzrBuiltinFunctions
{
    /// <summary>
    /// Basic console print function. Implements <see cref="Console.WriteLine()"/>.
    /// </summary>
    /// <remarks>
    /// ezr² parameters:
    /// <list type="table">
    ///     <item>
    ///         <term>Extra Positional Arguments</term>
    ///         <description>(<see cref="List{T}"/> of <see cref="Reference"/>s) The message(s) to display on the console.</description>
    ///     </item>
    ///     <item>
    ///         <term>line_end</term>
    ///         <description>(Optional, <see cref="IEzrString"/>) Line end character(s) to use instead of \n.</description>
    ///     </item>
    ///     <item>
    ///         <term>separator</term>
    ///         <description>(Optional, <see cref="IEzrString"/>) separator to separate each message to be printed.</description>
    ///     </item>
    /// </list>
    /// 
    /// ezr² return type:
    /// <see cref="EzrNothing"/>
    /// <br/>
    /// ezr² errors:
    /// <list type="table">
    ///     <item>
    ///         <term><see cref="EzrMissingRequiredArgumentError"/></term>
    ///         <description>Thrown if no messages are provided.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="EzrUnexpectedTypeError"/></term>
    ///         <description>Thrown if "line_end" or "separator" are not of the specified types.</description>
    ///     </item>
    /// </list>
    /// </remarks>
    /// <param name="arguments">The method arguments.</param>
    [WrappedMember]
    public static void Show(
        [Expose(true)] string lineEnd,
        [Expose(true)] string separator,
        [Runtime(Feature.PositionalArguments)] ExtraPositionalArguments messages,
        [Runtime(Feature.CallerRef)] IEzrObject wrapper,
        [Runtime(Feature.ResultRef)] RuntimeResult result,
        [Runtime(Feature.ExecutionRef)] Context executionContext)
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
    /// <remarks>
    /// ezr² parameters:
    /// <list type="table">
    ///     <item>
    ///         <term>error</term>
    ///         <description>(<see cref="IEzrRuntimeError"/>) The error to throw.</description>
    ///     </item>
    /// </list>
    /// 
    /// ezr² errors:
    /// <list type="table">
    ///     <item>
    ///         <term><see cref="EzrUnexpectedTypeError"/></term>
    ///         <description>if "error" is not of the specified type.</description>
    ///     </item>
    ///     <item>
    ///         <term>error</term>
    ///         <description>the given error.</description>
    ///     </item>
    /// </list>
    /// </remarks>
    /// <param name="arguments">The method arguments.</param>
    [WrappedMember]
    public static void ThrowError(IEzrRuntimeError error, [Runtime(Feature.ResultRef)] RuntimeResult result)
    {
        result.Failure(error);
    }

    /// <summary>
    /// Basic console input function. Implements <see cref="Console.ReadLine()"/>.
    /// </summary>
    /// <remarks>
    /// ezr² parameters:
    /// <list type="table">
    ///     <item>
    ///         <term>message</term>
    ///         <description>(<see cref="IEzrObject"/>) The message to display on the console before waiting for user input.</description>
    ///     </item>
    /// </list>
    /// 
    /// ezr² return type:
    /// <see cref="EzrString"/>
    /// </remarks>
    /// <param name="arguments">The method arguments.</param>
    [WrappedMember]
    public static string Get([Expose(true)] IEzrObject? message, [Runtime(Feature.ResultRef)] RuntimeResult result)
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
    /// <remarks>
    /// ezr² return type:
    /// <see cref="EzrNothing"/>
    /// </remarks>
    /// <param name="arguments">The method arguments.</param>
    [WrappedMember]
    public static void Clear()
    {
        Console.Clear();
    }

    /// <summary>
    /// Assertion function to assert conditions.
    /// </summary>
    /// <remarks>
    /// ezr² parameters:
    /// <list type="table">
    ///     <item>
    ///         <term>condition</term>
    ///         <description>(<see cref="IEzrObject"/>) The condition to assert.</description>
    ///     </item>
    /// </list>
    /// 
    /// ezr² return type:
    /// <see cref="EzrNothing"/>
    /// <br/>
    /// ezr² errors:
    /// <see cref="EzrAssertionError"/> if the condition is not met.
    /// </remarks>
    /// <param name="arguments">The method arguments.</param>
    [WrappedMember]
    public static void Assert(IEzrObject condition,
        [Runtime(Feature.CallerRef)] IEzrObject wrapper,
        [Runtime(Feature.ResultRef)] RuntimeResult result,
        [Runtime(Feature.ExecutionRef)] Context executionContext)
    {
        bool conditionResult = condition.EvaluateBoolean(result);
        if (!result.ShouldReturn && !conditionResult)
            result.Failure(new EzrAssertionError(executionContext, wrapper.StartPosition, wrapper.EndPosition));
    }

    /// <summary>
    /// Basic hash function. Implements <see cref="IEzrObject.ComputeHashCode(RuntimeResult)"/>.
    /// </summary>
    /// <remarks>
    /// ezr² parameters:
    /// <list type="table">
    ///     <item>
    ///         <term>to_hash</term>
    ///         <description>(<see cref="IEzrObject"/>) The object to hash.</description>
    ///     </item>
    /// </list>
    /// 
    /// ezr² return type:
    /// <see cref="EzrInteger"/>
    /// </remarks>
    /// <param name="arguments">The method arguments.</param>
    [WrappedMember]
    public static int Hash(IEzrObject toHash, [Runtime(Feature.ResultRef)] RuntimeResult result)
    {
        return toHash.ComputeHashCode(result);
    }

    /// <summary>
    /// Basic <see langword="typeof"/>-like function. Uses <see cref="IEzrObject.Tag"/>.
    /// </summary>
    /// <remarks>
    /// ezr² parameters:
    /// <list type="table">
    ///     <item>
    ///         <term>to_check</term>
    ///         <description>(<see cref="IEzrObject"/>) The object to check the type of.</description>
    ///     </item>
    /// </list>
    /// 
    /// ezr² return type:
    /// <see cref="EzrString"/>
    /// </remarks>
    /// <param name="arguments">The method arguments.</param>
    [WrappedMember]
    public static string TypeOf(IEzrObject toCheck)
    {
        return toCheck.Tag;
    }

    /// <summary>
    /// Gets the plain text name of the type of an object. Uses <see cref="IEzrObject.TypeName"/>.
    /// </summary>
    /// <remarks>
    /// ezr² parameters:
    /// <list type="table">
    ///     <item>
    ///         <term>to_check</term>
    ///         <description>(<see cref="IEzrObject"/>) The object to check the type-name of.</description>
    ///     </item>
    /// </list>
    /// 
    /// ezr² return type:
    /// <see cref="EzrString"/>
    /// </remarks>
    /// <param name="arguments">The method arguments.</param>
    [WrappedMember]
    public static string TypeNameOf(IEzrObject toCheck)
    {
        return toCheck.TypeName;
    }

    /// <summary>
    /// Gets the hash ID of the type of an object. Uses <see cref="IEzrObject.HashTag"/>.
    /// </summary>
    /// <remarks>
    /// ezr² parameters:
    /// <list type="table">
    ///     <item>
    ///         <term>to_check</term>
    ///         <description>(<see cref="IEzrObject"/>) The object to check the type hash of.</description>
    ///     </item>
    /// </list>
    /// 
    /// ezr² return type:
    /// <see cref="EzrString"/>
    /// </remarks>
    /// <param name="arguments">The method arguments.</param>
    [WrappedMember]
    public static int TypeHashOf(IEzrObject toCheck)
    {
        return toCheck.HashTag;
    }

    /// <summary>
    /// Creates a copy of an <see cref="IEzrMutableObject"/>. Uses <see cref="IMutable{T}.DeepCopy(RuntimeResult)"/>.
    /// </summary>
    /// <remarks>
    /// ezr² parameters:
    /// <list type="table">
    ///     <item>
    ///         <term>to_copy</term>
    ///         <description>(<see cref="IEzrMutableObject"/>) The object to copy.</description>
    ///     </item>
    /// </list>
    /// 
    /// ezr² return type:
    /// <see cref="IEzrMutableObject"/>
    /// 
    /// ezr² errors:
    /// <see cref="EzrUnexpectedTypeError"/> if "to_copy" is not of the expected type.
    /// </remarks>
    /// <param name="arguments">The method arguments.</param>
    [WrappedMember]
    public static IEzrObject Copy(IEzrMutableObject toCopy, [Runtime(Feature.ResultRef)] RuntimeResult result)
    {
        return (IEzrObject?)toCopy.DeepCopy(result) ?? EzrConstants.Nothing;
    }

    /// <summary>
    /// Wraps the given ezr² object so that the runtime has access to its raw C# object.
    /// </summary>
    /// <remarks>
    /// ezr² parameters:
    /// <list type="table">
    ///     <item>
    ///         <term>to_wrap</term>
    ///         <description>(<see cref="IEzrObject"/>) The object to wrap.</description>
    ///     </item>
    /// </list>
    /// 
    /// ezr² return type:
    /// <see cref="IEzrObject"/>
    /// </remarks>
    /// <param name="arguments">The method arguments.</param>
    [WrappedMember]
    public static EzrSharpCompatibilityObjectInstance GetRaw(IEzrObject toGet,
        [Runtime(Feature.CallerRef)] IEzrObject wrapper,
        [Runtime(Feature.ExecutionRef)] Context executionContext)
    {
        return new EzrSharpCompatibilityObjectInstance(toGet, toGet.GetType(), executionContext, wrapper.StartPosition, wrapper.EndPosition);
    }

    /// <summary>
    /// Returns an <see cref="EzrDictionary"/> of the references (as &lt;name, object&gt;) contained in the <see cref="Context"/> of the given object.
    /// </summary>
    /// <remarks>
    /// ezr² parameters:
    /// <list type="table">
    ///     <item>
    ///         <term>to_get</term>
    ///         <description>(<see cref="IEzrObject"/>) The object to get the context of.</description>
    ///     </item>
    /// </list>
    /// 
    /// ezr² return type:
    /// <see cref="EzrDictionary"/>
    /// </remarks>
    /// <param name="arguments">The method arguments.</param>
    [WrappedMember]
    public static IEzrObject GetContext(IEzrObject toGet,
        [Runtime(Feature.CallerRef)] IEzrObject wrapper,
        [Runtime(Feature.ResultRef)] RuntimeResult result,
        [Runtime(Feature.ExecutionRef)] Context executionContext)
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
