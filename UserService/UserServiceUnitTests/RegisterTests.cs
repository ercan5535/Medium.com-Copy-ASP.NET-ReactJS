using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class RegisterTests
    {
        private Mock<IUserServiceLayer> _service;
        private RegisterController _registerController;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IUserServiceLayer>(MockBehavior.Strict);
            _registerController = new RegisterController(_service.Object);
        }

        [Test]
        public async Task Register_WithValidUserCreateDto_ShouldReturnOkResult()
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

            _service.Setup(x => x.RegisterUser(validUserCreateDto)).ReturnsAsync(serviceResponse);

            // Act
            var result = await _registerController.Register(validUserCreateDto) as OkObjectResult;

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
        public async Task Register_InvalidBodyNoUsername_ShouldReturnBadRequestResult()
        {
            // Arrange
            var invalidUserCreateDto = new UserCreateDto
            {
                Password = "password"
            };

            // Add model state error to simulate invalid ModelState
            _registerController.ModelState.AddModelError("UserName", "UserName is required");

            // Act
            var result = await _registerController.Register(invalidUserCreateDto) as BadRequestObjectResult;

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
        public async Task Register_InvalidBodyNoPassword_ShouldReturnBadRequestResult()
        {
            // Arrange
            var invalidUserCreateDto = new UserCreateDto
            {
                UserName = "UserName"
            };

            // Add model state error to simulate invalid ModelState
            _registerController.ModelState.AddModelError("Password", "Password is required");

            // Act
            var result = await _registerController.Register(invalidUserCreateDto) as BadRequestObjectResult;

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
        public async Task Register_WithUnsuccessfulRegistration_ShouldReturnBadRequestResult()
        {
            // Arrange
            var invalidUserCreateDto = new UserCreateDto
            {
                UserName = "UserName",
                Password = "Password"
            };

            var serviceResponse = new ServiceResponse<UserGetDto>
            {
                Success = false,
                Message = "Registration failed"
            };

            _service.Setup(x => x.RegisterUser(invalidUserCreateDto)).ReturnsAsync(serviceResponse);

            // Act
            var result = await _registerController.Register(invalidUserCreateDto) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual(serviceResponse, result.Value);

            var response = result.Value as ServiceResponse<UserGetDto>;
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.IsNull(response.Data);
            Assert.AreEqual("Registration failed", response.Message);
        }
    }
}