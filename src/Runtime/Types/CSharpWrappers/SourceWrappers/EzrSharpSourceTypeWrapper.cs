using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables;
using EzrSquared.Runtime.WrapperAttributes;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.SourceWrappers;

/// <summary>
/// A class to wrap types written in C# so that they can be used in ezr².
/// </summary>
public class EzrSharpSourceTypeWrapper : EzrSharpSourceExecutableWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp source type wrapper";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpSourceTypeWrapper";

    /// <summary>
    /// The wrapped type.
    /// </summary>
    public readonly Type SharpType;

    /// <summary>
    /// The name of the type, in snake_case.
    /// </summary>
    public readonly string SharpTypeName;

    /// <summary>
    /// Reflection information about the type's constructor.
    /// </summary>
    public readonly ConstructorInfo Constructor;

    /// <summary>
    /// Creates a new <see cref="EzrSharpSourceTypeWrapper"/>.
    /// </summary>
    /// <param name="type">The type to wrap.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the type to be wrapped is generic or if the <see cref="SharpTypeWrapperAttribute"/> was not found in the type.
    /// </exception>
    /// <exception cref="AmbiguousMatchException">
    /// Thrown if there are multiple methods/properties/fields with the same name.
    /// </exception>
    public EzrSharpSourceTypeWrapper(

        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods
            | DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.NonPublicFields
            | DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.NonPublicProperties
            | DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.NonPublicConstructors
        )] Type type,

        Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        SharpType = type;

        // Check generics.
        if (type.IsGenericType)
            throw new ArgumentException($"Cannot wrap generic CSharp type {type.Name}!", nameof(type));

        // Check for type attribute.
        SharpTypeWrapperAttribute typeAttribute = type.GetCustomAttribute<SharpTypeWrapperAttribute>(true)
            ?? throw new ArgumentException($"No \"{nameof(SharpTypeWrapperAttribute)}\" attribute found for type \"{type.Name}\"!", nameof(type));

        // Set name and tag.
        SharpTypeName = typeAttribute.Name;
        Tag = $"{Tag}.{SharpTypeName}.{Utils.GetNextUniqueId()}";

        Exception? typeAttributeException = SharpTypeWrapperAttribute.ValidateMethodParameters(type,
            out (ConstructorInfo Info, SharpMethodWrapperAttribute Attribute)? constructor);

        if (typeAttributeException is not null)
            throw typeAttributeException;

        Exception? parameterException = SharpMethodWrapperAttribute.ValidateMethodParameters(constructor!.Value.Info);
        if (parameterException is not null)
            throw parameterException;

        // Get constructor parameters.
        int requiredParameters = constructor.Value.Attribute.RequiredParameters.Length;
        Parameters = new (string Name, bool IsRequired)[constructor.Value.Attribute.RequiredParameters.Length + constructor.Value.Attribute.OptionalParameters.Length];

        // Required parameters.
        for (int j = 0; j < requiredParameters; j++)
            Parameters[j] = new(constructor.Value.Attribute.RequiredParameters[j], true);

        // Optional parameters.
        for (int j = 0; j < constructor.Value.Attribute.OptionalParameters.Length; j++)
            Parameters[j + requiredParameters] = new(constructor.Value.Attribute.OptionalParameters[j], false);

        // Set variables.
        HasKeywordArguments = constructor.Value.Attribute.HasKeywordArguments;
        Constructor = constructor.Value.Info;

        // Get static methods to wrap.
        MethodInfo[] staticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < staticMethods.Length; i++)
        {
            MethodInfo method = staticMethods[i];

            // Check if abstract.
            if (method.IsAbstract)
                continue;

            IEzrObject wrappedMethod;
            string methodName;

            // Check if method can be wrapped.
            if (method.GetCustomAttribute<SharpMethodWrapperAttribute>(false) is SharpMethodWrapperAttribute methodAttribute)
                (wrappedMethod, methodName) = GetWrappedMethod(method);
            else if (method.GetCustomAttribute<SharpAutoCompatibilityWrapperAttribute>(false) is SharpAutoCompatibilityWrapperAttribute autoWrapAttribute)
                (wrappedMethod, methodName) = GetAutoWrappedMethod(method, autoWrapAttribute);
            else
                continue;

            // Check if name already defined.
            if (Context.IsDefined(methodName))
                throw new AmbiguousMatchException($"Cannot wrap CSharp static method \"{methodName}\" of type {type.Name} as another member with the same name already exists!");

            // Set in context.
            Context.Set(null, methodName, ReferencePool.Get(wrappedMethod, AccessMod.Constant));
        }

        // Get public static properties.
        PropertyInfo[] publicStaticProperties = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < publicStaticProperties.Length; i++)
        {
            PropertyInfo property = publicStaticProperties[i];
            IEzrObject wrappedProperty;
            string propertyName;

            // Check if property can be wrapped.
            if (property.GetCustomAttribute<SharpFieldWrapperAttribute>(false) is SharpFieldWrapperAttribute propertyAttribute)
            {
                EzrSharpSourcePropertyWrapper sourceWrapper = new(property, null, Context, StartPosition, EndPosition);
                (wrappedProperty, propertyName) = (sourceWrapper, sourceWrapper.SharpPropertyName);
            }
            else if (property.GetCustomAttribute<SharpAutoCompatibilityWrapperAttribute>(false) is SharpAutoCompatibilityWrapperAttribute autoWrapAttribute)
            {
                EzrSharpCompatibilityProperty compatWrapper = !string.IsNullOrEmpty(autoWrapAttribute.Name)
                                                                ? new(autoWrapAttribute.Name, property, null, Context, StartPosition, EndPosition)
                                                                : new(property, null, Context, StartPosition, EndPosition);

                (wrappedProperty, propertyName) = (compatWrapper, compatWrapper.SharpPropertyName);
            }
            else
                continue;

            // Check if name already defined.
            if (Context.IsDefined(propertyName))
                throw new AmbiguousMatchException($"Cannot wrap CSharp static property \"{propertyName}\" of type {type.Name} as another member with the same name already exists!");

            Context.Set(null, propertyName, ReferencePool.Get(wrappedProperty, AccessMod.Constant));
        }

        // Get public static fields.
        FieldInfo[] publicStaticFields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < publicStaticFields.Length; i++)
        {
            FieldInfo field = publicStaticFields[i];
            IEzrObject wrappedField;
            string fieldName;

            // Check if property can be wrapped.
            if (field.GetCustomAttribute<SharpFieldWrapperAttribute>(false) is SharpFieldWrapperAttribute fieldAttribute)
            {
                EzrSharpSourceFieldWrapper sourceWrapper = new(field, null, Context, StartPosition, EndPosition);
                (wrappedField, fieldName) = (sourceWrapper, sourceWrapper.SharpFieldName);
            }
            else if (field.GetCustomAttribute<SharpAutoCompatibilityWrapperAttribute>(false) is SharpAutoCompatibilityWrapperAttribute autoWrapAttribute)
            {
                EzrSharpCompatibilityField compatWrapper = !string.IsNullOrEmpty(autoWrapAttribute.Name)
                                                                ? new(autoWrapAttribute.Name, field, null, Context, StartPosition, EndPosition)
                                                                : new(field, null, Context, StartPosition, EndPosition);

                (wrappedField, fieldName) = (compatWrapper, compatWrapper.SharpFieldName);
            }
            else
                continue;

            // Check if name already defined.
            if (Context.IsDefined(fieldName))
                throw new AmbiguousMatchException($"Cannot wrap CSharp static fieldName \"{fieldName}\" of type {type.Name} as another member with the same name already exists!");

            Context.Set(null, fieldName, ReferencePool.Get(wrappedField, AccessMod.Constant));
        }
    }

    private (IEzrObject, string) GetWrappedMethod(MethodInfo method)
    {
        // Check if parameters of the method are valid.
        Exception? parameterException = SharpMethodWrapperAttribute.ValidateMethodParameters(method);
        if (parameterException is not null)
            throw parameterException;

        // Create the wrapper object.
        EzrSharpSourceFunctionWrapper wrapper = new(method, Context, StartPosition, EndPosition);
        return (wrapper, wrapper.SharpFunctionName);
    }

    private (IEzrObject, string) GetAutoWrappedMethod(MethodInfo method, SharpAutoCompatibilityWrapperAttribute attribute)
    {
        // Check if the method is eligible for automatic wrapping.
        ArgumentException? formatException = SharpAutoCompatibilityWrapperAttribute.ValidateMethod(method);
        if (formatException is not null)
            throw formatException;

        // Create the wrapper object.
        EzrSharpCompatibilityFunction wrapper = !string.IsNullOrEmpty(attribute.Name)
            ? new(attribute.Name, method, null, Context, StartPosition, EndPosition)
            : new(method, null, Context, StartPosition, EndPosition);
        
        return (wrapper, wrapper.SharpRuntimeExecutableName);
    }

    /// <inheritdoc/>
    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        Dictionary<string, Reference> argumentReferences = CheckAndPopulateArguments(arguments, result);
        if (result.ShouldReturn)
            return;

        IEzrObject newObject = (IEzrObject)Constructor.Invoke(
            [
                new SharpMethodParameters
                (
                    argumentReferences,
                    _executionContext,
                    CreationContext,
                    Context,
                    StartPosition,
                    EndPosition,
                    interpreter,
                    result
                )
            ]
        );

        if (!result.ShouldReturn)
            result.Success(ReferencePool.Get(newObject, AccessMod.PrivateConstant));
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
