using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class SharepointHierarchyFolderCreationTopicDto
    {
        public string TopicName { get; set; }
        public List<string> TopLevelMetricCodes { get; set; } = new List<string>();
        
    }
}
