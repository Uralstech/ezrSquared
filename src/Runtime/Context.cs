using EzrSquared.Runtime.Types;
using EzrSquared.Util;
using System;
using System.Collections.Generic;

namespace EzrSquared.Runtime;

/// <summary>
/// Stores all user defined variables/constant references, called symbols.
/// </summary>
public class Context
{
    /// <summary>
    /// Represents the status of a <see cref="Get(Context?, string, out Reference, AccessMod, bool)"/> call.
    /// </summary>
    public enum GetStatus
    {
        /// <summary>
        /// The operation was successful.
        /// </summary>
        Ok,

        /// <summary>
        /// The requested symbol was not found.
        /// </summary>
        UndefinedSymbolAccessNotAllowed,

        /// <summary>
        /// Accessing privates symbols from <see cref="Context"/>s without permission is not allowed.
        /// </summary>
        AccessToPrivateSymbolNotAllowed,

        /// <summary>
        /// Cannot access static symbols without a known static <see cref="Context"/>.
        /// </summary>
        StaticAccessWithoutDefinedContextNotAllowed,

        /// <summary>
        /// Cannot access symbols with both <see cref="AccessMod.Global"/> and <see cref="AccessMod.Private"/> accessibility modifiers.
        /// </summary>
        InvalidGlobalPrivateAccessNotAllowed,
    }

    /// <summary>
    /// Represents the status of a set call.
    /// </summary>
    /// <remarks>
    /// This includes both <see cref="Set(Context?, string, Reference)"/> and <see cref="Set(Context?, ValueTuple{IEzrObject, string}, Reference, AccessMod)"/>.
    /// </remarks>
    public enum SetStatus
    {
        /// <summary>
        /// The operation was successful.
        /// </summary>
        Ok,

        /// <summary>
        /// Assigning new values to a constant reference is not allowed.
        /// </summary>
        ConstantAssignmentNotAllowed,

        /// <summary>
        /// Assigning privates symbols from <see cref="Context"/>s without permission is not allowed.
        /// </summary>
        PrivateSymbolAssignmentNotAllowed,

        /// <summary>
        /// Changing the accessibility of an existing symbol in a parent <see cref="Context"/> is not allowed.
        /// </summary>
        SymbolAccessibilityChangeInParentNotAllowed,

        /// <summary>
        /// Assigning a private symbol to a child <see cref="Context"/> is not allowed.
        /// </summary>
        PrivateSymbolAssignmentInChildNotAllowed,

        /// <summary>
        /// Changing the accessibility of a symbol which has not been assigned to a <see cref="Context"/> is not allowed.
        /// </summary>
        UnregisteredSymbolScopeOrVariabilityChangeNotAllowed,

        /// <summary>
        /// Assigning static symbols without a known static <see cref="Context"/> is not allowed.
        /// </summary>
        StaticAssignmentWithoutDefinedContextNotAllowed,

        /// <summary>
        /// Cannot assign symbols with both <see cref="AccessMod.Global"/> and <see cref="AccessMod.Private"/> accessibility modifiers.
        /// </summary>
        InvalidGlobalPrivateAssignmentNotAllowed,
    }

    /// <summary>
    /// An empty context.
    /// </summary>
    public static readonly Context Empty = new(string.Empty, false, Position.None);

    /// <summary>
    /// The number of symbols in the context.
    /// </summary>
    public int Count => _symbols.Count;

    /// <summary>
    /// The name of the <see cref="Context"/>.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The unique identifier of the <see cref="Context"/>.
    /// </summary>
    public readonly long Id;

    /// <summary>
    /// Is this a static <see cref="Context"/>?
    /// </summary>
    public readonly bool IsStatic;

    /// <summary>
    /// The starting <see cref="Position"/> of the <see cref="Context"/>.
    /// </summary>
    public Position StartPosition { get; private set; }

    /// <summary>
    /// Other <see cref="Context"/>s which are linked to this.
    /// </summary>
    /// <remarks>
    /// This allowed the <see cref="Interpreter"/> to access variables from multiple <see cref="Context"/>s while in a local scope.
    /// </remarks>
    public Context[] LinkedContexts { get; private set; }

