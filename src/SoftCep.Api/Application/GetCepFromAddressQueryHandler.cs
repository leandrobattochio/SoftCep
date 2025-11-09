using Microsoft.Extensions.Caching.Hybrid;
using SoftCep.Api.Core;
using SoftCep.Api.Domain;
using SoftCep.Api.Infrastructure.ViaCep;

namespace SoftCep.Api.Application;

public class GetCepFromAddressQueryHandler(
    HybridCache cache,
    IViaCepClient viaCepClient,
    ILogger<GetCepQueryHandler> logger)
{
    public async Task<List<CepResult>> HandleAsync(GetCepFromAddressQuery query, CancellationToken cancellationToken)
    {
        var viaCepResults = await GetCepFromCache(query.State, query.City, query.Term, cancellationToken);
        return viaCepResults.ViaCepToCepResults();
    }


    private async Task<List<ViaCepJsonModel>> GetCepFromCache(string state, string city, string term,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"cache/cep/address/{state}/{city}/{term}";
        return await cache.GetOrCreateAsync(
            cacheKey,
            async cancel =>
            {
                logger.LogDebug("Cache key expired. Calling ViaCep. {Key}", cacheKey);
                return await viaCepClient.GetCepAsync(state, city, term, cancel);
            },
            options: new HybridCacheEntryOptions()
            {
                Expiration = Consts.CepCacheTime
            },
            cancellationToken: cancellationToken
        );
    }
}