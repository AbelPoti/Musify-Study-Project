using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Musify.Dtos.Auth;
using Musify.Models;
using Musify.Services;
using System.Text;

namespace Musify.Controllers
{
    /// <summary>
    ///     Provides endpoints for various authentication-related operations.
    /// </summary>
    /// <remarks>
    ///     This controller allows users to register, log in, confirm their email addresses, and reset their passwords.
    ///     It utilizes ASP.NET Core Identity for user management and JWT for token generation.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailConfirmTokenService _emailConfirmTokenService;
        private readonly IEmailSender _emailSender;

        private const int RateLimitMinutes = 2;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="userManager">The user manager instance used to interact with ASP.NET Core Identity users.</param>
        /// <param name="signInManager">The sign in manager instance used to manage sign ins.</param>
        /// <param name="tokenService">The token service used to generate JWT tokens for user authentication.</param>
        /// <param name="emailConfirmTokenService">The token service used to generate email confirmation tokens.</param>
        /// <param name="emailSender">The email sender service used to send authentication related emails to users.</param>
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

        /// <summary>
        ///     Handles the user registration process.
        /// </summary>
        /// <remarks>
        ///     Handles the registration of a new user by creating an account with the provided username, email, and password.
        ///     This method assigns the default "User" role to the newly registered user, generates a JWT token for authentication,
        ///     and send an email confirmation link to the user's email address.
        ///     The username must be unique; if it is already taken, a <see cref="BadRequestObjectResult"/> response is returned.
        /// </remarks>
        /// <param name="dto">Contains the necessary user data to initialize registration.</param>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> if the registration is successful, containing a success message and JWT token;
        ///     otherwise, a <see cref="BadRequestObjectResult"/> with error details.
        /// </returns>
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto dto)
        {
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

                user.LastConfirmEmailSent = DateTimeOffset.UtcNow;
                await _userManager.UpdateAsync(user);

                return Ok(new { Message = "User registered successfully", jwtToken });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        ///     Authenticates a user and generates a JWT token upon successful login.
        /// </summary>
        /// <remarks>
        ///     Authenticates a user based on their credentials. The credentials must be valid, and the user's email must be confirmed;
        ///     otherwise, an <see cref="UnauthorizedObjectResult"/> response is returned.
        /// </remarks>
        /// <param name="dto">Contains the necessary data to authenticate the user.</param>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> if the login is successful, containing a success message and JWT token;
        ///     othewise an <see cref="UnauthorizedObjectResult"/> with error details.
        /// </returns>
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
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

        /// <summary>
        ///     Handles the email confirmation process for a user.
        /// </summary>
        /// <remarks>
        ///     Handles the email confirmation process by validating the provided user ID and confirmation token.
        ///     The provided user identifier must correspond to an existing user, and the token must be valid;
        ///     otherwise, appropriate error responses are returned.
        /// </remarks>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="token">The confirmation token sent to the user's email address.</param>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> response if the confirmation is successful;
        ///     otherwise, a <see cref="NotFoundObjectResult"/> if the user is not found, or a <see cref="BadRequestObjectResult"/> if the confirmation fails.
        /// </returns>
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

        /// <summary>
        ///     Handles resending of the email confirmation link to the user's email address.
        /// </summary>
        /// <remarks>
        ///     Resends the email confirmation link to the specified email address.
        ///     The email is only resent if the user exists, the email is not already confirmed,
        ///     and the last email was sent more than a predefined rate limit duration ago to prevent abuse.
        /// </remarks>
        /// <param name="dto">Contains the email address to resend the confirmation email to.</param>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> response regardless of the outcome to prevent email enumeration.
        /// </returns>
        [HttpPost("resend-confirmation-email")]
        public async Task<ActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                // To prevent email enumeration, always return OK
                return Ok(new { Message = "Confirmation email resent successfully" });
            }

            if (user.EmailConfirmed)
            {
                return Ok(new { Message = "Email is already confirmed" });
            }

            // Rate limiting: Allow resending only if last sent was more than n minutes ago
            if (user.LastConfirmEmailSent.HasValue)
            {
                var diff = RateLimitMinutes - (DateTimeOffset.UtcNow - user.LastConfirmEmailSent.Value).TotalMinutes;

                if (diff > 0)
                {
                    return Ok(new { Message = "Confirmation email resent successfully" });
                }
            }

            string emailConfirmToken = await _emailConfirmTokenService.GenerateEmailConfirmationToken(user);

            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var confirmationLink = 
                $"{baseUrl}/api/auth/confirmemail?userId={user.Id}&token={Uri.EscapeDataString(emailConfirmToken)}";

            await _emailSender.SendEmailAsync(
                dto.Email,
                "Musify email confirmation",
                $"Please confirm your account by <a href='{confirmationLink}'>Clicking here</a>.");

            user.LastConfirmEmailSent = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok(new { Message = "Confirmation email resent successfully" });
        }

        /// <summary>
        ///     Handles forgot password requests by sending a password reset link to the user's email.
        /// </summary>
        /// <remarks>
        ///     The provided user identified by the email address must exist and have a confirmed email, and the last forgot password request
        ///     must be sent within a rate limit interval to prevent abuse. Even if these conditions are not met,
        ///     an <see cref="OkObjectResult"/> is returned to prevent email enumeration.
        /// </remarks>
        /// <param name="dto">Contains the email address of the user to send the email to.</param>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> response regardless of the outcome to prevent email enumeration.
        /// </returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // To prevent email enumeration, always return OK
                return Ok(new { Message = "If a user was registered with the provided email, a password reset link has been sent." });
            }

            // Rate limiting: Allow sending only if last sent was more than n minutes ago
            if (user.LastPasswordResetSent.HasValue)
            {
                var diff = RateLimitMinutes - (DateTimeOffset.UtcNow - user.LastPasswordResetSent.Value).TotalMinutes;

                if (diff > 0)
                {
                    return Ok(new { Message = "If a user was registered with the provided email, a password reset link has been sent." });
                }
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var resetLink = $"{baseUrl}/reset-password?userId={user.Id}&token={encodedToken}";

            await _emailSender.SendEmailAsync(
                dto.Email,
                "Musify Password Reset",
                $"You can reset your password by <a href='{resetLink}'>Clicking here</a>. If you did not request a password reset, please ignore this email.");

            user.LastPasswordResetSent = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok(new { Message = "If a user was registered with the provided email, a password reset link has been sent." });
        }

        /// <summary>
        ///     Handles resetting the user's password.
        /// </summary>
        /// <remarks>
        ///     Handles resetting the user's password using a valid reset token. The provided passwords must match.
        ///     If the user does not exist, an <see cref="OkObjectResult"/> is still returned to prevent user enumeration.
        /// </remarks>
        /// <param name="dto">Contains the reset token and the necessary information to reset a user's password.</param>
        /// <returns>
        ///     An <see cref="OkObjectResult"/> response if the reset is successful or the user does not exist;
        ///     otherwise, a <see cref="BadRequestObjectResult"/> with error details.
        /// </returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "New password and confirmation password do not match.");
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                // To prevent user enumeration, always return OK
                return Ok(new { Message = "Password has been reset successfully." });
            }

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(dto.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Password has been reset successfully." });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }
    }
}
