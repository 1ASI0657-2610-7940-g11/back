using FuelTrack.Api.Features.Client.Domain;
using FuelTrack.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FuelTrack.Api.Features.Client.Api;

[ApiController]
[Route("api/client")]
public sealed class ClientController : ControllerBase
{
    private readonly IClientRepository _repository;

    public ClientController(IClientRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("kpis")]
    public async Task<ActionResult<ClientKpis>> GetClientKpis()
    {
        return Ok(await _repository.GetClientKpisAsync(User.GetRequiredUserId()));
    }
}
