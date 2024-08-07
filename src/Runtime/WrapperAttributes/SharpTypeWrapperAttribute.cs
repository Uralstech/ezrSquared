using EzrSquared.Runtime.Types;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EzrSquared.Runtime.WrapperAttributes;

/// <summary>
/// Attribute for C# classes which will be wrapped into ezr².
/// </summary>
/// <param name="name">The ezr² name for the type.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class SharpTypeWrapperAttribute(string name) : Attribute
{
    /// <summary>
    /// The ezr² name for the type.
    /// </summary>
    public readonly string Name = name;

    /// <summary>
    /// Checks if the given type has a constructor with the <see cref="SharpMethodWrapperAttribute"/> attribute.
    /// </summary>
    /// <param name="typeInfo">The type to check.</param>
    /// <param name="constructor">The constructor and its <see cref="SharpMethodWrapperAttribute"/> attribute.</param>
    /// <returns><see langword="null"/> if the check was successful, an <see cref="Exception"/> otherwise.</returns>
    public static Exception? ValidateMethodParameters(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        Type typeInfo,
        
        out (ConstructorInfo Info, SharpMethodWrapperAttribute Attribute)? constructor)
    {
        constructor = null;
        if (!typeof(IEzrObject).IsAssignableFrom(typeInfo))
            return new ArgumentException($"Expected type \"{typeInfo.Name}\" to inherit from {nameof(IEzrObject)}, as it uses the attribute \"{nameof(SharpTypeWrapperAttribute)}\"", nameof(typeInfo));

        ConstructorInfo[] constructors = typeInfo.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);

        foreach (ConstructorInfo constructorInfo in constructors)
        {
            SharpMethodWrapperAttribute? attribute = constructorInfo.GetCustomAttribute<SharpMethodWrapperAttribute>(false);
            if (constructor is not null && attribute is not null)
                return new AmbiguousMatchException($"Found multiple constructors for type \"{typeInfo.Name}\" with attribute {nameof(SharpMethodWrapperAttribute)}");
            else if (attribute is not null)
            {
                constructor = (constructorInfo, attribute);
                if (SharpMethodWrapperAttribute.ValidateMethodParameters(constructorInfo) is Exception exception)
                    return exception;
            }
        }

        return constructor is null
            ? new ArgumentException($"No constructor with attribute {nameof(SharpMethodWrapperAttribute)} found for type \"{typeInfo.Name}\"!", nameof(typeInfo))
            : null;
    }
}