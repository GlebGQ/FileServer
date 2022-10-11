using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Client.Models;
using System.IO;

namespace Client.Services
{
    internal class AesSecurityService : IAesSecurityService, IDisposable
    {
        private readonly Aes _aes;

        public AesSecurityService()
        {
            _aes = Aes.Create();
        }

        public void SetAesConfiguration(GenerateSessionKeyResponse sessionKeyResponse)
        {
            _aes.IV = sessionKeyResponse.IV;
            _aes.Key = sessionKeyResponse.EncryptedSessionKey;
        }

        public async Task<byte[]> EncryptTextAsync(string text)
        {
            var encryptor = _aes.CreateEncryptor();
            using var memoryStream = new MemoryStream();
            await using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write, leaveOpen: true);
            await using (var streamWriter = new StreamWriter(cryptoStream))
            {
                await streamWriter.WriteAsync(text);
            }
            return memoryStream.ToArray();
        }

        public async Task<string> DecryptTextAsync(Stream input)
        {
            var decryptor = _aes.CreateDecryptor(_aes.Key, _aes.IV);
            await using var encryptStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(encryptStream);
            return await streamReader.ReadToEndAsync();
        }


        public void Dispose()
        {
            _aes.Dispose();
        }
    }

    internal interface IAesSecurityService
    {
        public void SetAesConfiguration(GenerateSessionKeyResponse sessionKeyResponse);
        public Task<byte[]> EncryptTextAsync(string text);
        public Task<string> DecryptTextAsync(Stream input);
    }
}
