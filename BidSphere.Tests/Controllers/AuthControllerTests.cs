using BidSphere.Controllers;
using BidSphere.Models.Domain;
using BidSphere.Models.Dtos.Auth;
using BidSphere.Models.Enums;
using BidSphere.Validators;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace BidSphere.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<RegisterDtoValidator> _registerValidatorMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                contextAccessorMock.Object,
                userPrincipalFactoryMock.Object,
                null, null, null, null);

            _registerValidatorMock = new Mock<RegisterDtoValidator>();

            _controller = new AuthController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _registerValidatorMock.Object);
        }

        // Note: Validator tests removed as FluentValidation's ValidateAsync cannot be mocked with Moq
        // The validators have their own comprehensive test suite in Validators/RegisterDtoValidatorTests.cs

        [Fact]
        public async Task GetProfile_WithValidUser_ShouldReturnUserDto()
        {
            // Arrange
            var userId = "1";
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetProfile();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var userDto = okResult!.Value as UserDto;
            userDto.Should().NotBeNull();
            userDto!.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetProfile_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetProfile();

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task UpdateProfile_WithValidData_ShouldReturnUpdatedUser()
        {
            // Arrange
            var userId = "1";
            var user = new User
            {
                Id = 1,
                Email = "old@example.com",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            var updateDto = new UserDto
            {
                UserId = 1,
                Email = "new@example.com",
                Role = "Admin",
                CreatedAt = user.CreatedAt
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.UpdateProfile(updateDto);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var userDto = okResult!.Value as UserDto;
            userDto.Should().NotBeNull();
            userDto!.Email.Should().Be(updateDto.Email);
        }

        [Fact]
        public async Task UpdateProfile_WithInvalidRole_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = "1";
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            var updateDto = new UserDto
            {
                UserId = 1,
                Email = "test@example.com",
                Role = "InvalidRole",
                CreatedAt = user.CreatedAt
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _controller.UpdateProfile(updateDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
