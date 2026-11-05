using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class SharepointHierarchyFolderCreationDto
    {
        public string ProjectPin { get; set; }
        public long ImpactMasterId { get; set; }
        public string Title { get; set; }
        public string Quarter { get; set; }
        public Dictionary<string, SharepointHierarchyFolderCreationL1TopTopic> L1MetricCodes { get; set; } = new Dictionary<string, SharepointHierarchyFolderCreationL1TopTopic>();

        public List<SharepointHierarchyFolderCreationTopicDto> Topics = new List<SharepointHierarchyFolderCreationTopicDto>();
    }
}
