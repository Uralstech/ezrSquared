using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.CSharpWrappers.SourceWrappers;

namespace EzrSquared.Runtime.Types.CSharpWrappers.Builtins;

/// <summary>
/// Utility to add built-ins to contexts.
/// </summary>
public static class EzrBuiltinsUtility
{
    /// <summary>
    /// Adds all built-in functions to the given context.
    /// </summary>
    /// <param name="context">The context to add to.</param>
    public static void AddBuiltinFunctions(Context context)
    {
        EzrSharpSourceFunctionWrapper show = new(EzrBuiltinFunctions.Show, context, Position.None, Position.None);
        EzrSharpSourceFunctionWrapper throwError = new(EzrBuiltinFunctions.ThrowError, context, Position.None, Position.None);
        EzrSharpSourceFunctionWrapper get = new(EzrBuiltinFunctions.Get, context, Position.None, Position.None);
        EzrSharpSourceFunctionWrapper clear = new(EzrBuiltinFunctions.Clear, context, Position.None, Position.None);
        EzrSharpSourceFunctionWrapper assert = new(EzrBuiltinFunctions.Assert, context, Position.None, Position.None);
        EzrSharpSourceFunctionWrapper hash = new(EzrBuiltinFunctions.Hash, context, Position.None, Position.None);
        EzrSharpSourceFunctionWrapper typeOf = new(EzrBuiltinFunctions.TypeOf, context, Position.None, Position.None);
        EzrSharpSourceFunctionWrapper typeNameOf = new(EzrBuiltinFunctions.TypeNameOf, context, Position.None, Position.None);
        EzrSharpSourceFunctionWrapper typeHashOf = new(EzrBuiltinFunctions.TypeHashOf, context, Position.None, Position.None);
        EzrSharpSourceFunctionWrapper copy = new(EzrBuiltinFunctions.Copy, context, Position.None, Position.None);

        context.Set(null, show.SharpFunctionName, ReferencePool.Get(show, AccessMod.Constant));
        context.Set(null, throwError.SharpFunctionName, ReferencePool.Get(throwError, AccessMod.Constant));
        context.Set(null, get.SharpFunctionName, ReferencePool.Get(get, AccessMod.Constant));
        context.Set(null, clear.SharpFunctionName, ReferencePool.Get(clear, AccessMod.Constant));
        context.Set(null, assert.SharpFunctionName, ReferencePool.Get(assert, AccessMod.Constant));
        context.Set(null, hash.SharpFunctionName, ReferencePool.Get(hash, AccessMod.Constant));
        context.Set(null, typeOf.SharpFunctionName, ReferencePool.Get(typeOf, AccessMod.Constant));
        context.Set(null, typeNameOf.SharpFunctionName, ReferencePool.Get(typeNameOf, AccessMod.Constant));
        context.Set(null, typeHashOf.SharpFunctionName, ReferencePool.Get(typeHashOf, AccessMod.Constant));
        context.Set(null, copy.SharpFunctionName, ReferencePool.Get(copy, AccessMod.Constant));
    }

    /// <summary>
    /// Adds all built-in types to the given context.
    /// </summary>
    /// <param name="context">The context to add to.</param>
    public static void AddBuiltinTypes(Context context)
    {
        EzrSharpSourceTypeWrapper runtimeError = new(typeof(EzrRuntimeError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper illegalOperationError = new(typeof(EzrIllegalOperationError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper undefinedValueError = new(typeof(EzrUndefinedValueError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper privateValueAccessError = new(typeof(EzrPrivateMemberOperationError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper mathError = new(typeof(EzrMathError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper valueOutOfRangeError = new(typeof(EzrValueOutOfRangeError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper unexpectedTypeError = new(typeof(EzrUnexpectedTypeError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper keyNotFoundError = new(typeof(EzrKeyNotFoundError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper missingRequiredArgumentError = new(typeof(EzrMissingRequiredArgumentError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper unexpectedArgumentError = new(typeof(EzrUnexpectedArgumentError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper unsupportedWrappingError = new(typeof(EzrUnsupportedWrappingError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper wrapperExecutionError = new(typeof(EzrWrapperExecutionError), context, Position.None, Position.None);

        context.Set(null, runtimeError.SharpTypeName, ReferencePool.Get(runtimeError, AccessMod.Constant));
        context.Set(null, illegalOperationError.SharpTypeName, ReferencePool.Get(illegalOperationError, AccessMod.Constant));
        context.Set(null, undefinedValueError.SharpTypeName, ReferencePool.Get(undefinedValueError, AccessMod.Constant));
        context.Set(null, privateValueAccessError.SharpTypeName, ReferencePool.Get(privateValueAccessError, AccessMod.Constant));
        context.Set(null, mathError.SharpTypeName, ReferencePool.Get(mathError, AccessMod.Constant));
        context.Set(null, valueOutOfRangeError.SharpTypeName, ReferencePool.Get(valueOutOfRangeError, AccessMod.Constant));
        context.Set(null, unexpectedTypeError.SharpTypeName, ReferencePool.Get(unexpectedTypeError, AccessMod.Constant));
        context.Set(null, keyNotFoundError.SharpTypeName, ReferencePool.Get(keyNotFoundError, AccessMod.Constant));
        context.Set(null, missingRequiredArgumentError.SharpTypeName, ReferencePool.Get(missingRequiredArgumentError, AccessMod.Constant));
        context.Set(null, unexpectedArgumentError.SharpTypeName, ReferencePool.Get(unexpectedArgumentError, AccessMod.Constant));
        context.Set(null, unsupportedWrappingError.SharpTypeName, ReferencePool.Get(unsupportedWrappingError, AccessMod.Constant));
        context.Set(null, wrapperExecutionError.SharpTypeName, ReferencePool.Get(wrapperExecutionError, AccessMod.Constant));
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
