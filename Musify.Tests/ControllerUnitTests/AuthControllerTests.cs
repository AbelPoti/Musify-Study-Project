using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Musify.Controllers;
using Musify.Dtos.AuthDtos;
using Musify.Models;
using Musify.Services;

namespace Musify.Tests.ControllerUnitTests
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<IEmailConfirmTokenService> _emailConfirmTokenServiceMock;
        private Mock<IEmailSender> _emailSenderMock;
        private Mock<IDateTimeProvider> _dateTimeProviderMock;
        private AuthController _authController;

        [SetUp]
        public void Setup()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null!, null!, null!, null!);

            _tokenServiceMock = new Mock<ITokenService>();
            _emailConfirmTokenServiceMock = new Mock<IEmailConfirmTokenService>();
            _emailSenderMock = new Mock<IEmailSender>();
            _dateTimeProviderMock = new Mock<IDateTimeProvider>();

            _authController = new AuthController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _emailConfirmTokenServiceMock.Object,
                _emailSenderMock.Object,
                _dateTimeProviderMock.Object
            );
        }

        #region RegisterTests

        [Test]
        public async Task Register_WhenUserProvidesValidData_ShouldReturnOk()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "test.user",
                Email = "test@example.com",
                Password = "Password123"
            };

            var createdUser = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                Id = "new-id"
            };

            string sampleJwtToken = "jwt-token";

            // No existing user with the same username -> return null
            // Then return created user
            _userManagerMock.SetupSequence(u => u.FindByNameAsync(dto.Username))
                .ReturnsAsync((ApplicationUser?)null)
                .ReturnsAsync(createdUser);

            // Make CreateAsync succeed
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Mock jwt token generation
            _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<ApplicationUser>(), It.IsAny<IList<string>>()))
                .Returns(sampleJwtToken);

            // Mock email confirmation token generation
            _emailConfirmTokenServiceMock.Setup(t => t.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("email-confirmation-token");

            // Provide a usable HttpContext so controller can build base URL
            var httpContext = TestUtils.CreateHttpContext();
            _authController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _authController.Register(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<RegisterOkResponseDto>().Subject;

            payload.Message.Should().Be("User registered successfully");
            payload.JwtToken.Should().Be(sampleJwtToken);

            // Verify that required dependencies were called correctly
            _userManagerMock.Verify(u => u.FindByNameAsync(dto.Username), Times.Exactly(2));

            _userManagerMock.Verify(u => u.CreateAsync(It.Is<ApplicationUser>(user =>
                user.UserName == dto.Username && user.Email == dto.Email), dto.Password), Times.Once);

            _tokenServiceMock.Verify(t => t.GenerateToken(
                It.Is<ApplicationUser>(user =>
                    user.UserName == dto.Username && user.Email == dto.Email
                ),
                It.Is<IList<string>>(rl =>
                    rl.Count == 1 && rl[0] == UserRole.User
                )
            ), Times.Once);

            _emailConfirmTokenServiceMock.Verify(t => t.GenerateEmailConfirmationTokenAsync(
                It.Is<ApplicationUser>(user =>
                    user.UserName == dto.Username && user.Email == dto.Email
                )
            ), Times.Once);

            _emailSenderMock.Verify(e => e.SendEmailAsync(
                dto.Email,
                "Musify email confirmation",
                It.IsAny<string>()));

            _userManagerMock.Verify(u => u.UpdateAsync(It.Is<ApplicationUser>(user =>
                user.UserName == dto.Username && user.Email == dto.Email)), Times.Once);
        }

        [Test]
        public async Task Register_WhenUserProvidesExistingUsername_ShouldReturnBadRequest()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "test.existing.user",
                Email = "test@example.com",
                Password = "Password123"
            };

            // Return an existing user when checking for username
            _userManagerMock.Setup(u => u.FindByNameAsync(dto.Username))
                .ReturnsAsync(new ApplicationUser
                {
                    UserName = dto.Username,
                    Email = dto.Email,
                    Id = "existing-id"
                });

            // Act
            var result = await _authController.Register(dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<RegisterUsernameAlreadyTakenDto>().Subject;

            payload.Message.Should().Be("Username already taken");

            // Verify that no user creation was attempted
            _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Register_WhenUserCreationFails_ShouldReturnBadRequest()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "test.user.fail",
                Email = "fail@example.com",
                Password = "weak"
            };

            // No existing user
            _userManagerMock.Setup(u => u.FindByNameAsync(dto.Username))
                .ReturnsAsync((ApplicationUser?)null);

            // But user creation fails
            var failedResult = IdentityResult.Failed(
                new IdentityError { Description = "Password too weak" },
                new IdentityError { Description = "Email already used" }
            );

            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _authController.Register(dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;

            // ModelState passed as the Value
            var modelState = badRequest.Value.Should().BeOfType<SerializableError>().Subject;

            // The controller puts all errors under the empty key (model level errors)
            modelState.Should().ContainKey(string.Empty);
            var errors = modelState[string.Empty] as string[];
            errors.Should().Contain("Password too weak");
            errors.Should().Contain("Email already used");

            // Verify that no email was sent and no user update was attempted
            _emailSenderMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            _userManagerMock.Verify(u => u.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        #endregion

        #region LoginTests

        [Test]
        public async Task Login_WhenUserProvidesValidCredentials_ShouldReturnOk()
        {
            // Arrange
            var dto = new LoginDto
            {
                Username = "valid.user",
                Password = "ValidPassword123"
            };

            var returnedUser = new ApplicationUser
            {
                UserName = dto.Username,
                Email = "email@example.com",
                EmailConfirmed = true
            };

            IList<string> returnedRoles = [UserRole.User];

            string returnedToken = "sampleJwtToken";

            _userManagerMock.Setup(u => u.FindByNameAsync(dto.Username))
                .ReturnsAsync(returnedUser);

            _signInManagerMock.Setup(si => si.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), dto.Password, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _userManagerMock.Setup(u => u.GetRolesAsync(returnedUser))
                .ReturnsAsync(returnedRoles);

            _tokenServiceMock.Setup(t => t.GenerateToken(returnedUser, returnedRoles))
                .Returns(returnedToken);

            // Act
            var result = await _authController.Login(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<LoginOkResponseDto>().Subject;

            payload.Message.Should().Be("Login successful");
            payload.JwtToken.Should().Be(returnedToken);

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByNameAsync(dto.Username), Times.Once);

            _signInManagerMock.Verify(si => si.CheckPasswordSignInAsync(
                It.Is<ApplicationUser>(user => user.UserName == dto.Username && user.EmailConfirmed),
                dto.Password,
                false
            ), Times.Once);

            _userManagerMock.Verify(u => u.GetRolesAsync(It.Is<ApplicationUser>(user =>
                user.UserName == dto.Username && user.EmailConfirmed == true)), Times.Once);

            _tokenServiceMock.Verify(t => t.GenerateToken(
                It.Is<ApplicationUser>(user =>
                    user.UserName == dto.Username && user.EmailConfirmed
                ),
                It.Is<IList<string>>(roleList =>
                    roleList == returnedRoles
                )
            ), Times.Once);
        }

        [Test]
        public async Task Login_WhenUserProvidesNonexistentUsername_ShouldReturnUnauthorized()
        {
            // Arrange
            var dto = new LoginDto
            {
                Username = "nonexistent.user",
                Password = "Password123"
            };

            _userManagerMock.Setup(u => u.FindByNameAsync(dto.Username))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authController.Login(dto);

            // Assert
            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            var payload = unauthorized.Value.Should().BeOfType<LoginUnauthorizedResponseDto>().Subject;

            payload.Message.Should().Be("Invalid username or password");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByNameAsync(dto.Username), Times.Once);

            // Verify only this call, subsequent calls should really never happen
            _signInManagerMock.Verify(si => si.CheckPasswordSignInAsync(
                It.Is<ApplicationUser>(user => user.UserName == dto.Username && user.EmailConfirmed),
                dto.Password,
                false
            ), Times.Never);
        }

        [Test]
        public async Task Login_WhenUserProvidesUnconfirmedAccount_ShouldReturnUnauthorized()
        {
            // Arrange
            var dto = new LoginDto
            {
                Username = "unconfirmed.user",
                Password = "Password123"
            };

            var returnedUser = new ApplicationUser
            {
                UserName = dto.Username,
                Email = "email.unconfirmed@example.com",
                EmailConfirmed = false
            };

            _userManagerMock.Setup(u => u.FindByNameAsync(dto.Username))
                .ReturnsAsync(returnedUser);

            // Act
            var result = await _authController.Login(dto);


            // Assert
            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            var payload = unauthorized.Value.Should().BeOfType<LoginUnauthorizedResponseDto>().Subject;

            payload.Message.Should().Be("Email not confirmed. Please confirm your email before logging in.");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByNameAsync(dto.Username), Times.Once);

            // Verify only this call, subsequent calls should really never happen
            _signInManagerMock.Verify(si => si.CheckPasswordSignInAsync(
                It.Is<ApplicationUser>(user => user.UserName == dto.Username && user.EmailConfirmed),
                dto.Password,
                false
            ), Times.Never);
        }

        [Test]
        public async Task Login_WhenUserProvidesInvalidPassword_ShouldReturnUnauthorized()
        {
            // Arrange
            var dto = new LoginDto
            {
                Username = "confirmed.user",
                Password = "WrongPassword123"
            };

            var returnedUser = new ApplicationUser
            {
                UserName = dto.Username,
                Email = "email.wrongpassword@example.com",
                EmailConfirmed = true
            };

            _userManagerMock.Setup(u => u.FindByNameAsync(dto.Username))
                .ReturnsAsync(returnedUser);

            _signInManagerMock.Setup(si => si.CheckPasswordSignInAsync(It.Is<ApplicationUser>(user =>
                user.UserName == dto.Username && user.EmailConfirmed),
                dto.Password,
                false
            )).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _authController.Login(dto);

            // Assert
            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            var payload = unauthorized.Value.Should().BeOfType<LoginUnauthorizedResponseDto>().Subject;

            payload.Message.Should().Be("Invalid username or password");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByNameAsync(dto.Username), Times.Once);

            _signInManagerMock.Verify(si => si.CheckPasswordSignInAsync(
                It.Is<ApplicationUser>(user => user.UserName == dto.Username && user.EmailConfirmed),
                dto.Password,
                false
            ), Times.Once);

            _userManagerMock.Verify(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        #endregion

        #region ConfirmEmailTests

        [Test]
        public async Task ConfirmEmail_WhenInputIsValid_ShouldReturnOk()
        {
            // Arrange
            const string sampleUserId = "valid-user-id";
            var sampleToken = TestUtils.GenerateTestToken(length: 64);

            var returnedUser = new ApplicationUser
            {
                Id = sampleUserId,
                UserName = "user.to.confirm",
                EmailConfirmed = false
            };

            _userManagerMock.Setup(u => u.FindByIdAsync(sampleUserId))
                .ReturnsAsync(returnedUser);

            _userManagerMock.Setup(u => u.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authController.ConfirmEmail(sampleUserId, sampleToken);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<EmailConfirmOkResponseDto>().Subject;

            payload.Message.Should().Be("Email confirmed successfully");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByIdAsync(sampleUserId), Times.Once);

            _userManagerMock.Verify(u => u.ConfirmEmailAsync(
                It.Is<ApplicationUser>(user => 
                    user.Id == sampleUserId
                ),
                It.IsAny<string>()
            ), Times.Once);
        }

        [Test]
        public async Task ConfirmEmail_WhenEmailIsAlreadyConfirmed_ShouldReturnOk()
        {
            // Arrange
            const string sampleUserId = "valid-user-id";
            var sampleToken = TestUtils.GenerateTestToken(length: 64);

            var returnedUser = new ApplicationUser
            {
                Id = sampleUserId,
                UserName = "user.to.confirm",
                EmailConfirmed = true
            };

            _userManagerMock.Setup(u => u.FindByIdAsync(sampleUserId))
                .ReturnsAsync(returnedUser);

            // Act
            var result = await _authController.ConfirmEmail(sampleUserId, sampleToken);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<EmailConfirmOkResponseDto>().Subject;

            payload.Message.Should().Be("Email confirmed successfully");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByIdAsync(sampleUserId), Times.Once);

            _userManagerMock.Verify(u => u.ConfirmEmailAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()
            ), Times.Never);
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("   ", "   ")]
        [TestCase("", null)]
        [TestCase(null, "")]
        public async Task ConfirmEmail_WhenInputIsNullOrWhitespace_ShouldReturnBadRequest(string? userId, string? token)
        {
            // Arranged by TestCase parameters

            // Act
            var result = await _authController.ConfirmEmail(userId!, token!);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<EmailConfirmBadRequestResponseDto>().Subject;

            payload.Message.Should().Be("UserId and Token are required");
            payload.Errors.Should().HaveCount(1).And.Contain("UserId and Token cannot be null or empty");
        }

        [Test]
        public async Task ConfirmEmail_WhenProvidedUserDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            const string sampleUserId = "nonexistent-user-id";
            var sampleToken = TestUtils.GenerateTestToken(length: 64);

            _userManagerMock.Setup(u => u.FindByIdAsync(sampleUserId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authController.ConfirmEmail(sampleUserId, sampleToken);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var payload = notFound.Value.Should().BeOfType<EmailConfirmNotFoundResponseDto>().Subject;
            payload.Message.Should().Be("User not found");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByIdAsync(sampleUserId), Times.Once);

            _userManagerMock.Verify(u => u.ConfirmEmailAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()
            ), Times.Never);
        }

        [Test]
        public async Task ConfirmEmail_WhenInvalidInputGiben_ShouldReturnBadRequest()
        {
            // Arrange
            const string sampleUserId = "invalid-user-id";
            var sampleToken = TestUtils.GenerateTestToken(length: 64);

            var returnedUser = new ApplicationUser
            {
                Id = sampleUserId,
                UserName = "user.to.confirm",
                EmailConfirmed = false
            };

            var failedResult = IdentityResult.Failed(
                new IdentityError { Description = "Invalid user Id" },
                new IdentityError { Description = "Invalid confirmation token" }
            );

            _userManagerMock.Setup(u => u.FindByIdAsync(sampleUserId))
                .ReturnsAsync(returnedUser);

            _userManagerMock.Setup(u => u.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _authController.ConfirmEmail(sampleUserId, sampleToken);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var payload = badRequest.Value.Should().BeOfType<EmailConfirmBadRequestResponseDto>().Subject;

            payload.Message.Should().Be("Email confirmation failed");
            payload.Errors.Should().HaveCount(2)
                .And.Contain("Invalid user Id")
                .And.Contain("Invalid confirmation token");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByIdAsync(sampleUserId), Times.Once);
            _userManagerMock.Verify(u => u.ConfirmEmailAsync(
                It.Is<ApplicationUser>(user =>
                    user.Id == sampleUserId
                ),
                It.IsAny<string>()
            ), Times.Once);
        }

        #endregion

        #region ResendConfirmEmailTests

        [Test]
        public async Task ResendConfirmEmail_WhenCalledWithValidInput_ShouldReturnOk()
        {
            // Arrange
            var dto = new ResendConfirmationEmailDto
            {
                Email = "email.to.confirm@example.com"
            };

            // Set last sent time to more than rate limit duration ago
            var returnedUser = new ApplicationUser
            {
                UserName = "user.to.confirm",
                Email = dto.Email,
                EmailConfirmed = false,
                LastConfirmEmailSent = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero)
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email))
                .ReturnsAsync(returnedUser);

            _dateTimeProviderMock.Setup(dtp => dtp.UtcNow)
                .Returns(new DateTimeOffset(2025, 1, 1, 12, 10, 0, TimeSpan.Zero));

            _emailConfirmTokenServiceMock.Setup(ect =>
                ect.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("new-email-confirmation-token");

            _emailSenderMock.Setup(es => es.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()));

            _userManagerMock.Setup(u => u.UpdateAsync(It.Is<ApplicationUser>(user => user.Email == dto.Email)));

            // Provide a usable HttpContext so controller can build base URL
            var httpContext = TestUtils.CreateHttpContext();
            _authController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _authController.ResendConfirmationEmail(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ResendConfirmationEmailOkResponseDto>().Subject;

            payload.Message.Should().Be("Confirmation email resent successfully");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByEmailAsync(dto.Email), Times.Once);

            _emailConfirmTokenServiceMock.Verify(ect =>
                ect.GenerateEmailConfirmationTokenAsync(
                    It.Is<ApplicationUser>(user => user.Email == dto.Email)
                ), Times.Once);

            _emailSenderMock.Verify(es => es.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);

            _userManagerMock.Verify(u => u.UpdateAsync(It.Is<ApplicationUser>(user =>
                user.Email == dto.Email)), Times.Once);
        }

        [Test]
        public async Task ResendConfirmEmail_WhenCalledWithInvalidEmail_ShouldReturnOk()
        {
            // Arrange
            var dto = new ResendConfirmationEmailDto
            {
                Email = "nonexistent@example.com"
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authController.ResendConfirmationEmail(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ResendConfirmationEmailOkResponseDto>().Subject;

            payload.Message.Should().Be("Confirmation email resent successfully");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByEmailAsync(dto.Email), Times.Once);
            _emailConfirmTokenServiceMock.Verify(ets =>
                ets.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Test]
        public async Task ResendConfirmEmail_WhenCalledWithConfirmedUser_ShouldReturnOk()
        {
            // Arrange
            var dto = new ResendConfirmationEmailDto
            {
                Email = "already.confirmed@example.com"
            };

            var returnedUser = new ApplicationUser
            {
                UserName = "already.confirmed.user",
                Email = dto.Email,
                EmailConfirmed = true
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email))
                .ReturnsAsync(returnedUser);

            // Act
            var result = await _authController.ResendConfirmationEmail(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ResendConfirmationEmailOkResponseDto>().Subject;

            payload.Message.Should().Be("Confirmation email resent successfully");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByEmailAsync(dto.Email), Times.Once);
            _emailConfirmTokenServiceMock.Verify(ets =>
                ets.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Test]
        public async Task ResendConfirmEmail_WhenCalledWithinRateLimiting_ShouldReturnOk()
        {
            // Arrange
            var dto = new ResendConfirmationEmailDto
            {
                Email = "email.to.confirm@example.com"
            };

            // Set last sent time to more than rate limit duration ago
            var returnedUser = new ApplicationUser
            {
                UserName = "user.to.confirm",
                Email = dto.Email,
                EmailConfirmed = false,
                LastConfirmEmailSent = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero)
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email))
                .ReturnsAsync(returnedUser);

            _dateTimeProviderMock.Setup(dtp => dtp.UtcNow)
                .Returns(new DateTimeOffset(2025, 1, 1, 12, 1, 59, TimeSpan.Zero));

            // Act
            var result = await _authController.ResendConfirmationEmail(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ResendConfirmationEmailOkResponseDto>().Subject;

            payload.Message.Should().Be("Confirmation email resent successfully");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByEmailAsync(dto.Email), Times.Once);
            _emailConfirmTokenServiceMock.Verify(ets =>
                ets.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        #endregion

        #region ForgotPasswordTests

        [Test]
        public async Task ForgotPassword_WhenCalledWithValidInput_ShouldReturnOk()
        {
            // Arrange
            var dto = new ForgotPasswordDto
            {
                Email = "email@example.com"
            };

            var returnedUser = new ApplicationUser
            {
                UserName = "user.name",
                Email = dto.Email,
                EmailConfirmed = true,
                LastPasswordResetSent = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero)
            };

            var sampleToken = TestUtils.GenerateTestToken(length: 64);

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email))
                .ReturnsAsync(returnedUser);

            _userManagerMock.Setup(u => u.IsEmailConfirmedAsync(returnedUser))
                .ReturnsAsync(true);

            _dateTimeProviderMock.Setup(u => u.UtcNow)
                .Returns(new DateTimeOffset(2025, 1, 1, 12, 5, 0, TimeSpan.Zero));

            _userManagerMock.Setup(u => u.GeneratePasswordResetTokenAsync(returnedUser))
                .ReturnsAsync(sampleToken);

            var httpContext = TestUtils.CreateHttpContext();
            _authController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _emailSenderMock.Setup(es => es.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()));

            _userManagerMock.Setup(u => u.UpdateAsync(It.Is<ApplicationUser>(user => user.Email == dto.Email)));

            // Act
            var result = await _authController.ForgotPassword(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ForgotPasswordOkResponseDto>().Subject;

            payload.Message.Should().Be(
                "If a user was registered with the provided email, a password reset link has been sent.");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByEmailAsync(dto.Email), Times.Once);

            _userManagerMock.Verify(u => u.IsEmailConfirmedAsync(
                It.Is<ApplicationUser>(user => user.Email == dto.Email)), Times.Once);

            _dateTimeProviderMock.Verify(dt => dt.UtcNow, Times.Exactly(2));

            _userManagerMock.Verify(u =>
                u.GeneratePasswordResetTokenAsync(It.Is<ApplicationUser>(user =>
                    user.Email == dto.Email)
                ), Times.Once);

            _emailSenderMock.Verify(es => es.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);

            _userManagerMock.Verify(u => u.UpdateAsync(It.Is<ApplicationUser>(user => user.Email == dto.Email)), Times.Once);
        }

        [Test]
        public async Task ForgotPassword_WhenCalledWithUnconfirmedUser_ShouldReturnOk()
        {
            // Arrange
            var dto = new ForgotPasswordDto
            {
                Email = "email@example.com"
            };

            var returnedUser = new ApplicationUser
            {
                UserName = "user.name",
                Email = dto.Email,
                EmailConfirmed = false
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email))
                .ReturnsAsync(returnedUser);

            _userManagerMock.Setup(u => u.IsEmailConfirmedAsync(returnedUser))
                .ReturnsAsync(false);

            // Act
            var result = await _authController.ForgotPassword(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ForgotPasswordOkResponseDto>().Subject;

            payload.Message.Should().Be(
                "If a user was registered with the provided email, a password reset link has been sent.");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByEmailAsync(dto.Email), Times.Once);
            _userManagerMock.Verify(u => u.IsEmailConfirmedAsync(
                It.Is<ApplicationUser>(user => user.Email == dto.Email)), Times.Once);
            _dateTimeProviderMock.Verify(d => d.UtcNow, Times.Never);
        }

        [Test]
        public async Task ForgotPassword_WhenCalledWithNonexistentEmail_ShouldReturnOk()
        {
            // Arrange
            var dto = new ForgotPasswordDto
            {
                Email = "nonexistent@example.com"
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authController.ForgotPassword(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ForgotPasswordOkResponseDto>().Subject;

            payload.Message.Should().Be(
                "If a user was registered with the provided email, a password reset link has been sent.");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByEmailAsync(dto.Email), Times.Once);
            _userManagerMock.Verify(u => u.IsEmailConfirmedAsync(
                It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Test]
        public async Task ForgotPassword_WhenCalledWithinRateLimiting_ShouldReturnOk()
        {
            // Arrange
            var dto = new ForgotPasswordDto
            {
                Email = "email@example.com"
            };

            var returnedUser = new ApplicationUser
            {
                UserName = "user.name",
                Email = dto.Email,
                EmailConfirmed = true,
                LastPasswordResetSent = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero)
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email))
                .ReturnsAsync(returnedUser);

            _userManagerMock.Setup(u => u.IsEmailConfirmedAsync(returnedUser))
                .ReturnsAsync(true);

            _dateTimeProviderMock.Setup(u => u.UtcNow)
                .Returns(new DateTimeOffset(2025, 1, 1, 12, 1, 59, TimeSpan.Zero));

            // Act
            var result = await _authController.ForgotPassword(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ForgotPasswordOkResponseDto>().Subject;
            payload.Message.Should().Be(
                "If a user was registered with the provided email, a password reset link has been sent.");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByEmailAsync(dto.Email), Times.Once);

            _userManagerMock.Verify(u => u.IsEmailConfirmedAsync(
                It.Is<ApplicationUser>(user => user.Email == dto.Email)), Times.Once);

            _dateTimeProviderMock.Verify(dt => dt.UtcNow, Times.Once);

            _userManagerMock.Verify(u => u.GeneratePasswordResetTokenAsync(It.Is<ApplicationUser>(user =>
                user.Email == dto.Email)), Times.Never);
        }

        #endregion

        #region ResetPasswordTests

        [Test]
        public async Task ResetPassword_WhenCalledWithValidInput_ShouldReturnOk()
        {
            // Arrange
            var dto = new ResetPasswordDto
            {
                UserId = "valid-user-id",
                Token = TestUtils.GenerateTestToken(length: 64),
                NewPassword = "NewStrongPassword123_",
                ConfirmPassword = "NewStrongPassword123_"
            };

            var returnedUser = new ApplicationUser
            {
                Id = dto.UserId,
                UserName = "user.name",
                Email = "email@example.com"
            };

            _userManagerMock.Setup(u => u.FindByIdAsync(dto.UserId))
                .ReturnsAsync(returnedUser);

            _userManagerMock.Setup(u => u.ResetPasswordAsync(
                It.Is<ApplicationUser>(user => user.Id == dto.UserId),
                It.IsAny<string>(),
                dto.NewPassword)
            ).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authController.ResetPassword(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<ResetPasswordOkResponseDto>().Subject;

            payload.Message.Should().Be("Password has been reset successfully.");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByIdAsync(dto.UserId), Times.Once);
            _userManagerMock.Verify(u => u.ResetPasswordAsync(
                It.Is<ApplicationUser>(user => user.Id == dto.UserId),
                It.IsAny<string>(),
                dto.NewPassword), Times.Once);
        }

        [Test]
        public async Task ResetPassword_WhenConfirmPasswordDoesNotMatch_ShouldReturnBadRequest()
        {
            // Arrange
            var dto = new ResetPasswordDto
            {
                UserId = "valid-user-id",
                Token = TestUtils.GenerateTestToken(length: 64),
                NewPassword = "NewStrongPassword123_",
                ConfirmPassword = "DifferentPassword123_"
            };

            // Act
            var result = await _authController.ResetPassword(dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var modelState = badRequest.Value.Should().BeOfType<SerializableError>().Subject;

            // The controller puts all errors under the empty key (model level errors)
            modelState.Should().ContainKey(string.Empty);
            var errors = modelState[string.Empty] as string[];
            errors.Should().Contain("New password and confirmation password do not match.");

            // Verify dependency calls - no user lookup performed
            _userManagerMock.Verify(u => u.FindByIdAsync(dto.UserId), Times.Never);
        }

        [Test]
        public async Task ResetPassword_WhenCalledWithInvalidUserId_ShouldReturnNotFound()
        {
            // Arrange
            var dto = new ResetPasswordDto
            {
                UserId = "nonexistent-user-id",
                Token = TestUtils.GenerateTestToken(length: 64),
                NewPassword = "NewStrongPassword123_",
                ConfirmPassword = "NewStrongPassword123_"
            };

            _userManagerMock.Setup(u => u.FindByIdAsync(dto.UserId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _authController.ResetPassword(dto);

            // Assert
            var notFound = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = notFound.Value.Should().BeOfType<ResetPasswordOkResponseDto>().Subject;

            payload.Message.Should().Be("Password has been reset successfully.");

            // Verify dependency calls
            _userManagerMock.Verify(u => u.FindByIdAsync(dto.UserId), Times.Once);
            _userManagerMock.Verify(u => u.ResetPasswordAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ResetPassword_WhenResetDoesNotSucceed_ShouldReturnBadRequest()
        {
            // Arrange
            var dto = new ResetPasswordDto
            {
                UserId = "valid-user-id",
                Token = TestUtils.GenerateTestToken(length: 64),
                NewPassword = "NewStrongPassword123_",
                ConfirmPassword = "NewStrongPassword123_"
            };

            var returnedUser = new ApplicationUser
            {
                Id = dto.UserId,
                UserName = "user.name",
                Email = "email@example.com"
            };

            _userManagerMock.Setup(u => u.FindByIdAsync(dto.UserId))
                .ReturnsAsync(returnedUser);

            _userManagerMock.Setup(u => u.ResetPasswordAsync(
                It.Is<ApplicationUser>(user => user.Id == dto.UserId),
                It.IsAny<string>(),
                dto.NewPassword)
            ).ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _authController.ResetPassword(dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().BeOfType<SerializableError>();

            // Since errors are not specified, we do not check for errors

            // Verify dependency calls -> all should have happened as expected
            _userManagerMock.Verify(u => u.FindByIdAsync(dto.UserId), Times.Once);
            _userManagerMock.Verify(u => u.ResetPasswordAsync(
                It.Is<ApplicationUser>(user => user.Id == dto.UserId),
                It.IsAny<string>(),
                dto.NewPassword), Times.Once);
        }

        #endregion
    }
}