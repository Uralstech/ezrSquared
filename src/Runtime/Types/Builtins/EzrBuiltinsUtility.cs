using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Wrappers;
using EzrSquared.Runtime.Types.Wrappers.Members.Methods;

namespace EzrSquared.Runtime.Types.Builtins;

/// <summary>
/// Utility to add built-ins to contexts.
/// </summary>
public static class EzrBuiltinsUtility
{
    /// <summary>
    /// Adds all built-in functions to the given context, excluding I/O functions.
    /// </summary>
    /// <param name="context">The context to add to.</param>
    public static void AddBuiltinFunctions(Context context)
    {
        EzrMethodWrapper throwError = new(EzrBuiltinFunctions.ThrowError, context, Position.None, Position.None);
        EzrMethodWrapper assert = new(EzrBuiltinFunctions.Assert, context, Position.None, Position.None);
        EzrMethodWrapper hash = new(EzrBuiltinFunctions.Hash, context, Position.None, Position.None);
        EzrMethodWrapper typeOf = new(EzrBuiltinFunctions.TypeOf, context, Position.None, Position.None);
        EzrMethodWrapper typeNameOf = new(EzrBuiltinFunctions.TypeNameOf, context, Position.None, Position.None);
        EzrMethodWrapper typeHashOf = new(EzrBuiltinFunctions.TypeHashOf, context, Position.None, Position.None);
        EzrMethodWrapper copy = new(EzrBuiltinFunctions.Copy, context, Position.None, Position.None);
        EzrMethodWrapper getRaw = new(EzrBuiltinFunctions.GetRaw, context, Position.None, Position.None);
        EzrMethodWrapper getContext = new(EzrBuiltinFunctions.GetContext, context, Position.None, Position.None);

        context.Set(null, throwError.SharpMemberName, ReferencePool.Get(throwError, AccessMod.Constant));
        context.Set(null, assert.SharpMemberName, ReferencePool.Get(assert, AccessMod.Constant));
        context.Set(null, hash.SharpMemberName, ReferencePool.Get(hash, AccessMod.Constant));
        context.Set(null, typeOf.SharpMemberName, ReferencePool.Get(typeOf, AccessMod.Constant));
        context.Set(null, typeNameOf.SharpMemberName, ReferencePool.Get(typeNameOf, AccessMod.Constant));
        context.Set(null, typeHashOf.SharpMemberName, ReferencePool.Get(typeHashOf, AccessMod.Constant));
        context.Set(null, copy.SharpMemberName, ReferencePool.Get(copy, AccessMod.Constant));
        context.Set(null, getRaw.SharpMemberName, ReferencePool.Get(getRaw, AccessMod.Constant));
        context.Set(null, getContext.SharpMemberName, ReferencePool.Get(getContext, AccessMod.Constant));
    }

    /// <summary>
    /// Adds all built-in I/O functions to the given context.
    /// </summary>
    /// <param name="context">The context to add to.</param>
    public static void AddBuiltinIOFunctions(Context context)
    {
        EzrMethodWrapper show = new(EzrBuiltinFunctions.Show, context, Position.None, Position.None);
        EzrMethodWrapper get = new(EzrBuiltinFunctions.Get, context, Position.None, Position.None);
        EzrMethodWrapper clear = new(EzrBuiltinFunctions.Clear, context, Position.None, Position.None);

        context.Set(null, show.SharpMemberName, ReferencePool.Get(show, AccessMod.Constant));
        context.Set(null, get.SharpMemberName, ReferencePool.Get(get, AccessMod.Constant));
        context.Set(null, clear.SharpMemberName, ReferencePool.Get(clear, AccessMod.Constant));
    }

