using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.Attributes;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables.Attributes;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers;

/// <summary>
/// Class to automatically wrap C# types so that they can be used in ezr².
/// </summary>
public class EzrSharpCompatibilityType : EzrSharpCompatibilityWrapper<Type>, IEzrObject
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp type";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpType";

    /// <summary>
    /// The primary wrapped constructor for the type. May be <see langword="null"/>.
    /// </summary>
    public readonly EzrSharpCompatibilityConstructor? PrimaryConstructor;

    /// <summary>
    /// Creates a new <see cref="EzrSharpCompatibilityType"/>.
    /// </summary>
    /// <param name="sharpType">The type to wrap.</param>
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

        Context parentContext, Position startPosition, Position endPosition) : base(sharpType, null, parentContext, startPosition, endPosition)
    {
        Tag = $"{Tag}.{SharpMemberName}.{UIDProvider.Get()}";
        WrappedMemberAttribute.ValidateType(SharpMember, AutoWrapperAttribute is null);

        MethodInfo[] allStaticMethods = sharpType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Dictionary<string, int> duplicateNames = new(allStaticMethods.Length);
        for (int i = 0; i < allStaticMethods.Length; i++)
        {
            MethodInfo method = allStaticMethods[i];
            if (!WrappedMemberAttribute.ShouldBeWrapped(method))
                continue;

            EzrSharpCompatibilityFunction methodObject = new(method, null, Context, StartPosition, EndPosition, skipValidation: true);
            if (!WrappedMemberAttribute.ValidateMethod(method, methodObject.AutoWrapperAttribute is null))
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
            if (!WrappedMemberAttribute.ShouldBeWrapped(property))
                continue;

            EzrSharpCompatibilityProperty propertyObject = new(property, null, Context, StartPosition, EndPosition);
            Context.Set(null, propertyObject.SharpMemberName, ReferencePool.Get(propertyObject, AccessMod.Constant));
        }

        FieldInfo[] allStaticFields = sharpType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < allStaticFields.Length; i++)
        {
            FieldInfo field = allStaticFields[i];
            if (!WrappedMemberAttribute.ShouldBeWrapped(field))
                continue;

            EzrSharpCompatibilityField fieldObject = new(field, null, Context, StartPosition, EndPosition);
            Context.Set(null, fieldObject.SharpMemberName, ReferencePool.Get(fieldObject, AccessMod.Constant));
        }

        int definedConstructors = 0;
        ConstructorInfo[] publicConstructors = sharpType.GetConstructors();
        for (int i = 0; i < publicConstructors.Length; i++)
        {
            ConstructorInfo constructor = publicConstructors[i];
            if (!WrappedMemberAttribute.ShouldBeWrapped(constructor))
                continue;

            EzrSharpCompatibilityConstructor constructorObject = new(sharpType, constructor, Context, StartPosition, EndPosition, skipValidation: true);
            if (!WrappedMemberAttribute.ValidateMethod(constructor, constructorObject.AutoWrapperAttribute is null))
                continue;

            if (constructor.GetCustomAttribute<PrimaryConstructorAttribute>() is not null)
            {
                if (PrimaryConstructor is not null)
                    throw new ArgumentException($"Type \"{SharpMember.Name}\" cannot have multiple primary constructors!", nameof(sharpType));

                PrimaryConstructor = constructorObject;
                continue;
            }

            string name = definedConstructors == 0 ? "make" : $"make_{definedConstructors}";
            if (constructorObject.AutoWrapperAttribute is not WrappedMemberAttribute attr || string.IsNullOrEmpty(attr.Name))
                definedConstructors++;
            else
            {
                if (Context.IsDefined(attr.Name))
                    throw new ArgumentException($"Wrapped member with name \"{attr.Name}\" is already defined for type \"{SharpMember.Name}\".", nameof(sharpType));

                name = attr.Name;
            }

            Context.Set(null, name, ReferencePool.Get(constructorObject, AccessMod.Constant));
        }
    }

    /// <inheritdoc/>
    public new void Update(Context context, Position startPosition, Position endPosition)
    {
        base.Update(context, startPosition, endPosition);
        PrimaryConstructor?.Update(Context, startPosition, endPosition);
    }

    /// <inheritdoc/>
    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        if (PrimaryConstructor is null)
        {
            base.Execute(arguments, interpreter, result);
            return;
        }

        PrimaryConstructor.Execute(arguments, interpreter, result);
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"<{TypeName} \"{SharpMemberName}\">";
    }
}
