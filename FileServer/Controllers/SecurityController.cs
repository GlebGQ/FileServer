using System.Security.Claims;
using FileServer.Infrastructure;
using FileServer.Models.Request;
using FileServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FileServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly ILogger<SecurityController> _logger;
        private readonly SignInManager<IdentityUser> _signinManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IJwtGenerator _generator;
        private readonly ISecurityService _securityService;

        public SecurityController(
            ILogger<SecurityController> logger, 
            SignInManager<IdentityUser> signinManager,
            UserManager<IdentityUser> userManager,
            IJwtGenerator generator, 
            ISecurityService securityService)
        {
            _logger = logger;
            _signinManager = signinManager;
            _userManager = userManager;
            _generator = generator;
            _securityService = securityService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LogIn(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            if (user != null)
            {
                var signInResult = await _signinManager.CheckPasswordSignInAsync(user, loginRequest.Password, false);
                if (signInResult.Succeeded)
                {
                    return Ok(_generator.GenerateToken(user));
                }
            }

            return Unauthorized();
        }

        [HttpPost("get-session-key")]
        [Authorize]
        public async Task<IActionResult> GenerateSessionKey([FromBody]GenerateSessionKeyRequest request)
        {
            var clientId = request.ClientId;
            var clientPublicKey = request.ClientPublicKey;

            var response = _securityService.GenerateSessionKey(clientId, clientPublicKey);

            return Unauthorized();
        }
    }
}
