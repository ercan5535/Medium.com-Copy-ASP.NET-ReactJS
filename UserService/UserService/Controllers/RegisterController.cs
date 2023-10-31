using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UserService.Data;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly IUserServiceLayer _service;

        public RegisterController(IUserServiceLayer service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserCreateDto userCreateDto)
        {
            // Ensure request is proper
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _service.RegisterUser(userCreateDto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}