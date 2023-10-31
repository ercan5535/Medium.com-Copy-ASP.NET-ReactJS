using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace BlogService.Models
{
    public class BlogCommentItem
    {
        [BsonElement("Id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [BsonElement("userName")]
        public string Author { get; set; }
        [BsonElement("comment")]
        public string Comment { get; set; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}