using System.Threading;

namespace EzrSquared.Util;

/// <summary>
/// Generates unique IDs for objects.
/// </summary>
public static class UIDProvider
{
    /// <summary>
    /// <see cref="long"/> value for the last generated unique identifier.
    /// </summary>
    private static long s_currentId = 0;

    /// <summary>
    /// Generates an incremental unique identifier.
    /// </summary>
    /// <returns>A new unique identifier.</returns>
    public static long Get()
    {
        return Interlocked.Increment(ref s_currentId);
    }
}
