using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using UserService.Controllers;
using UserService.Data;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Tests.Controllers
{
    [TestFixture]
    public class LoginControllerTests
    {
        private Mock<IUserServiceLayer> _service;
        private LoginController _loginController;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IUserServiceLayer>(MockBehavior.Strict);
            _loginController = new LoginController(_service.Object);
        }

        [Test]
        public async Task Login_ValidUser_ReturnsOkResultWithTokens()
        {
            // Arrange
            var userLoginDto = new UserLoginDto
            {
                UserName = "testuser",
                Password = "password123"
            };

            var userResponse = new ServiceResponse<UserGetDto>
            {
                Success = true,
                Data = new UserGetDto { UserName = userLoginDto.UserName }
            };

            var accessToken = "dummyAccessToken";
            var refreshToken = "dummyRefreshToken";

            _service.Setup(x => x.Authenticate(userLoginDto))
                                  .ReturnsAsync(userResponse);

            _service.Setup(x => x.CreateNewTokens(userResponse.Data))
                                  .ReturnsAsync((accessToken, refreshToken));

            var cookiesMock = new Mock<IResponseCookies>(MockBehavior.Strict);
            cookiesMock.Setup(c => c.Append("AccessToken", accessToken, It.IsAny<CookieOptions>()))
                .Verifiable();
            cookiesMock.Setup(c => c.Append("RefreshToken", refreshToken, It.IsAny<CookieOptions>()))
                .Verifiable();

            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.Setup(ctx => ctx.Response.Cookies).Returns(cookiesMock.Object);

            _loginController.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            };

            // Act
            var result = await _loginController.Login(userLoginDto) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var response = result.Value as ServiceResponse<UserGetDto>;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(userLoginDto.UserName, response.Data.UserName);

            cookiesMock.Verify(c => c.Append("AccessToken", accessToken, It.IsAny<CookieOptions>()));
            cookiesMock.Verify(c => c.Append("RefreshToken", refreshToken, It.IsAny<CookieOptions>()));
        }

        [Test]
        public async Task Login_InvalidBodyNoUsername_ReturnsBadRequestResult()
        {
            // Arrange
            var userLoginDto = new UserLoginDto() { Password = "123"}; // Invalid model state as properties are not set

            _loginController.ModelState.AddModelError("UserName", "UserName is required");

            // Act
            var result = await _loginController.Login(userLoginDto) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);

            var errorResponse = result.Value as SerializableError;
            Assert.IsNotNull(errorResponse);
            Assert.IsTrue(errorResponse.ContainsKey("UserName"));
            var errorMessage = (string[])errorResponse["UserName"];
            Assert.IsNotNull(errorMessage);
            Assert.AreEqual("UserName is required", errorMessage[0]);
        }

        [Test]
        public async Task Login_InvalidBodyNoPassword_ReturnsBadRequestResult()
        {
            // Arrange
            var userLoginDto = new UserLoginDto() { UserName = "Username"}; // Invalid model state as properties are not set

            _loginController.ModelState.AddModelError("Password", "Password is required");

            // Act
            var result = await _loginController.Login(userLoginDto) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);

            var errorResponse = result.Value as SerializableError;
            Assert.IsNotNull(errorResponse);
            Assert.IsTrue(errorResponse.ContainsKey("Password"));
            var errorMessage = (string[])errorResponse["Password"];
            Assert.IsNotNull(errorMessage);
            Assert.AreEqual("Password is required", errorMessage[0]);
        }

        [Test]
        public async Task Login_InvalidCredentials_ReturnsBadRequestResult()
        {
            // Arrange
            var userLoginDto = new UserLoginDto
            {
                UserName = "invaliduser",
                Password = "invalidpassword"
            };

            var userResponse = new ServiceResponse<UserGetDto>
            {
                Success = false,
                Message = "Invalid credentials"
            };

            _service.Setup(x => x.Authenticate(userLoginDto))
                .ReturnsAsync(userResponse);

            // Act
            var result = await _loginController.Login(userLoginDto) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);

            var response = result.Value as ServiceResponse<UserGetDto>;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.IsNull(response.Data);
            Assert.AreEqual(response.Message, "Invalid credentials");
        }
    }
}