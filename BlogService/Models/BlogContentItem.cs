using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogService.Models
{
    public class BlogContentItem
    {
        //text or image
        [BsonElement("type")]
        [RegularExpression("^(text|image)$", ErrorMessage = "Type must be 'text' or 'image'.")]
        public string Type { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

    }
}