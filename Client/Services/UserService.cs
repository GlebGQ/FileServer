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
        private readonly HttpClient _httpClient;

        public Guid ClientId { get; set; }

        public UserService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("WpfClient");
        }

        public async Task<LoginResponse> LogIn(string userEmail, string userPassword)
        {
            var loginRequest = new LoginRequest() { Email = userEmail, Password = userPassword };
            var jsonContent = JsonConvert.SerializeObject(loginRequest);
            var loginContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var regResponse = await _httpClient.PostAsync("/api/Security/login", loginContent);
            if (!regResponse.IsSuccessStatusCode)
            {
                return new LoginResponse()
                {
                    Message = $"Login failed! Status code: {regResponse.StatusCode}",
                    Token = string.Empty
                };
            }

            var token = await regResponse.Content.ReadAsStringAsync();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return new LoginResponse()
            {
                Message = $"Login failed! Status code: {regResponse.StatusCode}",
                Token = token
            };
        }

        public async Task<CreateConnectionResponse> CreateConnection()
        {
            if (_httpClient.DefaultRequestHeaders.Authorization is null)
            {
                return new CreateConnectionResponse
                {
                    Message = "Please LogIn first",
                    SessionKeyResponse = null
                };
            }

            ClientId = Guid.NewGuid();
            using var rsaKey = new RSACryptoServiceProvider();
            byte[] key = rsaKey.ExportCspBlob(false);

            var data = JsonConvert.SerializeObject(new {ClientId, ClientPublicKey = key });
            var connectionContent = new StringContent(data, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/Security/generate-session-key", connectionContent);
            if (!response.IsSuccessStatusCode)
            {
                return new CreateConnectionResponse
                {
                    Message = $"Connection failed! Status code: {response.StatusCode}", 
                    SessionKeyResponse = null
                };
            }

            var sessionKeyResponse = await GetGenerateSessionKeyResponse(response, rsaKey);
            return new CreateConnectionResponse
            {
                Message = $"Connection established! Secret Key: {sessionKeyResponse.EncryptedSessionKey}",
                SessionKeyResponse = sessionKeyResponse
            };
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
            var sessionKeyResponse = serializer.Deserialize<GenerateSessionKeyResponse>(jsonReader);

            var keyDeformatter = new RSAOAEPKeyExchangeDeformatter(rsaKey);
            sessionKeyResponse.EncryptedSessionKey = 
                keyDeformatter.DecryptKeyExchange(sessionKeyResponse.EncryptedSessionKey);

            return sessionKeyResponse;
        }
    }
}