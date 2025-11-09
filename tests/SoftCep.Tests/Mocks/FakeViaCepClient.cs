using SoftCep.Api.Infrastructure.ViaCep;
namespace SoftCep.Tests.Mocks;

public class FakeViaCepClient : IViaCepClient
{
    public Func<string, ViaCepJsonModel>? CepResponder { get; set; }
    public Func<string,string,string,List<ViaCepJsonModel>>? AddressResponder { get; set; }

    public Task<ViaCepJsonModel> GetCepAsync(string cep, CancellationToken cancellationToken = default)
    {
        return CepResponder == null ? throw new InvalidOperationException("CepResponder not configured") : Task.FromResult(CepResponder(cep));
    }

    public Task<List<ViaCepJsonModel>> GetCepAsync(string state, string city, string term, CancellationToken cancellationToken = default)
    {
        return AddressResponder == null ? throw new InvalidOperationException("AddressResponder not configured") : Task.FromResult(AddressResponder(state, city, term));
    }
}
