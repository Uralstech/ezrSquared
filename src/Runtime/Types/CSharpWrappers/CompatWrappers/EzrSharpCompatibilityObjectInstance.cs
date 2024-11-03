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
/// Class to automatically wrap <i>instances</i> of already-wrapped C# types so that they can be used in ezr².
/// </summary>
public class EzrSharpCompatibilityObjectInstance : EzrSharpCompatibilityWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp object instance";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpObjectInstance";

    /// <summary>
    /// The object to wrap.
    /// </summary>
    public readonly object Instance;

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityObjectInstance"/>.
    /// </summary>
    /// <param name="instance">The object to wrap.</param>
    /// <param name="instanceType">The C# object's type.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrSharpCompatibilityObjectInstance(
        object instance,

        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods
            | DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.NonPublicProperties
            | DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.NonPublicFields)]
        Type instanceType,
        
        Context parentContext, Position startPosition, Position endPosition) : base(instanceType, parentContext, startPosition, endPosition)
    {
        Instance = instance;
        Tag = $"{Tag}.{SharpMemberName}.{UIDProvider.Get()}";

        MethodInfo[] allMethods = instanceType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Dictionary<string, int> duplicateNames = new(allMethods.Length);
        for (int i = 0; i < allMethods.Length; i++)
        {
            MethodInfo method = allMethods[i];
            if (method.IsAbstract || (!method.IsPublic && method.GetCustomAttribute<SharpAutoWrapperAttribute>() is null))
                continue;

            EzrSharpCompatibilityFunction methodObject = new(method, Instance, Context, StartPosition, EndPosition, skipValidation: true);
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

        PropertyInfo[] allProperties = instanceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        for (int i = 0; i < allProperties.Length; i++)
        {
            PropertyInfo property = allProperties[i];
            if (property.GetMethod?.IsPublic != true && property.SetMethod?.IsPublic != true && property.GetCustomAttribute<SharpAutoWrapperAttribute>() is null)
                continue;

            EzrSharpCompatibilityProperty propertyObject = new(property, Instance, Context, StartPosition, EndPosition, skipValidation: true);
            if (!propertyObject.Validate())
                continue;

            Context.Set(null, propertyObject.SharpMemberName, ReferencePool.Get(propertyObject, AccessMod.Constant));
        }

        FieldInfo[] allFields = instanceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        for (int i = 0; i < allFields.Length; i++)
        {
            FieldInfo field = allFields[i];
            if (!field.IsPublic && field.GetCustomAttribute<SharpAutoWrapperAttribute>() is null)
                continue;

            EzrSharpCompatibilityField fieldObject = new(field, Instance, Context, StartPosition, EndPosition, skipValidation: true);
            if (!fieldObject.Validate())
                continue;

            Context.Set(null, fieldObject.SharpMemberName, ReferencePool.Get(fieldObject, AccessMod.Constant));
        }
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, Instance);
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return (other as EzrSharpCompatibilityObjectInstance)?.Instance.GetHashCode() == Instance.GetHashCode() && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"<{TypeName} of type \"{SharpMemberName}\">";
    }
}
