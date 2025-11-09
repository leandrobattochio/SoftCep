using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SoftCep.Api.Application;
using SoftCep.Api.Infrastructure.ViaCep;
using Moq;

namespace SoftCep.Tests.Handlers;

public class GetCepFromAddressQueryHandlerTests
{
    private static ViaCepJsonModel Make(string cep, string erro = "false") => new(
        cep, "Logradouro", "Comp", "Unidade", "Bairro", "Localidade", "UF", "Estado", "Regiao", "Ibge", "Gia", "11",
        "Siafi", erro);

    private static HybridCache CreateCache()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }

    [Fact]
    public async Task Should_Return_List_And_Cache_Result()
    {
        var cache = CreateCache();
        var mockClient = new Mock<IViaCepClient>(MockBehavior.Strict);
        mockClient.Setup(c => c.GetCepAsync("SP", "Sao Paulo", "Praca", It.IsAny<CancellationToken>()))
            .ReturnsAsync([Make("01001000"), Make("01002000")]);

        var handler =
            new GetCepFromAddressQueryHandler(cache, mockClient.Object, NullLogger<GetCepQueryHandler>.Instance);

        var result = await handler.HandleAsync(new GetCepFromAddressQuery("SP", "Sao Paulo", "Praca"),
            CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);

        var second = await handler.HandleAsync(new GetCepFromAddressQuery("SP", "Sao Paulo", "Praca"),
            CancellationToken.None);
        second.Count.ShouldBe(2);

        mockClient.Verify(c => c.GetCepAsync("SP", "Sao Paulo", "Praca", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Map_All_Items_Even_With_Error_Flag_CurrentBehavior()
    {
        var cache = CreateCache();
        var mockClient = new Mock<IViaCepClient>(MockBehavior.Strict);
        mockClient.Setup(c => c.GetCepAsync("SP", "Sao Paulo", "Praca", It.IsAny<CancellationToken>()))
            .ReturnsAsync([Make("01001000", "false"), Make("01002000", "true")]);

        var handler =
            new GetCepFromAddressQueryHandler(cache, mockClient.Object, NullLogger<GetCepQueryHandler>.Instance);

        var result = await handler.HandleAsync(new GetCepFromAddressQuery("SP", "Sao Paulo", "Praca"),
            CancellationToken.None);
        result.Count.ShouldBe(2);
        mockClient.Verify(c => c.GetCepAsync("SP", "Sao Paulo", "Praca", It.IsAny<CancellationToken>()), Times.Once);
    }
}