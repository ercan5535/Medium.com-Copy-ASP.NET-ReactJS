using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogService.Dtos
{
    public class UpdateCommentDto
    {
    public string CommentId { get; set; }
    public string Comment { get; set; }
    }
}