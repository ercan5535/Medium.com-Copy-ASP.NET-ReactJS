using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogService.Dtos
{
    public class GetBlogMetaDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("blogTitle")]
        public string BlogTitle { get; set; }

        [BsonElement("blogAuthor")]
        public string BlogAuthor { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("likesCount")]
        public int LikesCount { get; set; }
    
        [BsonElement("commentsCount")]
        public int CommentsCount { get; set; }
    }
}