using System.Net;
using Shouldly;
using SoftCep.Integration.Tests.Infrastructure;

namespace SoftCep.Integration.Tests.Controllers;

public class CepControllerRateLimitSoftTests(RateLimitWebApplicationFactory factory) : RateLimitSoftTestBase(factory)
{
    [Fact]
    public async Task Should_RateLimit_SearchByCep_Per_IP()
    {
        const string ip = "9.9.9.9";

        var initialRequests = new List<HttpRequestMessage>();
        for (var i = 0; i < 21; i++)
        {
            var rCep = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cep/17209660");
            rCep.Headers.Add("X-Test-IP", ip);
            initialRequests.Add(rCep);
        }

        var responses = await Task.WhenAll(initialRequests.Select(r => Client.SendAsync(r)));
        responses.Length.ShouldBe(21);
        responses.Count(c => c.StatusCode == HttpStatusCode.OK).ShouldBe(20);
        responses.Count(c => c.StatusCode == HttpStatusCode.TooManyRequests).ShouldBe(1);

        await Task.Delay(1100);
        var afterWindow = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cep/17209660");
        afterWindow.Headers.Add("X-Test-IP", ip);
        var afterResp = await Client.SendAsync(afterWindow);
        afterResp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_RateLimit_SearchByTerm_Per_IP()
    {
        const string ip = "9.9.9.9";

        var initialRequests = new List<HttpRequestMessage>();
        for (var i = 0; i < 21; i++)
        {
            var rCep = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cep/SP/Sao Paulo/Praca");
            rCep.Headers.Add("X-Test-IP", ip);
            initialRequests.Add(rCep);
        }

        var responses = await Task.WhenAll(initialRequests.Select(r => Client.SendAsync(r)));
        responses.Length.ShouldBe(21);
        responses.Count(c => c.StatusCode == HttpStatusCode.OK).ShouldBe(20);
        responses.Count(c => c.StatusCode == HttpStatusCode.TooManyRequests).ShouldBe(1);

        await Task.Delay(1100);
        var afterWindow = new HttpRequestMessage(HttpMethod.Get, "/api/v1/cep/SP/Sao Paulo/Praca");
        afterWindow.Headers.Add("X-Test-IP", ip);
        var afterResp = await Client.SendAsync(afterWindow);
        afterResp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}