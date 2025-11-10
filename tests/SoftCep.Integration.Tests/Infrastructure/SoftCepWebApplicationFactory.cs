using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SoftCep.Api.Core;
using SoftCep.Api.Infrastructure.ViaCep;

namespace SoftCep.Integration.Tests.Infrastructure;

public class SoftCepWebApplicationFactory : WebApplicationFactory<SoftCep.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Consts.TestEnvironmentName);

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings__Redis"] = "redis:6379"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IViaCepClient));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            var mock = new Mock<IViaCepClient>();
            CreateMocks(mock);

            services.AddSingleton(mock.Object);
        });
    }

    private static void CreateMocks(Mock<IViaCepClient> mock)
    {
        mock.Setup(c => c.GetCepAsync(It.Is<string>(cep => cep != "17209660"), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string cep, CancellationToken _) => new ViaCepJsonModel(
                cep,
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "true"));

        mock.Setup(c => c.GetCepAsync("17209660", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ViaCepJsonModel(
                "17209660",
                "Rua Teste",
                "Comp",
                "Unidade",
                "Bairro",
                "Cidade",
                "SP",
                "Estado",
                "Regiao",
                "Ibge",
                "Gia",
                "11",
                "Siafi"));

        mock.Setup(c => c.GetCepAsync(
                It.Is<string>(s => s == "SP"),
                It.Is<string>(f => f == "Sao Paulo"),
                It.Is<string>(t => t == "Praca"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ViaCepJsonModel(
                    "01001000", "Rua A", "Comp", "Unidade", "Bairro", "Sao Paulo", "SP", "Estado", "Regiao", "Ibge",
                    "Gia", "11", "Siafi", "false"),

                new ViaCepJsonModel(
                    "01002000", "Rua B", "Comp", "Unidade", "Bairro", "Sao Paulo", "SP", "Estado", "Regiao", "Ibge",
                    "Gia", "11", "Siafi", "false")
            ]);

        mock.Setup(c => c.GetCepAsync(
                It.Is<string>(s => s == "SP"),
                It.Is<string>(f => f == "Sao Paulo"),
                It.Is<string>(t => t != "Praca"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }
}