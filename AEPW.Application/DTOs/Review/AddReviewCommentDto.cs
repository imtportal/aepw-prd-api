using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class AddReviewCommentDto
    {   
        public Int64 ReviewId { get; set; }
        public string Body { get; set; }
        public Int64 UserId { get; set; }
        public int Stage { get; set; }

        public List<long>? MentionedUsers { get; set; }
    }
}
