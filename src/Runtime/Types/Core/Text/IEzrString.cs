namespace EzrSquared.Runtime.Types.Core.Text;

/// <summary>
/// Interface for getting the value of string-like ezr² objects as C# strings.
/// </summary>
public interface IEzrString : IEzrObject
{
    /// <summary>
    /// The string value.
    /// </summary>
    public string StringValue { get; }
}
