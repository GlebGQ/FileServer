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
        private readonly IUserService _userService;
        private readonly HttpClient _httpClient;
        private readonly AesCryptoServiceProvider _aes = new AesCryptoServiceProvider();

        public TextService(IUserService userService, IHttpClientFactory httpClientFactory)
        {
            _userService = userService;
            _httpClient = httpClientFactory.CreateClient("WpfClient");
        }

        public string Text { get; private set; }

        public async Task<string> GetText(string textName)
        {
            var encryptor = _aes.CreateEncryptor();

            using var memoryStream = new MemoryStream();
            await using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write, leaveOpen: true);
            await using (var streamWriter = new StreamWriter(cryptoStream))
            {
                await streamWriter.WriteAsync(textName);
            }
            var encryptedFileName = memoryStream.ToArray();
            var base64EncryptedFileName = Convert.ToBase64String(encryptedFileName, 0, encryptedFileName.Length);
            //var queryParamsDictionary = new Dictionary<string, string>
            //{
            //    [nameof(base64EncryptedFileName)] = base64EncryptedFileName,
            //    ["clientId"] = _userService.ClientId.ToString(),

            //};
            //using var encodedContent = new FormUrlEncodedContent(queryParamsDictionary);
            //var params = await encodedContent.ReadAsStringAsync();
            var response = await _httpClient.GetAsync($"api/Text/get-text?base64EncryptedTextName={base64EncryptedFileName}&clientId={_userService.ClientId}");
            if (!response.IsSuccessStatusCode)
            {
                return $"Text didn't get! Status code: {response.StatusCode}";
            }

            var contentStream = await response.Content.ReadAsStreamAsync();
            await using CryptoStream cs = new CryptoStream(contentStream, _aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cs, Encoding.UTF8);

            Text = await streamReader.ReadToEndAsync();
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
