using JwtAuthApi.Models;

namespace JwtAuthApi.Services;

public interface ITokenService
{
    string GenerateToken(User user, JwtSettings jwtSettings);
}
