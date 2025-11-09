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
        var r1 = new HttpRequestMessage(HttpMethod.Get, "/api/cep/17209660");
        r1.Headers.Add("X-Test-IP", ip);
        var resp1 = await _client.SendAsync(r1);
        resp1.StatusCode.ShouldBe(HttpStatusCode.OK);

        var r2 = new HttpRequestMessage(HttpMethod.Get, "/api/cep/17209660");
        r2.Headers.Add("X-Test-IP", ip);
        var resp2 = await _client.SendAsync(r2);
        resp2.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        await Task.Delay(1100);

        var r3 = new HttpRequestMessage(HttpMethod.Get, "/api/cep/SP/Sao Paulo/Praca");
        r3.Headers.Add("X-Test-IP", ip);
        var resp3 = await _client.SendAsync(r3);
        resp3.StatusCode.ShouldBe(HttpStatusCode.OK);

        var r4 = new HttpRequestMessage(HttpMethod.Get, "/api/cep/SP/Sao Paulo/Praca");
        r4.Headers.Add("X-Test-IP", ip);
        var resp4 = await _client.SendAsync(r4);
        resp4.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }
}