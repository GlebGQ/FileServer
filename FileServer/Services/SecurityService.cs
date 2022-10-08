
using System.Security.Cryptography;
using FileServer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FileServer.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly KeyStore.KeyStore _keyStore;

        public SecurityService(KeyStore.KeyStore keyStore)
        {
            _keyStore = keyStore;
        }

        public GenerateSessionKeyResponse GenerateSessionKey(Guid clientId, byte[] clientPublicKey)
        {
            if(_keyStore.)
            var response = new GenerateSessionKeyResponse();
            using var rsaKey = new RSACryptoServiceProvider();
            rsaKey.ImportCspBlob(clientPublicKey);
            using var aes = Aes.Create();
            var sessionKey = aes.Key;
            var keyFormatter = new RSAOAEPKeyExchangeFormatter(rsaKey);
            var encryptedSessionKey = keyFormatter.CreateKeyExchange(sessionKey, typeof(Aes));
            response.IV = aes.IV;
            response.EncryptedSessionKey = encryptedSessionKey;

            _keyStore.AddSessionKey(clientId, sessionKey);

            return response;
        }
    }
}
