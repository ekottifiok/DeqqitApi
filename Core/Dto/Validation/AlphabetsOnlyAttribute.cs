using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Core.Dto.Validation;

public partial class AlphabetsOnlyAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IEnumerable<string> tags) return ValidationResult.Success;

        foreach (string tag in tags)
            // Allows only a-z and A-Z. No spaces, numbers, or symbols.
            if (!MyRegex().IsMatch(tag))
                return new ValidationResult($"Tag '{tag}' is invalid. Tags must contain only alphabets.");

        return ValidationResult.Success;
    }

    [GeneratedRegex(@"^[a-zA-Z]+$")]
    private static partial Regex MyRegex();
}