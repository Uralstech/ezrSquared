using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers;
using System;
using System.Reflection;

namespace EzrSquared.Runtime.WrapperAttributes;

/// <summary>
/// Attribute for C# type members which to be automatically wrapped from C# types into ezr² types.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class SharpAutoWrapperAttribute : Attribute
{
    /// <summary>
    /// The ezr² name for the member.
    /// </summary>
    public readonly string Name = string.Empty;

    /// <summary>
    /// Is the member read-only? Only for properties and fields.
    /// </summary>
    public readonly bool IsReadOnly;

    /// <summary>
    /// Is the member write-only? Only for properties and fields.
    /// </summary>
    public readonly bool IsWriteOnly;

    /// <summary>
    /// Creates a new <see cref="SharpAutoWrapperAttribute"/>.
    /// </summary>
    /// <param name="isReadOnly">Is the member read-only? Only for properties and fields.</param>
    /// <param name="isWriteOnly">Is the member write-only? Only for properties and fields.</param>
    /// <exception cref="ArgumentException">Thrown if both <see cref="IsReadOnly"/> and <see cref="IsWriteOnly"/> are set to <see langword="true"/>.</exception>
    public SharpAutoWrapperAttribute(bool isReadOnly = false, bool isWriteOnly = false)
    {
        IsReadOnly = isReadOnly;
        IsWriteOnly = isWriteOnly;

        if (IsWriteOnly && IsReadOnly)
            throw new ArgumentException("You can't make a property or field both read-only and write-only.");
    }

    /// <summary>
    /// Creates a new <see cref="SharpAutoWrapperAttribute"/>.
    /// </summary>
    /// <param name="name">The ezr² name for the member.</param>
    /// <param name="isReadOnly">Is the member read-only? Only for properties and fields.</param>
    /// <param name="isWriteOnly">Is the member write-only? Only for properties and fields.</param>
    public SharpAutoWrapperAttribute(string name, bool isReadOnly = false, bool isWriteOnly = false) : this(isReadOnly, isWriteOnly)
    {
        Name = name;
    }

    /// <summary>
    /// Checks if the given member is supported for wrapping.
    /// </summary>
    /// <param name="memberInfo">The member.</param>
    /// <returns><see langword="null"/> if the check was successful, an <see cref="Exception"/> otherwise.</returns>
    public static Exception? Validate(MemberInfo memberInfo)
    {
        return memberInfo switch
        {
            FieldInfo field => ValidateField(field),
            PropertyInfo property => ValidateProperty(property),
            MethodBase method => ValidateMethod(method),
            _ => throw new ArgumentException($"Unsupported {nameof(MemberInfo)} type {memberInfo.GetType().Name} for validation!", nameof(memberInfo))
        };
    }

    /// <summary>
    /// Checks if the given field is a supported type for wrapping.
    /// </summary>
    /// <param name="fieldInfo">The field.</param>
    /// <returns><see langword="null"/> if the check was successful, an <see cref="ArgumentException"/> otherwise.</returns>
    public static ArgumentException? ValidateField(FieldInfo fieldInfo)
    {
        return !EzrSharpCompatibilityWrapper.IsSupportedType(fieldInfo.FieldType)
            ? new($"Expected field \"{fieldInfo.Name}\" to be of a supported primitive type, as it uses the attribute \"{nameof(SharpAutoWrapperAttribute)}\"", nameof(fieldInfo))
            : null;
    }

    /// <summary>
    /// Checks if the given property is a supported type for wrapping.
    /// </summary>
    /// <param name="propertyInfo">The property.</param>
    /// <returns><see langword="null"/> if the check was successful, an <see cref="ArgumentException"/> otherwise.</returns>
    public static ArgumentException? ValidateProperty(PropertyInfo propertyInfo)
    {
        return !EzrSharpCompatibilityWrapper.IsSupportedType(propertyInfo.PropertyType)
            ? new($"Expected property \"{propertyInfo.Name}\" to be of a supported primitive type, as it uses the attribute \"{nameof(SharpAutoWrapperAttribute)}\"", nameof(propertyInfo))
            : null;
    }

    /// <summary>
    /// Checks if the given method has the supported signature for wrapping.
    /// </summary>
    /// <param name="methodBase">The method.</param>
    /// <returns><see langword="null"/> if the check was successful, an <see cref="ArgumentException"/> otherwise.</returns>
    public static ArgumentException? ValidateMethod(MethodBase methodBase)
    {
        if (methodBase.IsGenericMethod || methodBase.ContainsGenericParameters)
            return new($"The \"{nameof(SharpAutoWrapperAttribute)}\" attribute does not support generic method/constructor \"{methodBase.Name}\".", nameof(methodBase));

        if (methodBase is MethodInfo methodInfo && methodInfo.ReturnType != typeof(void) && !EzrSharpCompatibilityWrapper.IsSupportedReturnType(methodInfo.ReturnType))
            return new($"Expected method \"{methodBase.Name}\"'s return type to be of a supported primitive type, as it uses the attribute \"{nameof(SharpAutoWrapperAttribute)}\"", nameof(methodBase));

        foreach (ParameterInfo parameterInfo in methodBase.GetParameters())
        {
            if (!EzrSharpCompatibilityWrapper.IsSupportedType(parameterInfo.ParameterType))
                return new($"Expected all of method/constructor \"{methodBase.Name}\"'s parameters to be of a supported primitive type, as it uses the attribute \"{nameof(SharpAutoWrapperAttribute)}\", but found parameter \"{parameterInfo.Name}\" of type \"{parameterInfo.ParameterType.Name}\"", nameof(methodBase));
        }

        return null;
    }
}
