using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Dtos;

namespace UserService.Helpers
{
    public interface IJwtHelper
    {
        string CreateJwtToken(UserGetDto userGetDto, string tokenType, string secretKey, DateTime expiration);
        (UserGetDto UserGetDto, DateTime Expiration, string TokenType) DecodeJwtToken(string jwtToken, string secretKey);
    }
}