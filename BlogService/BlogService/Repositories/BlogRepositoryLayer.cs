using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogService.Dtos;
using BlogService.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace BlogService.Services
{
    public class BlogRepositoryLayer: IBlogRepositoryLayer
    {
        private readonly IMongoCollection<Blog> _blogCollection;
        private readonly IDistributedCache _distributedCache;
        
        public BlogRepositoryLayer(IMongoCollection<Blog> blogCollection, IDistributedCache distributedCache)
        {
            _blogCollection = blogCollection;
            _distributedCache = distributedCache;
        }

        public async Task<UserGetDto> GetUserCache(string accessToken)
        {
            // Get byte format from cache
            byte[] cacheValue = await _distributedCache.GetAsync(accessToken);
            if (cacheValue is null)
            {
                return null;
            }
            // Convert bytes to JSON string
            string utf8String = Encoding.UTF8.GetString(cacheValue);
            if (utf8String == "blacklisted")
            {
                return null;
            }
            // Deserialize JSON string to object
            return JsonConvert.DeserializeObject<UserGetDto>(utf8String);
        }
        
        public async Task CreateBlogAsync(Blog newBlog)
        {
            await _blogCollection.InsertOneAsync(newBlog);
        }

        public async Task<List<Blog>> GetAllBlogsAsync()
        {
            var result = await _blogCollection.FindAsync(new BsonDocument());
            return await result.ToListAsync();
        }

        public async Task<List<GetBlogMetaDto>> GetAllBlogsMetaAsync(
            int page, int pageSize, string? username = null, string? query = null
        )
        {
            var pipeline = new List<BsonDocument>
            {
                // Filter out unwanted fields (in this case, blogContent)
                new BsonDocument("$project",
                    new BsonDocument
                    {
                        { "blogTitle", 1 },
                        { "blogAuthor", 1 },
                        { "createdAt", 1 },
                        { "likesCount", new BsonDocument("$size", "$likes") },
                        { "commentsCount", new BsonDocument("$size", "$comments") }
                    }
                ),
                // Sort by createdAt in descending order
                new BsonDocument("$sort",
                    new BsonDocument
                    {
                        { "createdAt", -1 }
                    }
                ),
                // Skip documents based on the page and limit the number of documents returned per page
                new BsonDocument("$skip", (page - 1) * pageSize),
                new BsonDocument("$limit", pageSize)
            };

            if (!string.IsNullOrEmpty(username))
            {
                // Add a $match stage to filter by username if it is provided
                pipeline.InsertRange(0, new[] { new BsonDocument("$match", new BsonDocument("blogAuthor", username)) });
            }

            if (!string.IsNullOrEmpty(query))
            {
                // Create separate BsonDocument objects for blogTitle and blogContent properties
                var blogTitleMatch = new BsonDocument("blogTitle", new BsonDocument("$regex", query).Add("$options", "i"));
                var blogContentMatch = new BsonDocument
                {
                    { "blogContent.type", "text" },
                    { "blogContent.content", new BsonDocument("$regex", query).Add("$options", "i") }
                };

                // Add a $match stage to filter by blogTitle or blogContent containing the query
                pipeline.InsertRange(0, new[]
                {
                    new BsonDocument("$match",
                        new BsonDocument("$or", new BsonArray
                        {
                            blogTitleMatch,
                            blogContentMatch
                        })
                    )
                });
            }

            var cursor = await _blogCollection.AggregateAsync<GetBlogMetaDto>(pipeline);

            return await cursor.ToListAsync();
        }

        public async Task<Blog> GetSingleBlogAsync(string blogId)
        {
            var filter = Builders<Blog>.Filter.Eq(b => b.Id, blogId);
            return await _blogCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Blog> UpdateBlogContentAsync(string blogId, List<BlogContentItem> contentList, string userName)
        {
            var filter = Builders<Blog>.Filter.And(
                Builders<Blog>.Filter.Eq(b => b.Id, blogId),
                Builders<Blog>.Filter.Eq(b => b.BlogAuthor, userName)
            );
            var update = Builders<Blog>.Update.Set(b => b.BlogContent, contentList);

            var options = new FindOneAndUpdateOptions<Blog>
            {
                ReturnDocument = ReturnDocument.After // This option returns the updated document
            };

            return await _blogCollection.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task DeleteBlogAsync(string blogId, string userName)
        {
            var filter = Builders<Blog>.Filter.And(
                Builders<Blog>.Filter.Eq(b => b.Id, blogId),
                Builders<Blog>.Filter.Eq(b => b.BlogAuthor, userName)
            );
            await _blogCollection.DeleteOneAsync(filter);
        }

        public async Task LikeBlog(string blogId, string userName)
        {
            var filter = Builders<Blog>.Filter.Eq(b => b.Id, blogId);
            var update = Builders<Blog>.Update.AddToSet(b => b.Likes, userName);

            await _blogCollection.UpdateOneAsync(filter, update);
        }

        public async Task RemoveLikeBlog(string blogId, string userName)
        {
            var filter = Builders<Blog>.Filter.Eq(b => b.Id, blogId);
            var update = Builders<Blog>.Update.Pull(b => b.Likes, userName);

            await _blogCollection.UpdateOneAsync(filter, update);
        }

        public async Task CreateCommentAsync(string blogId, BlogCommentItem newComment)
        {
            var filter = Builders<Blog>.Filter.Eq(b => b.Id, blogId);
            var update = Builders<Blog>.Update.Push(b => b.Comments, newComment);

            await _blogCollection.UpdateOneAsync(filter, update);
        }

        public async Task DeleteCommentAsync(string blogId, string userName, DeleteCommentDto deleteCommentDto)
        {
            var filter = Builders<Blog>.Filter.Eq(b => b.Id, blogId);
            var update = Builders<Blog>.Update.PullFilter(b => b.Comments, c => c.Id == deleteCommentDto.CommentId && c.Author == userName);

            await _blogCollection.UpdateOneAsync(filter, update);
        }

        public async Task UpdateCommentAsync(string blogId, string userName, UpdateCommentDto updateCommentDto)
        {
            var filter = Builders<Blog>.Filter.And(
                Builders<Blog>.Filter.Eq(x => x.Id, blogId),
                Builders<Blog>.Filter.ElemMatch(x => x.Comments, c => c.Id == updateCommentDto.CommentId && c.Author == userName)
            );

            var update = Builders<Blog>.Update.Set("comments.$.comment", updateCommentDto.Comment);

            await _blogCollection.UpdateOneAsync(filter, update);
        }
    }
}