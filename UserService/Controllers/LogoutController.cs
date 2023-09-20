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
        private readonly IUserServiceRepo _repository;

        public LogoutController(IUserServiceRepo repository)
        {
            _repository = repository;
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
                await _repository.DeleteCache(AccessToken);
            }

            // Blacklist refresh token
            if (RefreshToken != null)
            {
                await _repository.ValidateBlacklistRefreshToken(RefreshToken);
            }
            
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");
            return Ok();
        }
    }
}