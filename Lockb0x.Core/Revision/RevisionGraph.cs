using System;
using System.Collections.Generic;
using Lockb0x.Core.Models;

namespace Lockb0x.Core.Revision;

public sealed class RevisionGraph : IRevisionGraph
{
    public RevisionTraversalResult Traverse(CodexEntry head, Func<string, CodexEntry?> resolver, int? maxDepth = null)
    {
        if (head is null) throw new ArgumentNullException(nameof(head));
        if (resolver is null) throw new ArgumentNullException(nameof(resolver));

        var chain = new List<CodexEntry> { head };
        var issues = new List<RevisionTraversalIssue>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        if (!string.IsNullOrWhiteSpace(head.Id))
        {
            seen.Add(head.Id);
        }

        var current = head;
        var depth = 0;
        while (!string.IsNullOrWhiteSpace(current.PreviousId))
        {
            if (maxDepth is not null && depth >= maxDepth)
            {
                issues.Add(new("core.revision.max_depth", "Maximum revision traversal depth reached", RevisionIssueSeverity.Warning, current.Id));
                break;
            }

            if (!seen.Add(current.PreviousId!))
            {
                issues.Add(new("core.revision.cycle_detected", "Cycle detected in revision chain", RevisionIssueSeverity.Error, current.PreviousId));
                break;
            }

            var predecessor = resolver(current.PreviousId!);
            if (predecessor is null)
            {
                issues.Add(new("core.revision.missing_predecessor", $"Revision predecessor '{current.PreviousId}' could not be resolved", RevisionIssueSeverity.Error, current.PreviousId));
                break;
            }

            chain.Add(predecessor);
            current = predecessor;
            depth++;
        }

        return new RevisionTraversalResult(chain, issues);
    }
}
