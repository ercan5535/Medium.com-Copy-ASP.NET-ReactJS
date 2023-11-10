using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UserService.Data;
using UserService.Helpers;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogoutController : ControllerBase
    {
        private readonly IUserServiceLayer _service;

        public LogoutController(IUserServiceLayer service)
        {
            _service = service;
        }

        [HttpDelete]
        public async Task<ActionResult> Logout()
        {
            // Get tokens from cookie
            string accessToken = Request.Cookies["AccessToken"];
            string refreshToken = Request.Cookies["RefreshToken"];
             
            // Delete access token from cache
            if (!string.IsNullOrEmpty(accessToken))
            {
                await _service.LogoutUser(accessToken);
                Response.Cookies.Delete("AccessToken");
            }

            // Blacklist refresh token
            if (!string.IsNullOrEmpty(accessToken))
            {
                await _service.ValidateBlacklistRefreshToken(refreshToken);
                Response.Cookies.Delete("RefreshToken");
            }
            
            return Ok();
        }
    }
}