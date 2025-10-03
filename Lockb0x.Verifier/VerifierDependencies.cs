using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lockb0x.Anchor.Stellar;
using Lockb0x.Certificates;
using Lockb0x.Core.Canonicalization;
using Lockb0x.Core.Models;
using Lockb0x.Core.Revision;
using Lockb0x.Core.Validation;
using Lockb0x.Signing;
using Lockb0x.Storage;

namespace Lockb0x.Verifier;

/// <summary>
/// Delegate used to resolve the concrete storage resource for a Codex Entry.
/// </summary>
/// <param name="entry">The entry being verified.</param>
/// <param name="cancellationToken">Token used to signal cancellation.</param>
/// <returns>The provider specific resource identifier (e.g. CID, object key) or <c>null</c> when unavailable.</returns>
public delegate Task<string?> StorageLocationResolver(CodexEntry entry, CancellationToken cancellationToken);

/// <summary>
/// Provides the dependency graph required to compose the verifier service.
/// </summary>
public sealed class VerifierDependencies
{
    public required ICodexEntryValidator Validator { get; init; }

    public required IJsonCanonicalizer Canonicalizer { get; init; }

    public required ISigningService SigningService { get; init; }

    public IReadOnlyDictionary<string, IStorageAdapter>? StorageAdapters { get; init; }

    public IStellarAnchorService? AnchorService { get; init; }

    public ICertificateService? CertificateService { get; init; }

    public IRevisionGraph? RevisionGraph { get; init; }

    public Func<string, CodexEntry?>? RevisionResolver { get; init; }

    public StorageLocationResolver? StorageLocationResolver { get; init; }

    public Func<AnchorProof, string?>? AnchorNetworkResolver { get; init; }

    public string? DefaultAnchorNetwork { get; init; }

    public int? MaxRevisionDepth { get; init; }
}
