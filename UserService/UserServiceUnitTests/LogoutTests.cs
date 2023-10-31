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
    public class LogoutTests
    {
        private Mock<IUserServiceLayer> _service;
        private LogoutController _logoutController;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IUserServiceLayer>(MockBehavior.Strict);
            _logoutController = new LogoutController(_service.Object);
        }

        [Test]
        public async Task Logout_WithTokens_ShouldReturnOkResult()
        {
            var mockAccessToken = "dummyAccessToken";
            var mockRefreshToken = "dummyRefreshToken";

            _service.Setup(x => x.LogoutUser(mockAccessToken))
                    .ReturnsAsync(new ServiceResponse<string>())
                    .Verifiable();
            _service.Setup(x => x.ValidateBlacklistRefreshToken(mockRefreshToken))
                    .ReturnsAsync(new ServiceResponse<UserGetDto>())
                    .Verifiable();

            var cookiesMock = new Mock<IResponseCookies>(MockBehavior.Strict);
            cookiesMock.Setup(c => c.Delete("AccessToken"))
                    .Verifiable();
            cookiesMock.Setup(c => c.Delete("RefreshToken"))
                    .Verifiable();
            
            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.Setup(x => x.Request.Cookies["AccessToken"]).Returns(mockAccessToken);
            httpContextMock.Setup(x => x.Request.Cookies["RefreshToken"]).Returns(mockRefreshToken);
            httpContextMock.Setup(ctx => ctx.Response.Cookies).Returns(cookiesMock.Object);

            _logoutController.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            };

            // Act
            var result = await _logoutController.Logout() as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            _service.Verify(x => x.LogoutUser(mockAccessToken), Times.Once);
            _service.Verify(x => x.ValidateBlacklistRefreshToken(mockRefreshToken), Times.Once);
        }

        [Test]
        public async Task Logout_WithNoTokens_ShouldReturnOkResult()
        {
            var mockAccessToken = "dummyAccessToken";
            var mockRefreshToken = "dummyRefreshToken";

            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.Setup(x => x.Request.Cookies["AccessToken"]).Returns((string)null);
            httpContextMock.Setup(x => x.Request.Cookies["RefreshToken"]).Returns((string)null);

            _logoutController.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            };

            // Act
            var result = await _logoutController.Logout() as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            _service.Verify(x => x.LogoutUser(mockAccessToken), Times.Never);
            _service.Verify(x => x.ValidateBlacklistRefreshToken(mockRefreshToken), Times.Never);
        }
    }
}