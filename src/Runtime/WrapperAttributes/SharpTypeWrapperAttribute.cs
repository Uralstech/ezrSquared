using System;

namespace EzrSquared.Runtime.WrapperAttributes;

/// <summary>
/// Attribute for C# classes which will be wrapped into ezr².
/// </summary>
/// <param name="name">The ezr² name for the type.</param>
/// <param name="constructor">The name of the static constructor method. See <seealso cref="Constructor"/> for more details.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class SharpTypeWrapperAttribute(string name, string constructor) : Attribute
{
    /// <summary>
    /// The ezr² name for the type.
    /// </summary>
    public readonly string Name = name;

    /// <summary>
    /// The name of the static constructor method.
    /// </summary>
    /// <remarks>
    /// The static constructor method should have the following arguments:
    /// <br/><see cref="Context">Context</see> context,
    /// <br/><see cref="Position">Position</see> startPosition,
    /// <br/><see cref="Position">Position</see> endPosition,
    /// <br/><see cref="System.Collections.Generic.Dictionary{TKey, TValue}">Dictionary&lt;<see cref="string"/>, <see cref="Reference">EzrObjectReference</see>&gt;</see> arguments,
    /// <br/><see cref="Interpreter"/> interpreter,
    /// <br/><see cref="RuntimeResult"/> result
    /// </remarks>
    public readonly string Constructor = constructor;
}