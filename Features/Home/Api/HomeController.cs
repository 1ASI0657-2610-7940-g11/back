using FuelTrack.Api.Features.Home.Domain;
using FuelTrack.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FuelTrack.Api.Features.Home.Api;

[ApiController]
[Route("api/client")]
public sealed class HomeController : ControllerBase
{
    private readonly IHomeRepository _repository;

    public HomeController(IHomeRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardSummary>> GetDashboard(
        CancellationToken cancellationToken)
    {
        return Ok(await _repository.GetDashboardAsync(
            User.GetRequiredUserId(), cancellationToken));
    }
}
