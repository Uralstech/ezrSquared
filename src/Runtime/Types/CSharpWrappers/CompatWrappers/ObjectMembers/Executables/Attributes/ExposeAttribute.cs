using System;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables.Attributes;

/// <summary>
/// Attribute to expose additonal data about the wrapped C# method parameter.
/// </summary>
/// <param name="name">The ezr² name for the parameter.</param>
/// <param name="optional">Is the parameter optional?</param>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class ExposeAttribute(string name, bool optional) : Attribute
{
    /// <summary>
    /// The ezr² name for the parameter.
    /// </summary>
    public readonly string Name = name;

    /// <summary>
    /// Is the parameter optional?
    /// </summary>
    public readonly bool Optional = optional;

    /// <param name="name">The ezr² name for the parameter.</param>
    public ExposeAttribute(string name) : this(name, false) { }

    /// <param name="optional">Is the parameter optional?</param>
    public ExposeAttribute(bool optional) : this("", optional) { }
}
