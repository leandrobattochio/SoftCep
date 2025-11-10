using Microsoft.AspNetCore.Mvc.Testing;

namespace SoftCep.Integration.Tests.Infrastructure;

public abstract class BaseSoftTestBase<TFactory>(TFactory factory) : IClassFixture<TFactory>
    where TFactory : WebApplicationFactory<SoftCep.Api.Program>
{
    protected readonly HttpClient Client = factory.CreateClient();
}