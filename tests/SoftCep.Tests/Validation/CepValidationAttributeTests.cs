using Shouldly;
using SoftCep.Api.Core;

namespace SoftCep.Tests.Validation;

public class CepValidationAttributeTests
{
    private readonly CepValidationAttribute _attr = new();

    [Theory]
    [InlineData("01001000")]
    [InlineData("01001-000")]
    public void Valid_Cep_Should_Pass(string cep)
    {
        var result = _attr.GetValidationResult(cep, new System.ComponentModel.DataAnnotations.ValidationContext(new object()));
        result.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234567")] // 7 digits
    [InlineData("123456789")] // 9 digits
    [InlineData("ABCDEF12")] // non numeric
    public void Invalid_Cep_Should_Return_Error(object? cep)
    {
        var result = _attr.GetValidationResult(cep, new System.ComponentModel.DataAnnotations.ValidationContext(new object()));
        result.ShouldNotBeNull();
        result!.ErrorMessage.ShouldNotBeNull();
    }
}

