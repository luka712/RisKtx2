namespace RisKtx2;

/// <summary>
/// Exception thrown when a RisKtx2Native operation fails.
/// </summary>
public sealed class KtxException : Exception
{
    /// <summary>The native error code that caused this exception.</summary>
    public int ErrorCode { get; }

    /// <summary>Initialise a new <see cref="KtxException"/> with a message and error code.</summary>
    public KtxException(string message, int errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
