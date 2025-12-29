using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Core.Dto.Validation;

public partial class ValidTemplateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text) return ValidationResult.Success;

        // Regex explanation:
        // {{ : matches the literal opening braces
        // ([^{}]+) : captures everything inside that isn't a brace
        // }} : matches the literal closing braces
        MatchCollection matches = MyRegex().Matches(text);

        foreach (Match match in matches)
        {
            string content = match.Groups[1].Value;

            // Check if alphabets only
            if (!MyRegex1().IsMatch(content))
                return new ValidationResult($"The placeholder '{{{{{content}}}}}' must contain only alphabets.");
        }

        return ValidationResult.Success;
    }

    [GeneratedRegex(@"\{\{([^{}]+)\}\}")]
    private static partial Regex MyRegex();

    [GeneratedRegex(@"^[a-zA-Z]+$")]
    private static partial Regex MyRegex1();
}