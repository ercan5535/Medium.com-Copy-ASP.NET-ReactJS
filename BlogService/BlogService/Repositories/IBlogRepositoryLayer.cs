using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogService.Dtos;
using BlogService.Models;

namespace BlogService.Services
{
    public interface IBlogRepositoryLayer
    {
        Task<UserGetDto> GetUserCache(string AccessToken);
        Task CreateBlogAsync(Blog newBlog);
        Task<List<Blog>> GetAllBlogsAsync();
        Task<List<GetBlogMetaDto>> GetAllBlogsMetaAsync(int page, int pageSize, string? username = null, string? query = null);
        Task<Blog> GetSingleBlogAsync(string blogId);
        Task<Blog> UpdateBlogContentAsync(string blogId, List<BlogContentItem> contentList, string userName);
        Task DeleteBlogAsync(string blogId, string userName);
        Task LikeBlog(string blogId, string userId);
        Task RemoveLikeBlog(string blogId, string userId);
        Task CreateCommentAsync(string blogId, BlogCommentItem newComment);
        Task DeleteCommentAsync(string blogId, string userName, DeleteCommentDto deleteCommentDto);
        Task UpdateCommentAsync(string blogId, string userName, UpdateCommentDto updateCommentDto);
    }
}