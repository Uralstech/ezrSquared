using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using EzrSquared.Runtime.Types.Core.Text;
using EzrSquared.Runtime.WrapperAttributes;
using System;

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
    ///         <term>message</term>
    ///         <description>(<see cref="IEzrObject"/>) The message to display on the console.</description>
    ///     </item>
    ///     <item>
    ///         <term>line_end</term>
    ///         <description>(Optional, <see cref="EzrString"/>, <see cref="EzrCharacterList"/>, <see cref="EzrCharacter"/>) Line end character(s) to use instead of \n.</description>
    ///     </item>
    /// </list>
    /// 
    /// ezr² return type:
    /// <see cref="EzrNothing"/>
    /// <br/>
    /// ezr² errors:
    /// <see cref="EzrUnexpectedTypeError"/> if "line_end" is not one of the specified types.
    /// </remarks>
    /// <param name="arguments">The constructor arguments.</param>
    [SharpMethodWrapper("show", RequiredParameters = ["message"], OptionalParameters = ["line_end"])]
    public static void Show(SharpMethodParameters arguments)
    {
        RuntimeResult result = arguments.Result;
        Reference messageReference = arguments.ArgumentReferences["message"];

        string message = messageReference.Object.ToPureString(result);
        if (result.ShouldReturn)
            return;

        string lineEnd = Environment.NewLine;
        if (arguments.ArgumentReferences.TryGetValue("line_end", out Reference? lineEndReference))
        {
            IEzrObject lineEndObject = lineEndReference.Object;
            if (lineEndObject is EzrString lineEndString)
                lineEnd = lineEndString.Value;
            else if (lineEndObject is EzrCharacter lineEndCharacter)
                lineEnd = lineEndCharacter.Value.ToString();
            else if (lineEndObject is EzrCharacterList lineEndCharacterList)
                lineEnd = lineEndCharacterList.StringValue;
            else
            {
                result.Failure(new EzrUnexpectedTypeError($"Expected line ending of type string, character or character list, but got object of type \"{lineEndObject.TypeName}\"", arguments.ExecutionContext, lineEndObject.StartPosition, lineEndObject.EndPosition));
                return;
            }
        }

        Console.Write($"{message}{lineEnd}");
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
    /// <param name="arguments">The constructor arguments.</param>
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
    /// <param name="arguments">The constructor arguments.</param>
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
    /// <param name="arguments">The constructor arguments.</param>
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
    /// <param name="arguments">The constructor arguments.</param>
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
    /// <param name="arguments">The constructor arguments.</param>
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
    /// <param name="arguments">The constructor arguments.</param>
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
    /// <param name="arguments">The constructor arguments.</param>
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
    /// <param name="arguments">The constructor arguments.</param>
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
    /// <param name="arguments">The constructor arguments.</param>
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
}
