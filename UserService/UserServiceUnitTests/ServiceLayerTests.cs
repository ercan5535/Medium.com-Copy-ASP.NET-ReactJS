using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using UserService.Data;
using UserService.Dtos;
using UserService.Helpers;
using UserService.Models;
using UserService.Repositories;

namespace UserServiceUnitTests
{
    [TestFixture]
    public class ServiceLayerTests
    {
        private Mock<IUserRepositoryLayer> _repositoryMock;
        private Mock<IMapper> _mapperMock;
        private Mock<IJwtHelper> _jwtHelperMock;

        private IConfiguration _configuration;
        private UserServiceLayer _userServiceLayer;

        private string jwtSecretKey;

        [SetUp]
        public void Setup()
        {
            jwtSecretKey = "secretKey";

            var inMemorySettings = new Dictionary<string, string> 
            {
                {"JwtSecretKey", jwtSecretKey}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            _repositoryMock = new Mock<IUserRepositoryLayer>(MockBehavior.Strict);       
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);         
            _jwtHelperMock = new Mock<IJwtHelper>(MockBehavior.Strict);

            _userServiceLayer = new UserServiceLayer
            (
                repository: _repositoryMock.Object, 
                mapper: _mapperMock.Object, 
                configuration: _configuration, 
                jwtHelper:_jwtHelperMock.Object
            );
        }

