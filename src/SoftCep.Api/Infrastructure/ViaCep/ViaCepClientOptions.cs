using System.Diagnostics.CodeAnalysis;

namespace SoftCep.Api.Infrastructure.ViaCep;

[ExcludeFromCodeCoverage]
public class ViaCepClientOptions
{
    public string BaseUrl { get; set; } = string.Empty;
}