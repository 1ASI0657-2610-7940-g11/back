using System.Threading;
using System.Threading.Tasks;

namespace FuelTrack.Api.Features.Home.Domain;

public interface IHomeRepository
{
    Task<DashboardSummary> GetDashboardAsync(
        string userId,
        CancellationToken cancellationToken = default
    );
}
