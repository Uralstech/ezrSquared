using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;
using EzrSquared.Runtime.WrapperAttributes;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers;

/// <summary>
/// Class to automatically wrap C# types so that they can be used in ezr².
/// </summary>
public class EzrSharpCompatibilityType : EzrSharpCompatibilityWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp type";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpType";

    /// <summary>
    /// The type to wrap.
    /// </summary>
    [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.NonPublicFields
            | DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods
            | DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.NonPublicProperties
            | DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
    public readonly Type SharpType;

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityType"/>.
    /// </summary>
    /// <param name="type">The type to wrap.</param>
    /// <param name="result">Runtime result for carrying any errors.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrSharpCompatibilityType(
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.NonPublicFields
            | DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods
            | DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.NonPublicProperties
            | DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        Type type,

        RuntimeResult result, Context parentContext, Position startPosition, Position endPosition) : base(type, parentContext, startPosition, endPosition)
    {
        SharpType = type;
        Tag = $"{Tag}.{SharpMemberName}.{Utils.GetNextUniqueId()}";

        if (SharpType.IsGenericType)
        {
            result.Failure(new EzrUnsupportedWrappingError($"Cannot wrap generic CSharp type \"{SharpType.Name}\"!", Context, StartPosition, EndPosition));
            return;
        }

        MethodInfo[] allStaticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Dictionary<string, int> duplicateNames = new(allStaticMethods.Length);
        for (int i = 0; i < allStaticMethods.Length; i++)
        {
            MethodInfo method = allStaticMethods[i];
            if (method.IsAbstract || (!method.IsPublic && method.GetCustomAttribute<SharpAutoWrapperAttribute>() is null))
                continue;

            EzrSharpCompatibilityFunction methodObject = new(method, null, Context, StartPosition, EndPosition, skipValidation: true);
            if (!methodObject.Validate())
                continue;

            string methodObjectName = methodObject.SharpMemberName;
            if (duplicateNames.TryGetValue(method.Name, out int duplicates))
            {
                methodObjectName += $"_{duplicates}";
                duplicateNames[method.Name] += 1;
            }
            else
                duplicateNames.Add(method.Name, 1);

            Context.Set(null, methodObjectName, ReferencePool.Get(methodObject, AccessMod.Constant));
        }

        PropertyInfo[] allStaticProperties = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < allStaticProperties.Length; i++)
        {
            PropertyInfo property = allStaticProperties[i];
            if (property.GetMethod?.IsPublic != true && property.SetMethod?.IsPublic != true && property.GetCustomAttribute<SharpAutoWrapperAttribute>() is null)
                continue;

            EzrSharpCompatibilityProperty propertyObject = new(property, null, Context, StartPosition, EndPosition, skipValidation: true);
            if (!propertyObject.Validate())
                continue;

            Context.Set(null, propertyObject.SharpMemberName, ReferencePool.Get(propertyObject, AccessMod.Constant));
        }

        FieldInfo[] allStaticFields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < allStaticFields.Length; i++)
        {
            FieldInfo field = allStaticFields[i];
            if (!field.IsPublic && field.GetCustomAttribute<SharpAutoWrapperAttribute>() is null)
                continue;

            EzrSharpCompatibilityField fieldObject = new(field, null, Context, StartPosition, EndPosition, skipValidation: true);
            if (!fieldObject.Validate())
                continue;

            Context.Set(null, fieldObject.SharpMemberName, ReferencePool.Get(fieldObject, AccessMod.Constant));
        }

        ConstructorInfo[] publicConstructors = type.GetConstructors();
        for (int i = 0; i < publicConstructors.Length; i++)
        {
            ConstructorInfo constructor = publicConstructors[i];
            if (!constructor.IsPublic && constructor.GetCustomAttribute<SharpAutoWrapperAttribute>() is null)
                continue;

            EzrSharpCompatibilityConstructor constructorObject = new(this, constructor, Context, StartPosition, EndPosition, skipValidation: true);
            if (!constructorObject.Validate())
                continue;

            Context.Set(null, $"make_{i}", ReferencePool.Get(constructorObject, AccessMod.Constant));
        }
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, SharpType);
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return (other as EzrSharpCompatibilityType)?.SharpType == SharpType && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"<{TypeName} \"{SharpMemberName}\">";
    }
}
