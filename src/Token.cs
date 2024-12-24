namespace EzrSquared;

/// <summary>
/// The smallest component in the script identified by the <see cref="TokenType"/>, grouped together into <see cref="Runtime.Nodes.Node"/> objects to from source code constructs.
/// </summary>
public record Token
{
    /// <summary>
    /// The identifying <see cref="TokenType"/> of the <see cref="Token"/>.
    /// </summary>
    public readonly TokenType Type;

    /// <summary>
    /// The <see cref="TokenTypeGroup"/> of the <see cref="Token"/>.
    /// </summary>
    public readonly TokenTypeGroup TypeGroup;

    /// <summary>
    /// The value of the <see cref="Token"/>; may be empty.
    /// </summary>
    public readonly string Value;

    /// <summary>
    /// The starting <see cref="Position"/> of the <see cref="Token"/> in the script.
    /// </summary>
    public readonly Position StartPosition;

    /// <summary>
    /// The ending <see cref="Position"/> of the <see cref="Token"/> in the script.
    /// </summary>
    public readonly Position EndPosition;

    /// <summary>
    /// An empty token value.
    /// </summary>
    public static readonly Token Empty = new(TokenType.Invalid, TokenTypeGroup.Special, string.Empty, Position.None);

    /// <summary>
    /// Creates a new <see cref="Token"/> object.
    /// </summary>
    /// <param name="type">The identifying <see cref="TokenType"/> of the <see cref="Token"/>.</param>
    /// <param name="typeGroup">The <see cref="TokenTypeGroup"/> of the <see cref="Token"/>.</param>
    /// <param name="value">The value of the <see cref="Token"/>; may be empty.</param>
    /// <param name="startPosition">The starting <see cref="Position"/> of the <see cref="Token"/> in the script.</param>
    /// <param name="endPosition">The ending <see cref="Position"/> of the <see cref="Token"/> in the script. If not given, copies <paramref name="startPosition"/> and advances it.</param>
    public Token(TokenType type, TokenTypeGroup typeGroup, string value, Position startPosition, Position? endPosition = null)
    {
        Type = type;
        TypeGroup = typeGroup;
        Value = value;

        StartPosition = startPosition;
        EndPosition = endPosition is not null ? endPosition.Value : startPosition.Advance();
    }

    /// <summary>
    /// Converts the <see cref="Token"/> into a <see cref="string"/>, for debugging purposes.
    /// </summary>
    /// <returns>The <see cref="string"/> representation of the <see cref="Token"/>.</returns>
    public override string ToString()
    {
        return $"{nameof(Token)}({Type}, {TypeGroup}, \"{Value}\")";
    }
}