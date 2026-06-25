namespace FuelTrack.Api.Features.Client.Domain;

public interface IClientRepository
{
    Task<ClientKpis> GetClientKpisAsync(string userId);
}
