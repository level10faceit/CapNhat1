using System;

namespace BlogMVC.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        public int PostId { get; set; }

        public string Author { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