    /// <summary>
    /// Adds all built-in types to the given context.
    /// </summary>
    /// <param name="context">The context to add to.</param>
    public static void AddBuiltinTypes(Context context)
    {
        EzrTypeWrapper runtimeError = new(typeof(EzrRuntimeError), context, Position.None, Position.None);
        context.Set(null, runtimeError.SharpMemberName, ReferencePool.Get(runtimeError, AccessMod.Constant));

        EzrTypeWrapper assertionError = new(typeof(EzrAssertionError), context, Position.None, Position.None);
        EzrTypeWrapper illegalOperationError = new(typeof(EzrIllegalOperationError), context, Position.None, Position.None);
        EzrTypeWrapper keyNotFoundError = new(typeof(EzrKeyNotFoundError), context, Position.None, Position.None);
        EzrTypeWrapper mathError = new(typeof(EzrMathError), context, Position.None, Position.None);
        EzrTypeWrapper missingRequiredArgumentError = new(typeof(EzrMissingRequiredArgumentError), context, Position.None, Position.None);
        EzrTypeWrapper privateMemberOperationError = new(typeof(EzrPrivateMemberOperationError), context, Position.None, Position.None);
        EzrTypeWrapper undefinedValueError = new(typeof(EzrUndefinedValueError), context, Position.None, Position.None);
        EzrTypeWrapper unexpectedArgumentError = new(typeof(EzrUnexpectedArgumentError), context, Position.None, Position.None);
        EzrTypeWrapper unexpectedTypeError = new(typeof(EzrUnexpectedTypeError), context, Position.None, Position.None);
        EzrTypeWrapper unsupportedWrappingError = new(typeof(EzrUnsupportedWrappingError), context, Position.None, Position.None);
        EzrTypeWrapper valueOutOfRangeError = new(typeof(EzrValueOutOfRangeError), context, Position.None, Position.None);
        EzrTypeWrapper wrapperExecutionError = new(typeof(EzrWrapperExecutionError), context, Position.None, Position.None);

        context.Set(null, assertionError.SharpMemberName, ReferencePool.Get(assertionError, AccessMod.Constant));
        context.Set(null, illegalOperationError.SharpMemberName, ReferencePool.Get(illegalOperationError, AccessMod.Constant));
        context.Set(null, keyNotFoundError.SharpMemberName, ReferencePool.Get(keyNotFoundError, AccessMod.Constant));
        context.Set(null, mathError.SharpMemberName, ReferencePool.Get(mathError, AccessMod.Constant));
        context.Set(null, missingRequiredArgumentError.SharpMemberName, ReferencePool.Get(missingRequiredArgumentError, AccessMod.Constant));
        context.Set(null, privateMemberOperationError.SharpMemberName, ReferencePool.Get(privateMemberOperationError, AccessMod.Constant));
        context.Set(null, undefinedValueError.SharpMemberName, ReferencePool.Get(undefinedValueError, AccessMod.Constant));
        context.Set(null, unexpectedArgumentError.SharpMemberName, ReferencePool.Get(unexpectedArgumentError, AccessMod.Constant));
        context.Set(null, unexpectedTypeError.SharpMemberName, ReferencePool.Get(unexpectedTypeError, AccessMod.Constant));
        context.Set(null, unsupportedWrappingError.SharpMemberName, ReferencePool.Get(unsupportedWrappingError, AccessMod.Constant));
        context.Set(null, valueOutOfRangeError.SharpMemberName, ReferencePool.Get(valueOutOfRangeError, AccessMod.Constant));
        context.Set(null, wrapperExecutionError.SharpMemberName, ReferencePool.Get(wrapperExecutionError, AccessMod.Constant));
    }

    /// <summary>
    /// Adds all built-in constants to the given context.
    /// </summary>
    /// <param name="context">The context to add to.</param>
    public static void AddBuiltinConstants(Context context)
    {
        context.Set(
            null,
            "true",
            ReferencePool.Get(EzrConstants.True, AccessMod.Constant));

        context.Set(
            null,
            "false",
            ReferencePool.Get(EzrConstants.False, AccessMod.Constant));

        context.Set(
            null,
            "nothing",
            ReferencePool.Get(EzrConstants.Nothing, AccessMod.Constant));
    }
}
