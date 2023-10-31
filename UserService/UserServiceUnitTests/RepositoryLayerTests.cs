using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
    public class RepositoryLayerTests
    {
        private IDistributedCache _cache;
        private ApplicationDbContext _dbContext;
        private UserRepositoryLayer _userRepository;
        
        private string tokenInCache;
        private byte[] dataInCache;
        private User userInDb;

        [SetUp]
        public void Setup()
        {
            
            var cacheOptions = Options.Create<MemoryDistributedCacheOptions>
                (new MemoryDistributedCacheOptions());
            _cache = new MemoryDistributedCache(cacheOptions);

            tokenInCache = "sampleToken";
            dataInCache = new byte[] { 100, 200 };
            _cache.Set(tokenInCache, dataInCache);
          
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
       
            _dbContext = new ApplicationDbContext(dbOptions);

            userInDb =  new User 
            {
                UserId = 1,
                UserName = "UsernameInUse",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("ValidPassword")
            };

            _dbContext.UsersTable.Add(userInDb);
            _dbContext.SaveChanges();

            _userRepository = new UserRepositoryLayer(_dbContext, _cache);
        }

        [Test]
        public async Task GetUserById_WhenUserExists_ShouldReturnUserDto()
        {
            // Arrange
            int exitenUserId = userInDb.UserId;

            // Act
            var result = await _userRepository.GetUserById(exitenUserId);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(exitenUserId, result.UserId);
        }

        [Test]
        public async Task GetUserById_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            int nonExistentUserId = 999;

            // Act
            var result = await _userRepository.GetUserById(nonExistentUserId);

            // Assert
            Assert.Null(result);
        }
        [Test]
        public async Task GetUserByUsername_WhenUserExists_ShouldReturnUserDto()
        {
            // Arrange
            string exitenUserName = userInDb.UserName;

            // Act
            var result = await _userRepository.GetUserByUsername(exitenUserName);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(exitenUserName, result.UserName);
        }

        [Test]
        public async Task GetUserByUsername_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            string nonExistentUserName = "UsernameNotInDb";

            // Act
            var result = await _userRepository.GetUserByUsername(nonExistentUserName);

            // Assert
            Assert.Null(result);
        }

        [Test]
        public async Task AddUserAsync_ShouldAddUserToDatabase()
        {
            // Arrange
            User newUser = new User
            {
                UserId = 2,
                UserName = "NewUser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("NewPassword")
            };

            // Act
            await _userRepository.AddUserAsync(newUser);

            // Assert
            User addedUser = await _dbContext.UsersTable.FindAsync(newUser.UserId);
            Assert.IsNotNull(addedUser);
            Assert.AreEqual(newUser.UserName, addedUser.UserName);
            Assert.AreEqual(newUser.PasswordHash, addedUser.PasswordHash);
        }

        [Test]
        public async Task AddUserAsync_WhenUserExists_ShouldThrowException()
        {
            // Arrange
            User existUser = new User
            {
                UserId = userInDb.UserId
            };

            // Action
            async Task Action() => await _userRepository.AddUserAsync(existUser);

            // Act
            Assert.ThrowsAsync<InvalidOperationException>(Action);
        }

        [Test]
        public async Task AnyCache_WhenTokenExists_ShouldReturnTrue()
        {
            // Arrange
            string token = tokenInCache;

            // Act
            var result = await _userRepository.AnyCache(token);

            // Assert
            Assert.IsTrue(result);
        }
        
        [Test]
        public async Task AnyCache_WhenTokenDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            string token = "tokenNotInCache";

            // Act
            var result = await _userRepository.AnyCache(token);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task GetCache_WhenTokenExists_ShouldReturnData()
        {
            // Arrange
            string token = tokenInCache;

            // Act
            var result = await _userRepository.GetCache(token);

            // Assert
            Assert.AreEqual(result, dataInCache);
        }
        
        [Test]
        public async Task GetCache_WhenTokenDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            string token = "tokenNotInCache";

            // Act
            var result = await _userRepository.GetCache(token);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task DeleteCache_ShouldCallRemoveAsync()
        {
            // Arrange
            string token = tokenInCache;

            // Act
            await _userRepository.DeleteCache(token);

            // Assert
            Assert.IsNull(await _cache.GetAsync(tokenInCache));
        }
    }
}