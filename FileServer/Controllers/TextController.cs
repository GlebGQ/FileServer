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
        public async Task GetText([FromQuery] FileContentRequest fileContentRequest)
        {
            var encryptedTextName = Convert.FromBase64String(fileContentRequest.Base64EncryptedTextName);
            var textName = await _securityService.DecryptTextAsync(encryptedTextName, fileContentRequest.ClientId).ConfigureAwait(false);
            if (!System.IO.File.Exists($"files\\{textName}"))
            {
                return;
            }

            await using var fileStream = new FileStream($"files\\{textName}", FileMode.Open);
            await _securityService.EncryptTextAsync(fileStream, HttpContext.Response.Body, fileContentRequest.ClientId);
        }

        [HttpPut("edit-text")]
        public async Task<IActionResult> EditText([FromQuery] FileContentRequest fileContentRequest)
        {
            var encryptedTextName = Convert.FromBase64String(fileContentRequest.Base64EncryptedTextName);
            var textName = await _securityService.DecryptTextAsync(encryptedTextName, fileContentRequest.ClientId).ConfigureAwait(false);
            if (!System.IO.File.Exists($"files\\{textName}"))
            {
                return BadRequest("File Not Found");
            }

            await using var fileStream = new FileStream($"files\\{textName}", FileMode.Create);
            await _securityService.DecryptTextAsync(HttpContext.Request.Body, fileStream, fileContentRequest.ClientId);
            return Ok("File edited successfully");
        }

        [HttpDelete("delete-text")]
        public async Task<IActionResult> DeleteText([FromQuery] FileContentRequest fileContentRequest)
        {
            var encryptedText = Convert.FromBase64String(fileContentRequest.Base64EncryptedTextName);
            var textName = await _securityService.DecryptTextAsync(encryptedText, fileContentRequest.ClientId).ConfigureAwait(false);

            if (!System.IO.File.Exists($"files\\{textName}"))
            {
                return BadRequest("File Not Found");
            }

            System.IO.File.Delete($"files\\{textName}");
            return Ok("File deleted successfully");
        }
    }
}
