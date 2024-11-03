using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;
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
    /// <param name="result">Runtime result for carrying any errors.</param>
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
        
        RuntimeResult result, Context parentContext, Position startPosition, Position endPosition) : base(instanceType, parentContext, startPosition, endPosition)
    {
        Instance = instance;
        Tag = $"{Tag}.{SharpMemberName}.{Utils.GetNextUniqueId()}";

        MethodInfo[] publicMethods = instanceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        Dictionary<string, int> duplicateNames = new(publicMethods.Length);
        for (int i = 0; i < publicMethods.Length; i++)
        {
            MethodInfo method = publicMethods[i];
            if (method.IsAbstract)
                continue;

            string methodObjectName = Utils.PascalToSnakeCase(method.Name);
            if (duplicateNames.TryGetValue(method.Name, out int duplicates))
            {
                methodObjectName += $"_{duplicates}";
                duplicateNames[method.Name] += 1;
            }
            else
                duplicateNames.Add(method.Name, 1);

            EzrSharpCompatibilityFunction methodObject = new(method, Instance, Context, StartPosition, EndPosition);
            if (methodObject.Validate(result))
                return;

            Context.Set(null, methodObjectName, ReferencePool.Get(methodObject, AccessMod.Constant));
        }

        PropertyInfo[] publicProperties = instanceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < publicProperties.Length; i++)
        {
            EzrSharpCompatibilityProperty property = new(publicProperties[i], Instance, Context, StartPosition, EndPosition);
            if (property.Validate(result))
                return;

            Context.Set(null, property.SharpMemberName, ReferencePool.Get(property, AccessMod.Constant));
        }

        FieldInfo[] publicFields = instanceType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < publicFields.Length; i++)
        {
            EzrSharpCompatibilityField field = new(publicFields[i], Instance, Context, StartPosition, EndPosition);
            if (field.Validate(result))
                return;

            Context.Set(null, field.SharpMemberName, ReferencePool.Get(field, AccessMod.Constant));
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
