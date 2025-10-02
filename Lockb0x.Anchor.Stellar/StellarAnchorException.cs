using System;

namespace Lockb0x.Anchor.Stellar;

/// <summary>
/// Represents failures encountered while anchoring or verifying transactions on Stellar.
/// </summary>
public sealed class StellarAnchorException : Exception
{
    public StellarAnchorException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public StellarAnchorException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}
