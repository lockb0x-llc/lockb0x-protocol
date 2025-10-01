using System.Collections.ObjectModel;
using System.Linq;
using Lockb0x.Core.Models;

namespace Lockb0x.Core.Revision;

public sealed class RevisionTraversalResult
{
    public RevisionTraversalResult(IEnumerable<CodexEntry> chain, IEnumerable<RevisionTraversalIssue> issues)
    {
        Chain = new ReadOnlyCollection<CodexEntry>(chain?.ToList() ?? new List<CodexEntry>());
        Issues = new ReadOnlyCollection<RevisionTraversalIssue>(issues?.ToList() ?? new List<RevisionTraversalIssue>());
    }

    public IReadOnlyList<CodexEntry> Chain { get; }

    public IReadOnlyList<RevisionTraversalIssue> Issues { get; }

    public bool Success => Issues.All(issue => issue.Severity != RevisionIssueSeverity.Error);
}

public sealed record RevisionTraversalIssue(string Code, string Message, RevisionIssueSeverity Severity, string? EntryId = null);

public enum RevisionIssueSeverity
{
    Warning,
    Error
}
