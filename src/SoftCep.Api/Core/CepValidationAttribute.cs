using System.ComponentModel.DataAnnotations;

namespace SoftCep.Api.Core;

[AttributeUsage(AttributeTargets.Parameter)]
public class CepValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult("Cep inválido!");

        var cep = value.ToString()!.Replace("-", "").Trim();
        if (cep.Length != 8 || !long.TryParse(cep, out _))
            return new ValidationResult("Cep inválido!");

        return ValidationResult.Success;
    }
}