using EzrSquared.Runtime.Collections;
using EzrSquared.Runtime.Types.Collections;
using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using EzrSquared.Runtime.Types.Core.Text;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers;
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
    [SharpMethodWrapper("show", HasExtraPositionalArguments = true, OptionalParameters = ["line_end", "separator"])]
    public static void Show(SharpMethodParameters arguments)
    {
        RuntimeResult result = arguments.Result;
        List<Reference> messageReferences = arguments.ExtraPositionalArgumentReferences!;

        if (messageReferences.Count == 0)
        {
            result.Failure(new EzrMissingRequiredArgumentError("At least one message must be provided!", arguments.ExecutionContext, arguments.StartPosition, arguments.EndPosition));
            return;
        }

        string separator = string.Empty;
        if (arguments.ArgumentReferences.TryGetValue("separator", out Reference? separatorReference))
        {
            IEzrObject separatorObject = separatorReference.Object;
            if (separatorObject is IEzrString separatorString)
                separator = separatorString.StringValue;
            else
            {
                result.Failure(new EzrUnexpectedTypeError($"Expected separator of type string, character or character list, but got object of type \"{separatorObject.TypeName}\"", arguments.ExecutionContext, separatorObject.StartPosition, separatorObject.EndPosition));
                return;
            }
        }

        StringBuilder messageBuilder = new();
        int messagesCount = messageReferences.Count;

        for (int i = 0; i < messagesCount; i++)
        {
            string messagePart = messageReferences[i].Object.ToPureString(result);
            if (result.ShouldReturn)
                return;

            messageBuilder.Append(messagePart);
            if (i < messagesCount - 1)
                messageBuilder.Append(separator);
        }

        string lineEnd = Environment.NewLine;
        if (arguments.ArgumentReferences.TryGetValue("line_end", out Reference? lineEndReference))
        {
            IEzrObject lineEndObject = lineEndReference.Object;
            if (lineEndObject is IEzrString lineEndString)
                lineEnd = lineEndString.StringValue;
            else
            {
                result.Failure(new EzrUnexpectedTypeError($"Expected line ending of type string, character or character list, but got object of type \"{lineEndObject.TypeName}\"", arguments.ExecutionContext, lineEndObject.StartPosition, lineEndObject.EndPosition));
                return;
            }
        }

        Console.Write(messageBuilder.Append(lineEnd).ToString());
        result.Success(ReferencePool.Get(EzrConstants.Nothing, AccessMod.PrivateConstant));
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
    [SharpMethodWrapper("throw_error", RequiredParameters = ["error"])]
    public static void ThrowError(SharpMethodParameters arguments)
    {
        Reference reference = arguments.ArgumentReferences["error"];

        IEzrObject referenceObject = reference.Object;
        if (referenceObject is not IEzrRuntimeError error)
            arguments.Result.Failure(new EzrUnexpectedTypeError($"Expected runtime error, but got object of type \"{referenceObject.TypeName}\"!", arguments.ExecutionContext, referenceObject.StartPosition, referenceObject.EndPosition));
        else
            arguments.Result.Failure(error);
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
    [SharpMethodWrapper("get", OptionalParameters = ["message"])]
    public static void Get(SharpMethodParameters arguments)
    {
        RuntimeResult result = arguments.Result;
        if (arguments.ArgumentReferences.TryGetValue("message", out Reference? messageReference))
        {
            string message = messageReference.Object.ToPureString(result);
            if (result.ShouldReturn)
                return;

            Console.Write(message);
        }

        result.Success(ReferencePool.Get(new EzrString(Console.ReadLine() ?? string.Empty, arguments.ExecutionContext, arguments.StartPosition, arguments.EndPosition), AccessMod.PrivateConstant));
    }

    /// <summary>
    /// Basic console clear function. Implements <see cref="Console.Clear()"/>.
    /// </summary>
    /// <remarks>
    /// ezr² return type:
    /// <see cref="EzrNothing"/>
    /// </remarks>
    /// <param name="arguments">The method arguments.</param>
    [SharpMethodWrapper("clear")]
    public static void Clear(SharpMethodParameters arguments)
    {
        Console.Clear();
        arguments.Result.Success(ReferencePool.Get(EzrConstants.Nothing, AccessMod.PrivateConstant));
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
    [SharpMethodWrapper("assert", RequiredParameters = ["condition"])]
    public static void Assert(SharpMethodParameters arguments)
    {
        RuntimeResult result = arguments.Result;
        IEzrObject condition = arguments.ArgumentReferences["condition"].Object;

        bool conditionResult = condition.EvaluateBoolean(result);
        if (result.ShouldReturn)
            return;

        if (!conditionResult)
        {
            result.Failure(new EzrAssertionError(arguments.ExecutionContext, arguments.StartPosition, arguments.EndPosition));
            return;
        }

        result.Success(ReferencePool.Get(EzrConstants.Nothing, AccessMod.PrivateConstant));
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
    [SharpMethodWrapper("hash", RequiredParameters = ["to_hash"])]
    public static void Hash(SharpMethodParameters arguments)
    {
        RuntimeResult result = arguments.Result;
        Reference reference = arguments.ArgumentReferences["to_hash"];

        int hash = reference.Object.ComputeHashCode(result);
        if (result.ShouldReturn)
            return;

        result.Success(ReferencePool.Get(new EzrInteger(hash, arguments.ExecutionContext, arguments.StartPosition, arguments.EndPosition), AccessMod.PrivateConstant));
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
    [SharpMethodWrapper("type_of", RequiredParameters = ["to_check"])]
    public static void TypeOf(SharpMethodParameters arguments)
    {
        arguments.Result.Success(ReferencePool.Get(
            new EzrString(
                arguments.ArgumentReferences["to_check"].Object.Tag,
                arguments.ExecutionContext,
                arguments.StartPosition,
                arguments.EndPosition),
            AccessMod.PrivateConstant));
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
    [SharpMethodWrapper("type_name_of", RequiredParameters = ["to_check"])]
    public static void TypeNameOf(SharpMethodParameters arguments)
    {
        arguments.Result.Success(ReferencePool.Get(
            new EzrString(
                arguments.ArgumentReferences["to_check"].Object.TypeName,
                arguments.ExecutionContext,
                arguments.StartPosition,
                arguments.EndPosition),
            AccessMod.PrivateConstant));
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
    [SharpMethodWrapper("type_hash_of", RequiredParameters = ["to_check"])]
    public static void TypeHashOf(SharpMethodParameters arguments)
    {
        arguments.Result.Success(ReferencePool.Get(
            new EzrInteger(
                arguments.ArgumentReferences["to_check"].Object.HashTag,
                arguments.ExecutionContext,
                arguments.StartPosition,
                arguments.EndPosition),
            AccessMod.PrivateConstant));
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
    [SharpMethodWrapper("copy", RequiredParameters = ["to_copy"])]
    public static void Copy(SharpMethodParameters arguments)
    {
        RuntimeResult result = arguments.Result;
        IEzrObject objectToCopy = arguments.ArgumentReferences["to_copy"].Object;
        if (objectToCopy is not IEzrMutableObject mutableObject)
        {
            result.Failure(new EzrUnexpectedTypeError($"Cannot create copy of immutable object of type \"{objectToCopy.TypeName}\"!", arguments.ExecutionContext, objectToCopy.StartPosition, objectToCopy.EndPosition));
            return;
        }

        IEzrObject? copy = (IEzrObject?)mutableObject.DeepCopy(result);
        if (result.ShouldReturn)
            return;

        result.Success(ReferencePool.Get(copy, AccessMod.PrivateConstant));
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
    [SharpMethodWrapper("get_raw", RequiredParameters = ["to_wrap"])]
    public static void GetRaw(SharpMethodParameters arguments)
    {
        RuntimeResult result = arguments.Result;
        IEzrObject objectToWrap = arguments.ArgumentReferences["to_wrap"].Object;

        IEzrObject wrapped = new EzrSharpCompatibilityObjectInstance(objectToWrap, objectToWrap.GetType(), arguments.ExecutionContext, arguments.StartPosition, arguments.EndPosition);
        result.Success(ReferencePool.Get(wrapped, AccessMod.PrivateConstant));
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
    [SharpMethodWrapper("get_context", RequiredParameters = ["to_get"])]
    public static void GetContext(SharpMethodParameters arguments)
    {
        RuntimeResult result = arguments.Result;
        IEzrObject objectToWrap = arguments.ArgumentReferences["to_get"].Object;

        RuntimeEzrObjectDictionary context = new();
        foreach (KeyValuePair<string, Reference> pair in objectToWrap.Context)
        {
            if (pair.Value.AccessibilityModifiers.HasFlag(AccessMod.Private))
                continue;

            context.Update(new EzrString(pair.Key, arguments.ExecutionContext, arguments.StartPosition, arguments.EndPosition), pair.Value.Object, result);
            if (result.ShouldReturn)
                return;
        }

        IEzrObject dictionary = new EzrDictionary(context, arguments.ExecutionContext, arguments.StartPosition, arguments.EndPosition);
        result.Success(ReferencePool.Get(dictionary, AccessMod.PrivateConstant));
    }
}