        [Test]
        public async Task RegisterUser_WithValidUser_ReturnsSuccessResponse()
        {
            // Arrange
            var userCreateDto = new UserCreateDto
            {
                UserName = "NewUserName",
                Password = "password123"
            };
            
            _repositoryMock.Setup(repo => repo.GetUserByUsername(userCreateDto.UserName))
                .ReturnsAsync(() => null);

            _repositoryMock.Setup(repo => repo.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            
            _mapperMock.Setup(mapper => mapper.Map<UserGetDto>(It.IsAny<User>()))
                      .Returns(new UserGetDto { UserName = userCreateDto.UserName });
            
            // Act
            var result = await _userServiceLayer.RegisterUser(userCreateDto);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual("NewUserName", result.Data.UserName);

            _repositoryMock.Verify(repo => repo.GetUserByUsername(userCreateDto.UserName), Times.Once);
            _repositoryMock.Verify(repo => repo.AddUserAsync(It.IsAny<User>()), Times.Once);
            _mapperMock.Verify(mapper => mapper.Map<UserGetDto>(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public async Task RegisterUser_DuplicateUsername_ReturnsErrorResponse()
        {
            // Arrange
            var existingUser = new User();
            var userCreateDto = new UserCreateDto
            {
                UserName = "existinguser",
                Password = "password123"
            };

            _repositoryMock.Setup(repo => repo.GetUserByUsername(userCreateDto.UserName))
                        .ReturnsAsync(existingUser);

            // Act
            var result = await _userServiceLayer.RegisterUser(userCreateDto);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Username is already used", result.Message);
            Assert.IsNull(result.Data);

            _repositoryMock.Verify(repo => repo.GetUserByUsername(userCreateDto.UserName), Times.Once);
            _repositoryMock.Verify(repo => repo.AddUserAsync(It.IsAny<User>()), Times.Never);
            _mapperMock.Verify(mapper => mapper.Map<UserGetDto>(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task Authenticate_ValidCredentials_ReturnsSuccessResponse()
        {
            // Arrange
            var userLoginDto = new UserLoginDto
            {
                UserName = "existinguser",
                Password = "password123"
            };

            var mockUser = new User
            {
                UserId = 1,
                UserName = userLoginDto.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userLoginDto.Password)
            };

            _repositoryMock.Setup(repo => repo.GetUserByUsername(userLoginDto.UserName))
                        .ReturnsAsync(mockUser);

            _mapperMock.Setup(mapper => mapper.Map<UserGetDto>(mockUser))
                    .Returns(new UserGetDto { UserName = userLoginDto.UserName });

            // Act
            var result = await _userServiceLayer.Authenticate(userLoginDto);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsEmpty(result.Message);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual("existinguser", result.Data.UserName);
            
            _repositoryMock.Verify(repo => repo.GetUserByUsername(userLoginDto.UserName), Times.Once);
            _mapperMock.Verify(mapper => mapper.Map<UserGetDto>(mockUser), Times.Once);
        }

        [Test]
        public async Task Authenticate_InvalidUsername_ReturnsErrorResponse()
        {
            // Arrange
            var userLoginDto = new UserLoginDto
            {
                UserName = "nonexistentuser",
                Password = "password123"
            };

            _repositoryMock.Setup(repo => repo.GetUserByUsername(userLoginDto.UserName))
                        .ReturnsAsync((User)null);

            // Act
            var result = await _userServiceLayer.Authenticate(userLoginDto);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual($"Username {userLoginDto.UserName} not found", result.Message);
            Assert.IsNull(result.Data);
            
            _repositoryMock.Verify(repo => repo.GetUserByUsername(userLoginDto.UserName), Times.Once);
            _mapperMock.Verify(mapper => mapper.Map<UserGetDto>(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task Authenticate_InvalidPassword_ReturnsErrorResponse()
        {
            // Arrange
            var userLoginDto = new UserLoginDto
            {
                UserName = "existinguser",
                Password = "wrongpassword"
            };

            var mockUser = new User
            {
                UserId = 1,
                UserName = userLoginDto.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
            };

            _repositoryMock.Setup(repo => repo.GetUserByUsername(userLoginDto.UserName))
                        .ReturnsAsync(mockUser);

            // Act
            var result = await _userServiceLayer.Authenticate(userLoginDto);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Invalid Password", result.Message);
            Assert.IsNull(result.Data);
            
            _repositoryMock.Verify(repo => repo.GetUserByUsername(userLoginDto.UserName), Times.Once);
            _mapperMock.Verify(mapper => mapper.Map<UserGetDto>(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task CheckAccessTokenCache_ValidToken_ReturnsUserData()
        {
            // Arrange
            var accessToken = "validAccessToken";
            var cachedUser = new UserGetDto
            {
                UserName = "cacheduser"
            };

            var jsonUserData = JsonConvert.SerializeObject(cachedUser);
            var utf8Bytes = Encoding.UTF8.GetBytes(jsonUserData);

            _repositoryMock.Setup(repo => repo.GetCache(accessToken))
                    .ReturnsAsync(utf8Bytes);

            // Act
            var result = await _userServiceLayer.CheckAccessTokenCache(accessToken);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsEmpty(result.Message);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual("cacheduser", result.Data.UserName);
            
            _repositoryMock.Verify(repo => repo.GetCache(accessToken), Times.Once);
        }

        [Test]
        public async Task CheckAccessTokenCache_TokenNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var accessToken = "nonexistentToken";

            _repositoryMock.Setup(repo => repo.GetCache(accessToken))
                    .ReturnsAsync((byte[])null);

            // Act
            var result = await _userServiceLayer.CheckAccessTokenCache(accessToken);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Token not found in cache", result.Message);
            Assert.IsNull(result.Data);
            
            _repositoryMock.Verify(repo => repo.GetCache(accessToken), Times.Once);
        }

        [Test]
        public async Task CheckAccessTokenCache_BlacklistedToken_ReturnsErrorResponse()
        {
            // Arrange
            var accessToken = "blacklistedToken";

            var utf8Bytes = Encoding.UTF8.GetBytes("blacklisted");

            _repositoryMock.Setup(repo => repo.GetCache(accessToken))
                    .ReturnsAsync(utf8Bytes);

            // Act
            var result = await _userServiceLayer.CheckAccessTokenCache(accessToken);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Token is not valid", result.Message);
            Assert.IsNull(result.Data);
            
            _repositoryMock.Verify(repo => repo.GetCache(accessToken), Times.Once);
        }

        [Test]
        public async Task ValidateBlacklistRefreshToken_ValidToken_ReturnsUserData()
        {
            // Arrange
            var refreshToken = "validRefreshToken";
            var currentUser = new UserGetDto
            {
                UserName = "currentuser"
            };
            var expiration = DateTime.UtcNow.AddHours(1);

            var tokenPayload = JsonConvert.SerializeObject(currentUser);


            _repositoryMock.Setup(repo => repo.AnyCache(refreshToken))
                        .ReturnsAsync(false); // Token not blacklisted

            _jwtHelperMock.Setup(jwt => jwt.DecodeJwtToken(refreshToken, jwtSecretKey))
                        .Returns((currentUser, expiration, "refresh")); // Valid refresh token

            _repositoryMock.Setup(repo => repo.SetCache(refreshToken, It.IsAny<string>(), expiration))
                        .Returns(Task.CompletedTask); // Blacklist token in cache

            // Act
            var result = await _userServiceLayer.ValidateBlacklistRefreshToken(refreshToken);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsEmpty(result.Message);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual("currentuser", result.Data.UserName);

            _repositoryMock.Verify(repo => repo.AnyCache(refreshToken), Times.Once);
            _jwtHelperMock.Verify(jwt => jwt.DecodeJwtToken(refreshToken, jwtSecretKey), Times.Once);
            _repositoryMock.Verify(repo => repo.SetCache(refreshToken, It.IsAny<string>(), expiration), Times.Once);
        }

        [Test]
        public async Task ValidateBlacklistRefreshToken_BlacklistedToken_ReturnsErrorResponse()
        {
            // Arrange
            var refreshToken = "blacklistedRefreshToken";

            _repositoryMock.Setup(repo => repo.AnyCache(refreshToken))
                        .ReturnsAsync(true); // Token blacklisted

            // Act
            var result = await _userServiceLayer.ValidateBlacklistRefreshToken(refreshToken);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Blacklisted token", result.Message);
            Assert.IsNull(result.Data);

            _repositoryMock.Verify(repo => repo.AnyCache(refreshToken), Times.Once);
            _jwtHelperMock.Verify(jwt => jwt.DecodeJwtToken(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(repo => repo.SetCache(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Test]
        public async Task ValidateBlacklistRefreshToken_InvalidRefreshToken_ReturnsErrorResponse()
        {
            // Arrange
            var refreshToken = "invalidRefreshToken";

            _repositoryMock.Setup(repo => repo.AnyCache(refreshToken))
                        .ReturnsAsync(false); // Token not blacklisted

            _jwtHelperMock.Setup(jwt => jwt.DecodeJwtToken(refreshToken, jwtSecretKey))
                        .Throws(new Exception("Invalid token")); // Invalid refresh token

            // Act
            var result = await _userServiceLayer.ValidateBlacklistRefreshToken(refreshToken);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Invalid token", result.Message);
            Assert.IsNull(result.Data);

            _repositoryMock.Verify(repo => repo.AnyCache(refreshToken), Times.Once);
            _jwtHelperMock.Verify(jwt => jwt.DecodeJwtToken(refreshToken, jwtSecretKey), Times.Once);
            _repositoryMock.Verify(repo => repo.SetCache(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Test]
        public async Task CreateNewTokens_ValidUser_ReturnsTokensAndCachesAccessToken()
        {
            // Arrange
            var currentUser = new UserGetDto
            {
                UserName = "currentuser"
            };

            var expectedAccessToken = "newAccessToken";
            var expectedRefreshToken = "newRefreshToken";

            _jwtHelperMock.Setup(jwt => jwt.CreateJwtToken(currentUser, "access", jwtSecretKey, It.IsAny<DateTime>()))
                        .Returns(expectedAccessToken);

            _jwtHelperMock.Setup(jwt => jwt.CreateJwtToken(currentUser, "refresh", jwtSecretKey, It.IsAny<DateTime>()))
                        .Returns(expectedRefreshToken);

            string expectedPayload = JsonConvert.SerializeObject(currentUser);

            _repositoryMock.Setup(repo => repo.SetCache(expectedAccessToken, expectedPayload, It.IsAny<DateTime>()))
                        .Returns(Task.CompletedTask);

            // Act
            var (newAccessToken, newRefreshToken) = await _userServiceLayer.CreateNewTokens(currentUser);

            // Assert
            Assert.AreEqual(expectedAccessToken, newAccessToken);
            Assert.AreEqual(expectedRefreshToken, newRefreshToken);

            _jwtHelperMock.Verify(jwt => jwt.CreateJwtToken(currentUser, "access", jwtSecretKey, It.IsAny<DateTime>()), Times.Once);
            _jwtHelperMock.Verify(jwt => jwt.CreateJwtToken(currentUser, "refresh", jwtSecretKey, It.IsAny<DateTime>()), Times.Once);
            _repositoryMock.Verify(repo => repo.SetCache(expectedAccessToken, expectedPayload, It.IsAny<DateTime>()), Times.Once);
        }
    }
}