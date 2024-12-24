using EzrSquared.Runtime.Types;
using System.Collections.Concurrent;

namespace EzrSquared.Runtime;

/// <summary>
/// A static pool for <see cref="Reference"/>.
/// </summary>
public static class ReferencePool
{
    /// <summary>
    /// The pool of references.
    /// </summary>
    private static readonly ConcurrentQueue<Reference> s_ezrObjectReferencePool = new();

    /// <summary>
    /// Gets a freshly-reset reference from the pool or creates a new one.
    /// </summary>
    /// <param name="object">The <see cref="IEzrObject"/> being referenced.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers of the reference.</param>
    /// <param name="name">The name of the reference.</param>
    /// <returns>The reference.</returns>
    public static Reference Get(IEzrObject? @object = null, AccessMod accessibilityModifiers = AccessMod.None, string name = "")
    {
        return s_ezrObjectReferencePool.TryDequeue(out Reference? ezrObjectReference) && ezrObjectReference is not null
            ? ezrObjectReference.Reset(@object, accessibilityModifiers, name)
            : new Reference(@object, accessibilityModifiers, name);
    }

    /// <summary>
    /// Releases a reference to the pool.
    /// </summary>
    /// <param name="reference">The reference to release.</param>
    public static void TryRelease(Reference reference)
    {
        if (!reference.IsRegisteredIncludingResult)
        {
            reference.Reset(EzrRuntimeInvalidObject.s_instance, AccessMod.None, string.Empty);
            s_ezrObjectReferencePool.Enqueue(reference);
        }
    }
}
