using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserService.Data;
using UserService.Dtos;
using UserService.Helpers;
using UserService.Models;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckLoginStatusController : ControllerBase
    {
        private readonly IUserServiceRepo _repository;
        private readonly IConfiguration _configuration;

        public CheckLoginStatusController(IUserServiceRepo repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult<UserGetDto>> CheckLoginStatus()
        {
            // Get tokens from cookie
            string AccessToken = Request.Cookies["AccessToken"];
            string RefreshToken = Request.Cookies["RefreshToken"];
            
            ServiceResponse<UserGetDto> response;
            // First Check Access Token 
            if (AccessToken != null)
            {
                response = await _repository.CheckAccessTokenCache(AccessToken);
                if (response.Success)
                {
                    return Ok(response);
                }
            }

            // Check Refresh Token
            if (RefreshToken == null)
            {
                return Unauthorized();
            }

            // Validate refresh token
            response = await _repository.ValidateBlacklistRefreshToken(RefreshToken);
            if (!response.Success){
                return Unauthorized(response);
            }
            UserGetDto currentUser = response.Data;

            // Create new tokens
            (string newAccessToken, string newRefreshToken) = await _repository.CreateNewTokens(currentUser);

            // Set the tokens in cookie
            Response.Cookies.Append("AccessToken", newAccessToken, new CookieOptions
            {
                HttpOnly = true
            });

            Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true
            });

            return Ok(response);
        }
    }

}