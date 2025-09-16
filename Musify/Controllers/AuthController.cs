using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Musify.Dtos;
using Musify.Services;
using System.Text;

namespace Musify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailSender _emailSender;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ITokenService tokenService,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user != null)
            {
                return BadRequest(new { Message = "Username already taken" });
            }

            user = new IdentityUser { UserName = dto.Username, Email = dto.Email };
            var result = await _userManager.CreateAsync(user, dto.Password);

            // Assign role and generate token, as well as send confirmation email
            if (result.Succeeded)
            {
                // Default role assignment
                string jwtToken = _tokenService.GenerateToken(user, ["User"]);

                // Fetch user again for Id
                user = await _userManager.FindByNameAsync(dto.Username);

                string emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user!);
                // Since tokens may contain special characters, encode it
                emailConfirmToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmToken));

                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var confirmationLink = $"{baseUrl}/api/auth/confirmemail?userId={user!.Id}&token={Uri.EscapeDataString(emailConfirmToken)}";

                await _emailSender.SendEmailAsync(
                    dto.Email,
                    "Musify email confirmation",
                    $"Please confirm your account by <a href='{confirmationLink}'>Clicking here</a>.");

                return Ok(new { Message = "User registered successfully", jwtToken });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(user, roles);
            return Ok(new { Message = "Login successful", token });
        }

        [HttpGet("confirmemail")]
        public async Task<ActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { Message = "UserId and Token are required" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            // Decode the token
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Email confirmed successfully" });
            }
            return BadRequest(new { Message = "Email confirmation failed", Errors = result.Errors.Select(e => e.Description) });
        }
    }
}
