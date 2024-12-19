global using ExtraKeywordArguments = System.Collections.Generic.Dictionary<string, EzrSquared.Runtime.Types.IEzrObject>;
global using ExtraPositionalArguments = System.Collections.Generic.List<EzrSquared.Runtime.Types.IEzrObject>;
using System;
using System.Reflection;

namespace EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables.Attributes;

/// <summary>
/// Attribute for wrapped C# methods that need to enable features or request data from the ezr² runtime.
/// </summary>
/// <remarks>
/// This can include enabling extra keyword arguments, extra positional arguments, requesting references<br/>
/// to the interpreter, wrapper, context, etc. The parameter must conform to the attributed <see cref="Feature"/>.<br/>
/// For example, a parameter attributed with <see cref="Feature.InterpreterRef"/> must be assignable from the<br/>
/// <see cref="Interpreter"/> type.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class RuntimeAttribute(Feature type) : Attribute
{
    /// <summary>
    /// The type of the parameter.
    /// </summary>
    public readonly Feature Type = type;

    /// <summary>
    /// Checks if the given parameter has the correct signature.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    public void ValidateParameter(ParameterInfo parameter)
    {
        switch (Type)
        {
            case Feature.KeywordArguments when !parameter.ParameterType.IsAssignableFrom(typeof(ExtraKeywordArguments)):
                throw new ArgumentException($"Parameter \"{parameter.Name}\" must be assignable from type {nameof(ExtraKeywordArguments)} as it has the {nameof(RuntimeAttribute)} attribute, with type {Type}.", nameof(parameter));

            case Feature.PositionalArguments when !parameter.ParameterType.IsAssignableFrom(typeof(ExtraPositionalArguments)):
                throw new ArgumentException($"Parameter \"{parameter.Name}\" must be assignable from type {nameof(ExtraPositionalArguments)} as it has the {nameof(RuntimeAttribute)} attribute, with type {Type}.", nameof(parameter));

            case Feature.CallerRef when !parameter.ParameterType.IsAssignableFrom(typeof(IEzrObject)):
                throw new ArgumentException($"Parameter \"{parameter.Name}\" must be assignable from type {nameof(IEzrObject)} as it has the {nameof(RuntimeAttribute)} attribute, with type {Type}.", nameof(parameter));

            case Feature.ExecutionRef when !parameter.ParameterType.IsAssignableFrom(typeof(Context)):
                throw new ArgumentException($"Parameter \"{parameter.Name}\" must be assignable from type {nameof(Context)} as it has the {nameof(RuntimeAttribute)} attribute, with type {Type}.", nameof(parameter));

            case Feature.InterpreterRef when !parameter.ParameterType.IsAssignableFrom(typeof(Interpreter)):
                throw new ArgumentException($"Parameter \"{parameter.Name}\" must be assignable from type {nameof(Interpreter)} as it has the {nameof(RuntimeAttribute)} attribute, with type {Type}.", nameof(parameter));

            case Feature.ResultRef when !parameter.ParameterType.IsAssignableFrom(typeof(RuntimeResult)):
                throw new ArgumentException($"Parameter \"{parameter.Name}\" must be assignable from type {nameof(RuntimeResult)} as it has the {nameof(RuntimeAttribute)} attribute, with type {Type}.", nameof(parameter));
        }
    }
}
