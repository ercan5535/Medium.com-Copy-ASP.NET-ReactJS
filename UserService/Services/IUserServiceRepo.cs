using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Data
{
    public interface IUserServiceRepo
    {
        Task<ServiceResponse<UserGetDto>> Authenticate(UserLoginDto userLoginDto);
        Task<ServiceResponse<UserGetDto>> RegisterUser(UserCreateDto userCreateDto);
        Task<ServiceResponse<UserGetDto>> ValidateBlacklistRefreshToken(string refreshToken);
        Task<ServiceResponse<UserGetDto>> CheckAccessTokenCache(string accessToken);
        Task SetAccessTokenCache(string token, UserGetDto userGetDto, DateTime lifeTime);
        Task BlacklistRefreshTokenCache(string token, DateTime lifeTime);
        Task<(string newAccessToken, string newRefreshToken)> CreateNewTokens(UserGetDto currentUser);
        Task<UserGetDto> GetUserById(int Id);
        Task<bool> AnyCache(string token);
        Task DeleteCache(string token);
    }
}