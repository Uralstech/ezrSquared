using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Util;
using System;
using System.Reflection;

namespace EzrSquared.Runtime.Types.Wrappers.Members;

/// <summary>
/// Class to automatically wrap C# fields so that they can be used in ezr².
/// </summary>
public class EzrFieldWrapper : EzrWrapper<FieldInfo>
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp field";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpField";

    /// <summary>
    /// Creates a new <see cref="EzrFieldWrapper"/>.
    /// </summary>
    /// <param name="sharpField">The field to wrap.</param>
    /// <param name="instance">The object which contains the field, <see langword="null"/> if static.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    public EzrFieldWrapper(FieldInfo sharpField, object? instance, Context parentContext, Position startPosition, Position endPosition) : base(sharpField, instance, parentContext, startPosition, endPosition)
    {
        Tag = $"{Tag}.{SharpMemberName}.{UIDProvider.Get()}";
    }

    /// <summary>
    /// If there are no arguments, accesses the field's value. If the field is not read-only and there is an arguments, sets the field's value.
    /// </summary>
    /// <inheritdoc/>
    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        switch (arguments)
        {
            case { Length: > 1 }:
                result.Failure(new EzrUnexpectedArgumentError($"Only expected 0 (for getting the value) or 1 (for setting the value) argument(s) for CSharp field wrapper \"{SharpMemberName}\"!", Context, StartPosition, EndPosition));
                break;

            case { Length: 1 } when AutoWrapperAttribute?.IsReadOnly == true || SharpMember.IsInitOnly:
                result.Failure(new EzrIllegalOperationError($"Cannot set value to CSharp field wrapper \"{SharpMemberName}\" as it is read-only!", Context, StartPosition, EndPosition));
                break;

            case { Length: 1 }:
                IEzrObject ezrObject = arguments[0].Object;
                object? convertedArgument = EzrObjectToCSharp(ezrObject, SharpMember.FieldType, result);
                if (result.ShouldReturn)
                    break;

                try
                {
                    SharpMember.SetValue(Instance, convertedArgument);
                    result.Success(ReferencePool.Get(ezrObject, AccessMod.PrivateConstant));
                }
                catch (Exception error)
                {
                    result.Failure(new EzrWrapperExecutionError(error.Message, Context, StartPosition, EndPosition));
                    break;
                }
                break;

            default:
                if (AutoWrapperAttribute?.IsWriteOnly == true)
                {
                    result.Failure(new EzrIllegalOperationError($"Cannot get value from CSharp field wrapper \"{SharpMemberName}\" as it is write-only!", Context, StartPosition, EndPosition));
                    break;
                }

                object? value = SharpMember.GetValue(Instance);
                CSharpToEzrObject(value, SharpMember.FieldType, result);
                break;
        }
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        return $"<{TypeName} \"{SharpMemberName}\">";
    }
}
