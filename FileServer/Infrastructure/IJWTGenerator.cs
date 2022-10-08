using Microsoft.AspNetCore.Identity;

namespace FileServer.Infrastructure
{
    public interface IJwtGenerator
    {
        string GenerateToken(IdentityUser identityUser);
    }
}
