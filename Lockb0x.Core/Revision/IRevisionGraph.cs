using Lockb0x.Core.Models;

namespace Lockb0x.Core.Revision;

public interface IRevisionGraph
{
    RevisionTraversalResult Traverse(CodexEntry head, Func<string, CodexEntry?> resolver, int? maxDepth = null);
}