    /// <summary>
    /// The parent of this <see cref="Context"/>. May be <see langword="null"/>.
    /// </summary>
    public Context? Parent { get; private set; }

    /// <summary>
    /// The static <see cref="Context"/> in relation to this <see cref="Context"/>. May be <see langword="null"/>.
    /// </summary>
    public Context? StaticContext { get; private set; }

    /// <summary>
    /// The symbols of this <see cref="Context"/>.
    /// </summary>
    private Dictionary<string, Reference> _symbols;

    /// <summary>
    /// Creates a new <see cref="Context"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="Context"/>.</param>
    /// <param name="isStatic">Is this a static <see cref="Context"/>?</param>
    /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="Context"/>.</param>
    /// <param name="parent">The parent of this <see cref="Context"/>. Can be <see langword="null"/>.</param>
    /// <param name="staticContext">The static <see cref="Context"/> in relation to this <see cref="Context"/>. Can be <see langword="null"/>.</param>
    /// <param name="linkedContexts">The number of other <see cref="Context"/>s which are linked to this. See <seealso cref="LinkedContexts"/>.</param>
    /// <exception cref="ArgumentException">Raised if <paramref name="isStatic"/> is <see langword="true"/> and a static <see cref="Context"/> as <paramref name="staticContext"/> was provided at the same time. This is invalid as a <see cref="Context"/> can either be static or have a static <see cref="Context"/> related to it, but not both.</exception>
    public Context(string name, bool isStatic, Position startPosition, Context? parent = null, Context? staticContext = null, int linkedContexts = 0)
    {
        Name = name;
        Id = Utils.GetNextUniqueId();
        LinkedContexts = linkedContexts == 0 ? [] : new Context[linkedContexts];

        if (isStatic && StaticContext != null)
            throw new ArgumentException("A context cannot be static AND have a static context assigned to it.", nameof(staticContext));

        IsStatic = isStatic;
        StaticContext = staticContext ?? (IsStatic ? this : null);

        _symbols = [];

        StartPosition = startPosition;
        Parent = parent;
    }

    /// <summary>
    /// Clears the existing <see cref="LinkedContexts"/> and creates a new empty <see cref="Array"/> of <see cref="Context"/>s.
    /// </summary>
    /// <param name="contexts">The number of <see cref="Context"/>s to allocate the <see cref="Array"/> for.</param>
    public void SetNewLinkedContexts(int contexts)
    {
        LinkedContexts = contexts == 0 ? [] : new Context[contexts];
    }

    /// <summary>
    /// Updates the <see cref="StaticContext"/> to a new one.
    /// </summary>
    /// <param name="newStaticContext">The new static <see cref="Context"/>.</param>
    public void UpdateStaticContext(Context? newStaticContext)
    {
        StaticContext = newStaticContext;
    }

    /// <summary>
    /// Updates the parent of this <see cref="Context"/>/
    /// </summary>
    /// <param name="newParent">The new parent <see cref="Context"/>.</param>
    public void UpdateParent(Context newParent)
    {
        if (newParent.Id != Id && !newParent.IsContextParent(this))
            Parent = newParent;
    }

    /// <summary>
    /// Updates the starting <see cref="Position"/> of this <see cref="Context"/>/
    /// </summary>
    /// <param name="startPosition">The new starting <see cref="Position"/>.</param>
    public void UpdatePosition(Position startPosition)
    {
        StartPosition = startPosition;
    }

    /// <summary>
    /// Checks if the given <see cref="Context"/> is a parent of this <see cref="Context"/>.
    /// </summary>
    /// <param name="context">The <see cref="Context"/> to check.</param>
    /// <returns><see langword="true"/> if it is, <see langword="false"/> otherwise.</returns>
    private bool IsContextParent(Context context)
    {
        return Parent is not null && (Parent.Id == context.Id || Parent.IsContextParent(context));
    }

