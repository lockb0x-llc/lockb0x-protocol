using Lockb0x.Core.Models;

namespace Lockb0x.Core.Validation;

public interface ICodexEntryValidator
{
    ValidationResult Validate(CodexEntry entry, CodexEntryValidationContext? context = null);
}
