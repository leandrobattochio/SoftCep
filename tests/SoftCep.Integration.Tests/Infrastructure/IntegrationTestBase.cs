namespace SoftCep.Integration.Tests.Infrastructure;


public abstract class IntegrationTestBase(SoftCepWebApplicationFactory factory)
    : IClassFixture<SoftCepWebApplicationFactory>
{
    protected readonly HttpClient Client = factory.CreateClient();
}