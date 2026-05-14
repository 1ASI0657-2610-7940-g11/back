using FuelTrack.Api.Features.Auth.Domain;

namespace FuelTrack.Api.Features.Auth.Data;

public class InMemoryAuthRepository : IAuthRepository
{
    private readonly List<AuthUser> _users = new()
    {
        new AuthUser
        {
            Id = "demo-client",
            FullName = "Cliente FuelTrack SAC",
            Email = "cliente@fueltrack.com",
            Password = "123456"
        }
    };

    public Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        // Validar correo duplicado
        if (_users.Any(u =>
                u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("EMAIL_ALREADY_EXISTS");
        }

        var user = new AuthUser
        {
            Id = Guid.NewGuid().ToString(),
            FullName = request.FullName,
            Email = request.Email,
            Password = request.Password // demo
        };

        _users.Add(user);

        var dto = new UserDto(user.Id, user.FullName, user.Email);
        var token = Guid.NewGuid().ToString("N");

        return Task.FromResult(new AuthResult(token, dto));
    }

    public Task<AuthResult?> LoginAsync(LoginRequest request)
    {
        var user = _users.FirstOrDefault(u =>
            u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) &&
            u.Password == request.Password);

        if (user is null)
        {
            return Task.FromResult<AuthResult?>(null);
        }

        var dto = new UserDto(user.Id, user.FullName, user.Email);
        var token = Guid.NewGuid().ToString("N");

        return Task.FromResult<AuthResult?>(new AuthResult(token, dto));
    }
}
