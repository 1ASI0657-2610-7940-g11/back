namespace FuelTrack.Api.Features.Company.Domain;

public interface ICompanyRepository
{
    Task<CompanyDetail?> GetCompanyDetailAsync(string id);
}
