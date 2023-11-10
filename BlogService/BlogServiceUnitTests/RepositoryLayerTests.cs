using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using BlogService.Models;
using MongoDB.Driver;
using BlogService.Dtos;
using Newtonsoft.Json;
using System.Text;
using BlogService.Services;
using MongoDB.Bson;


namespace BlogServiceUnitTests
{
    [TestFixture]
    public class RepositoryLayerTests
    {
        private IDistributedCache _cache;
        private Mock<IMongoCollection<Blog>> _collectionMock; 
        private BlogRepositoryLayer _blogRepository;
        private string validToken;
        private string blacklistedToken;
        private UserGetDto userInCache;

        [SetUp]
        public void Setup()
        {
            _collectionMock = new Mock<IMongoCollection<Blog>>(MockBehavior.Strict);

            // Define in memory cache for tests
            var cacheOptions = Options.Create<MemoryDistributedCacheOptions>
                (new MemoryDistributedCacheOptions());
            _cache = new MemoryDistributedCache(cacheOptions);
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(1));

            validToken = "sampleToken";
            blacklistedToken = "blacklistedToken";

            userInCache = new UserGetDto() {
                UserId = 1,
                UserName = "mockUserName"
            };

            // Set mock user to in memory cache
            string userPayload = JsonConvert.SerializeObject(userInCache);
            byte[] userPayloadBytes = Encoding.UTF8.GetBytes(userPayload);
            _cache.Set(validToken, userPayloadBytes, options);

            // Set "blacklisted" to in memory cache
            byte[] blacklistedPayloadBytes = Encoding.UTF8.GetBytes("blacklisted");
            _cache.Set(blacklistedToken, blacklistedPayloadBytes, options);

            _blogRepository = new BlogRepositoryLayer(_collectionMock.Object, _cache);
        }

        [Test]
        public async Task GetUserCache_ValidToken_ReturnsUserDto()
        {
            // Arrange
            var accessToken = validToken;

            // Act
            var result = await _blogRepository.GetUserCache(accessToken);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(userInCache.UserId, result.UserId);
            Assert.AreEqual(userInCache.UserName, result.UserName);
        }

        [Test]
        public async Task GetUserCache_BlacklistedToken_ReturnsNull()
        {
            // Arrange
            var accessToken = blacklistedToken;

            // Act
            var result = await _blogRepository.GetUserCache(accessToken);

            // Assert
            Assert.Null(result);
        }

        [Test]
        public async Task GetUserCache_InvalidToken_ReturnsNull()
        {
            // Arrange
            var accessToken = "invalidToken";

            // Act
            var result = await _blogRepository.GetUserCache(accessToken);

            // Assert
            Assert.Null(result);
        }

        [Test]
        public async Task CreateBlogAsync_ShouldInsertBlogIntoCollection()
        {
            // Arrange
            var newBlog = new Blog
            {
                BlogTitle = "Mock Title"
            };

            _collectionMock.Setup(x => x.InsertOneAsync(newBlog, It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

            // Act
            await _blogRepository.CreateBlogAsync(newBlog);

            // Assert
            _collectionMock.Verify(x => x.InsertOneAsync(newBlog, It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetAllBlogsAsync_ShouldReturnListOfBlogs()
        {
            // Arrange
            var expectedBlogs = new List<Blog>
            {
                new Blog { BlogTitle = "first blog" },
                new Blog { BlogTitle = "second blog" },
            };

            var asyncCursor = new Mock<IAsyncCursor<Blog>>();
            asyncCursor.Setup(_ => _.Current).Returns(expectedBlogs);
            asyncCursor
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            asyncCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));
            
            _collectionMock.Setup(_collection => _collection.FindAsync(
                It.IsAny<FilterDefinition<Blog>>(), 
                It.IsAny<FindOptions<Blog, Blog>>(), 
                It.IsAny<CancellationToken>()
             )).ReturnsAsync(asyncCursor.Object);

            // Act
            var result = await _blogRepository.GetAllBlogsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<Blog>>(result);
            Assert.AreEqual(expectedBlogs.Count, result.Count);

            // You can further assert individual blog properties if needed
            Assert.AreEqual(expectedBlogs[0].BlogTitle, result[0].BlogTitle);
            Assert.AreEqual(expectedBlogs[1].BlogTitle, result[1].BlogTitle);
        }
    }
}