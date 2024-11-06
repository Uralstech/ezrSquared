using EzrSquared.Runtime.Types;

namespace EzrSquared.Runtime;

/// <summary>
/// A reference to an <see cref="IEzrObject"/>, with its scope, usable name and more metadata.
/// </summary>
/// <remarks>
/// It is recommended to get references from the <see cref="ReferencePool"/> instead of creating them manually.
/// </remarks>
/// <param name="object">The <see cref="IEzrObject"/> being referenced.</param>
/// <param name="accessibilityModifiers">The accessibility modifiers of the reference.</param>
/// <param name="name">The name of the reference.</param>
public class Reference(IEzrObject? @object = null, AccessMod accessibilityModifiers = AccessMod.None, string name = "") : IMutable<Reference>
{
    /// <summary>
    /// The <see cref="Context"/> in which the reference is defined in. May be <see langword="null"/>.
    /// </summary>
    public Context? RegisteredContext { get; private set; }

    /// <summary>
    /// Is this reference registered in a <see cref="RuntimeResult"/>?
    /// </summary>
    public bool RegisteredInResult { get; private set; }

    /// <summary>
    /// The number of other objects that have this reference registered.
    /// </summary>
    public int RegisteredIn { get; private set; }

    /// <summary>
    /// The <see cref="IEzrObject"/> this references.
    /// </summary>
    public IEzrObject Object { get; private set; } = @object ?? EzrRuntimeInvalidObject.s_instance;

    /// <summary>
    /// The name of the reference, may be <see cref="string.Empty"/>.
    /// </summary>
    public string Name { get; private set; } = name;

    /// <summary>
    /// The accessibility modifiers of the reference. See <seealso cref="AccessMod"/>.
    /// </summary>
    public AccessMod AccessibilityModifiers = accessibilityModifiers;

    /// <summary>
    /// Is this reference registered anywhere, including <see cref="RuntimeResult"/>s?
    /// </summary>
    public bool IsRegisteredIncludingResult => IsRegistered || RegisteredInResult;

    /// <summary>
    /// Is this reference registered anywhere, except <see cref="RuntimeResult"/>s?
    /// </summary>
    public bool IsRegistered => RegisteredIn > 0 || RegisteredContext is not null;

    /// <summary>
    /// Does this reference actually reference anything?
    /// </summary>
    public bool IsEmpty { get; private set; } = @object is null;

    /// <summary>
    /// Static readonly empty reference.
    /// </summary>
    public static readonly Reference Empty = new();

    /// <summary>
    /// Resets the reference.
    /// </summary>
    /// <param name="object">The <see cref="IEzrObject"/> being referenced.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers of the reference.</param>
    /// <param name="name">The name of the reference.</param>
    public Reference Reset(IEzrObject? @object, AccessMod accessibilityModifiers, string name)
    {
        Object = @object ?? EzrRuntimeInvalidObject.s_instance;
        AccessibilityModifiers = accessibilityModifiers;

        Name = name;
        IsEmpty = @object is null;

        RegisteredContext = null;
        RegisteredIn = 0;

        return this;
    }

    /// <summary>
    /// Updates the reference with new <see cref="Object"/> and <see cref="AccessibilityModifiers"/>  values.
    /// </summary>
    /// <param name="object">The new object to reference.</param>
    /// <param name="accessibilityModifiers">The new accessibility modifiers.</param>
    public void UpdateObject(IEzrObject @object, AccessMod accessibilityModifiers = AccessMod.None)
    {
        Object = @object;
        AccessibilityModifiers |= accessibilityModifiers;
    }

    /// <summary>
    /// Updates the reference with a new <see cref="RegisteredContext"/> value.
    /// </summary>
    /// <remarks>
    /// This does not actually change the <see cref="Context"/> this reference is defined in,
    /// but it changes the <i>reference</i> to the <see cref="Context"/> this is defined in.<br/>
    /// </remarks>
    /// <param name="context">The new context.</param>
    /// <param name="releasingContext">
    /// The context that may be deleted after this is called. If the referenced object's creation context
    /// matches this, it is updated.
    /// </param>
    public void UpdateRegisteredContext(Context? context, Context? releasingContext = null)
    {
        RegisteredContext = context;

        if (context is not null && (ReferenceEquals(Object.CreationContext, Context.Empty) || Object.CreationContext.Id == releasingContext?.Id))
            Object.UpdateCreationContext(context);
    }

    /// <summary>
    /// Updates the reference with a new <see cref="RegisteredIn"/> value.
    /// </summary>
    /// <param name="isRegistered">Is it an unregister or register operation?</param>
    public void UpdateRegister(bool isRegistered)
    {
        RegisteredIn += isRegistered ? 1 : -1;
    }

    /// <summary>
    /// Updates the reference with a new <see cref="RegisteredInResult"/> value.
    /// </summary>
    /// <param name="isRegistered">The new value.</param>
    public void UpdateResultRegister(bool isRegistered)
    {
        RegisteredInResult = isRegistered;
    }

    /// <summary>
    /// Updates the reference with a new <see cref="Name"/> value.
    /// </summary>
    /// <param name="name">The new name.</param>
    public void UpdateName(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Creates a very shallow copy of the reference.
    /// </summary>
    /// <remarks>
    /// This copy only includes the original's <see cref="Object"/> and <see cref="AccessibilityModifiers"/>.
    /// </remarks>
    /// <returns>The copy.</returns>
    public Reference ShallowCopy()
    {
        return ReferencePool.Get(Object, AccessibilityModifiers);
    }

    /// <summary>
    /// Creates a copy of the reference AND the referenced object, if it is mutable.
    /// </summary>
    /// <remarks>
    /// This copy only includes the original's <see cref="Object"/>, <see cref="AccessibilityModifiers"/> and <see cref="Name"/>.
    /// </remarks>
    /// <param name="result">Runtime result to return errors in.</param>
    /// <returns>The copy, or, <see langword="null"/> if failed.</returns>
    public IMutable<Reference>? DeepCopy(RuntimeResult result)
    {
        IEzrObject newObject = ((IEzrObject?)(Object as IEzrMutableObject)?.DeepCopy(result)) ?? Object;
        if (result.ShouldReturn)
            return null;

        Reference copy = ReferencePool.Get(newObject, AccessibilityModifiers, Name);
        return copy;
    }
}
