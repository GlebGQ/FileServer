using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Client.Models;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Client.Services
{
    internal class UserService : IUserService, IDisposable
    {
        private readonly IAesSecurityService _aesSecurityService;
        private readonly HttpClient _httpClient;

        public Guid ClientId { get; set; }

        public UserService(HttpClient httpClient, IAesSecurityService aesSecurityService)
        {
            _aesSecurityService = aesSecurityService;
            _httpClient = httpClient;
        }

        public async Task<string> LogInAsync(string userEmail, string userPassword)
        {
            var loginRequest = new LoginRequest() { Email = userEmail, Password = userPassword };
            var jsonContent = JsonConvert.SerializeObject(loginRequest);
            var loginContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var regResponse = await _httpClient.PostAsync("/api/Security/login", loginContent);
            if (!regResponse.IsSuccessStatusCode)
            {
                return $"Login failed! Status code: {await regResponse.Content.ReadAsStringAsync()}";
            }

            var token = await regResponse.Content.ReadAsStringAsync();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return $"Login successfully! JwtToken: {token}";
        }

        public async Task<string> CreateConnectionAsync()
        {
            if (_httpClient.DefaultRequestHeaders.Authorization is null)
            {
                return "Please Login first";
            }

            ClientId = Guid.NewGuid();
            using var rsaKey = new RSACryptoServiceProvider();
            byte[] key = rsaKey.ExportCspBlob(false);

            var data = JsonConvert.SerializeObject(new {ClientId, ClientPublicKey = key });
            var connectionContent = new StringContent(data, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/Security/generate-session-key", connectionContent);
            if (!response.IsSuccessStatusCode)
            {
                return $"Connection failed! Status code: {await response.Content.ReadAsStringAsync()}";
            }

            var generateSessionKeyResponse = await GetGenerateSessionKeyResponse(response, rsaKey);
            _aesSecurityService.SetAesConfiguration(generateSessionKeyResponse);
            return $"Connection established! Secret Key: {Encoding.UTF8.GetString(generateSessionKeyResponse.EncryptedSessionKey)}";
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        private async Task<GenerateSessionKeyResponse> GetGenerateSessionKeyResponse(
            HttpResponseMessage message,
            RSACryptoServiceProvider rsaKey)
        {
            var contentStream = await message.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(contentStream);
            using var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();
            var generateSessionKeyResponse = serializer.Deserialize<GenerateSessionKeyResponse>(jsonReader);

            var keyDeformatter = new RSAOAEPKeyExchangeDeformatter(rsaKey);
            generateSessionKeyResponse.EncryptedSessionKey = 
                keyDeformatter.DecryptKeyExchange(generateSessionKeyResponse.EncryptedSessionKey);

            return generateSessionKeyResponse;
        }
    }
}