using System.Net;
using Shouldly;
using SoftCep.Integration.Tests.Infrastructure;

namespace SoftCep.Integration.Tests.Controllers;

public class CepControllerRateLimitTests(RateLimitWebApplicationFactory factory)
    : IClassFixture<RateLimitWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_RateLimit_Both_Routes_Per_IP()
    {
        const string ip = "9.9.9.9";

        var successResponses = new List<HttpStatusCode>();
        for (var i = 0; i < 10; i++)
        {
            var rCep = new HttpRequestMessage(HttpMethod.Get, "/api/cep/17209660");
            rCep.Headers.Add("X-Test-IP", ip);
            var rAddr = new HttpRequestMessage(HttpMethod.Get, "/api/cep/SP/Sao Paulo/Praca");
            rAddr.Headers.Add("X-Test-IP", ip);

            var responses = await Task.WhenAll(_client.SendAsync(rCep), _client.SendAsync(rAddr));
            successResponses.Add(responses[0].StatusCode);
            successResponses.Add(responses[1].StatusCode);
        }

        successResponses.Count.ShouldBe(20);
        successResponses.ShouldAllBe(s => s == HttpStatusCode.OK);

        var blocked = new HttpRequestMessage(HttpMethod.Get, "/api/cep/17209660");
        blocked.Headers.Add("X-Test-IP", ip);
        var blockedResp = await _client.SendAsync(blocked);
        blockedResp.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        await Task.Delay(1050);
        var afterWindow = new HttpRequestMessage(HttpMethod.Get, "/api/cep/SP/Sao Paulo/Praca");
        afterWindow.Headers.Add("X-Test-IP", ip);
        var afterResp = await _client.SendAsync(afterWindow);
        afterResp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}