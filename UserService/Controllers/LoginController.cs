using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using UserService.Data;
using UserService.Dtos;
using UserService.Helpers;
using UserService.Models;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUserServiceRepo _repository;

        public LoginController(IUserServiceRepo repository, IConfiguration configuration)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            // Ensure request is proper
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check credentials
            var response = await _repository.Authenticate(userLoginDto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            UserGetDto currentUser = response.Data;

            // Create JWT tokens
            (string AccessToken, string RefreshToken) = await _repository.CreateNewTokens(currentUser);

            // Set the tokens in cookie
            Response.Cookies.Append("AccessToken", AccessToken, new CookieOptions
            {
                HttpOnly = true
            });

            Response.Cookies.Append("RefreshToken", RefreshToken, new CookieOptions
            {
                HttpOnly = true
            });

            return Ok(response);
        }
    }
}