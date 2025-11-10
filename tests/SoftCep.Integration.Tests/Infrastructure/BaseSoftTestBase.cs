using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.Redis;

namespace SoftCep.Integration.Tests.Infrastructure;

public abstract class BaseSoftTestBase<TFactory>(TFactory factory) : IClassFixture<TFactory>, IAsyncLifetime
    where TFactory : WebApplicationFactory<SoftCep.Api.Program>
{
    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .WithName("redis")
        .WithCleanUp(true)
        .Build();

    protected readonly HttpClient Client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _redisContainer.DisposeAsync();
    }
}