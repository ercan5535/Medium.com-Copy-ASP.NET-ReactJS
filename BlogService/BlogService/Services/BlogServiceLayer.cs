using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogService.Dtos;
using BlogService.Models;
using MongoDB.Bson;

namespace BlogService.Services
{
    public class BlogServiceLayer : IBlogServiceLayer
    {
        private readonly IBlogRepositoryLayer _repository;
        
        public BlogServiceLayer(IBlogRepositoryLayer repository)
        {
            _repository = repository;
        }
        
        public async Task<ServiceResponse<UserGetDto>> Authenticate(string accessToken)
        {
            try
            {
                UserGetDto userData = await _repository.GetUserCache(accessToken);
                
                if(userData is null)
                {
                    throw new Exception("Authenticaiton failed");
                }
                
                return new ServiceResponse<UserGetDto> {
                    Data = userData,
                    Message = "Authentication is successful"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserGetDto> {
                    Success = false,
                    Message = "Authenticaiton failed, error:" + ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Blog>> CreateBlogAsync(string userName, CreateBlogDto createBlogDto)
        {
            try
            {
                if (createBlogDto.BlogAuthor != userName)
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
                
                await _repository.CreateBlogAsync(newBlog);

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
                var allBlogs = await _repository.GetAllBlogsAsync();

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
            int page, int pageSize, string? username = null, string? query = null
        )
        {
            try
            {

                var allBlogsMeta =  await _repository.GetAllBlogsMetaAsync(
                    page, pageSize, username, query
                );
                
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
                var blog = await _repository.GetSingleBlogAsync(blogId);

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

        public async Task<ServiceResponse<Blog>> UpdateBlogContentAsync(string blogId, List<BlogContentItem> contentList, string userName)
        {
            try
            {
                var updatedBlog = await _repository.UpdateBlogContentAsync(blogId, contentList, userName);
                
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


        public async Task<ServiceResponse<bool>> DeleteBlogAsync(string blogId, string userName)
        {
            try
            {              
                await _repository.DeleteBlogAsync(blogId, userName);
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

        public async Task<ServiceResponse<bool>> LikeBlog(string blogId, string userName)
        {
            try
            {
                await _repository.LikeBlog(blogId, userName);
                
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

        public async Task<ServiceResponse<bool>> RemoveLikeBlog(string blogId, string userName)
        {
            try
            {
                await _repository.RemoveLikeBlog(blogId, userName);
                
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

        public async Task<ServiceResponse<BlogCommentItem>> CreateCommentAsync(string blogId, string userName, CreateCommentDto createCommentDto)
        {
            try
            {
                var newComment = new BlogCommentItem
                {
                    Author = userName,
                    Comment = createCommentDto.Comment
                };

                await _repository.CreateCommentAsync(blogId, newComment);

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

        public async Task<ServiceResponse<bool>> DeleteCommentAsync(string blogId, string userName, DeleteCommentDto deleteCommentDto)
        {
            try
            {
                await _repository.DeleteCommentAsync(blogId, userName, deleteCommentDto);

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

        public async Task<ServiceResponse<bool>> UpdateCommentAsync(string blogId, string userName, UpdateCommentDto updateCommentDto)
        {
            try
            {
                _repository.UpdateCommentAsync(blogId, userName, updateCommentDto);

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