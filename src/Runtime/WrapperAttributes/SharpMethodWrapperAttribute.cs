global using EzrSharpSourceWrappableMethod = System.Action<EzrSquared.Runtime.WrapperAttributes.SharpMethodParameters>;

using System;
using System.Reflection;

namespace EzrSquared.Runtime.WrapperAttributes;

/// <summary>
/// Attribute for C# methods which will be wrapped into ezr².
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class SharpMethodWrapperAttribute : Attribute
{
    /// <summary>
    /// The ezr² name for the method.
    /// </summary>
    public readonly string Name = string.Empty;

    /// <summary>
    /// The ezr² names of the required parameters of the method.
    /// </summary>
    public string[] RequiredParameters = [];

    /// <summary>
    /// The ezr² names of the optional parameters of the method.
    /// </summary>
    public string[] OptionalParameters = [];

    /// <summary>
    /// Does this method support additional keyword arguments (kwargs)?
    /// </summary>
    public bool HasKeywordArguments;

    /// <summary>
    /// Creates a new instance of <see cref="SharpMethodWrapperAttribute"/> with a name.
    /// </summary>
    /// <param name="name">The ezr² name for the method.</param>
    public SharpMethodWrapperAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Creates a new instance of <see cref="SharpMethodWrapperAttribute"/>.
    /// </summary>
    public SharpMethodWrapperAttribute() { }

    /// <summary>
    /// Checks if the given method has all the required parameter.
    /// </summary>
    /// <param name="methodInfo">The method.</param>
    /// <returns><see langword="null"/> if the check was successful, an <see cref="Exception"/> otherwise.</returns>
    public static Exception? ValidateMethodParameters(MethodInfo methodInfo)
    {
        // Get the parameter types of the method
        Type[] parameterTypes = Array.ConvertAll(methodInfo.GetParameters(), p => p.ParameterType);

        // Check if the number of parameters matches
        if (parameterTypes.Length != 1)
            return new TargetParameterCountException($"\"{methodInfo.Name}\": Method must have exactly one parameter, as it uses the {nameof(SharpMethodWrapperAttribute)} attribute.");

        // Check if the required parameter types match
        return parameterTypes[0] != typeof(SharpMethodParameters)
            ? new FormatException($"\"{methodInfo.Name}\": Parameter 1 must be of type {nameof(SharpMethodParameters)}, as it uses the {nameof(SharpMethodWrapperAttribute)} attribute.")
            : null;
    }
}
