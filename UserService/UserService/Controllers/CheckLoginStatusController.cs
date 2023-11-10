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
        private readonly IUserServiceLayer _service;

        public CheckLoginStatusController(IUserServiceLayer service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult> CheckLoginStatus()
        {
            // Get tokens from cookie
            string accessToken = Request.Cookies["AccessToken"];
            string refreshToken = Request.Cookies["RefreshToken"];
            
            ServiceResponse<UserGetDto> response;
            // First Check Access Token 
            if (!string.IsNullOrEmpty(accessToken))
            {
                response = await _service.CheckAccessTokenCache(accessToken);
                if (response.Success)
                {
                    return Ok(response);
                }
            }

            // Check Refresh Token
            if (string.IsNullOrEmpty(refreshToken))
            {
                response = new ServiceResponse<UserGetDto>
                {
                    Success = false,
                    Message = "No tokens found"
                };
                return Unauthorized(response);
            }

            // Validate refresh token
            response = await _service.ValidateBlacklistRefreshToken(refreshToken);
            if (!response.Success){
                return Unauthorized(response);
            }
            UserGetDto currentUser = response.Data;

            // Create new tokens
            (string newAccessToken, string newRefreshToken) = await _service.CreateNewTokens(currentUser);

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