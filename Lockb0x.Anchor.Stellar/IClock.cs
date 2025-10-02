using System;

namespace Lockb0x.Anchor.Stellar;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
