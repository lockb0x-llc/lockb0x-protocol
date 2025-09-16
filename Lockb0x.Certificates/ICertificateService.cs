using System.Threading.Tasks;
using Lockb0x.Core;

namespace Lockb0x.Certificates;

public interface ICertificateService
{
    Task<Certificate> IssueCertificateAsync(CodexEntry entry);
    Task<bool> VerifyCertificateAsync(Certificate certificate, CodexEntry entry);
}

public class Certificate
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "Lockb0xCertificate";
    public string Issuer { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Jws { get; set; } = string.Empty; // JWS binding
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? Vc { get; set; } // Optional VC binding
    public string? X509 { get; set; } // Optional X.509 binding
}

// Stub implementation
public class CertificateService : ICertificateService
{
    public Task<Certificate> IssueCertificateAsync(CodexEntry entry) => throw new System.NotImplementedException();
    public Task<bool> VerifyCertificateAsync(Certificate certificate, CodexEntry entry) => throw new System.NotImplementedException();
}
