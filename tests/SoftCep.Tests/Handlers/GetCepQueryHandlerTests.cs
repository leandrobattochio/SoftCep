using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SoftCep.Api.Application;
using SoftCep.Api.Infrastructure.ViaCep;
using Moq;

namespace SoftCep.Tests.Handlers;

public class GetCepQueryHandlerTests
{
    private static ViaCepJsonModel MakeOk(string cep) => new(
        cep, "Logradouro", "Comp", "Unidade", "Bairro", "Localidade", "UF", "Estado", "Regiao", "Ibge", "Gia", "11", "Siafi", "false");
    private static ViaCepJsonModel MakeErro(string cep) => new(
        cep, "Logradouro", "Comp", "Unidade", "Bairro", "Localidade", "UF", "Estado", "Regiao", "Ibge", "Gia", "11", "Siafi", "true");

    private static Microsoft.Extensions.Caching.Hybrid.HybridCache CreateCache()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        return services.BuildServiceProvider().GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
    }

    [Fact]
    public async Task Should_Return_Result_On_Cache_Miss_And_Store_In_Cache()
    {
        var cache = CreateCache();
        var mockClient = new Mock<IViaCepClient>(MockBehavior.Strict);
        mockClient.Setup(c => c.GetCepAsync("01001000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeOk("01001000"));

        var handler = new GetCepQueryHandler(cache, mockClient.Object, NullLogger<GetCepQueryHandler>.Instance);

        var result = await handler.HandleAsync(new GetCepQuery("01001000"), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.Cep.ShouldBe("01001000");

        var second = await handler.HandleAsync(new GetCepQuery("01001000"), CancellationToken.None);
        second.ShouldNotBeNull();

        mockClient.Verify(c => c.GetCepAsync("01001000", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Return_Null_When_ViaCep_Returns_Error_Flag()
    {
        var cache = CreateCache();
        var mockClient = new Mock<IViaCepClient>(MockBehavior.Strict);
        mockClient.Setup(c => c.GetCepAsync("99999999", It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeErro("99999999"));

        var handler = new GetCepQueryHandler(cache, mockClient.Object, NullLogger<GetCepQueryHandler>.Instance);

        var result = await handler.HandleAsync(new GetCepQuery("99999999"), CancellationToken.None);
        result.ShouldBeNull();
        mockClient.Verify(c => c.GetCepAsync("99999999", It.IsAny<CancellationToken>()), Times.Once);
    }
}
