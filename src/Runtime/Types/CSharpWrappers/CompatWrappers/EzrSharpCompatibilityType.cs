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
public class EzrSharpCompatibilityType : EzrSharpCompatibilityWrapper<Type>
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp type";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpType";

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityType"/>.
    /// </summary>
    /// <param name="sharpType">The type to wrap.</param>
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
        Type sharpType,

        RuntimeResult result, Context parentContext, Position startPosition, Position endPosition) : base(sharpType, null, parentContext, startPosition, endPosition)
    {
        Tag = $"{Tag}.{SharpMemberName}.{UIDProvider.Get()}";

        if (sharpType.IsAbstract || sharpType.IsGenericTypeDefinition)
        {
            result.Failure(new EzrUnsupportedWrappingError($"Cannot wrap generic/abstract C# type \"{SharpMember.Name}\"!", Context, StartPosition, EndPosition));
            return;
        }

        MethodInfo[] allStaticMethods = sharpType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Dictionary<string, int> duplicateNames = new(allStaticMethods.Length);
        for (int i = 0; i < allStaticMethods.Length; i++)
        {
            MethodInfo method = allStaticMethods[i];
            if (!SharpAutoWrapperAttribute.ShouldBeWrapped(method))
                continue;

            EzrSharpCompatibilityFunction methodObject = new(method, null, Context, StartPosition, EndPosition, skipValidation: true);
            if (!SharpAutoWrapperAttribute.ValidateMethod(method, methodObject.AutoWrapperAttribute is null))
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

        PropertyInfo[] allStaticProperties = sharpType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < allStaticProperties.Length; i++)
        {
            PropertyInfo property = allStaticProperties[i];
            if (!SharpAutoWrapperAttribute.ShouldBeWrapped(property))
                continue;

            EzrSharpCompatibilityProperty propertyObject = new(property, null, Context, StartPosition, EndPosition);
            Context.Set(null, propertyObject.SharpMemberName, ReferencePool.Get(propertyObject, AccessMod.Constant));
        }

        FieldInfo[] allStaticFields = sharpType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < allStaticFields.Length; i++)
        {
            FieldInfo field = allStaticFields[i];
            if (!SharpAutoWrapperAttribute.ShouldBeWrapped(field))
                continue;

            EzrSharpCompatibilityField fieldObject = new(field, null, Context, StartPosition, EndPosition);
            Context.Set(null, fieldObject.SharpMemberName, ReferencePool.Get(fieldObject, AccessMod.Constant));
        }

        int definedConstructors = 0;
        ConstructorInfo[] publicConstructors = sharpType.GetConstructors();
        for (int i = 0; i < publicConstructors.Length; i++)
        {
            ConstructorInfo constructor = publicConstructors[i];
            if (!SharpAutoWrapperAttribute.ShouldBeWrapped(constructor))
                continue;

            EzrSharpCompatibilityConstructor constructorObject = new(sharpType, constructor, Context, StartPosition, EndPosition, skipValidation: true);
            if (!SharpAutoWrapperAttribute.ValidateMethod(constructor, constructorObject.AutoWrapperAttribute is null))
                continue;

            Context.Set(null, definedConstructors == 0 ? "make" : $"make_{definedConstructors}", ReferencePool.Get(constructorObject, AccessMod.Constant));
            definedConstructors++;
        }
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"<{TypeName} \"{SharpMemberName}\">";
    }
}
