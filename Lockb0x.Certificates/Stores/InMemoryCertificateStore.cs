using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Lockb0x.Certificates.Models;

namespace Lockb0x.Certificates.Stores;

/// <summary>
/// Simple in-memory certificate store suitable for tests and reference deployments.
/// </summary>
public sealed class InMemoryCertificateStore : ICertificateStore
{
    private readonly ConcurrentDictionary<string, CertificateDescriptor> _certificates = new(StringComparer.Ordinal);

    public Task SaveAsync(CertificateDescriptor descriptor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (!_certificates.TryAdd(descriptor.CertificateId, descriptor))
        {
            throw new InvalidOperationException($"A certificate with id '{descriptor.CertificateId}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<CertificateDescriptor?> GetAsync(string certificateId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(certificateId))
        {
            return Task.FromResult<CertificateDescriptor?>(null);
        }

        _certificates.TryGetValue(certificateId, out var descriptor);
        return Task.FromResult(descriptor);
    }

    public Task UpdateAsync(CertificateDescriptor descriptor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        _certificates[descriptor.CertificateId] = descriptor;
        return Task.CompletedTask;
    }
}
