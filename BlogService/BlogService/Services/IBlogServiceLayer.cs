using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogService.Dtos;
using BlogService.Models;

namespace BlogService.Services
{
    public interface IBlogServiceLayer
    {
        Task<ServiceResponse<UserGetDto>> Authenticate(string AccessToken);
        Task<ServiceResponse<Blog>> CreateBlogAsync(string userName, CreateBlogDto createBlogDto);
        Task<ServiceResponse<List<Blog>>> GetAllBlogsAsync();
        Task<ServiceResponse<List<GetBlogMetaDto>>> GetAllBlogsMetaAsync(int page, int pageSize, string? username = null, string? query = null);
        Task<ServiceResponse<Blog>> GetSingleBlogAsync(string blogId);
        Task<ServiceResponse<Blog>> UpdateBlogContentAsync(string blogId, List<BlogContentItem> contentList, string userName);
        Task<ServiceResponse<bool>> DeleteBlogAsync(string blogId, string userName);
        Task<ServiceResponse<bool>> LikeBlog(string blogId, string userName);
        Task<ServiceResponse<bool>> RemoveLikeBlog(string blogId, string userName);
        Task<ServiceResponse<BlogCommentItem>> CreateCommentAsync(string blogId, string userName, CreateCommentDto createCommentDto);
        Task<ServiceResponse<bool>> DeleteCommentAsync(string blogId, string userName, DeleteCommentDto deleteCommentDto);
        Task<ServiceResponse<bool>> UpdateCommentAsync(string blogId, string userName, UpdateCommentDto updateCommentDto);
    
    
    
    
    }
}