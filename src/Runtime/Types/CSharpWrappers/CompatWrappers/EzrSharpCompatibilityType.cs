using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers;

public class EzrSharpCompatibilityType : EzrSharpCompatibilityWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp type";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpType";

    public readonly Type SharpType;
    
    public readonly string SharpTypeName;

    public EzrSharpCompatibilityType(
        string name,

        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.NonPublicConstructors
        )] Type type,

        RuntimeResult result, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        SharpType = type;
        SharpTypeName = name;
        Tag = $"{Tag}.{SharpTypeName}.{Utils.GetNextUniqueId()}";

        if (SharpType.IsGenericType)
        {
            result.Failure(new EzrUnsupportedWrappingError($"Cannot wrap generic CSharp type \"{SharpType.Name}\"!", Context, StartPosition, EndPosition));
            return;
        }

        MethodInfo[] publicStaticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
        Dictionary<string, int> duplicateNames = new(publicStaticMethods.Length);
        for (int i = 0; i < publicStaticMethods.Length; i++)
        {
            MethodInfo method = publicStaticMethods[i];
            if (method.ContainsGenericParameters || method.IsGenericMethod)
            {
                result.Failure(new EzrUnsupportedWrappingError($"Cannot wrap CSharp static method \"{method.Name}\" of type \"{SharpType.Name}\"! Reasons can include the method being generic or containing generic parameters.", Context, StartPosition, EndPosition));
                return;
            }

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

            EzrSharpCompatibilityFunction methodObject = new(method, null, Context, StartPosition, EndPosition);
            Context.Set(null, methodObjectName, ReferencePool.Get(methodObject, AccessMod.Constant));
        }

        PropertyInfo[] publicStaticProperties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
        for (int i = 0; i < publicStaticProperties.Length; i++)
        {
            EzrSharpCompatibilityProperty property = new(publicStaticProperties[i], null, Context, StartPosition, EndPosition);
            Context.Set(null, property.SharpPropertyName, ReferencePool.Get(property, AccessMod.Constant));
        }

        FieldInfo[] publicStaticFields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
        for (int i = 0; i < publicStaticFields.Length; i++)
        {
            EzrSharpCompatibilityField field = new(publicStaticFields[i], null, Context, StartPosition, EndPosition);
            Context.Set(null, field.SharpFieldName, ReferencePool.Get(field, AccessMod.Constant));
        }

        ConstructorInfo[] publicConstructors = type.GetConstructors();
        for (int i = 0; i < publicConstructors.Length; i++)
        {
            ConstructorInfo constructor = publicConstructors[i];
            if (!constructor.IsPublic)
                continue;

            IEzrObject constructorObject = new EzrSharpCompatibilityConstructor(constructor, type, Context, StartPosition, EndPosition);
            Context.Set(null, $"make_{i}", ReferencePool.Get(constructorObject, AccessMod.Constant));
        }
    }

    public EzrSharpCompatibilityType(

        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.NonPublicConstructors
        )] Type type,

        RuntimeResult result, Context parentContext, Position startPosition, Position endPosition)
        : this(Utils.PascalToSnakeCase(type.Name), type, result, parentContext, startPosition, endPosition) { }

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
        return $"<{TypeName} \"{SharpTypeName}\">";
    }
}
