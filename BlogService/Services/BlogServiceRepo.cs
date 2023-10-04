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
    public class BlogServiceRepo: IBlogServiceRepo
    {
        private readonly IMongoCollection<Blog> _blogCollection;
        private readonly IDistributedCache _distributedCache;
        
        public BlogServiceRepo(IOptions<MongoDBSettings> mongoDBSettings, IDistributedCache distributedCache)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _blogCollection = database.GetCollection<Blog>(mongoDBSettings.Value.CollectionName);
            _distributedCache = distributedCache;
        }

        public async Task<UserGetDto> Authenticate(string AccessToken)
        {
            if (AccessToken == null)
            {
                throw new Exception("Authentication token not found");
            }

            UserGetDto userData = await this.GetUserCache(AccessToken);
            if(userData == null)
            {
                throw new Exception("Authenticaiton failed");
            }
            
            return userData;
        }

        public async Task<UserGetDto> GetUserCache(string AccessToken)
        {
            // Get byte format from cache
            byte[] cacheValue = await _distributedCache.GetAsync(AccessToken);
            if (cacheValue == null)
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
        
        public async Task<ServiceResponse<Blog>> CreateBlogAsync(string AccessToken, CreateBlogDto createBlogDto)
        {
            try
            {
                UserGetDto currentUser = await this.Authenticate(AccessToken);

                if (createBlogDto.BlogAuthor != currentUser.UserName)
                {
                    throw new Exception("Users doesnt match");
                }

                var newBlog = new Blog
                {
                    BlogTitle=createBlogDto.BlogTitle,
                    BlogAuthor=createBlogDto.BlogAuthor,
                    BlogContent=createBlogDto.BlogContent,
                    Likes = new List<string>(),
                    Comments = new List<BlogCommentItem>()
                };
                
                await _blogCollection.InsertOneAsync(newBlog);

                return new ServiceResponse<Blog> {
                    Data = newBlog,
                    Message = "Blog created succesfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Blog> {
                    Success = false,
                    Message = "Failed to create blog, error:" + ex.Message
                };
            }
        }

        public async Task<ServiceResponse<List<Blog>>> GetAllBlogsAsync()
        {
            try
            {
                var allBlogs = await _blogCollection.Find(new BsonDocument()).ToListAsync();
                return new ServiceResponse<List<Blog>> {
                    Data = allBlogs,
                    Message = "All blogs returned successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Blog>> {
                    Success = false,
                    Message = "Failed to return all blogs, error:" + ex.Message
                };
            }
        }

        public async Task<ServiceResponse<List<GetBlogMetaDto>>> GetAllBlogsMetaAsync(
            int page, int pageSize, string? username = null, string? query = null)
        {
            try
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

                var allBlogsMeta =  await cursor.ToListAsync();
                return new ServiceResponse<List<GetBlogMetaDto>> {
                    Data = allBlogsMeta,
                    Message = "Blogs metadata returned successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GetBlogMetaDto>> {
                    Success = false,
                    Message = "Failed to return blogs metadata, error:" + ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Blog>> GetSingleBlogAsync(string blogId)
        {
            try
            {
                var filter = Builders<Blog>.Filter.Eq(b => b.Id, blogId);

                var blog = await _blogCollection.Find(filter).FirstOrDefaultAsync();
                return new ServiceResponse<Blog> {
                    Data = blog,
                    Message = $"Blog {blogId} returned succesfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Blog> {
                    Success = false,
                    Message = $"failted to return blog {blogId}, error:" + ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Blog>> UpdateBlogContentAsync(string blogId, List<BlogContentItem> contentList, string AccessToken)
        {
            try
            {
                UserGetDto currentUser = await this.Authenticate(AccessToken);
                
                var filter = Builders<Blog>.Filter.And(
                    Builders<Blog>.Filter.Eq(b => b.Id, blogId),
                    Builders<Blog>.Filter.Eq(b => b.BlogAuthor, currentUser.UserName)
                );
                var update = Builders<Blog>.Update.Set(b => b.BlogContent, contentList);

                var options = new FindOneAndUpdateOptions<Blog>
                {
                    ReturnDocument = ReturnDocument.After // This option returns the updated document
                };

                var updatedBlog = await _blogCollection.FindOneAndUpdateAsync(filter, update, options);
                
                return new ServiceResponse<Blog> {
                    Data = updatedBlog,
                    Message = $"Blog {blogId} updated succesfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Blog> {
                    Success = false,
                    Message = $"Failed to update blog {blogId}, error:" + ex.Message
                };
            }
        }

        public async Task<ServiceResponse<bool>> DeleteBlogAsync(string blogId, string AccessToken)
        {
            try
            {
                UserGetDto currentUser = await this.Authenticate(AccessToken);
                
                var filter = Builders<Blog>.Filter.And(
                    Builders<Blog>.Filter.Eq(b => b.Id, blogId),
                    Builders<Blog>.Filter.Eq(b => b.BlogAuthor, currentUser.UserName)
                );
                await _blogCollection.DeleteOneAsync(filter);                
                
                return new ServiceResponse<bool> {
                    Message = "Blog deleted succesfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool> {
                    Success = false,
                    Message = "Failed to delete blog, error:" + ex.Message
                };
            }
        }

        public async Task<ServiceResponse<bool>> LikeBlog(string blogId, string AccessToken)
        {
            try
            {
                UserGetDto currentUser = await this.Authenticate(AccessToken);
                
                var filter = Builders<Blog>.Filter.Eq(b => b.Id, blogId);
                var update = Builders<Blog>.Update.AddToSet(b => b.Likes, currentUser.UserName);

                await _blogCollection.UpdateOneAsync(filter, update);
                
                return new ServiceResponse<bool> {
                    Message = "Blog liked succesfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool> {
                    Success = false,
                    Message = "Failed to like blog, error:" + ex.Message
                };
            }
        }

        public async Task<ServiceResponse<bool>> RemoveLikeBlog(string blogId, string AccessToken)
        {
            try
            {
                UserGetDto currentUser = await this.Authenticate(AccessToken);
                
                var filter = Builders<Blog>.Filter.Eq(b => b.Id, blogId);
                var update = Builders<Blog>.Update.Pull(b => b.Likes, currentUser.UserName);

                await _blogCollection.UpdateOneAsync(filter, update);
                
                return new ServiceResponse<bool> {
                    Message = "Blog unliked succesfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool> {
                    Success = false,
                    Message = "Failed to unlike blog, error:" + ex.Message
                };
            }
        }

        public async Task<ServiceResponse<BlogCommentItem>> CreateCommentAsync(string blogId, string AccessToken, CreateCommentDto createCommentDto)
        {
            try
            {
                UserGetDto currentUser = await this.Authenticate(AccessToken);

                var newComment = new BlogCommentItem
                {
                    Author=currentUser.UserName,
                    Comment=createCommentDto.Comment
                };

                var filter = Builders<Blog>.Filter.Eq(b => b.Id, blogId);
                var update = Builders<Blog>.Update.Push(b => b.Comments, newComment);

                await _blogCollection.UpdateOneAsync(filter, update);
                return new ServiceResponse<BlogCommentItem> {
                    Data = newComment,
                    Message = "Comment created succesfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<BlogCommentItem> {
                    Success = false,
                    Message = "Failed to create comment, error:" + ex.Message
                };
            }
        }

        public async Task<ServiceResponse<bool>> DeleteCommentAsync(string blogId, string AccessToken, DeleteCommentDto deleteCommentDto)
        {
            try
            {
                UserGetDto currentUser = await this.Authenticate(AccessToken);

                var filter = Builders<Blog>.Filter.Eq(b => b.Id, blogId);
                var update = Builders<Blog>.Update.PullFilter(b => b.Comments, c => c.Id == deleteCommentDto.CommentId && c.Author == currentUser.UserName);

                await _blogCollection.UpdateOneAsync(filter, update);
                return new ServiceResponse<bool> {
                    Message = "Comment deleted succesfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool> {
                    Success = false,
                    Message = "Failed to delete comment, error:" + ex.Message
                };
            }
        }

        public async Task<ServiceResponse<bool>> UpdateCommentAsync(string blogId, string AccessToken, UpdateCommentDto updateCommentDto)
        {
            try
            {
                UserGetDto currentUser = await this.Authenticate(AccessToken);

                var filter = Builders<Blog>.Filter.And(
                    Builders<Blog>.Filter.Eq(x => x.Id, blogId),
                    Builders<Blog>.Filter.ElemMatch(x => x.Comments, c => c.Id == updateCommentDto.CommentId && c.Author == currentUser.UserName)
                );

                var update = Builders<Blog>.Update.Set("comments.$.comment", updateCommentDto.Comment);

                await _blogCollection.UpdateOneAsync(filter, update);
                return new ServiceResponse<bool> {
                    Message = "Comment updated succesfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool> {
                    Success = false,
                    Message = "Failed to update comment, error:" + ex.Message
                };
            }
        }
    }
}