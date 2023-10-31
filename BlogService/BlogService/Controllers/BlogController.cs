using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlogService.Models;
using BlogService.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using BlogService.Dtos;

namespace BlogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly IBlogServiceRepo _repository;

        public BlogController(IBlogServiceRepo repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<Blog>> GetAllBlogs() 
        {
            var response = await _repository.GetAllBlogsAsync();
            if (!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("meta")]
        public async Task<ActionResult<IEnumerable<GetBlogMetaDto>>> GetBlogsMeta(
            int page = 1, int pageSize = 3, string? username = null, string? query = null)
        {
            var response = await _repository.GetAllBlogsMetaAsync(page, pageSize, username, query);
            if (!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("{blogId}")]
        public async Task<ActionResult<Blog>> GetSingleBlog(string blogId) 
        {
            var response = await _repository.GetSingleBlogAsync(blogId);
            if (!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<Blog>> CreateBlog([FromBody] CreateBlogDto createBlogDto) 
        {
            string AccessToken = Request.Cookies["AccessToken"];
            var response = await _repository.CreateBlogAsync(AccessToken, createBlogDto);
            if (!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut("{blogId}")]
        public async Task<ActionResult> UpdateContent(string blogId, [FromBody] List<BlogContentItem> contentList) 
        {
            string AccessToken = Request.Cookies["AccessToken"];
            var response = await _repository.UpdateBlogContentAsync(blogId, contentList, AccessToken);
            if (!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("{blogId}")]
        public async Task<ActionResult> DeleteBlog(string blogId) 
        {
            string AccessToken = Request.Cookies["AccessToken"];
            var response = await _repository.DeleteBlogAsync(blogId, AccessToken);
            if (!response.Success){
                return BadRequest(response);
            }
            return NoContent();
        }

        [HttpPost("like/{blogId}")]
        public async Task<ActionResult> LikeBlog(string blogId)
        {
            string AccessToken = Request.Cookies["AccessToken"];
            var response = await _repository.LikeBlog(blogId, AccessToken);
            if (!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("unlike/{blogId}")]
        public async Task<ActionResult> UnlikeBlog(string blogId)
        {
            string AccessToken = Request.Cookies["AccessToken"];
            var response = await _repository.RemoveLikeBlog(blogId, AccessToken);
            if (!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }
    
        [HttpPost("comment/{blogId}")]
        public async Task<ActionResult> CreateComment(string blogId, [FromBody] CreateCommentDto createCommentDto )
        {
            string AccessToken = Request.Cookies["AccessToken"];
            var response = await _repository.CreateCommentAsync(blogId, AccessToken, createCommentDto);
            if (!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("comment/{blogId}")]
        public async Task<ActionResult> DeleteComment(string blogId, [FromBody] DeleteCommentDto deleteCommentDto)
        {
            string AccessToken = Request.Cookies["AccessToken"];
            var response = await _repository.DeleteCommentAsync(blogId, AccessToken, deleteCommentDto);
            if (!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpPut("comment/{blogId}")]
        public async Task<ActionResult> UpdateComment(string blogId, [FromBody] UpdateCommentDto updateCommentDto)
        {
            string AccessToken = Request.Cookies["AccessToken"];
            var response = await _repository.UpdateCommentAsync(blogId, AccessToken, updateCommentDto);
            if (!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}