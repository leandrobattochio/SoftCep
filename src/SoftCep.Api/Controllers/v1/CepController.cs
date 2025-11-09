using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SoftCep.Api.Application;
using SoftCep.Api.Core;
using SoftCep.Api.Domain;

namespace SoftCep.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public sealed class CepController : ControllerBase
{
    [EnableRateLimiting("PerIp20Rps")]
    [MapToApiVersion("1.0")]
    [HttpGet("{cep}")]
    [EndpointDescription("Pesquisa um CEP específico.")]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(CepResult), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetCepAsync([FromRoute, CepValidation, MinLength(8), MaxLength(8)] string cep,
        [FromServices] GetCepQueryHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCepQuery(cep);
        var result = await handler.HandleAsync(query, cancellationToken);

        if (result == null)
            return NoContent();

        var seconds = (int)Consts.CepCacheTime.TotalSeconds;
        Response.Headers.CacheControl = $"public, max-age={seconds}";
        Response.Headers.Expires = DateTime.UtcNow.Add(Consts.CepCacheTime).ToString("R");
        Response.Headers["X-Cache-Duration"] = seconds.ToString();

        return new OkObjectResult(result);
    }


    [EnableRateLimiting("PerIp20Rps")]
    [MapToApiVersion("1.0")]
    [HttpGet("{state}/{city}/{term}")]
    [EndpointDescription("Pesquisa um CEP a partir do endereço informado.")]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(List<CepResult>), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetCepAsync([FromRoute] BrazilianState state,
        [FromRoute, MinLength(3)] string city,
        [FromRoute, MinLength(3)] string term, [FromServices] GetCepFromAddressQueryHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCepFromAddressQuery(state, city, term);
        var result = await handler.HandleAsync(query, cancellationToken);

        if (result.Count == 0)
            return NoContent();

        var seconds = (int)Consts.CepCacheTime.TotalSeconds;
        Response.Headers.CacheControl = $"public, max-age={seconds}";
        Response.Headers.Expires = DateTime.UtcNow.Add(Consts.CepCacheTime).ToString("R");
        Response.Headers["X-Cache-Duration"] = seconds.ToString();

        return new OkObjectResult(result);
    }
}