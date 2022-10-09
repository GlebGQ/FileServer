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

        public UserService(string baseUserServiceUri)
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(baseUserServiceUri)
            };
        }

        public GenerateSessionKeyResponse SessionKeyResponse { get; private set; }
        public string AuthenticationToken { get; private set; } = string.Empty;

        public async Task<(string message, string token)> LogIn(string userEmail, string userPassword)
        {
            var userData = JsonConvert.SerializeObject(new LoginRequest(){ Email = userEmail, Password = userPassword });
            var loginContent = new StringContent(userData, Encoding.UTF8, "application/json");

            var regResponse = await _httpClient.PostAsync("/api/Security/login", loginContent);
            if (!regResponse.IsSuccessStatusCode)
            {
                return ($"Login failed! Status code: {regResponse.StatusCode}", string.Empty);
            }

            var token = await regResponse.Content.ReadAsStringAsync();
            AuthenticationToken = token;
            return ($"Login was successfully! Token: {token}", token);
        }

        public async Task<(string message, GenerateSessionKeyResponse? sessionKeyResponse)> 
            CreateConnection(Guid appIdentifier)
        {
            if (AuthenticationToken == string.Empty)
            {
                return ("Please LogIn first", null);
            }

            using var rsaKey = new RSACryptoServiceProvider();
            byte[] key = rsaKey.ExportCspBlob(false);

            var data = JsonConvert.SerializeObject(new { ClientId = appIdentifier, ClientPublicKey = key });
            var connectionContent = new StringContent(data, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", AuthenticationToken);

            var response = await _httpClient.PostAsync("/api/Security/get-session-key", connectionContent);
            if (!response.IsSuccessStatusCode)
            {
                return ($"Connection failed! Status code: {response.StatusCode}", null);
            }

            var contentStream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(contentStream);
            using var jsonReader = new JsonTextReader(streamReader);

            try
            {
                var serializer = new JsonSerializer();
                SessionKeyResponse = serializer.Deserialize<GenerateSessionKeyResponse>(jsonReader);
                var keyDeformatter = new RSAOAEPKeyExchangeDeformatter(rsaKey);
                SessionKeyResponse.EncryptedSessionKey = keyDeformatter.DecryptKeyExchange(SessionKeyResponse.EncryptedSessionKey);
                return ($"Connection established! Secret Key: {SessionKeyResponse.EncryptedSessionKey}", null);
            }
            catch (JsonReaderException ex)
            {
                return ($"Connection failed: {ex.Message}", null);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
