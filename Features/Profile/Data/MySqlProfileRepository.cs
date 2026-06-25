using FuelTrack.Api.Features.Profile.Domain;
using FuelTrack.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FuelTrack.Api.Features.Profile.Data;

public sealed class MySqlProfileRepository : IProfileRepository
{
    private readonly FuelTrackDbContext _db;

    public MySqlProfileRepository(FuelTrackDbContext db)
    {
        _db = db;
    }

    public async Task<ProfileInfo> GetProfileAsync(string userId)
    {
        var profile = await _db.Profiles.SingleAsync(x => x.UserId == userId);
        return ToDomain(profile);
    }

    public async Task<ProfileInfo> UpdateProfileAsync(string userId, ProfileInfo profile)
    {
        var entity = await _db.Profiles.SingleAsync(x => x.UserId == userId);
        entity.CompanyName = profile.CompanyName?.Trim() ?? "";
        entity.Ruc = profile.Ruc?.Trim() ?? "";
        entity.Email = profile.Email?.Trim() ?? "";
        entity.Phone = profile.Phone?.Trim() ?? "";
        entity.ContactName = profile.ContactName?.Trim();
        await _db.SaveChangesAsync();
        return ToDomain(entity);
    }

    public async Task<ProfileInfo> UpdateAvatarAsync(
        string userId,
        byte[] content,
        string contentType)
    {
        var entity = await _db.Profiles.SingleAsync(x => x.UserId == userId);
        entity.AvatarContent = content;
        entity.AvatarContentType = contentType;
        await _db.SaveChangesAsync();
        return ToDomain(entity);
    }

    public async Task<AvatarData?> GetAvatarAsync(string userId)
    {
        var avatar = await _db.Profiles
            .Where(x => x.UserId == userId && x.AvatarContent != null)
            .Select(x => new { x.AvatarContent, x.AvatarContentType })
            .SingleOrDefaultAsync();
        return avatar?.AvatarContent is null
            ? null
            : new AvatarData(
                avatar.AvatarContent,
                avatar.AvatarContentType ?? "application/octet-stream");
    }

    private static ProfileInfo ToDomain(Infrastructure.Data.Entities.ProfileEntity profile)
    {
        return new ProfileInfo
        {
            CompanyName = profile.CompanyName,
            Ruc = profile.Ruc,
            Email = profile.Email,
            Phone = profile.Phone,
            ContactName = profile.ContactName,
            AvatarUrl = profile.AvatarContent is null ? null : "/api/profile/avatar",
            LastPasswordChange = profile.LastPasswordChangeUtc?.ToString("yyyy-MM-dd")
        };
    }
}
