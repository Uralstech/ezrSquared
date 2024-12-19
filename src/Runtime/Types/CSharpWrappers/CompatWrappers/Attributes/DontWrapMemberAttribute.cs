using System;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.Attributes;

/// <summary>
/// Attribute for C# types and members which should NOT be automatically wrapped from C# types into ezr² types by the interpreter.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class DontWrapMemberAttribute : Attribute
{
}
