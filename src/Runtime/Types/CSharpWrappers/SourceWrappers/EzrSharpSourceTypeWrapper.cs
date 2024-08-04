using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers;
using EzrSquared.Runtime.WrapperAttributes;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.SourceWrappers;

public class EzrSharpSourceTypeWrapper : EzrSharpSourceExecutableWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp source type wrapper";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpSourceTypeWrapper";

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public readonly Type SharpType;

    public readonly string SharpTypeName;

    public readonly EzrSharpSourceWrappableMethod Constructor;

    public EzrSharpSourceTypeWrapper(

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        Type type,

        Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        SharpType = type;

        // Check generics.
        if (SharpType.IsGenericType)
            throw new ArgumentException($"Cannot wrap generic CSharp type {SharpType.Name}!", nameof(type));

        // Check for type attribute.
        SharpTypeWrapperAttribute typeAttribute = SharpType.GetCustomAttribute<SharpTypeWrapperAttribute>(true)
            ?? throw new ArgumentException($"No \"{nameof(SharpTypeWrapperAttribute)}\" attribute found for type \"{type.Name}\"!", nameof(type));

        // Set name and tag.
        SharpTypeName = typeAttribute.Name;
        Tag = $"{Tag}.{SharpTypeName}.{Utils.GetNextUniqueId()}";

        // Get constructor info.
        MethodInfo constructor = SharpType.GetMethod(typeAttribute.Constructor, BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreReturn)
            ?? throw new NullReferenceException($"Could not find constructor method \"{typeAttribute.Constructor}\" in type \"{type.Name}\"!");

        // Get constructor attribute.
        SharpMethodWrapperAttribute constructorAttribute = constructor.GetCustomAttribute<SharpMethodWrapperAttribute>(true)
            ?? throw new ArgumentException($"No \"{nameof(SharpMethodWrapperAttribute)}\" attribute found in constructor method \"{constructor.Name}\"!", nameof(type));

        Exception? parameterException = SharpMethodWrapperAttribute.ValidateMethodParameters(constructor);
        if (parameterException is not null)
            throw parameterException;

        // Get constructor parameters.
        int requiredParameters = constructorAttribute.RequiredParameters.Length;
        Parameters = new (string Name, bool IsRequired)[constructorAttribute.RequiredParameters.Length + constructorAttribute.OptionalParameters.Length];

        // Required parameters.
        for (int j = 0; j < requiredParameters; j++)
            Parameters[j] = new(constructorAttribute.RequiredParameters[j], true);

        // Optional parameters.
        for (int j = 0; j < constructorAttribute.OptionalParameters.Length; j++)
            Parameters[j + requiredParameters] = new(constructorAttribute.OptionalParameters[j], false);

        // Set variables.
        HasKeywordArguments = constructorAttribute.HasKeywordArguments;
        Constructor = (EzrSharpSourceWrappableMethod)constructor.CreateDelegate(typeof(EzrSharpSourceWrappableMethod));

        // Get static methods to wrap.
        MethodInfo[] staticMethods = SharpType.GetMethods(BindingFlags.Static);
        for (int i = 0; i < staticMethods.Length; i++)
        {
            MethodInfo method = staticMethods[i];

            // Check if abstract or same as constructor.
            if (method.IsAbstract || method.Name == constructor.Name)
                continue;

            // Check if method can be wrapped.
            SharpMethodWrapperAttribute? methodAttribute = constructor.GetCustomAttribute<SharpMethodWrapperAttribute>(true);
            if (methodAttribute is null)
                continue;

            // Check if parameters of the method are valid.
            parameterException = SharpMethodWrapperAttribute.ValidateMethodParameters(method);
            if (parameterException is not null)
                throw parameterException;

            // Set in context.
            EzrSharpSourceFunctionWrapper methodObject = new((EzrSharpSourceWrappableMethod)method.CreateDelegate(typeof(EzrSharpSourceWrappableMethod)), Context, startPosition, endPosition);

            // Check if name already defined.
            if (Context.IsDefined(methodObject.SharpFunctionName))
                throw new ArgumentException($"Cannot wrap CSharp static method \"{methodObject.SharpFunctionName}\" of type {SharpType.Name} as another method with the same name already exists!", nameof(type));

            Context.Set(null, methodObject.SharpFunctionName, ReferencePool.Get(methodObject, AccessMod.Constant));
        }

        // Get public static properties.
        PropertyInfo[] publicStaticProperties = SharpType.GetProperties(BindingFlags.Static | BindingFlags.Public);
        for (int i = 0; i < publicStaticProperties.Length; i++)
        {
            EzrSharpCompatibilityProperty property = new(publicStaticProperties[i], null, Context, StartPosition, EndPosition);
            Context.Set(null, property.SharpPropertyName, ReferencePool.Get(property, AccessMod.Constant));
        }

        // Get public static fields.
        FieldInfo[] publicStaticFields = SharpType.GetFields(BindingFlags.Static | BindingFlags.Public);
        for (int i = 0; i < publicStaticFields.Length; i++)
        {
            EzrSharpCompatibilityField field = new(publicStaticFields[i], null, Context, StartPosition, EndPosition);
            Context.Set(null, field.SharpFieldName, ReferencePool.Get(field, AccessMod.Constant));
        }
    }

    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        Dictionary<string, Reference> argumentReferences = CheckAndPopulateArguments(arguments, result);
        if (result.ShouldReturn)
            return;

        Constructor.Invoke(new SharpMethodParameters
        (
            argumentReferences,
            _executionContext,
            _creationContext,
            Context,
            StartPosition,
            EndPosition,
            interpreter,
            result
        ));
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, SharpType);
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return (other as EzrSharpSourceTypeWrapper)?.SharpType == SharpType && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"<{TypeName} \"{SharpTypeName}\">";
    }
}
