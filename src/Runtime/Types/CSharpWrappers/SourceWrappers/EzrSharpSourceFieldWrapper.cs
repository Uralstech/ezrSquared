using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.WrapperAttributes;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.SourceWrappers;

/// <summary>
/// A class to wrap fields written in C# so that they can be used in ezr².
/// </summary>
public class EzrSharpSourceFieldWrapper : EzrSharpSourceExecutableWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp source field wrapper";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpSourceFieldWrapper";

    /// <summary>
    /// The object which the field is a part of, <see langword="null"/> if static.
    /// </summary>
    public readonly object? SharpInstance;

    /// <summary>
    /// Reflection information about the field.
    /// </summary>
    public readonly FieldInfo SharpField;

    /// <summary>
    /// The type of the field's value.
    /// </summary>
    public readonly Type SharpFieldType;

    /// <summary>
    /// The name of the field, in snake_case.
    /// </summary>
    public readonly string SharpFieldName;

    /// <summary>
    /// Is the field read-only?
    /// </summary>
    public readonly bool IsReadOnlyField;

    /// <summary>
    /// Creates a new <see cref="EzrSharpSourceFieldWrapper"/>.
    /// </summary>
    /// <param name="fieldInfo">The field to wrap.</param>
    /// <param name="instance">The object which the field is a part of, <see langword="null"/> if static.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    /// <exception cref="ArgumentException">Thrown if <see cref="SharpFieldWrapperAttribute"/> is not found in the field.</exception>
    public EzrSharpSourceFieldWrapper(FieldInfo fieldInfo, object? instance, Context parentContext, Position startPosition, Position endPosition) : base(parentContext, startPosition, endPosition)
    {
        SharpInstance = instance;
        SharpField = fieldInfo;

        SharpFieldWrapperAttribute attribute = SharpField.GetCustomAttribute<SharpFieldWrapperAttribute>(true) ?? throw new ArgumentException($"No \"{nameof(SharpFieldWrapperAttribute)}\" attribute found!", nameof(fieldInfo));

        ArgumentException? fieldTypeException = SharpFieldWrapperAttribute.ValidateField(SharpField);
        if (fieldTypeException is not null)
            throw fieldTypeException;

        SharpFieldName = attribute.Name;
        SharpFieldType = fieldInfo.FieldType;
        IsReadOnlyField = attribute.IsReadOnly;

        if (!IsReadOnlyField && !SharpField.IsLiteral && !SharpField.IsInitOnly)
            Parameters = [("value", false)];

        Tag = $"{Tag}.{SharpFieldName}.{Utils.GetNextUniqueId()}";
    }

    /// <summary>
    /// If there are no arguments, accesses the field's value. If the field is not read-only and there is an arguments, sets the field's value.
    /// </summary>
    /// <inheritdoc/>
    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        Dictionary<string, Reference> argumentReferences = CheckAndPopulateArguments(arguments, result);
        if (result.ShouldReturn)
            return;

        if (argumentReferences.TryGetValue("value", out Reference? newValue))
        {
            if (!SharpFieldType.IsAssignableFrom(newValue.Object.GetType()))
            {
                string sharpFieldType = SharpFieldType.Name;
                if (sharpFieldType.StartsWith("Ezr"))
                    sharpFieldType = sharpFieldType[3..];

                result.Failure(new EzrUnexpectedTypeError($"Expected object of type \"{PascalCaseToLowerCasePlainText(sharpFieldType)}\", but got object of type \"{newValue.Object.TypeName}\"!", _executionContext, StartPosition, EndPosition));
                return;
            }

            SharpField.SetValue(SharpInstance, newValue.Object);
        }

        IEzrObject? fieldValue = (IEzrObject?)SharpField.GetValue(SharpInstance);
        Reference fieldReference = fieldValue is null ? NewNothingConstant() : ReferencePool.Get(fieldValue, AccessMod.PrivateConstant);

        result.Success(fieldReference);
    }

    /// <inheritdoc/>
    public override int ComputeHashCode(RuntimeResult result)
    {
        return HashCode.Combine(HashTag, SharpField);
    }

    /// <inheritdoc/>
    public override bool StrictEquals(IEzrObject other, RuntimeResult result)
    {
        return (other as EzrSharpSourceFieldWrapper)?.SharpField == SharpField && other.HashTag == HashTag;
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"<{TypeName} \"{SharpFieldName}\">";
    }
}
