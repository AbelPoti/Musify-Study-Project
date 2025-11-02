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

namespace Musify.Tests
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<IEmailConfirmTokenService> _emailConfirmTokenServiceMock;
        private Mock<IEmailSender> _emailSenderMock;
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

            _authController = new AuthController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _emailConfirmTokenServiceMock.Object,
                _emailSenderMock.Object);
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
            var httpContext = new DefaultHttpContext
            {
                Request =
                {
                    Scheme = "https",
                    Host = new HostString("localhost", 5073)
                }
            };
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
            _userManagerMock.Verify(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        #endregion
    }
}