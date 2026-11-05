using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class UserMentionedEmailNotificationInputDto
    {
        public long ReviewId { get; set; }
        public long CommentedByUserId { get; set; }
        public long TaggedUserId { get; set; }
        public string CommentBody { get; set; }
        public string Stage { get; set; }
    }
}
