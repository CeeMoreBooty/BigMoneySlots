using System;
using System.Security.Cryptography;

/// <summary>
/// Cryptographically secure random number generator for gambling outcomes.
/// Uses System.Security.Cryptography.RandomNumberGenerator to produce
/// unpredictable values suitable for slot-machine and jackpot decisions.
/// </summary>
public static class SecureRandom
{
    // Shared instance (thread-safe in .NET Core / Mono used by Unity)
    private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    /// <summary>Returns a cryptographically random float in [0, 1).</summary>
    public static float Value()
    {
        byte[] bytes = new byte[4];
        _rng.GetBytes(bytes);
        uint val = BitConverter.ToUInt32(bytes, 0);
        // Divide by uint.MaxValue + 1 to get [0, 1) (never exactly 1)
        return (float)(val / (double)(uint.MaxValue + 1UL));
    }

    /// <summary>
    /// Returns a cryptographically random int in [minInclusive, maxExclusive).
    /// Uses rejection sampling to eliminate modulo bias.
    /// </summary>
    public static int Range(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive) return minInclusive;
        uint range = (uint)(maxExclusive - minInclusive);
        // Largest multiple of range that fits in uint (rejection boundary)
        uint limit = uint.MaxValue - (uint.MaxValue % range);
        byte[] bytes = new byte[4];
        uint val;
        do
        {
            _rng.GetBytes(bytes);
            val = BitConverter.ToUInt32(bytes, 0);
        }
        while (val >= limit);
        return minInclusive + (int)(val % range);
    }
}
