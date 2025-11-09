using System.Net;
using System.Net.Http.Json;
using Shouldly;
using SoftCep.Api.Domain;
using SoftCep.Integration.Tests.Infrastructure;

namespace SoftCep.Integration.Tests.Controllers;

public class CepControllerTests(SoftCepWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private static bool HasHeader(HttpResponseMessage response, string name)
    {
        var all = response.Headers.Select(h => h.Key).Concat(response.Content?.Headers.Select(h => h.Key) ?? Enumerable.Empty<string>());
        return all.Any(k => k.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<string> GetHeaderValues(HttpResponseMessage response, string name)
    {
        if (response.Headers.TryGetValues(name, out var v)) return v;
        if (response.Content?.Headers.TryGetValues(name, out var cv) ?? false) return cv;
        return [];
    }

    [Fact]
    public async Task Cep_ShouldReturnAddress_WhenValid()
    {
        // Act
        var response = await Client.GetAsync("/api/cep/17209660");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK); // controller returns 200

        HasHeader(response, "Cache-Control").ShouldBeTrue();
        GetHeaderValues(response, "Cache-Control").First().ShouldContain("max-age=");

        HasHeader(response, "Expires").ShouldBeTrue();

        HasHeader(response, "X-Cache-Duration").ShouldBeTrue();
        int.Parse(GetHeaderValues(response, "X-Cache-Duration").First()).ShouldBeGreaterThan(0);

        var result = await response.Content.ReadFromJsonAsync<CepResult>();
        result.ShouldNotBeNull();
        result!.Cep.ShouldBe("17209660");
        result.Logradouro.ShouldBe("Rua Teste");
        result.Uf.ShouldBe("SP");

        var second = await Client.GetAsync("/api/cep/17209660");
        second.StatusCode.ShouldBe(HttpStatusCode.OK);
        GetHeaderValues(second, "X-Cache-Duration").First().ShouldBe(GetHeaderValues(response, "X-Cache-Duration").First());
    }

    [Fact]
    public async Task Cep_ShouldReturn_NoContent_When_NotFound()
    {
        // Act
        var response = await Client.GetAsync("/api/cep/00000000");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        HasHeader(response, "Cache-Control").ShouldBeFalse();
        HasHeader(response, "Expires").ShouldBeFalse();
        HasHeader(response, "X-Cache-Duration").ShouldBeFalse();
        (await response.Content.ReadAsStringAsync()).ShouldBe(string.Empty);
    }

    [Fact]
    public async Task Address_ShouldReturn_List_WhenValid()
    {
        var response = await Client.GetAsync("/api/cep/SP/Sao Paulo/Praca");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        HasHeader(response, "Cache-Control").ShouldBeTrue();
        HasHeader(response, "Expires").ShouldBeTrue();
        HasHeader(response, "X-Cache-Duration").ShouldBeTrue();

        var list = await response.Content.ReadFromJsonAsync<List<CepResult>>();
        list.ShouldNotBeNull();
        list!.Count.ShouldBe(2);
        list.Select(c => c.Cep).ShouldBe(["01001000", "01002000"]);
    }

    [Fact]
    public async Task Address_ShouldReturn_NoContent_When_NoMatches()
    {
        var response = await Client.GetAsync("/api/cep/SP/Sao Paulo/AlgoQueNaoExiste");
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        HasHeader(response, "Cache-Control").ShouldBeFalse();
        HasHeader(response, "Expires").ShouldBeFalse();
        HasHeader(response, "X-Cache-Duration").ShouldBeFalse();
    }

    [Fact]
    public async Task Address_ShouldReturn_BadRequest_When_Invalid_State()
    {
        var response = await Client.GetAsync("/api/cep/XX/Sao Paulo/Praca");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Address_ShouldReturn_BadRequest_When_Invalid_City()
    {
        var response = await Client.GetAsync($"/api/cep/{BrazilianState.SP}/p/ppp");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Address_ShouldReturn_BadRequest_When_Invalid_Term()
    {
        var response = await Client.GetAsync($"/api/cep/{BrazilianState.SP}/ppp/p");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}