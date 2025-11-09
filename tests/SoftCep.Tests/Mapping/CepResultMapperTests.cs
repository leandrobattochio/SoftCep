using Shouldly;
using SoftCep.Api.Domain;
using SoftCep.Api.Infrastructure.ViaCep;

namespace SoftCep.Tests.Mapping;

public class CepResultMapperTests
{
    [Fact]
    public void Should_Map_Fields_Correctly()
    {
        var via = new ViaCepJsonModel(
            "01001000","Logradouro","Comp","Unidade","Bairro","Localidade","UF","Estado","Regiao","Ibge","Gia","11","Siafi","false");

        var mapped = via.ViaCepToCepResult();
        mapped.ShouldNotBeNull();
        mapped.Cep.ShouldBe(via.Cep);
        mapped.Logradouro.ShouldBe(via.Logradouro);
        mapped.Complemento.ShouldBe(via.Complemento);
        mapped.Bairro.ShouldBe(via.Bairro);
        mapped.Localidade.ShouldBe(via.Localidade);
        mapped.Uf.ShouldBe(via.Uf);
        mapped.Regiao.ShouldBe(via.Regiao);
        mapped.Ddd.ShouldBe(via.Ddd);
    }

    [Fact]
    public void Should_Map_List()
    {
        var list = new List<ViaCepJsonModel>{
            new("01001000","Logradouro","Comp","Unidade","Bairro","Localidade","UF","Estado","Regiao","Ibge","Gia","11","Siafi","false"),
            new("01002000","Logradouro2","Comp2","Unidade2","Bairro2","Localidade2","UF","Estado","Regiao","Ibge","Gia","11","Siafi","false")
        };

        var mapped = list.ViaCepToCepResults();
        mapped.Count.ShouldBe(2);
        mapped[1].Cep.ShouldBe("01002000");
    }
}

