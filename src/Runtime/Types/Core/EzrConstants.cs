namespace EzrSquared.Runtime.Types.Core;

/// <summary>
/// Static constants for objects that won't change.
/// </summary>
public static class EzrConstants
{
    /// <summary>
    /// Constant boolean true value.
    /// </summary>
    public static readonly EzrBoolean True = new(true, Context.Empty, Position.None, Position.None);

    /// <summary>
    /// Constant boolean false value.
    /// </summary>
    public static readonly EzrBoolean False = new(false, Context.Empty, Position.None, Position.None);

    /// <summary>
    /// Constant "nothing" value.
    /// </summary>
    public static readonly EzrNothing Nothing = new(Context.Empty, Position.None, Position.None);
}
