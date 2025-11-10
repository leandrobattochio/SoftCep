using System.Net;
using Shouldly;
using SoftCep.Api.Core;
using SoftCep.Integration.Tests.Infrastructure;

namespace SoftCep.Integration.Tests.Controllers;

public class CepControllerRateLimitSoftTests(RateLimitWebApplicationFactory factory) : RateLimitSoftTestBase(factory)
{
    [Theory]
    [InlineData("/api/v1/cep/17209660")]
    [InlineData("/api/v1/cep/SP/Sao Paulo/Praca")]
    public async Task Should_RateLimit_Both_Routes_Per_IP(string endpoint)
    {
        // Arrange
        const string ip = "9.9.9.9";

        var initialRequests = new List<HttpRequestMessage>();
        for (var i = 0; i < Consts.MaxRequestPerIp; i++)
        {
            var rCep = new HttpRequestMessage(HttpMethod.Get, endpoint);
            rCep.Headers.Add("X-Test-IP", ip);
            initialRequests.Add(rCep);
        }

        // Act
        var responses = await Task.WhenAll(initialRequests.Select(r => Client.SendAsync(r)));
        responses.Length.ShouldBe(Consts.MaxRequestPerIp);
        responses.ShouldAllBe(r => r.StatusCode == HttpStatusCode.OK);


        // Assert
        var blocked = new HttpRequestMessage(HttpMethod.Get, endpoint);
        blocked.Headers.Add("X-Test-IP", ip);
        var blockedResp = await Client.SendAsync(blocked);
        blockedResp.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        await Task.Delay(Consts.RequestWindow.Add(TimeSpan.FromMilliseconds(100)));

        var afterWindow = new HttpRequestMessage(HttpMethod.Get, endpoint);
        afterWindow.Headers.Add("X-Test-IP", ip);
        var afterResp = await Client.SendAsync(afterWindow);
        afterResp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}