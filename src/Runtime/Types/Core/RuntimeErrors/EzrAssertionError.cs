using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.Attributes;
using EzrSquared.Runtime.Types.CSharpWrappers.CompatWrappers.ObjectMembers.Executables.Attributes;

namespace EzrSquared.Runtime.Types.Core.Errors;

/// <summary>
/// Error type for when an assertion fails.
/// </summary>
/// <param name="context">The context in which the error occurred.</param>
/// <param name="startPosition">The starting position of the error.</param>
/// <param name="endPosition">The ending position of the error.</param>
[WrappedMember("assertion_error")]
public class EzrAssertionError(Context context, Position startPosition, Position endPosition) : EzrRuntimeError("Assertion failed", "The assertion conditions were not met!", context, startPosition, endPosition)
{
    /// <inheritdoc/>
    public override string TypeName { get; protected internal set; } = "assertion error";

    /// <inheritdoc/>
    public override string Tag { get; protected internal set; } = "ezrSquared.AssertionError";

    /// <summary>
    /// Wrapper constructor for creating the error object.
    /// </summary>
    /// <param name="wrapper">The caller.</param>
    /// <param name="executionContext">The execution context.</param>
    [WrappedMember, PrimaryConstructor]
    public EzrAssertionError(
        [Runtime(Feature.CallerRef)] IEzrObject wrapper,
        [Runtime(Feature.ExecutionRef)] Context executionContext)
        : this(executionContext, wrapper.StartPosition, wrapper.EndPosition) { }
}
