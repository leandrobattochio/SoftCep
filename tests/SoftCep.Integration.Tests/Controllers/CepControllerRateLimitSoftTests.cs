using System.Net;
using Shouldly;
using SoftCep.Integration.Tests.Infrastructure;

namespace SoftCep.Integration.Tests.Controllers;

public class CepControllerRateLimitSoftTests(RateLimitWebApplicationFactory factory) : RateLimitSoftTestBase(factory)
{
    [Fact]
    public async Task Should_RateLimit_Both_Routes_Per_IP()
    {
        const string ip = "9.9.9.9";

        var initialRequests = new List<HttpRequestMessage>();
        for (var i = 0; i < 10; i++)
        {
            var rCep = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cep/17209660");
            rCep.Headers.Add("X-Test-IP", ip);
            initialRequests.Add(rCep);

            var rAddr = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cep/SP/Sao Paulo/Praca");
            rAddr.Headers.Add("X-Test-IP", ip);
            initialRequests.Add(rAddr);
        }

        var responses = await Task.WhenAll(initialRequests.Select(r => Client.SendAsync(r)));
        responses.Length.ShouldBe(20);
        responses.ShouldAllBe(r => r.StatusCode == HttpStatusCode.OK);

        var blocked = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cep/17209660");
        blocked.Headers.Add("X-Test-IP", ip);
        var blockedResp = await Client.SendAsync(blocked);
        blockedResp.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        await Task.Delay(1100);
        var afterWindow = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cep/SP/Sao Paulo/Praca");
        afterWindow.Headers.Add("X-Test-IP", ip);
        var afterResp = await Client.SendAsync(afterWindow);
        afterResp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}