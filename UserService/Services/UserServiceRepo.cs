using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using UserService.Dtos;
using UserService.Helpers;
using UserService.Models;

namespace UserService.Data
{
    public class UserServiceRepo : IUserServiceRepo
    {
        private readonly ApplicationDbContext _db;
        private readonly IDistributedCache _distributedCache;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserServiceRepo(ApplicationDbContext db, IDistributedCache distributedCache, IMapper mapper, IConfiguration configuration)
        {
            _db = db;
            _distributedCache = distributedCache;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<UserGetDto>> RegisterUser(UserCreateDto userCreateDto)
        {
            var serviceResponse = new ServiceResponse<UserGetDto>();
            try
            {
                // Ensure username is available
                var userNameCheck = await _db.UsersTable.FirstOrDefaultAsync(u=>u.UserName.ToLower() == userCreateDto.UserName.ToLower());
                if(userNameCheck != null)
                {
                    throw new Exception("Username is already used");
                }

                // Create new user model
                var newUser = new User
                {
                    UserId = 0,
                    UserName = userCreateDto.UserName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userCreateDto.Password)
                };

                await _db.UsersTable.AddAsync(newUser);
                await _db.SaveChangesAsync();

                serviceResponse.Data =  _mapper.Map<UserGetDto>(newUser);
            }
            catch(Exception ex){
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<UserGetDto>> Authenticate(UserLoginDto userLoginDto)
        {
            var serviceResponse =  new ServiceResponse<UserGetDto>();
            try
            {
                var user = await _db.UsersTable.FirstOrDefaultAsync(u=>u.UserName.ToLower() == userLoginDto.UserName.ToLower());
                if (user == null)
                {
                    throw new Exception($"Username {userLoginDto.UserName} not found");
                }

                if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
                {
                    throw new Exception("Invalid Password");
                }
                
                serviceResponse.Data = _mapper.Map<UserGetDto>(user);
                
            }
            catch(Exception ex){
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task SetAccessTokenCache(string token, UserGetDto userGetDto, DateTime lifeTime)
        {            
            // Serialize the payload dictionary to JSON format
            string jsonPayload = JsonConvert.SerializeObject(userGetDto);
            // Convert the JSON string to bytes
            byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);
            // Define cache life time same with token life time
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(lifeTime);

            await _distributedCache.SetAsync(token, payloadBytes, options);
        }

        public async Task BlacklistRefreshTokenCache(string token, DateTime lifeTime)
        {
            // Convert the string to bytes
            byte[] payloadBytes = Encoding.UTF8.GetBytes("blacklisted");
            // Define cache life time same with token life time
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(lifeTime);

            await _distributedCache.SetAsync(token, payloadBytes, options);
        }

        public async Task<ServiceResponse<UserGetDto>> CheckAccessTokenCache(string accessToken)
        {
            var serviceResponse = new ServiceResponse<UserGetDto>();
            try
            {
                if (accessToken == null)
                {
                    throw new Exception("Token not found in cookie"); 
                }
                // Get byte format from cache
                byte[] cacheValue = await _distributedCache.GetAsync(accessToken);
                if (cacheValue == null)
                {
                    throw new Exception("Token not found in cache"); 
                }
                // Convert bytes to JSON string
                string utf8String = Encoding.UTF8.GetString(cacheValue);
                if (utf8String == "blacklisted")
                {
                    throw new Exception("Token is not valid"); 
                }
                // Deserialize JSON string to object
                serviceResponse.Data = JsonConvert.DeserializeObject<UserGetDto>(utf8String);
                
            }
            catch(Exception ex){
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<UserGetDto> GetUserById(int Id)
        {
            var user = await _db.UsersTable.FirstOrDefaultAsync(u=>u.UserId == Id);
            return _mapper.Map<UserGetDto>(user);
        }

        public async Task<bool> AnyCache(string token)
        {
            return await _distributedCache.GetAsync(token) != null;
        }
        
        public async Task DeleteCache(string token)
        {
            await _distributedCache.RemoveAsync(token);
        }

        public async Task<ServiceResponse<UserGetDto>> ValidateBlacklistRefreshToken(string refreshToken)
        {
            var serviceResponse = new ServiceResponse<UserGetDto>();
            try
            {
                // Get secret from app settings and initial values
                string JwtSecret = _configuration.GetSection("JwtSecretKey").Value;

                // Ensure token is not blacklisted in cache
                bool blackListed = await this.AnyCache(refreshToken);
                if (blackListed)
                {
                    throw new Exception("Blacklisted token");
                }

                // Validate refresh token by decoding
                (int userId, DateTime expiration, string tokenType) = JwtHelper.DecodeJwtToken(refreshToken, JwtSecret);

                // Ensure token is refresh token
                if (tokenType != "refresh")
                {
                    throw new Exception("Token is not refresh token");
                }

                // Ensure user still exists
                UserGetDto currentUser = await this.GetUserById(userId);
                if (currentUser == null)
                {
                    throw new Exception("User not exists!");
                }

                // After all validations are done blacklist old refresh token
                await this.BlacklistRefreshTokenCache(refreshToken, expiration);

                // Return current user
                serviceResponse.Data = currentUser;
            }
            catch(Exception ex){
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<(string newAccessToken, string newRefreshToken)> CreateNewTokens(UserGetDto currentUser)
        {
            // Get secret, tokenlifetime from app settings and initial values
            string JwtSecret = _configuration.GetSection("JwtSecretKey").Value;
            DateTime AccesTokenLifeTime = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("AccessTokenLifetime"));
            DateTime RefreshTokenLifeTime = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("RefreshTokenLifeTime"));

            // Create new tokens
            string newAccessToken = JwtHelper.CreateJwtToken(currentUser, "access", JwtSecret, AccesTokenLifeTime);
            string newRefreshToken = JwtHelper.CreateJwtToken(currentUser, "refresh", JwtSecret, RefreshTokenLifeTime);

            // Cache new access token for authentication
            await SetAccessTokenCache(newAccessToken, currentUser, AccesTokenLifeTime);

            return (newAccessToken, newRefreshToken);
        }
    }
}