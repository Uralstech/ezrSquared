using System;

namespace EzrSquared.Runtime.WrapperAttributes;

/// <summary>
/// Attribute for C# type members which should NOT be automatically wrapped from C# types into ezr² types by <see cref="Types.CSharpWrappers.CompatWrappers.EzrSharpCompatibilityObjectInstance"/> and <see cref="Types.CSharpWrappers.CompatWrappers.EzrSharpCompatibilityType"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class SharpDoNotWrapAttribute : Attribute
{
}
