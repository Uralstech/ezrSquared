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
    /// Checks if the given method has the supported signature for wrapping.
    /// </summary>
    /// <param name="methodBase">The method.</param>
    /// <param name="dontThrow">Disable exception throwing.</param>
    /// <returns><see langword="true"/> if valid, an exception or <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if the method is generic or has generic parameters.</exception>
    public static bool ValidateMethod(MethodBase methodBase, bool dontThrow = false)
    {
        if (methodBase.IsGenericMethodDefinition || methodBase.ContainsGenericParameters)
            return dontThrow ? false : throw new ArgumentException($"The \"{nameof(SharpAutoWrapperAttribute)}\" attribute does not support generic method/constructor \"{methodBase.Name}\".", nameof(methodBase));

        return true;
    }

    /// <summary>
    /// Is the member eligible to be wrapped?
    /// </summary>
    /// <param name="member">The member to be wrapped.</param>
    /// <returns><see langword="true"/> if yes, <see langword="false"/> otherwise.</returns>
    public static bool ShouldBeWrapped(MemberInfo member)
    {
        return member.GetCustomAttribute<SharpDoNotWrapAttribute>() is null && (GetIsPublic(member) || member.GetCustomAttribute<SharpAutoWrapperAttribute>() is not null);
    }

    /// <summary>
    /// Is the member publicly accessible in some way?
    /// </summary>
    /// <param name="member">The member to check.</param>
    /// <returns><see langword="true"/> if yes, <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentException">If <paramref name="member"/> is of an unknown or unsupported type.</exception>
    public static bool GetIsPublic(MemberInfo member)
    {
        return member switch
        {
            MethodBase method => method.IsPublic,
            FieldInfo field => field.IsPublic,
            PropertyInfo property => property.GetMethod?.IsPublic == true || property.SetMethod?.IsPublic == true,
            _ => throw new ArgumentException($"Unknown or unsupported {nameof(MemberInfo)} type {member.GetType().Name}!", nameof(member))
        };
    }
}
