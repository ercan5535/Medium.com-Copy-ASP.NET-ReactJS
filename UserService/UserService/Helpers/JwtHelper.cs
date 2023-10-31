using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Helpers
{
    public class JwtHelper : IJwtHelper
    {
        public string CreateJwtToken(UserGetDto userGetDto, string tokenType, string secretKey, DateTime expiration)
        {
            // Set the key used to sign the token (convert the secret key to bytes)
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var key = new SymmetricSecurityKey(keyBytes);

            // Set the signing credentials using the key
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create claims for the token
            var claims = new[]
            {
                new Claim("UserId", userGetDto.UserId.ToString()),
                new Claim("UserName", userGetDto.UserName),
                new Claim("token_type", tokenType)
            };

            // Create the token
            var token = new JwtSecurityToken(
                claims: claims,
                expires: expiration,
                signingCredentials: signingCredentials
            );

            // Generate the JWT token string
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            //Console.WriteLine("Generated JWT Token: " + tokenString);
            return (tokenString);
        }

        public (UserGetDto UserGetDto, DateTime Expiration, string TokenType) DecodeJwtToken(string jwtToken, string secretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Set the key used to validate the token (convert the secret key to bytes)
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var key = new SymmetricSecurityKey(keyBytes);

            // Set the validation parameters
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = key
            };

            // Try to validate and parse the token
            var claimsPrincipal = tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out SecurityToken validatedToken);
            var jwtSecurityToken = validatedToken as JwtSecurityToken;

            // Retrieve payload from claims
            var userId = Int16.Parse(claimsPrincipal.FindFirst("UserId")?.Value);
            var username = claimsPrincipal.FindFirst("UserName")?.Value;
            var tokenType = claimsPrincipal.FindFirst("token_type")?.Value;
            var expiration = jwtSecurityToken.ValidTo;

            UserGetDto userGetDto = new() 
            {
                UserId = userId,
                UserName = username
            };

            //Console.WriteLine("Decoded Payload:");
            //Console.WriteLine("User ID: " + userId);
            //Console.WriteLine("Username: " + username);
            //Console.WriteLine("Token Type: " + tokenType);
            //Console.WriteLine("Expiration: " + expiration);


            return (userGetDto, expiration, tokenType);
        }
    }
}