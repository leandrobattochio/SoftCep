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

        var successResponses = new List<HttpStatusCode>();
        for (var i = 0; i < 10; i++)
        {
            var rCep = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cep/17209660");
            rCep.Headers.Add("X-Test-IP", ip);
            var rAddr = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cep/SP/Sao Paulo/Praca");
            rAddr.Headers.Add("X-Test-IP", ip);

            var responses = await Task.WhenAll(Client.SendAsync(rCep), Client.SendAsync(rAddr));
            successResponses.Add(responses[0].StatusCode);
            successResponses.Add(responses[1].StatusCode);
        }

        successResponses.Count.ShouldBe(20);
        successResponses.ShouldAllBe(s => s == HttpStatusCode.OK);

        var blocked = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cep/17209660");
        blocked.Headers.Add("X-Test-IP", ip);
        var blockedResp = await Client.SendAsync(blocked);
        blockedResp.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        await Task.Delay(1050);
        var afterWindow = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cep/SP/Sao Paulo/Praca");
        afterWindow.Headers.Add("X-Test-IP", ip);
        var afterResp = await Client.SendAsync(afterWindow);
        afterResp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}