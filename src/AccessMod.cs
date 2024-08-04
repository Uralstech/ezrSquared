using System;

namespace EzrSquared;

/// <summary>
/// Accessibility modifiers for all <see cref="Runtime.Context"/> defined scope.
/// </summary>
[Flags]
public enum AccessMod
{
    /// <summary>
    /// Default value.
    /// </summary>
    None = 0,

    /// <summary>
    /// Stand-in for a global scope.
    /// </summary>
    Global = 1,

    /// <summary>
    /// Stand-in for a private scope.
    /// </summary>
    Private = 2,

    /// <summary>
    /// Stand-in for a static scope.
    /// </summary>
    Static = 4,

    /// <summary>
    /// Stand-in for a constant reference.
    /// </summary>
    Constant = 8,

    /// <summary>
    /// Stand-in for a local-only scope.
    /// </summary>
    LocalScope = 16,

    /// <summary>
    /// Stand-in for a global static scope.
    /// </summary>
    GlobalStatic = Global | Static,

    /// <summary>
    /// Stand-in for a private constant scope.
    /// </summary>
    PrivateConstant = Private | Constant,

    /// <summary>
    /// Stand-in for a private static constant scope.
    /// </summary>
    PrivateStaticConstant = Private | Static | Constant,

    /// <summary>
    /// Stand-in for an <b><i>INVALID</i></b> global private scope.
    /// </summary>
    InvalidGlobalPrivate = Global | Private,
}
