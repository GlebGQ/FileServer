using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Client.Models;
using Aes = System.Runtime.Intrinsics.Arm.Aes;

namespace Client.Services
{
    internal class TextService : ITextService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly AesCryptoServiceProvider _aes = new AesCryptoServiceProvider();

        public TextService(string basePath)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(basePath);
        }

        public string Text { get; private set; }

        public async Task<string> GetText(string textName)
        {
            var response = await _httpClient.GetAsync($"api/Text/get-text?textName={textName}");
            if (!response.IsSuccessStatusCode)
            {
                return $"Text didn't get! Status code: {response.StatusCode}";
            }

            var encryptedText = await response.Content.ReadAsByteArrayAsync();
            using MemoryStream plaintext = new MemoryStream();
            await using CryptoStream cs = new CryptoStream(plaintext, _aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(encryptedText, 0, encryptedText.Length);
            cs.Close();

            Text = Encoding.UTF8.GetString(plaintext.ToArray());
            return "Text got successfully!";
        }

        public async Task<string> DeleteText(string textName)
        {
            var response = await _httpClient.DeleteAsync($"api/Text/delete-text?textName={textName}");
            if (!response.IsSuccessStatusCode)
            {
                return $"Text didn't delete! Status code: {response.StatusCode}";
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> UpdateText(string textName, string editedText)
        {
            var content = new StringContent(editedText, Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"api/Text/edit-text?textName={textName}", content);
            if (!response.IsSuccessStatusCode)
            {
                return $"Text didn't edit! Status code: {response.StatusCode}";
            }

            return await response.Content.ReadAsStringAsync();
        }

        public void SetAuthorizationToken(string authorizationToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authorizationToken);
        }

        public void SetUpAes(GenerateSessionKeyResponse sessionKeyResponse)
        {
            _aes.IV = sessionKeyResponse.IV;
            _aes.Key = sessionKeyResponse.EncryptedSessionKey;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _aes.Dispose();
        }
    }
}