    /// <summary>
    /// Retrieves the <see cref="Reference"/> of the requested symbol.
    /// </summary>
    /// <param name="callingContext">The <see cref="Context"/> from which the operation is being called.</param>
    /// <param name="symbol">The symbol to retrieve.</param>
    /// <param name="objectReference">The resulting <see cref="Reference"/>.</param>
    /// <param name="accessibilityModifiers">Accessibility modifiers of the operation, <see cref="AccessMod.None"/> by default.</param>
    /// <param name="ignoreLinkedContexts">Should the <see cref="Context"/> ignore its linked contexts? (Used mainly for retrieving a reference before symbol assignment)</param>
    /// <returns>The <see cref="GetStatus"/>, representing the result of the operation.</returns>
    public GetStatus Get(Context? callingContext, string symbol, out Reference objectReference, AccessMod accessibilityModifiers = AccessMod.None, bool ignoreLinkedContexts = false)
    {
        callingContext ??= this;
        if ((accessibilityModifiers & AccessMod.Global) == AccessMod.Global && Parent is not null)
            return Parent.Get(callingContext, symbol, out objectReference, accessibilityModifiers, ignoreLinkedContexts);

        if ((accessibilityModifiers & AccessMod.InvalidGlobalPrivate) == AccessMod.InvalidGlobalPrivate)
        {
            objectReference = Reference.Empty;
            return GetStatus.InvalidGlobalPrivateAccessNotAllowed;
        }

        if ((accessibilityModifiers & AccessMod.Static) == AccessMod.Static)
            if (StaticContext is not null && StaticContext.Id != Id)
                return StaticContext.Get(callingContext, symbol, out objectReference, accessibilityModifiers, ignoreLinkedContexts);
            else if (!IsStatic)
            {
                objectReference = Reference.Empty;
                return GetStatus.StaticAccessWithoutDefinedContextNotAllowed;
            }

        if (_symbols.TryGetValue(symbol, out Reference? reference))
        {
            if ((reference.AccessibilityModifiers & AccessMod.Private) == AccessMod.Private
                && callingContext.Id != Id && !callingContext.IsContextParent(this))
            {
                objectReference = Reference.Empty;
                return GetStatus.AccessToPrivateSymbolNotAllowed;
            }

            objectReference = reference;
            objectReference.UpdateRegisteredContext(this);

            return GetStatus.Ok;
        }

        if ((accessibilityModifiers & AccessMod.Global) == AccessMod.Global)
        {
            objectReference = ReferencePool.Get(name: symbol);
            objectReference.UpdateRegisteredContext(this);

            return GetStatus.UndefinedSymbolAccessNotAllowed;
        }

        if (!ignoreLinkedContexts)
        {
            for (int i = 0; i < LinkedContexts.Length; i++)
            {
                Context linkedContext = LinkedContexts[i];
                if (linkedContext is null)
                    continue;

                GetStatus status = linkedContext.Get(callingContext, symbol, out objectReference, AccessMod.LocalScope);
                if (status == GetStatus.Ok)
                    return status;
            }
        }

        if (Parent is not null && (accessibilityModifiers & AccessMod.LocalScope) != AccessMod.LocalScope)
        {
            GetStatus status = Parent.Get(callingContext, symbol, out objectReference, accessibilityModifiers, ignoreLinkedContexts);
            if (status == GetStatus.Ok)
                return status;
        }

        objectReference = ReferencePool.Get(name: symbol);
        objectReference.UpdateRegisteredContext(this);
        return GetStatus.UndefinedSymbolAccessNotAllowed;
    }

