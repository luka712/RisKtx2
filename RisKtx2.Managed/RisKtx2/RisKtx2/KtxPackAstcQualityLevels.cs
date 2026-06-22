namespace RisKtx2;

/// <summary>
/// The quality levels for the ASTC compression algorithm.
/// </summary>
public enum KtxPackAstcQualityLevels : uint
{
    /// <summary>
    /// Fastest compression.
    /// </summary>
    FASTEST = 0,

    /// <summary>
    /// Fast compression.
    /// </summary>
    FAST = 10,

    /// <summary>
    /// Medium compression.
    /// </summary>
    MEDIUM = 60,

    /// <summary>
    /// Slower compression.
    /// </summary>
    THOROUGH = 98,

    /// <summary>
    /// Very slow compression.
    /// </summary>
    EXHAUSTIVE = 100,

    /// <summary>
    /// Maximum supported quality level.
    /// Same as <see cref="EXHAUSTIVE"/>.
    /// </summary>
    LEVEL_MAX = EXHAUSTIVE,
}