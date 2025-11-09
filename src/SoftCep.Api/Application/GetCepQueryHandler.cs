using Microsoft.Extensions.Caching.Hybrid;
using SoftCep.Api.Core;
using SoftCep.Api.Domain;
using SoftCep.Api.Infrastructure.ViaCep;

namespace SoftCep.Api.Application;

public class GetCepQueryHandler(HybridCache cache, IViaCepClient viaCepClient, ILogger<GetCepQueryHandler> logger)
{
    public async Task<CepResult?> HandleAsync(GetCepQuery query, CancellationToken cancellationToken)
    {
        var viaCepResult = await GetCepFromCache(query.Cep, cancellationToken);
        return viaCepResult?.ViaCepToCepResult();
    }

    private async Task<ViaCepJsonModel?> GetCepFromCache(string cep, CancellationToken cancellationToken)
    {
        var cacheKey = $"cache/cep/{cep}";
        return await cache.GetOrCreateAsync(
            cacheKey,
            async cancel =>
            {
                logger.LogDebug("Cache key expired. Calling ViaCep. {Key}", cacheKey);
                var viaCepResult = await viaCepClient.GetCepAsync(cep, cancel);

                return viaCepResult.Erro == "false" ? viaCepResult : null;
            },
            options: new HybridCacheEntryOptions()
            {
                Expiration = Consts.CepCacheTime
            },
            cancellationToken: cancellationToken
        );
    }
}