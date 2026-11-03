using System.ComponentModel.DataAnnotations;

namespace Software.Api.Infra;

[AttributeUsage(AttributeTargets.Class)]
public class AtLeastOneOfAttribute(params string[] propertyNames): ValidationAttribute 
{
    public string[] PropertyNames { get; } = propertyNames;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var hasOne = PropertyNames.Any(name => !string.IsNullOrWhiteSpace(value?.GetType().GetProperty(name)?.GetValue(value) as string));

        return hasOne ? ValidationResult.Success : new ValidationResult($"We need at least one of: {string.Join(", ", PropertyNames)}", PropertyNames);
    }
}
