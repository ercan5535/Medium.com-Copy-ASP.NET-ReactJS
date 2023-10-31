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
            string AccessToken = Request.Cookies["AccessToken"];
            string RefreshToken = Request.Cookies["RefreshToken"];
             
            // Delete access token from cache
            if (AccessToken != null)
            {
                await _service.LogoutUser(AccessToken);
                Response.Cookies.Delete("AccessToken");
            }

            // Blacklist refresh token
            if (RefreshToken != null)
            {
                await _service.ValidateBlacklistRefreshToken(RefreshToken);
                Response.Cookies.Delete("RefreshToken");
            }
            
            return Ok();
        }
    }
}