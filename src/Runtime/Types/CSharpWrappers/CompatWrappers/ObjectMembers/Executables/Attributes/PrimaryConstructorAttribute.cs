using System;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables.Attributes;

/// <summary>
/// Attribute that declares a primary constructor.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
public class PrimaryConstructorAttribute : Attribute
{
}
