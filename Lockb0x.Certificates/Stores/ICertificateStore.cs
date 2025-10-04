using System.Threading;
using System.Threading.Tasks;
using Lockb0x.Certificates.Models;

namespace Lockb0x.Certificates.Stores;

/// <summary>
/// Persists certificate descriptors for retrieval, revocation, and audit history queries.
/// </summary>
public interface ICertificateStore
{
    Task SaveAsync(CertificateDescriptor descriptor, CancellationToken cancellationToken = default);
    Task<CertificateDescriptor?> GetAsync(string certificateId, CancellationToken cancellationToken = default);
    Task UpdateAsync(CertificateDescriptor descriptor, CancellationToken cancellationToken = default);
}
