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
            var payload = ok.Value.Should().BeOfType<RegisterGoodResponseDto>().Subject;

            payload.Message.Should().Be("User registered successfully");
            payload.JwtToken.Should().Be(sampleJwtToken);
        }
    }
}