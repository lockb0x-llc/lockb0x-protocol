using System.Threading;
using System.Threading.Tasks;
using Lockb0x.Certificates.Models;
using Lockb0x.Core.Models;

namespace Lockb0x.Verifier;

/// <summary>
/// Provides high level verification services for Codex Entries and their associated certificates.
/// </summary>
public interface IVerifierService
{
    /// <summary>
    /// Executes the full verification pipeline for the supplied Codex Entry.
    /// </summary>
    Task<VerificationResult> VerifyAsync(CodexEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the supplied certificate descriptor against the Codex Entry it purports to attest.
    /// </summary>
    Task<VerificationResult> VerifyCertificateAsync(CertificateDescriptor certificate, CodexEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Traverses the revision chain for the specified entry identifier and returns any issues encountered.
    /// </summary>
    Task<RevisionChainResult> TraverseRevisionChainAsync(string entryId, CancellationToken cancellationToken = default);
}
