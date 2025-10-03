using System.Threading;
using System.Threading.Tasks;
using Lockb0x.Certificates.Models;
using Lockb0x.Core.Models;

namespace Lockb0x.Certificates;

/// <summary>
/// Defines the certificate issuance, retrieval, and validation contract for Lockb0x Codex entries.
/// </summary>
public interface ICertificateService
{
    /// <summary>
    /// Issues a certificate for the supplied Codex entry using the provided options.
    /// </summary>
    Task<CertificateDescriptor> IssueCertificateAsync(CodexEntry entry, CertificateOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the supplied certificate against the given Codex entry.
    /// </summary>
    Task<CertificateValidationResult> ValidateCertificateAsync(CertificateDescriptor certificate, CodexEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the certificate as revoked and records an audit event.
    /// </summary>
    Task<bool> RevokeCertificateAsync(string certificateId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a previously issued certificate by identifier.
    /// </summary>
    Task<CertificateDescriptor?> GetCertificateAsync(string certificateId, CancellationToken cancellationToken = default);
}
