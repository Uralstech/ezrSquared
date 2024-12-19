using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;
using EzrSquared.Runtime.Types.CSharpWrappers.SourceWrappers;

namespace EzrSquared.Runtime.Types.CSharpWrappers.Builtins;

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
        EzrSharpCompatibilityFunction throwError = new(EzrBuiltinFunctions.ThrowError, context, Position.None, Position.None);
        EzrSharpCompatibilityFunction assert = new(EzrBuiltinFunctions.Assert, context, Position.None, Position.None);
        EzrSharpCompatibilityFunction hash = new(EzrBuiltinFunctions.Hash, context, Position.None, Position.None);
        EzrSharpCompatibilityFunction typeOf = new(EzrBuiltinFunctions.TypeOf, context, Position.None, Position.None);
        EzrSharpCompatibilityFunction typeNameOf = new(EzrBuiltinFunctions.TypeNameOf, context, Position.None, Position.None);
        EzrSharpCompatibilityFunction typeHashOf = new(EzrBuiltinFunctions.TypeHashOf, context, Position.None, Position.None);
        EzrSharpCompatibilityFunction copy = new(EzrBuiltinFunctions.Copy, context, Position.None, Position.None);
        EzrSharpCompatibilityFunction getRaw = new(EzrBuiltinFunctions.GetRaw, context, Position.None, Position.None);
        EzrSharpCompatibilityFunction getContext = new(EzrBuiltinFunctions.GetContext, context, Position.None, Position.None);

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
        EzrSharpCompatibilityFunction show = new(EzrBuiltinFunctions.Show, context, Position.None, Position.None);
        EzrSharpCompatibilityFunction get = new(EzrBuiltinFunctions.Get, context, Position.None, Position.None);
        EzrSharpCompatibilityFunction clear = new(EzrBuiltinFunctions.Clear, context, Position.None, Position.None);

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
        EzrSharpSourceTypeWrapper runtimeError = new(typeof(EzrRuntimeError), context, Position.None, Position.None);
        context.Set(null, runtimeError.SharpTypeName, ReferencePool.Get(runtimeError, AccessMod.Constant));

        EzrSharpSourceTypeWrapper assertionError = new(typeof(EzrAssertionError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper illegalOperationError = new(typeof(EzrIllegalOperationError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper keyNotFoundError = new(typeof(EzrKeyNotFoundError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper mathError = new(typeof(EzrMathError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper missingRequiredArgumentError = new(typeof(EzrMissingRequiredArgumentError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper privateMemberOperationError = new(typeof(EzrPrivateMemberOperationError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper undefinedValueError = new(typeof(EzrUndefinedValueError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper unexpectedArgumentError = new(typeof(EzrUnexpectedArgumentError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper unexpectedTypeError = new(typeof(EzrUnexpectedTypeError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper unsupportedWrappingError = new(typeof(EzrUnsupportedWrappingError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper valueOutOfRangeError = new(typeof(EzrValueOutOfRangeError), context, Position.None, Position.None);
        EzrSharpSourceTypeWrapper wrapperExecutionError = new(typeof(EzrWrapperExecutionError), context, Position.None, Position.None);

        context.Set(null, assertionError.SharpTypeName, ReferencePool.Get(assertionError, AccessMod.Constant));
        context.Set(null, illegalOperationError.SharpTypeName, ReferencePool.Get(illegalOperationError, AccessMod.Constant));
        context.Set(null, keyNotFoundError.SharpTypeName, ReferencePool.Get(keyNotFoundError, AccessMod.Constant));
        context.Set(null, mathError.SharpTypeName, ReferencePool.Get(mathError, AccessMod.Constant));
        context.Set(null, missingRequiredArgumentError.SharpTypeName, ReferencePool.Get(missingRequiredArgumentError, AccessMod.Constant));
        context.Set(null, privateMemberOperationError.SharpTypeName, ReferencePool.Get(privateMemberOperationError, AccessMod.Constant));
        context.Set(null, undefinedValueError.SharpTypeName, ReferencePool.Get(undefinedValueError, AccessMod.Constant));
        context.Set(null, unexpectedArgumentError.SharpTypeName, ReferencePool.Get(unexpectedArgumentError, AccessMod.Constant));
        context.Set(null, unexpectedTypeError.SharpTypeName, ReferencePool.Get(unexpectedTypeError, AccessMod.Constant));
        context.Set(null, unsupportedWrappingError.SharpTypeName, ReferencePool.Get(unsupportedWrappingError, AccessMod.Constant));
        context.Set(null, valueOutOfRangeError.SharpTypeName, ReferencePool.Get(valueOutOfRangeError, AccessMod.Constant));
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
