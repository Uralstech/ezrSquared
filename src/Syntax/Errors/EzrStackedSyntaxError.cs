namespace EzrSquared.Syntax.Errors;

/// <summary>
/// The <see cref="EzrSyntaxError"/> returned when multiple <see cref="EzrSyntaxError"/> objects need to be returned to the user.
/// </summary>
/// <param name="parent">The 'parent' error, or the error that occurred first.</param>
/// <param name="child">The 'child' error, or the error that occurred because of the <paramref name="parent"/>.</param>
internal class EzrStackedSyntaxError(EzrSyntaxError parent, EzrSyntaxError child) : EzrSyntaxError("Multiple errors", $"{parent}\n\nDue to the above error, another one occurred:\n{child}", parent.ErrorStartPosition, parent.ErrorEndPosition)
{
    /// <summary>
    /// Creates the formatted text representation of the <see cref="EzrStackedSyntaxError"/>, which shows all child <see cref="EzrSyntaxError"/> objects as the 'details'.
    /// </summary>
    /// <returns>The formatted text.</returns>
    public override string ToString()
    {
        return Details;
    }
}
