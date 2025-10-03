using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Lockb0x.Core.Models;
using Lockb0x.Core.Revision;

namespace Lockb0x.Verifier;

/// <summary>
/// Represents the traversal result of a Codex Entry revision chain.
/// </summary>
public sealed class RevisionChainResult
{
    public RevisionChainResult(IReadOnlyList<CodexEntry> entries, IReadOnlyList<RevisionChainIssue> issues)
    {
        Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        Issues = issues ?? throw new ArgumentNullException(nameof(issues));
    }

    public IReadOnlyList<CodexEntry> Entries { get; }

    public IReadOnlyList<RevisionChainIssue> Issues { get; }

    public bool Success => Issues.All(issue => issue.Severity != RevisionChainIssueSeverity.Error);

    internal static RevisionChainResult FromTraversal(RevisionTraversalResult traversal)
    {
        ArgumentNullException.ThrowIfNull(traversal);

        var issues = traversal.Issues
            .Select(issue => new RevisionChainIssue(
                issue.Code,
                issue.Message,
                issue.Severity == RevisionIssueSeverity.Error
                    ? RevisionChainIssueSeverity.Error
                    : RevisionChainIssueSeverity.Warning,
                issue.EntryId))
            .ToList();

        return new RevisionChainResult(traversal.Chain, new ReadOnlyCollection<RevisionChainIssue>(issues));
    }
}

public sealed record RevisionChainIssue(string Code, string Message, RevisionChainIssueSeverity Severity, string? EntryId = null);

public enum RevisionChainIssueSeverity
{
    Warning,
    Error
}
