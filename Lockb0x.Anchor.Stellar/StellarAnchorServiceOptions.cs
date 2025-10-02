using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lockb0x.Anchor.Stellar;

/// <summary>
/// Configures network behaviour for the Stellar anchoring service.
/// </summary>
public sealed class StellarAnchorServiceOptions
{
    public StellarAnchorServiceOptions()
    {
        Networks = new Dictionary<string, StellarNetworkOptions>(StringComparer.OrdinalIgnoreCase);
        DefaultNetwork = "testnet";
    }

    /// <summary>
    /// Mapping of friendly network names to configuration entries.
    /// </summary>
    public IDictionary<string, StellarNetworkOptions> Networks { get; }

    /// <summary>
    /// The default network used when none is specified.
    /// </summary>
    public string DefaultNetwork { get; set; }

    /// <summary>
    /// When <c>true</c>, unknown networks fall back to a dry-run configuration instead of throwing.
    /// </summary>
    public bool AllowUnknownNetworks { get; set; }

    /// <summary>
    /// Optional factory used to produce signed transaction envelopes for Horizon submission.
    /// </summary>
    public Func<AnchorTransactionContext, CancellationToken, Task<string>>? TransactionEnvelopeFactory { get; set; }

    /// <summary>
    /// Creates a default option set covering Stellar testnet, pubnet, and an explicit dry-run environment.
    /// </summary>
    public static StellarAnchorServiceOptions CreateDefault()
    {
        var options = new StellarAnchorServiceOptions();

        var testNet = StellarNetworkOptions.CreateTestNet();
        options.Networks[testNet.Name] = testNet;

        var pubNet = StellarNetworkOptions.CreatePubNet();
        options.Networks[pubNet.Name] = pubNet;
        options.Networks["pubnet"] = pubNet;
        options.Networks["public"] = pubNet;
        options.Networks["mainnet"] = pubNet;

        var dryRun = StellarNetworkOptions.CreateDryRun();
        options.Networks[dryRun.Name] = dryRun;

        return options;
    }
}
