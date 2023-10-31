using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Dtos
{
    public class UserGetDto
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;
    }
}