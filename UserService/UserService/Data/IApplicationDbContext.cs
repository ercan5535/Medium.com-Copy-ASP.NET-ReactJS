using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data
{
    public interface IApplicationDbContext
    {
        DbSet<User> UsersTable { get; set; }
        int SaveChanges();
    }
}