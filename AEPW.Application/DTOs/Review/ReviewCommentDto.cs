using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AEPW.Application.DTOs.Review
{
    public class ReviewCommentDto
    {
        public Int64 ReviewCommentId { get; set; }
        public Int64 ReviewId { get; set; }
        public Int64 AuthorUserId { get; set; }
        public string AuthorName { get; set; }
        public string? Role { get; set; }
        public string Body { get; set; }
        public DateTime CreatedUtc { get; set; }
        public Int64? CreatedBy { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public int IsActive { get; set; }

        public Int64? UpdatedBy { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

    }
}
