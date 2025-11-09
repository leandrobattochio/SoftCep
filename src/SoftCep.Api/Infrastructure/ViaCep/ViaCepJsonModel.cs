namespace SoftCep.Api.Infrastructure.ViaCep;

public record ViaCepJsonModel(
    string? Cep,
    string? Logradouro,
    string? Complemento,
    string? Unidade,
    string? Bairro,
    string? Localidade,
    string? Uf,
    string? Estado,
    string? Regiao,
    string? Ibge,
    string? Gia,
    string? Ddd,
    string? Siafi,
    string? Erro = "false");