using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using UserService.Data;
using UserService.Models;

namespace UserService.Repositories
{

    public class UserRepositoryLayer  : IUserRepositoryLayer
    {
        private readonly ApplicationDbContext _db;
        private readonly IDistributedCache _distributedCache;

        public UserRepositoryLayer(ApplicationDbContext db, IDistributedCache distributedCache)
        {
            _db = db;
            _distributedCache = distributedCache;
        }
        
        public async Task AddUserAsync(User user)
        {
            await _db.UsersTable.AddAsync(user);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> AnyCache(string token)
        {
            return await _distributedCache.GetAsync(token) is not null;
        }

        public async Task DeleteCache(string token)
        {
            await _distributedCache.RemoveAsync(token);
        }

        public async Task<byte[]> GetCache(string token)
        {
            return await _distributedCache.GetAsync(token);
        }

        public async Task<User> GetUserById(int Id)
        {
            return await _db.UsersTable.FirstOrDefaultAsync(u=>u.UserId == Id);
        }

        public async Task<User> GetUserByUsername(string userName)
        {
            return await _db.UsersTable.FirstOrDefaultAsync(u=>u.UserName.ToLower() == userName.ToLower());
        }

        public async Task SetCache(string token, string payload, DateTime lifeTime)
        {
            // Convert the payload string to bytes
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            // Define cache life time same with token life time
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(lifeTime);

            await _distributedCache.SetAsync(token, payloadBytes, options);
        }
    }
}