    /// <summary>
    /// Sets a new <see cref="IEzrObject"/> to an existing <see cref="Reference"/>.
    /// </summary>
    /// <remarks>
    /// If <paramref name="oldReference"/> is <see cref="Reference.Empty"/>, the operation is passed onto <see cref="Set(Context?, string, Reference)"/>.
    /// </remarks>
    /// <param name="callingContext">The <see cref="Context"/> from which the operation is being called.</param>
    /// <param name="newObject">The new <see cref="IEzrObject"/> to be assigned and the name of the symbol, in the format (<see cref="IEzrObject"/> Object, <see cref="string"/> Name).</param>
    /// <param name="oldReference">The old reference to assign to.</param>
    /// <param name="accessibilityModifiers">Accessibility modifiers of the operation, <see cref="AccessMod.None"/> by default.</param>
    /// <returns>The <see cref="SetStatus"/>, representing the result of the operation, and the reference to the set symbol.</returns>
    public (SetStatus, Reference) Set(Context? callingContext, (IEzrObject Object, string Name) newObject, Reference oldReference, AccessMod accessibilityModifiers = AccessMod.None)
    {
        if (oldReference.IsEmpty)
        {
            Reference newReference = ReferencePool.Get(newObject.Object, accessibilityModifiers, newObject.Name);
            return (Set(callingContext, newObject.Name, newReference), newReference);
        }

        if ((accessibilityModifiers & AccessMod.InvalidGlobalPrivate) == AccessMod.InvalidGlobalPrivate)
            return (SetStatus.InvalidGlobalPrivateAssignmentNotAllowed, Reference.Empty);

        if ((oldReference.AccessibilityModifiers & AccessMod.Constant) == AccessMod.Constant)
            return (SetStatus.ConstantAssignmentNotAllowed, Reference.Empty);

        if ((accessibilityModifiers & AccessMod.Static) == AccessMod.Static)
            if (StaticContext is not null && StaticContext.Id != Id)
                return StaticContext.Set(callingContext, newObject, oldReference, accessibilityModifiers);
            else if (!IsStatic)
                return (SetStatus.StaticAssignmentWithoutDefinedContextNotAllowed, Reference.Empty);

        callingContext ??= this;
        if ((accessibilityModifiers & AccessMod.Private) == AccessMod.Private
            && (oldReference.RegisteredContext?.IsContextParent(callingContext) ?? false))
            return (SetStatus.PrivateSymbolAssignmentInChildNotAllowed, Reference.Empty);

        bool areCallingAndReceivingContextsSame = callingContext.Id == oldReference.RegisteredContext?.Id;
        bool isCallingContextRelatedToReceivingContext = areCallingAndReceivingContextsSame
            || (oldReference.RegisteredContext is not null && callingContext.IsContextParent(oldReference.RegisteredContext));

        if (oldReference.RegisteredContext is not null
            && ((oldReference.AccessibilityModifiers | accessibilityModifiers) & AccessMod.Private) == AccessMod.Private
            && !isCallingContextRelatedToReceivingContext)
            return (SetStatus.PrivateSymbolAssignmentNotAllowed, Reference.Empty);

        if ((accessibilityModifiers & AccessMod.Private) == AccessMod.Private
            || (accessibilityModifiers & AccessMod.Constant) == AccessMod.Constant)
            if (isCallingContextRelatedToReceivingContext && !areCallingAndReceivingContextsSame)
                return (SetStatus.SymbolAccessibilityChangeInParentNotAllowed, Reference.Empty);
            else if (oldReference.RegisteredContext is null)
                return (SetStatus.UnregisteredSymbolScopeOrVariabilityChangeNotAllowed, Reference.Empty);

        if (oldReference.RegisteredContext is null)
            oldReference.UpdateObject(newObject.Object);
        else
            oldReference.UpdateObject(newObject.Object, accessibilityModifiers);

        return (SetStatus.Ok, oldReference);
    }

