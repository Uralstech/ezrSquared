using EzrSquared.Runtime.Types.Core.Errors;
using System;
using System.Reflection;
using System.Text;

namespace EzrSquared.Runtime.Types.Wrappers.Members.Methods;

/// <summary>
/// Class to automatically wrap C# methods so that they can be used in ezr².
/// </summary>
/// <param name="sharpFunction">The method to wrap.</param>
/// <param name="instance">The object which contains the method, <see langword="null"/> if static.</param>
/// <param name="parentContext">The context in which this object was created.</param>
/// <param name="startPosition">The starting position of the object.</param>
/// <param name="endPosition">The ending position of the object.</param>
/// <param name="skipValidation">Skip method signature validation?</param>
public class EzrMethodWrapper(MethodInfo sharpFunction, object? instance, Context parentContext, Position startPosition, Position endPosition, bool skipValidation = false) : EzrMethodBaseWrapper<MethodInfo>(sharpFunction, instance, parentContext, startPosition, endPosition, skipValidation)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "csharp function";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.CSharpFunction";

    /// <summary>
    /// Creates a new <see cref="EzrMethodWrapper"/> from a delegate.
    /// </summary>
    /// <param name="sharpFunction">The method to wrap.</param>
    /// <param name="parentContext">The context in which this object was created.</param>
    /// <param name="startPosition">The starting position of the object.</param>
    /// <param name="endPosition">The ending position of the object.</param>
    /// <param name="skipValidation">Skip method signature validation?</param>
    public EzrMethodWrapper(Delegate sharpFunction, Context parentContext, Position startPosition, Position endPosition, bool skipValidation = false)
        : this(sharpFunction.Method, sharpFunction.Target, parentContext, startPosition, endPosition, skipValidation)
    { }

    /// <inheritdoc/>
    public override void Execute(Reference[] arguments, Interpreter interpreter, RuntimeResult result)
    {
        object?[] mappedArguments = CheckAndPopulateArguments(arguments, interpreter, result);
        if (result.ShouldReturn)
            return;

        try
        {
            object? output = SharpMember.Invoke(Instance, mappedArguments);
            if (result.ShouldReturn)
                return;

            CSharpToEzrObject(output, SharpMember.ReturnType, result);
        }
        catch (Exception error)
        {
            result.Failure(new EzrWrapperExecutionError(error.InnerException?.Message ?? error.Message, Context, StartPosition, EndPosition));
        }
    }

    /// <inheritdoc/>
    public override string ToString(RuntimeResult result)
    {
        bool hasParameters = ParameterNames.Length > 0;
        StringBuilder builder = new($"<{TypeName} \"{SharpMemberName}\"");

        if (hasParameters)
            builder.Append($", with \"{string.Join("\", \"", ParameterNames)}\"");

        if (AcceptsExtraKeywordArguments)
        {
            builder.Append(hasParameters && AcceptsExtraPositionalArguments ? ", " : AcceptsExtraPositionalArguments || !hasParameters ? ", with " : " and ");
            builder.Append("extra keyword arguments");
        }

        if (AcceptsExtraPositionalArguments)
        {
            builder.Append(hasParameters || AcceptsExtraKeywordArguments ? " and " : ", with ");
            builder.Append("extra positional arguments");
        }

        return builder.Append('>').ToString();
    }
}
