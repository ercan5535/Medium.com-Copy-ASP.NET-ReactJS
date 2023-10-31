using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using UserService.Controllers;
using UserService.Data;
using UserService.Dtos;
using UserService.Models;

namespace UserServiceUnitTests
{
    [TestFixture]
    public class CheckLoginStatusTests
    {
        private Mock<IUserServiceLayer> _service;
        private CheckLoginStatusController _checkLoginStatusController;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IUserServiceLayer>(MockBehavior.Strict);
            _checkLoginStatusController = new CheckLoginStatusController(_service.Object);
        }

        [Test]
        public async Task CheckLoginStatus_WithValidAccessToken_ShouldReturnOkResult()
        {
            // Arrange
            var validUserCreateDto = new UserCreateDto
            {
                UserName = "UserName",
                Password = "Password"
            };

            var serviceResponse = new ServiceResponse<UserGetDto>
            {
                Success = true,
                Data = new UserGetDto() 
                {
                    UserName = validUserCreateDto.UserName
                }
            };

            var mockAccessToken = "dummyAccessToken";

            _service.Setup(x => x.CheckAccessTokenCache(mockAccessToken))
                    .ReturnsAsync(serviceResponse)
                    .Verifiable();

            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.Setup(x => x.Request.Cookies["AccessToken"]).Returns(mockAccessToken);
            httpContextMock.Setup(x => x.Request.Cookies["RefreshToken"]).Returns((string)null);

            _checkLoginStatusController.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            };

            // Act
            var result = await _checkLoginStatusController.CheckLoginStatus() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(serviceResponse, result.Value);

            var response = result.Value as ServiceResponse<UserGetDto>;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(validUserCreateDto.UserName, response.Data.UserName);
        }

        [Test]
        public async Task CheckLoginStatus_WithValidRefreshToken_ShouldReturnOkResult()
        {
            // Arrange
            var validUserCreateDto = new UserCreateDto
            {
                UserName = "UserName",
                Password = "Password"
            };

            var serviceSuccessResponse = new ServiceResponse<UserGetDto>
            {
                Success = true,
                Data = new UserGetDto() 
                {
                    UserName = validUserCreateDto.UserName
                }
            };
            var serviceFailResponse = new ServiceResponse<UserGetDto>
            {
                Success = false,
            };

            var mockAccessToken = "dummyAccessToken";
            var mockRefreshToken = "dummyRefreshToken";

            _service.Setup(x => x.CheckAccessTokenCache(mockAccessToken))
                    .ReturnsAsync(serviceFailResponse)
                    .Verifiable();
            _service.Setup(x => x.ValidateBlacklistRefreshToken(mockRefreshToken))
                    .ReturnsAsync(serviceSuccessResponse)
                    .Verifiable();
            _service.Setup(x => x.CreateNewTokens(serviceSuccessResponse.Data))
                    .ReturnsAsync(("newAccessToken", "newRefreshToken"))
                    .Verifiable();

            var cookiesMock = new Mock<IResponseCookies>(MockBehavior.Strict);
            cookiesMock.Setup(c => c.Append("AccessToken", "newAccessToken", It.IsAny<CookieOptions>()))
                .Verifiable();
            cookiesMock.Setup(c => c.Append("RefreshToken", "newRefreshToken", It.IsAny<CookieOptions>()))
                .Verifiable();
            
            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.Setup(x => x.Request.Cookies["AccessToken"]).Returns(mockAccessToken);
            httpContextMock.Setup(x => x.Request.Cookies["RefreshToken"]).Returns(mockRefreshToken);
            httpContextMock.Setup(ctx => ctx.Response.Cookies).Returns(cookiesMock.Object);

            _checkLoginStatusController.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            };

            // Act
            var result = await _checkLoginStatusController.CheckLoginStatus() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(serviceSuccessResponse, result.Value);

            var response = result.Value as ServiceResponse<UserGetDto>;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(validUserCreateDto.UserName, response.Data.UserName);
        }

        [Test]
        public async Task CheckLoginStatus_WithInvalidRefreshToken_ShouldReturnUnauthorizedResult()
        {
            // Arrange
            var serviceFailResponse = new ServiceResponse<UserGetDto>
            {
                Success = false,
                Message = "Invalid tokens"
            };

            var mockAccessToken = "dummyAccessToken";
            var mockRefreshToken = "dummyRefreshToken";

            _service.Setup(x => x.CheckAccessTokenCache(mockAccessToken))
                    .ReturnsAsync(serviceFailResponse)
                    .Verifiable();
            _service.Setup(x => x.ValidateBlacklistRefreshToken(mockRefreshToken))
                    .ReturnsAsync(serviceFailResponse)
                    .Verifiable();

            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.Setup(x => x.Request.Cookies["AccessToken"]).Returns(mockAccessToken);
            httpContextMock.Setup(x => x.Request.Cookies["RefreshToken"]).Returns(mockRefreshToken);

            _checkLoginStatusController.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            };

            // Act
            var result = await _checkLoginStatusController.CheckLoginStatus() as UnauthorizedObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(401, result.StatusCode);
            Assert.AreEqual(serviceFailResponse, result.Value);

            var response = result.Value as ServiceResponse<UserGetDto>;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.IsNull(response.Data);
            Assert.AreEqual("Invalid tokens", response.Message);
        }

        [Test]
        public async Task CheckLoginStatus_WithNoTokens_ShouldReturnUnauthorizedResult()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.Setup(x => x.Request.Cookies["AccessToken"]).Returns((string)null);
            httpContextMock.Setup(x => x.Request.Cookies["RefreshToken"]).Returns((string)null);

            _checkLoginStatusController.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            };

            // Act
            var result = await _checkLoginStatusController.CheckLoginStatus() as UnauthorizedObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(401, result.StatusCode);

            var response = result.Value as ServiceResponse<UserGetDto>;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.IsNull(response.Data);
            Assert.AreEqual("No tokens found", response.Message);
        }
    }
}