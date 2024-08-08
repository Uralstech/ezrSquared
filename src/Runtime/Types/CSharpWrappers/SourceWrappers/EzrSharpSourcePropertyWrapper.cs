using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.WrapperAttributes;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.SourceWrappers;

/// <summary>
/// A class to wrap properties written in C# so that they can be used in ezr².
/// </summary>
public class EzrSharpSourcePropertyWrapper : EzrSharpSourceExecutableWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp source field wrapper";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpSourceFieldWrapper";

    /// <summary>
    /// The object which the property is a part of, <see langword="null"/> if static.
    /// </summary>
    public readonly object? SharpInstance;

    /// <summary>
    /// Reflection information about the property.
    /// </summary>
    public readonly PropertyInfo SharpProperty;

    /// <summary>
    /// The type of the property's value.
    /// </summary>
    public readonly Type SharpPropertyType;

    /// <summary>
    /// The name of the property, in snake_case.
    /// </summary>
    public readonly string SharpPropertyName;

    /// <summary>
    /// Creates a new <see cref="EzrSharpSourcePropertyWrapper"/>.
    /// </summary>
    /// <param name="propertyInfo">The property to wrap.</param>
    /// <param name="instance">The object which the property is a part of, <see langword="null"/> if static.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    /// <exception cref="ArgumentException">Thrown if <see cref="SharpFieldWrapperAttribute"/> is not found in the property.</exception>
    public EzrSharpSourcePropertyWrapper(PropertyInfo propertyInfo, object? instance, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        SharpInstance = instance;
        SharpProperty = propertyInfo;

        SharpFieldWrapperAttribute attribute = SharpProperty.GetCustomAttribute<SharpFieldWrapperAttribute>(true) ?? throw new ArgumentException($"No \"{nameof(SharpFieldWrapperAttribute)}\" attribute found!", nameof(propertyInfo));

        Exception? propertyTypeException = SharpFieldWrapperAttribute.ValidateProperty(SharpProperty);
        if (propertyTypeException is not null)
            throw propertyTypeException;

        SharpPropertyName = attribute.Name;
        SharpPropertyType = propertyInfo.PropertyType;

        if (SharpProperty.CanWrite && !attribute.IsReadOnly)
            Parameters = [("value", false)];

        Tag = $"{Tag}.{SharpPropertyName}.{Utils.GetNextUniqueId()}";
    }

    /// <summary>
    /// If there are no arguments, accesses the property's value. If the property is not read-only and there is an arguments, sets the property's value.
    /// </summary>
    /// <inheritdoc/>
    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        Dictionary<string, Reference> argumentReferences = CheckAndPopulateArguments(arguments, result);
        if (result.ShouldReturn)
            return;

        if (argumentReferences.TryGetValue("value", out Reference? newValue))
        {
            if (!SharpPropertyType.IsAssignableFrom(newValue.Object.GetType()))
            {
                string sharpFieldType = SharpPropertyType.Name;
                if (sharpFieldType.StartsWith("Ezr"))
                    sharpFieldType = sharpFieldType[3..];

                result.Failure(new EzrUnexpectedTypeError($"Expected object of type \"{PascalCaseToLowerCasePlainText(sharpFieldType)}\", but got object of type \"{newValue.Object.TypeName}\"!", _executionContext, StartPosition, EndPosition));
                return;
            }

            SharpProperty.SetValue(SharpInstance, newValue.Object);
        }

        IEzrObject value = (IEzrObject?)SharpProperty.GetValue(SharpInstance) ?? EzrConstants.Nothing;
        result.Success(ReferencePool.Get(value, AccessMod.PrivateConstant));
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, SharpProperty);
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return (other as EzrSharpSourceFieldWrapper)?.SharpField == SharpProperty && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"<{TypeName} \"{SharpPropertyName}\">";
    }
}
