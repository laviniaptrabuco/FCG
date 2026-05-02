using FCG.Domain.Entities;

namespace FCG.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    DateTime GetExpiration();
}
