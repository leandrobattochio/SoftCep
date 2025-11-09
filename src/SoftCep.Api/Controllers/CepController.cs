using Microsoft.AspNetCore.Mvc;
using SoftCep.Api.Application;
using SoftCep.Api.Core;
using SoftCep.Api.Domain;
using Microsoft.AspNetCore.RateLimiting; // added

namespace SoftCep.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CepController : ControllerBase
{
    [EnableRateLimiting("PerIp1Rps")]
    [HttpGet("{cep}")]
    [EndpointDescription("Pesquisa um CEP específico.")]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(CepResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetCepAsync([FromRoute, CepValidation] string cep,
        [FromServices] GetCepQueryHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCepQuery(cep);
        var result = await handler.HandleAsync(query, cancellationToken);

        if (result == null)
            return NoContent();

        // Add cache headers for successful result
        var seconds = (int)Consts.CepCacheTime.TotalSeconds;
        Response.Headers.CacheControl = $"public, max-age={seconds}";
        Response.Headers.Expires = DateTime.UtcNow.Add(Consts.CepCacheTime).ToString("R");
        Response.Headers["X-Cache-Duration"] = seconds.ToString();

        return new OkObjectResult(result);
    }


    [EnableRateLimiting("PerIp1Rps")]
    [HttpGet("{state}/{city}/{term}")]
    [EndpointDescription("Pesquisa um CEP a partir do endereço informado.")]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(List<CepResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetCepAsync([FromRoute] string state, [FromRoute] string city,
        [FromRoute] string term, [FromServices] GetCepFromAddressQueryHandler handler,
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