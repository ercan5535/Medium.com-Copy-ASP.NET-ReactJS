using System.Text;
using AutoMapper;
using Newtonsoft.Json;
using UserService.Dtos;
using UserService.Helpers;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Data
{
    public class UserServiceLayer : IUserServiceLayer
    {
        private readonly IUserRepositoryLayer  _repository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IJwtHelper _jwtHelper;

        public UserServiceLayer(IUserRepositoryLayer  repository, IMapper mapper, IConfiguration configuration, IJwtHelper jwtHelper)
        {
            _repository = repository;
            _mapper = mapper;
            _configuration = configuration;
            _jwtHelper = jwtHelper;
        }

        public async Task<ServiceResponse<UserGetDto>> RegisterUser(UserCreateDto userCreateDto)
        {
            var serviceResponse = new ServiceResponse<UserGetDto>();
            try
            {
                // Ensure username is available
                var userNameCheck = await _repository.GetUserByUsername(userCreateDto.UserName);
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

                await _repository.AddUserAsync(newUser);

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
                var user = await _repository.GetUserByUsername(userLoginDto.UserName);
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

        public async Task<ServiceResponse<UserGetDto>> CheckAccessTokenCache(string accessToken)
        {
            var serviceResponse = new ServiceResponse<UserGetDto>();
            try
            {
                // Get byte format from cache
                byte[] cacheValue = await _repository.GetCache(accessToken);
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

        public async Task<ServiceResponse<UserGetDto>> ValidateBlacklistRefreshToken(string refreshToken)
        {
            var serviceResponse = new ServiceResponse<UserGetDto>();
            try
            {
                // Get secret from app settings and initial values
                string JwtSecret = _configuration.GetSection("JwtSecretKey").Value;

                // Ensure token is not blacklisted in cache
                bool blackListed = await _repository.AnyCache(refreshToken);
                if (blackListed)
                {
                    throw new Exception("Blacklisted token");
                }

                // Validate refresh token by decoding
                (UserGetDto currentUser, DateTime expiration, string tokenType) = _jwtHelper.DecodeJwtToken(refreshToken, JwtSecret);

                // Ensure token is refresh token
                if (tokenType != "refresh")
                {
                    throw new Exception("Token is not refresh token");
                }

                // After all validations are done blacklist old refresh token
                string payload = JsonConvert.SerializeObject("blacklisted");
                await _repository.SetCache(refreshToken, payload, expiration);

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
            string newAccessToken = _jwtHelper.CreateJwtToken(currentUser, "access", JwtSecret, AccesTokenLifeTime);
            string newRefreshToken = _jwtHelper.CreateJwtToken(currentUser, "refresh", JwtSecret, RefreshTokenLifeTime);

            // Cache new access token for authentication
            string payload = JsonConvert.SerializeObject(currentUser);
            await _repository.SetCache(newAccessToken, payload, AccesTokenLifeTime);

            return (newAccessToken, newRefreshToken);
        }

        public async Task<ServiceResponse<string>> LogoutUser(string accessToken)
        {
            var serviceResponse = new ServiceResponse<string>();
            try
            {
                await _repository.DeleteCache(accessToken);
                serviceResponse.Message = "User logged out successfully.";
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

    }
}