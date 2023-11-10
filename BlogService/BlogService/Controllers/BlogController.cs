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
        private readonly IBlogServiceLayer _service;

        public BlogController(IBlogServiceLayer repository)
        {
            _service = repository;
        }

        [HttpGet]
        public async Task<ActionResult<Blog>> GetAllBlogs() 
        {
            var response = await _service.GetAllBlogsAsync();
            if (!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("meta")]
        public async Task<ActionResult<IEnumerable<GetBlogMetaDto>>> GetBlogsMeta(
            int page = 1, int pageSize = 3, string? username = null, string? query = null
        )
        {
            var response = await _service.GetAllBlogsMetaAsync(page, pageSize, username, query);
            if (!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("{blogId}")]
        public async Task<ActionResult<Blog>> GetSingleBlog(string blogId) 
        {
            var response = await _service.GetSingleBlogAsync(blogId);
            if (!response.Success){
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<Blog>> CreateBlog([FromBody] CreateBlogDto createBlogDto) 
        {
            string accessToken = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }

            var authResponse = await _service.Authenticate(accessToken);
            if (!authResponse.Success)
            {
                return Unauthorized(authResponse);
            }

            var response = await _service.CreateBlogAsync(authResponse.Data.UserName, createBlogDto);
            if (!response.Success){
                return BadRequest(response);
            }
            
            return Ok(response);
        }

        [HttpPut("{blogId}")]
        public async Task<ActionResult> UpdateContent(string blogId, [FromBody] List<BlogContentItem> contentList) 
        {
            string accessToken = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }

            var authResponse = await _service.Authenticate(accessToken);
            if (!authResponse.Success)
            {
                return Unauthorized(authResponse);
            }

            var response = await _service.UpdateBlogContentAsync(blogId, contentList, authResponse.Data.UserName);
            if (!response.Success){
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("{blogId}")]
        public async Task<ActionResult> DeleteBlog(string blogId) 
        {
            string accessToken = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }

            var authResponse = await _service.Authenticate(accessToken);
            if (!authResponse.Success)
            {
                return Unauthorized(authResponse);
            }

            var response = await _service.DeleteBlogAsync(blogId, authResponse.Data.UserName);
            if (!response.Success){
                return BadRequest(response);
            }
            
            return NoContent();
        }

        [HttpPost("like/{blogId}")]
        public async Task<ActionResult> LikeBlog(string blogId)
        {
            string accessToken = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }

            var authResponse = await _service.Authenticate(accessToken);
            if (!authResponse.Success)
            {
                return Unauthorized(authResponse);
            }

            var response = await _service.LikeBlog(blogId, authResponse.Data.UserName);
            if (!response.Success){
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("unlike/{blogId}")]
        public async Task<ActionResult> UnlikeBlog(string blogId)
        {
            string accessToken = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }

            var authResponse = await _service.Authenticate(accessToken);
            if (!authResponse.Success)
            {
                return Unauthorized(authResponse);
            }

            var response = await _service.RemoveLikeBlog(blogId, authResponse.Data.UserName);
            if (!response.Success){
                return BadRequest(response);
            }

            return Ok(response);
        }
    
        [HttpPost("comment/{blogId}")]
        public async Task<ActionResult> CreateComment(string blogId, [FromBody] CreateCommentDto createCommentDto )
        {
            string accessToken = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }

            var authResponse = await _service.Authenticate(accessToken);
            if (!authResponse.Success)
            {
                return Unauthorized(authResponse);
            }

            var response = await _service.CreateCommentAsync(blogId, authResponse.Data.UserName, createCommentDto);
            if (!response.Success){
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("comment/{blogId}")]
        public async Task<ActionResult> DeleteComment(string blogId, [FromBody] DeleteCommentDto deleteCommentDto)
        {
            string accessToken = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }

            var authResponse = await _service.Authenticate(accessToken);
            if (!authResponse.Success)
            {
                return Unauthorized(authResponse);
            }

            var response = await _service.DeleteCommentAsync(blogId, authResponse.Data.UserName, deleteCommentDto);
            if (!response.Success){
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut("comment/{blogId}")]
        public async Task<ActionResult> UpdateComment(string blogId, [FromBody] UpdateCommentDto updateCommentDto)
        {
            string accessToken = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }

            var authResponse = await _service.Authenticate(accessToken);
            if (!authResponse.Success)
            {
                return Unauthorized(authResponse);
            }

            var response = await _service.UpdateCommentAsync(blogId, authResponse.Data.UserName, updateCommentDto);
            if (!response.Success){
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}