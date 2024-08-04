using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.WrapperAttributes;
using EzrSquared.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EzrSquared.Runtime.Types.CSharpWrappers.SourceWrappers;

public class EzrSharpSourceFieldWrapper : EzrSharpSourceExecutableWrapper
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp source field wrapper";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpSourceFieldWrapper";

    public readonly object? SharpInstance;
    public readonly FieldInfo SharpField;
    public readonly Type SharpFieldType;
    public readonly string SharpFieldName;
    public readonly bool IsReadOnlyField;

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
    /// Converts a string from PascalCase to lowecase plain text, seperated by spaces.
    /// </summary>
    /// <param name="text">The text to convert in PascalCase.</param>
    /// <returns>The converted text in lowecase plain text.</returns>
    private static string PascalCaseToLowerCasePlainText(string text)
    {
        StringBuilder result = new();
        result.Append(char.ToLowerInvariant(text[0]));

        for (int i = 1; i < text.Length; ++i)
        {
            char c = text[i];
            if (char.IsUpper(c))
                result.Append(' ').Append(char.ToLowerInvariant(c));
            else
                result.Append(c);
        }

        return result.ToString();
    }

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
