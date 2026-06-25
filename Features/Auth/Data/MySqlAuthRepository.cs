using FuelTrack.Api.Features.Auth.Domain;
using FuelTrack.Api.Infrastructure.Auth;
using FuelTrack.Api.Infrastructure.Data;
using FuelTrack.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FuelTrack.Api.Features.Auth.Data;

public sealed class MySqlAuthRepository : IAuthRepository
{
    private readonly FuelTrackDbContext _db;
    private readonly PasswordHashService _passwords;
    private readonly TokenService _tokens;

    public MySqlAuthRepository(
        FuelTrackDbContext db,
        PasswordHashService passwords,
        TokenService tokens)
    {
        _db = db;
        _passwords = passwords;
        _tokens = tokens;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(x => x.Email == email))
            throw new InvalidOperationException("EMAIL_ALREADY_EXISTS");

        var user = new UserEntity
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = _passwords.Hash(request.Password),
            Profile = new ProfileEntity
            {
                CompanyName = "",
                Ruc = "",
                Email = email,
                Phone = "",
                ContactName = request.FullName.Trim()
            }
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return ToResult(user);
    }

    public async Task<AuthResult?> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);
        if (user is null || !_passwords.Verify(request.Password, user.PasswordHash))
            return null;

        return ToResult(user);
    }

    private AuthResult ToResult(UserEntity user)
    {
        return new AuthResult(
            _tokens.Create(user),
            new UserDto(user.Id, user.FullName, user.Email));
    }
}
