using EzrSquared.Runtime.Types;
using System;
using System.Reflection;

namespace EzrSquared.Runtime.WrapperAttributes;

/// <summary>
/// Attribute for C# fields and properties which will be wrapped into ezr².
/// </summary>
/// <param name="name">The ezr² name for the property or field.</param>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class SharpFieldWrapperAttribute(string name) : Attribute
{
    /// <summary>
    /// The ezr² name for the property or field.
    /// </summary>
    public string Name = name;

    /// <summary>
    /// Is the property or field read-only?
    /// </summary>
    public bool IsReadOnly;

    /// <summary>
    /// Checks if the given field is of type <see cref="IEzrObject"/>.
    /// </summary>
    /// <param name="fieldInfo">The field.</param>
    /// <returns><see langword="null"/> if the check was successful, an <see cref="ArgumentException"/> otherwise.</returns>
    public static ArgumentException? ValidateField(FieldInfo fieldInfo)
    {
        return !typeof(IEzrObject).IsAssignableFrom(fieldInfo.FieldType)
            ? new($"Expected field \"{fieldInfo.Name}\" to be of type {nameof(IEzrObject)}, as it uses the attribute \"{nameof(SharpFieldWrapperAttribute)}\"", nameof(fieldInfo))
            : null;
    }

    /// <summary>
    /// Checks if the given property is of type <see cref="IEzrObject"/>.
    /// </summary>
    /// <param name="propertyInfo">The property.</param>
    /// <returns><see langword="null"/> if the check was successful, an <see cref="ArgumentException"/> otherwise.</returns>
    public static ArgumentException? ValidateProperty(PropertyInfo propertyInfo)
    {
        return !typeof(IEzrObject).IsAssignableFrom(propertyInfo.PropertyType)
            ? new($"Expected property \"{propertyInfo.Name}\" to be of type {nameof(IEzrObject)}, as it uses the attribute \"{nameof(SharpFieldWrapperAttribute)}\"", nameof(propertyInfo))
            : null;
    }
}
