using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.EmailNotificationTemplate
{
    public class EmailNotificationTemplateDto
    {
        public string ToEmail { get; set; }
        public string CcEmails { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
