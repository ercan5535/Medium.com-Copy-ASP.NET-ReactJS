using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models;

namespace UserService.Repositories
{
    public interface IUserRepositoryLayer 
    {
        Task SetCache(string token, string payload, DateTime lifeTime);
        Task<bool> AnyCache(string token);
        Task<byte[]> GetCache(string token);
        Task DeleteCache(string token);
        Task<User> GetUserById(int Id);
        Task<User> GetUserByUsername(string userName);
        Task AddUserAsync(User user); 
    }
}