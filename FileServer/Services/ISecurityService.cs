using FileServer.Models;

namespace FileServer.Services;

public interface ISecurityService
{
    public GenerateSessionKeyResponse GenerateSessionKey(Guid clientId, byte[] clientPublicKey);

    public Task EncryptTextAsync(Stream input, Stream output, Guid clientId);

    public Task DecryptTextAsync(Stream input, Stream output, Guid clientId);

    public Task<string> DecryptTextAsync(byte[] input, Guid clientId);
}