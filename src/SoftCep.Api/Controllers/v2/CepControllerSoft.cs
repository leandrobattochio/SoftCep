using System.Diagnostics.CodeAnalysis;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SoftCep.Api.Domain;

namespace SoftCep.Api.Controllers.v2;

[ExcludeFromCodeCoverage]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public sealed class CepControllerSoft : ControllerBase
{
    [MapToApiVersion("2.0")]
    [HttpGet]
    [EndpointDescription("Pesquisa um CEP específico na versão v2")]
    [ProducesResponseType(typeof(CepResult), StatusCodes.Status200OK, "application/json")]
    public IActionResult GetCepAsync()
    {
        return new OkObjectResult("Hello From v2");
    }
}