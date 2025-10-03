using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lockb0x.Verifier;

/// <summary>
/// Represents the aggregated outcome of the verification pipeline.
/// </summary>
public sealed class VerificationResult
{
    private readonly List<VerificationStepResult> _steps = new();

    public IReadOnlyList<VerificationStepResult> Steps => _steps.AsReadOnly();

    public bool IsValid => _steps.All(step => step.Status != VerificationStepStatus.Failed);

    public IReadOnlyList<VerificationMessage> Errors => new ReadOnlyCollection<VerificationMessage>(_steps
        .SelectMany(step => step.Messages.Where(message => message.Severity == VerificationMessageSeverity.Error))
        .ToList());

    public IReadOnlyList<VerificationMessage> Warnings => new ReadOnlyCollection<VerificationMessage>(_steps
        .SelectMany(step => step.Messages.Where(message => message.Severity == VerificationMessageSeverity.Warning))
        .ToList());

    internal void AddStep(VerificationStepResult step)
    {
        ArgumentNullException.ThrowIfNull(step);
        _steps.Add(step);
    }
}

/// <summary>
/// Represents the outcome of a single verification step.
/// </summary>
public sealed class VerificationStepResult
{
    private readonly List<VerificationMessage> _messages = new();
    private readonly Dictionary<string, string> _metadata = new(StringComparer.OrdinalIgnoreCase);

    public VerificationStepResult(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Status = VerificationStepStatus.Succeeded;
    }

    public string Name { get; }

    public VerificationStepStatus Status { get; internal set; }

    public IReadOnlyList<VerificationMessage> Messages => _messages.AsReadOnly();

    public IReadOnlyDictionary<string, string> Metadata => new ReadOnlyDictionary<string, string>(_metadata);

    internal void AddMessage(VerificationMessage message) => _messages.Add(message);

    internal void SetMetadata(string key, string value) => _metadata[key] = value;

    internal void FinalizeStatus()
    {
        if (Status == VerificationStepStatus.Skipped)
        {
            return;
        }

        if (_messages.Any(message => message.Severity == VerificationMessageSeverity.Error))
        {
            Status = VerificationStepStatus.Failed;
            return;
        }

        if (_messages.Any(message => message.Severity == VerificationMessageSeverity.Warning))
        {
            Status = VerificationStepStatus.SucceededWithWarnings;
            return;
        }

        Status = VerificationStepStatus.Succeeded;
    }
}

/// <summary>
/// Provides helper methods for emitting messages and metadata during verification.
/// </summary>
public sealed class VerificationStepContext
{
    private readonly VerificationStepResult _step;

    internal VerificationStepContext(VerificationStepResult step)
    {
        _step = step ?? throw new ArgumentNullException(nameof(step));
    }

    public void AddError(string code, string message, string? path = null)
        => _step.AddMessage(new VerificationMessage(code, message, VerificationMessageSeverity.Error, path));

    public void AddWarning(string code, string message, string? path = null)
        => _step.AddMessage(new VerificationMessage(code, message, VerificationMessageSeverity.Warning, path));

    public void AddInfo(string code, string message, string? path = null)
        => _step.AddMessage(new VerificationMessage(code, message, VerificationMessageSeverity.Info, path));

    public void AddMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Metadata keys must be non-empty.", nameof(key));
        }

        _step.SetMetadata(key, value);
    }

    public void Skip(string code, string message)
    {
        AddInfo(code, message);
        _step.Status = VerificationStepStatus.Skipped;
    }

    internal bool IsSkipped => _step.Status == VerificationStepStatus.Skipped;
}

public sealed record VerificationMessage(string Code, string Message, VerificationMessageSeverity Severity, string? Path = null);

public enum VerificationMessageSeverity
{
    Info,
    Warning,
    Error
}

public enum VerificationStepStatus
{
    Succeeded,
    SucceededWithWarnings,
    Failed,
    Skipped
}
