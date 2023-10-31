using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogService.Models;

namespace BlogService.Dtos
{
    public class CreateBlogDto
    {
        public string BlogTitle { get; set; }
        public string BlogAuthor { get; set; }
        public List<BlogContentItem> BlogContent { get; set; }
    }
}