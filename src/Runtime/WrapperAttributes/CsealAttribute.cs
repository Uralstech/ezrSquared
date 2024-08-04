using System;

namespace EzrSquared.Runtime.WrapperAttributes;

/// <summary>
/// Attribute for identifying CSAELs (C# Assisted ezr² Libraries).
/// </summary>
/// <remarks>
/// This has not been implemented. It doesn't do anything right now.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public class CsealAttribute : Attribute
{
}
