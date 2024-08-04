using System.Numerics;

namespace EzrSquared.Util.Extensions;

/// <summary>
/// Static class extending the <see cref="BigInteger"/> type.
/// </summary>
public static class BigIntegerExtensions
{
    /// <summary>
    /// Extension for the power operation between two <see cref="BigInteger"/> values.
    /// </summary>
    /// <param name="thiz">The current <see cref="BigInteger"/>.</param>
    /// <param name="other">The other <see cref="BigInteger"/> which acts as the exponent.</param>
    /// <returns>The resulting <see cref="BigInteger"/>.</returns>
    public static BigInteger Power(this BigInteger thiz, BigInteger other)
    {
        BigInteger newValue = thiz;
        other--;

        for (BigInteger i = 0; i < other; i++)
            newValue *= thiz;

        return newValue;
    }

    /// <summary>
    /// Extension for the power operation between a <see cref="char"/> and a <see cref="BigInteger"/> value.
    /// </summary>
    /// <param name="thiz">The current <see cref="char"/>.</param>
    /// <param name="other">The other <see cref="BigInteger"/> which acts as the exponent.</param>
    /// <returns>The resulting <see cref="BigInteger"/>.</returns>
    public static BigInteger Power(this char thiz, BigInteger other)
    {
        BigInteger newValue = thiz;
        other--;

        for (BigInteger i = 0; i < other; i++)
            newValue *= thiz;

        return newValue;
    }
}
