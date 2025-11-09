namespace SoftCep.Api.Application;

using SoftCep.Api.Domain;

public record GetCepFromAddressQuery(BrazilianState State, string City, string Term);
