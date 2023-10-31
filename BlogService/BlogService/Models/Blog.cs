using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace BlogService.Models
{
    public class Blog
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

        [BsonElement("blogContent")]
        public List<BlogContentItem> BlogContent { get; set; }
    
        [BsonElement("likes")]
        public List<string> Likes { get; set; }

        [BsonElement("comments")]
        public List<BlogCommentItem> Comments { get; set; }
    }
}