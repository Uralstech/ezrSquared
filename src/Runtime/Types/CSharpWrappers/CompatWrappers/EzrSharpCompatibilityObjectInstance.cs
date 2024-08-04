using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers;

public class EzrSharpCompatibilityObjectInstance : EzrSharpCompatibilityWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp object instance";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpObjectInstance";

    public readonly object Instance;
    public readonly string InstanceTypeName;

    public EzrSharpCompatibilityObjectInstance(object instance,

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        Type instanceType,

        RuntimeResult result, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        Instance = instance;
        InstanceTypeName = Utils.PascalToSnakeCase(instanceType.Name);
        Tag = $"{Tag}.{InstanceTypeName}.{Utils.GetNextUniqueId()}";

        MethodInfo[] publicMethods = instanceType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        Dictionary<string, int> duplicateNames = new(publicMethods.Length);
        for (int i = 0; i < publicMethods.Length; i++)
        {
            MethodInfo method = publicMethods[i];
            if (method.ContainsGenericParameters || method.IsGenericMethod)
            {
                result.Failure(new EzrUnsupportedWrappingError($"Cannot wrap CSharp method \"{method.Name}\" of instance \"{instanceType.Name}\"! Reasons can include the method being generic or containing generic parameters.", Context, StartPosition, EndPosition));
                return;
            }

            if (method.IsAbstract)
                continue;

            string methodObjectName = method.Name;
            if (duplicateNames.TryGetValue(method.Name, out int duplicates))
            {
                methodObjectName += $"_{duplicates}";
                duplicateNames[method.Name] += 1;
            }
            else
                duplicateNames.Add(method.Name, 1);

            EzrSharpCompatibilityFunction methodObject = new(method, Instance, Context, StartPosition, EndPosition);
            Context.Set(null, methodObjectName, ReferencePool.Get(methodObject, AccessMod.Constant));
        }

        PropertyInfo[] publicProperties = instanceType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        for (int i = 0; i < publicProperties.Length; i++)
        {
            EzrSharpCompatibilityProperty property = new(publicProperties[i], Instance, Context, StartPosition, EndPosition);
            Context.Set(null, property.SharpPropertyName, ReferencePool.Get(property, AccessMod.Constant));
        }

        FieldInfo[] publicFields = instanceType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        for (int i = 0; i < publicFields.Length; i++)
        {
            EzrSharpCompatibilityField field = new(publicFields[i], Instance, Context, StartPosition, EndPosition);
            Context.Set(null, field.SharpFieldName, ReferencePool.Get(field, AccessMod.Constant));
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
        return $"<{TypeName} of type \"{InstanceTypeName}\">";
    }
}
