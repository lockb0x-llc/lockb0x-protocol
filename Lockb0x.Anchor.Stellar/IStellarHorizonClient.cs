using System.Threading;
using System.Threading.Tasks;

namespace Lockb0x.Anchor.Stellar;

/// <summary>
/// Encapsulates Horizon interactions used by the anchoring service.
/// </summary>
public interface IStellarHorizonClient
{
    Task<StellarTransactionRecord?> FindTransactionByMemoAsync(StellarMemo memo, StellarNetworkOptions network, CancellationToken cancellationToken);

    Task<StellarTransactionRecord?> GetTransactionAsync(string transactionHash, StellarNetworkOptions network, CancellationToken cancellationToken);

    Task<StellarTransactionRecord> SubmitTransactionAsync(StellarSubmitTransactionRequest request, CancellationToken cancellationToken);
}
