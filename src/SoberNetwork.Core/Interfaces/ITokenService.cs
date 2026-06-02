using SoberNetwork.Core.Entities;

namespace SoberNetwork.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user);
    DateTime GetExpiry();
}
