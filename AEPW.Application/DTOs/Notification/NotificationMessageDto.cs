using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Notification
{
    public sealed class NotificationMessageDto
    {
        public string NotificationType { get; set; } = default!;

        public string? ReviewIds { get; set; }
        public string? ReviewMetrics { get; set; }
        public int? StageCode { get; set; }

        public string? MetricCode { get; set; } = default!;

        public long? OldAssigneeId { get; set; }

        public long? NewAssigneeId { get; set; }
        public long ReassignedByUserId { get; set; }

        //public long DocumentId { get; set; }
    }
}
//{

//    "type": "object",

//  "properties": {

//        "ReviewIds": {
//            "type": "string"
//    },

//    "NotificationType": {

//            "type": "string"

//    }

//    }

//}
