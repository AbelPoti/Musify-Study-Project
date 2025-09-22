using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Musify.Dtos;
using Musify.Models;
using Musify.Services;
using System.Text;

namespace Musify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailConfirmTokenService _emailConfirmTokenService;
        private readonly IEmailSender _emailSender;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IEmailConfirmTokenService emailConfirmTokenService,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailConfirmTokenService = emailConfirmTokenService;
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

            user = new ApplicationUser { UserName = dto.Username, Email = dto.Email, RegistrationTime = DateTimeOffset.UtcNow };
            var result = await _userManager.CreateAsync(user, dto.Password);

            // Assign role and generate token, as well as send confirmation email
            if (result.Succeeded)
            {
                // Default role assignment
                string jwtToken = _tokenService.GenerateToken(user, [UserRole.User]);

                // Refetch user to get the Id
                user = await _userManager.FindByNameAsync(dto.Username);
                // Generate email confirmation token
                string emailConfirmToken = await _emailConfirmTokenService.GenerateEmailConfirmationToken(user!);

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

            if (user.EmailConfirmed == false)
            {
                return Unauthorized(new { Message = "Email not confirmed. Please confirm your email before logging in." });
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
