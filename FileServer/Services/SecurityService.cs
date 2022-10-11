
using System.Security.Cryptography;
using System.Text;
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
            using var ecdf = new ECDiffieHellmanCng();
            ecdf.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            ecdf.HashAlgorithm = CngAlgorithm.Sha256;
            var serverPublicKey = ecdf.PublicKey.ToByteArray();
            CngKey clientKey = CngKey.Import(clientPublicKey, CngKeyBlobFormat.EccPublicBlob);
            byte[] sessionKey = ecdf.DeriveKeyMaterial(clientKey);
            using var aes = Aes.Create();

            var response = new GenerateSessionKeyResponse
            {
                IV = aes.IV,
                PublicEcdfKey = serverPublicKey
            };

            _keyStore.AddOrUpdateSessionKey(clientId, sessionKey, aes.IV);

            return response;
        }

        public async Task EncryptTextAsync(Stream input, Stream output, Guid clientId)
        {
            var sessionKeyWrapper = _keyStore.GetSessionKey(clientId);
            if (sessionKeyWrapper == null || sessionKeyWrapper.ExpirationDateTime < DateTime.Now)
            {
                throw new ArgumentException("Session key is expired!");
            }
            using var aes = Aes.Create();
            aes.Key = sessionKeyWrapper.SessionKey;
            aes.IV = sessionKeyWrapper.IV;
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            await using var encryptStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write, leaveOpen: true);
            await input.CopyToAsync(encryptStream).ConfigureAwait(false);
        }

        public async Task DecryptTextAsync(Stream input, Stream output, Guid clientId)
        {
            var sessionKeyWrapper = _keyStore.GetSessionKey(clientId);
            if (sessionKeyWrapper == null || sessionKeyWrapper.ExpirationDateTime < DateTime.Now)
            {
                throw new ArgumentException("Session key is expired!");
            }
            using var aes = Aes.Create();
            aes.Key = sessionKeyWrapper.SessionKey;
            aes.IV = sessionKeyWrapper.IV;
            
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            await using var encryptStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            await encryptStream.CopyToAsync(output).ConfigureAwait(false);
        }

        public async Task<string> DecryptTextAsync(byte[] input, Guid clientId)
        {
            var sessionKeyWrapper = _keyStore.GetSessionKey(clientId);
            if (sessionKeyWrapper == null || sessionKeyWrapper.ExpirationDateTime < DateTime.Now)
            {
                throw new ArgumentException("Session key is expired!");
            }
            using var aes = Aes.Create();
            aes.Key = sessionKeyWrapper.SessionKey;
            aes.IV = sessionKeyWrapper.IV;
            var encryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream(input);
            await using var encryptStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Read, leaveOpen: true);
            using var streamReader = new StreamReader(encryptStream);
            return await streamReader.ReadToEndAsync();
        }
    }
}