    /// <summary>
    /// Sets the <see cref="Reference"/> to a new symbol.
    /// </summary>
    /// <param name="callingContext">The <see cref="Context"/> from which the operation is being called.</param>
    /// <param name="symbol">The symbol to assign.</param>
    /// <param name="objectReference">The reference to assign to the symbol.</param>
    /// <returns>The <see cref="SetStatus"/>, representing the result of the operation.</returns>
    public SetStatus Set(Context? callingContext, string symbol, Reference objectReference)
    {
        callingContext ??= this;
        AccessMod accessibilityModifiers = objectReference.AccessibilityModifiers;

        if ((accessibilityModifiers & AccessMod.Global) == AccessMod.Global && Parent is not null)
            return Parent.Set(callingContext, symbol, objectReference);

        if ((accessibilityModifiers & AccessMod.InvalidGlobalPrivate) == AccessMod.InvalidGlobalPrivate)
            return SetStatus.InvalidGlobalPrivateAssignmentNotAllowed;

        if ((accessibilityModifiers & AccessMod.Static) == AccessMod.Static)
            if (StaticContext is not null && StaticContext.Id != Id)
                return StaticContext.Set(callingContext, symbol, objectReference);
            else if (!IsStatic)
                return SetStatus.StaticAssignmentWithoutDefinedContextNotAllowed;

        bool objectReferenceIsPrivate = (objectReference.AccessibilityModifiers & AccessMod.Private) == AccessMod.Private;
        if (objectReferenceIsPrivate && IsContextParent(callingContext))
            return SetStatus.PrivateSymbolAssignmentInChildNotAllowed;

        bool areCallingAndReceivingContextsSame = callingContext.Id == Id;
        bool isCallingContextRelatedToReceivingContext = areCallingAndReceivingContextsSame || callingContext.IsContextParent(this);

        if (_symbols.TryGetValue(symbol, out Reference? reference))
        {
            if ((reference.AccessibilityModifiers & AccessMod.Constant) == AccessMod.Constant)
                return SetStatus.ConstantAssignmentNotAllowed;

            if (((reference.AccessibilityModifiers & AccessMod.Private) == AccessMod.Private || objectReferenceIsPrivate)
                && !isCallingContextRelatedToReceivingContext)
                return SetStatus.PrivateSymbolAssignmentNotAllowed;

            _symbols[symbol] = objectReference;
            objectReference.UpdateRegisteredContext(this);
            return SetStatus.Ok;
        }

        _symbols.Add(symbol, objectReference);
        objectReference.UpdateRegisteredContext(this);
        return SetStatus.Ok;
    }

    /// <summary>
    /// Checks if a symbol has been defined in this <see cref="Context"/>/
    /// </summary>
    /// <param name="name">The name of the symbol to check.</param>
    /// <returns><see langword="true"/> if it is, <see langword="false"/> otherwise.</returns>
    public bool IsDefined(string name)
    {
        return _symbols.ContainsKey(name);
    }

    /// <summary>
    /// Creates a deep copy of the context.
    /// </summary>
    /// <remarks>
    /// It is the caller's responsibility to handle linked contexts.
    /// </remarks>
    /// <param name="result">Runtime result for raising errors/</param>
    /// <param name="excludedSymbols">Symbols to exclude when copying.</param>
    /// <returns>The copy, or, <see langword="null"/> if failed.</returns>
    public Context? DeepCopy(RuntimeResult result, IEzrObject[] excludedSymbols)
    {
        Dictionary<string, Reference> newSymbols = new(_symbols.Count);

        Context newContext = new(Name, IsStatic, StartPosition, Parent, StaticContext, LinkedContexts.Length);
        foreach (KeyValuePair<string, Reference> keyValuePair in _symbols)
        {
            if (Array.Exists(excludedSymbols, excludedSymbol => ReferenceEquals(excludedSymbol, keyValuePair.Value.Object)))
                continue;

            Reference? copy = (Reference?)keyValuePair.Value.DeepCopy(result);
            if (result.ShouldReturn)
                return null;

            copy!.UpdateRegisteredContext(newContext);

            if (copy.Object is IEzrMutableObject mutable)
            {
                mutable.Context.UpdateParent(newContext);
                (mutable as EzrObject)?.UpdateCreationContext(newContext);
            }

            newSymbols[keyValuePair.Key] = copy;
        }

        newContext._symbols = newSymbols;
        return newContext;
    }

    /// <summary>
    /// Releases all references in the context and clears the symbols dictionary.
    /// </summary>
    public void Release()
    {
        foreach (KeyValuePair<string, Reference> reference in _symbols)
        {
            reference.Value.UpdateRegisteredContext(null);
            ReferencePool.TryRelease(reference.Value);
        }

        _symbols.Clear();
        _symbols.TrimExcess();

        for (int i = 0; i < LinkedContexts.Length; i++)
            LinkedContexts[i]?.Release();
    }

    /// <summary>Destructor.</summary>
    ~Context() => Release();
}
