using FileServer.Models.Request;
using FileServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TextController : ControllerBase
    {
        private readonly ILogger<TextController> _logger;
        private readonly ISecurityService _securityService;

        public TextController(
            ILogger<TextController> logger,
            ISecurityService securityService)
        {
            _logger = logger;
            _securityService = securityService;
        }

        [HttpGet("get-text")]
        public async Task GetText([FromQuery] GetFileContentRequest getFileContentRequest)
        {
            var encryptedText = Convert.FromBase64String(getFileContentRequest.Base64EncryptedTextName);
            var textName = await _securityService.DecryptTextAsync(encryptedText, getFileContentRequest.ClientId).ConfigureAwait(false);
            await using var fileStream = new FileStream($"files\\{textName}", FileMode.Open);
            await _securityService.EncryptTextAsync(fileStream, HttpContext.Response.Body, getFileContentRequest.ClientId);
        }
    }
}
