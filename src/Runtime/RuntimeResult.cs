using EzrSquared.Runtime.Types;
using EzrSquared.Runtime.Types.Core.Errors;

namespace EzrSquared.Runtime;

/// <summary>
/// The type of the object that is returned as the result of interpretation done by the <see cref="Interpreter"/>.
/// </summary>
public class RuntimeResult
{
    /// <summary>
    /// The reference to the resulting <see cref="IEzrObject"/>.
    /// </summary>
    public Reference Reference = Reference.Empty;

    /// <summary>
    /// The error that occured in interpretation, if none occured, this is <see langword="null"/>.
    /// </summary>
    public IEzrRuntimeError? Error;

    /// <summary>
    /// Should the interpreter return from the current execution?
    /// </summary>
    /// <remarks>
    /// This is <see langword="true"/> when either of the below conditions are met:<br/>
    /// - <see cref="Error"/> is <b>not</b> <see langword="null"/><br/>
    /// - <see cref="SkipSet"/> is set to <see langword="true"/><br/>
    /// - <see cref="StopSet"/> is set to <see langword="true"/><br/>
    /// - <see cref="ReturnSet"/> is set to <see langword="true"/><br/>
    /// </remarks>
    public bool ShouldReturn => Error is not null || SkipSet || StopSet || ReturnSet;

    /// <summary>
    /// Should the interpreter return from the current <i>function</i> execution?
    /// </summary>
    /// <remarks>
    /// This is <see langword="true"/> when either of the below conditions are met:<br/>
    /// - <see cref="Error"/> is <b>not</b> <see langword="null"/><br/>
    /// </remarks>
    public bool ShouldReturnFunction => Error is not null;

    /// <summary>
    /// Should the interpreter return from the current <i>loop</i> execution?
    /// </summary>
    /// <remarks>
    /// This is <see langword="true"/> when either of the below conditions are met:<br/>
    /// - <see cref="Error"/> is <b>not</b> <see langword="null"/><br/>
    /// - <see cref="ReturnSet"/> is set to <see langword="true"/><br/>
    /// </remarks>
    public bool ShouldReturnLoop => Error is not null || ReturnSet;

    /// <summary>
    /// Should the interpreter return from the current <i>try-catch block</i> execution?
    /// </summary>
    /// <remarks>
    /// This is <see langword="true"/> when either of the below conditions are met:<br/>
    /// - <see cref="SkipSet"/> is set to <see langword="true"/><br/>
    /// - <see cref="StopSet"/> is set to <see langword="true"/><br/>
    /// - <see cref="ReturnSet"/> is set to <see langword="true"/><br/>
    /// </remarks>
    public bool ShouldReturnTryCatch => SkipSet || StopSet || ReturnSet;

    /// <summary>
    /// The flag for when a loop skip is called.
    /// </summary>
    public bool SkipSet;

    /// <summary>
    /// The flag for when a loop stop is called.
    /// </summary>
    public bool StopSet;

    /// <summary>
    /// The flag for when a return call is called.
    /// </summary>
    public bool ReturnSet;

    /// <summary>
    /// Creates a new <see cref="RuntimeResult"/>.
    /// </summary>
    internal RuntimeResult() { }

    /// <summary>
    /// Resets the current <see cref="RuntimeResult"/>.
    /// </summary>
    /// <remarks>
    /// This:<br/>
    /// - Sets <see cref="SkipSet"/>, <see cref="StopSet"/> and <see cref="ReturnSet"/> to <see langword="false"/>.<br/>
    /// - Releases and sets <see cref="Reference"/> to <see cref="Reference.Empty"/>, unless specifically excluded.
    /// </remarks>
    /// <param name="exclude">Reference to exclude from release.</param>
    public void Reset(Reference? exclude = null)
    {
        // Reset the flags and error.
        SkipSet = StopSet = ReturnSet = false;
        Error = null;

        // If the reference is not empty and not excluded, release it.
        Reference.UpdateResultRegister(false);
        if (!ReferenceEquals(Reference, Reference.Empty) && !ReferenceEquals(Reference, exclude))
            ReferencePool.TryRelease(Reference);

        // Set it to empty.
        Reference = Reference.Empty;
    }

    /// <summary>
    /// Sets <see cref="Reference"/>.
    /// </summary>
    /// <param name="reference">The new value for <see cref="Reference"/>.</param>
    private void SetReference(Reference reference)
    {
        reference.UpdateResultRegister(true);
        Reference = reference;
    }

    /// <summary>
    /// Sets the <see cref="SkipSet"/> flag, signifying a loop skip.
    /// </summary>
    /// <param name="reference">The reference to return when the skip flag is used outside a loop.</param>
    public void SetSkipFlag(Reference reference)
    {
        Reset(reference);

        SetReference(reference);
        SkipSet = true;
    }

    /// <summary>
    /// Sets the <see cref="StopSet"/> flag, signifying a loop stop.
    /// </summary>
    /// <param name="reference">The reference to return when the stop flag is used outside a loop.</param>
    public void SetStopFlag(Reference reference)
    {
        Reset(reference);

        SetReference(reference);
        StopSet = true;
    }

    /// <summary>
    /// Sets the <see cref="ReturnSet"/> flag, signifying a function return.
    /// </summary>
    /// <param name="reference">The reference to the object returned by the function.</param>
    public void SetReturnFlag(Reference reference)
    {
        Reset(reference);

        SetReference(reference);
        ReturnSet = true;
    }

    /// <summary>
    /// Sets a successful expression execution.
    /// </summary>
    /// <param name="reference">The reference to the resulting <see cref="IEzrObject"/>.</param>
    public void Success(Reference reference)
    {
        Reset(reference);
        SetReference(reference);
    }

    /// <summary>
    /// Sets a failed expression execution.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    public void Failure(IEzrRuntimeError error)
    {
        Reset();
        Error = error;
    }

    /// <summary>
    /// Creates a shallow copy of the current <see cref="Reference"/> if it is eligible for release by the pool.
    /// </summary>
    /// <returns>The copy of the reference, or the reference itself if not eligible for release.</returns>
    public Reference GetReferenceCopyIfReleasable()
    {
        return Reference.IsRegistered ? Reference : Reference.ShallowCopy();
    }
}
