using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> UsersTable { get; set; }

        // create user1 on table initially
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    UserName = "user1",
                    PasswordHash = "$2a$11$SZmRPb6GZoKm3eQhj/lMOOOcQ9S2caIO60YfhaBSxEmsBbIQ1akQS"
                }
            );
        }
    }
}