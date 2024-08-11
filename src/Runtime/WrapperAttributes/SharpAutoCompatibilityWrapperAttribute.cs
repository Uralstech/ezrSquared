using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers;
using System;
using System.Reflection;

namespace EzrSquared.Runtime.WrapperAttributes;

/// <summary>
/// Attribute for C# type members which to be automatically wrapped from primitive C# types into ezr² types.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class SharpAutoCompatibilityWrapperAttribute : Attribute
{
    /// <summary>
    /// The ezr² name for the member.
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    /// Is the member read-only? Only for properties and fields.
    /// </summary>
    public bool IsReadOnly;

    /// <summary>
    /// Checks if the given field is a supported primitive type.
    /// </summary>
    /// <param name="fieldInfo">The field.</param>
    /// <returns><see langword="null"/> if the check was successful, an <see cref="ArgumentException"/> otherwise.</returns>
    public static ArgumentException? ValidateField(FieldInfo fieldInfo)
    {
        return !EzrSharpCompatibilityWrapper.IsSupportedPrimitiveType(fieldInfo.FieldType)
            ? new($"Expected field \"{fieldInfo.Name}\" to be of a supported primitive type, as it uses the attribute \"{nameof(SharpAutoCompatibilityWrapperAttribute)}\"", nameof(fieldInfo))
            : null;
    }

    /// <summary>
    /// Checks if the given property is a supported primitive type.
    /// </summary>
    /// <param name="propertyInfo">The property.</param>
    /// <returns><see langword="null"/> if the check was successful, an <see cref="ArgumentException"/> otherwise.</returns>
    public static ArgumentException? ValidateProperty(PropertyInfo propertyInfo)
    {
        return !EzrSharpCompatibilityWrapper.IsSupportedPrimitiveType(propertyInfo.PropertyType)
            ? new($"Expected property \"{propertyInfo.Name}\" to be of a supported primitive type, as it uses the attribute \"{nameof(SharpAutoCompatibilityWrapperAttribute)}\"", nameof(propertyInfo))
            : null;
    }

    /// <summary>
    /// Checks if the given method has the supported signature.
    /// </summary>
    /// <param name="methodInfo">The method.</param>
    /// <returns><see langword="null"/> if the check was successful, an <see cref="ArgumentException"/> otherwise.</returns>
    public static ArgumentException? ValidateMethod(MethodInfo methodInfo)
    {
        if (methodInfo.IsGenericMethod)
            return new($"The \"{nameof(SharpAutoCompatibilityWrapperAttribute)}\" attribute does not support generic method \"{methodInfo.Name}\".", nameof(methodInfo));

        if (methodInfo.ReturnType != typeof(void) && !EzrSharpCompatibilityWrapper.IsSupportedReturnType(methodInfo.ReturnType))
            return new($"Expected method \"{methodInfo.Name}\"'s return type to be of a supported primitive type, as it uses the attribute \"{nameof(SharpAutoCompatibilityWrapperAttribute)}\"", nameof(methodInfo));
    
        foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
        {
            if (!EzrSharpCompatibilityWrapper.IsSupportedPrimitiveType(parameterInfo.ParameterType))
                return new($"Expected all of method \"{methodInfo.Name}\"'s parameters to be of a supported primitive type, as it uses the attribute \"{nameof(SharpAutoCompatibilityWrapperAttribute)}\", but found parameter \"{parameterInfo.Name}\" of type \"{parameterInfo.ParameterType.Name}\"", nameof(methodInfo));
        }

        return null;
    }
}
