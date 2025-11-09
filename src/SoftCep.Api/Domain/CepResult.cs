using Riok.Mapperly.Abstractions;
using SoftCep.Api.Infrastructure.ViaCep;

namespace SoftCep.Api.Domain;

public record CepResult(
    string Cep,
    string Logradouro,
    string Complemento,
    string Bairro,
    string Localidade,
    string Uf,
    string Regiao,
    string Ddd);

[Mapper]
public static partial class CepResultMapper
{
#pragma warning disable RMG020
    public static partial CepResult ViaCepToCepResult(this ViaCepJsonModel result);
    public static partial List<CepResult> ViaCepToCepResults(this List<ViaCepJsonModel> result);
#pragma warning restore RMG020
}