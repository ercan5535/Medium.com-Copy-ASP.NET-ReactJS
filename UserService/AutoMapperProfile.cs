using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserService.Dtos;
using UserService.Models;

namespace UserService
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Source -> Target
            CreateMap<User, UserGetDto>();
        }
    }
}