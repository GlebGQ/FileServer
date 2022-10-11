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
using System.Web;
using Client.Models;
using Aes = System.Runtime.Intrinsics.Arm.Aes;

namespace Client.Services
{
    internal class TextService : ITextService, IDisposable
    {
        private readonly IUserService _userService;
        private readonly IAesSecurityService _aesSecurityService;
        private readonly HttpClient _httpClient;

        public TextService(IUserService userService, HttpClient httpClient, IAesSecurityService aesSecurityService)
        {
            _userService = userService;
            _aesSecurityService = aesSecurityService;
            _httpClient = httpClient;
        }

        public string EncryptedText { get; set; } = string.Empty;

        public string DecryptedText { get; set; } = string.Empty;

        public async Task<string> GetTextAsync(string textName)
        {
            var encryptedFileName = await _aesSecurityService.EncryptTextAsync(textName);
            var base64EncryptedFileName = Convert.ToBase64String(encryptedFileName, 0, encryptedFileName.Length);
            base64EncryptedFileName = HttpUtility.UrlEncode(base64EncryptedFileName);

            var response = await _httpClient.GetAsync($"api/Text/get-text?base64EncryptedTextName={base64EncryptedFileName}&clientId={_userService.ClientId}");
            if (!response.IsSuccessStatusCode)
            {
                return $"Text didn't get! Status code: {response.StatusCode} { await response.Content.ReadAsStringAsync()}";
            }

            EncryptedText = await response.Content.ReadAsStringAsync();
            var contentStream = await response.Content.ReadAsStreamAsync();
            DecryptedText = await _aesSecurityService.DecryptTextAsync(contentStream);

            return "Text got successfully!";
        }

        public async Task<string> DeleteTextAsync(string textName)
        {
            var response = await _httpClient.DeleteAsync($"api/Text/delete-text?textName={textName}");
            if (!response.IsSuccessStatusCode)
            {
                return $"Text didn't delete! Status code: {response.StatusCode} {await response.Content.ReadAsStringAsync()}";
            }

            return "Text deleted!";
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
        }
    }
}
