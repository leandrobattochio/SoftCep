using Refit;

namespace SoftCep.Api.Infrastructure.ViaCep;

public interface IViaCepClient
{
    [Get("/{cep}/json")]
    Task<ViaCepJsonModel> GetCepAsync(string cep, CancellationToken cancellationToken = default);

    [Get("/{state}/{city}/{term}/json")]
    Task<List<ViaCepJsonModel>> GetCepAsync(string state, string city, string term,
        CancellationToken cancellationToken = default);
}