using FileServer.Models;

namespace FileServer.Services;

public interface ISecurityService
{
    public GenerateSessionKeyResponse GenerateSessionKey(Guid clientId, byte[] clientPublicKey);
}