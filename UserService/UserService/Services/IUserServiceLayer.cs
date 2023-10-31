using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Data
{
    public interface IUserServiceLayer
    {
        Task<ServiceResponse<UserGetDto>> Authenticate(UserLoginDto userLoginDto);
        Task<ServiceResponse<UserGetDto>> RegisterUser(UserCreateDto userCreateDto);
        Task<ServiceResponse<UserGetDto>> ValidateBlacklistRefreshToken(string refreshToken);
        Task<ServiceResponse<UserGetDto>> CheckAccessTokenCache(string accessToken);
        Task<(string newAccessToken, string newRefreshToken)> CreateNewTokens(UserGetDto currentUser);
        Task<ServiceResponse<string>> LogoutUser(string accessToken);
    }
}