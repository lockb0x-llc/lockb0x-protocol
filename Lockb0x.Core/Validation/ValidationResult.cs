using System.Collections.ObjectModel;
using System.Linq;

namespace Lockb0x.Core.Validation;

public sealed class ValidationResult
{
    public bool Success => Errors.Count == 0;

    public IReadOnlyList<CodexEntryValidationError> Errors { get; }

    public IReadOnlyList<CodexEntryValidationWarning> Warnings { get; }

    public ValidationResult(IEnumerable<CodexEntryValidationError>? errors = null, IEnumerable<CodexEntryValidationWarning>? warnings = null)
    {
        Errors = new ReadOnlyCollection<CodexEntryValidationError>((errors ?? Array.Empty<CodexEntryValidationError>()).ToList());
        Warnings = new ReadOnlyCollection<CodexEntryValidationWarning>((warnings ?? Array.Empty<CodexEntryValidationWarning>()).ToList());
    }

    public static ValidationResult FromError(string code, string message, string? path = null)
        => new(new[] { new CodexEntryValidationError(code, message, path) });

    public static ValidationResult SuccessResult { get; } = new();
}

public sealed record CodexEntryValidationError(string Code, string Message, string? Path = null);

public sealed record CodexEntryValidationWarning(string Code, string Message, string? Path = null);

public sealed class CodexEntryValidationContext
{
    public CodexEntryValidationContext(string? network = null)
    {
        Network = network;
    }

    /// <summary>
    /// Optional anchor network hint (e.g., "stellar:pubnet") used for CAIP-2 validation.
    /// </summary>
    public string? Network { get; }
}
