using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class StartEndRestatementDto
    {
        public long ReviewId { get; set; }
        public long UserId { get; set; }
        public string Comment { get; set; }
        public List<StartEndRestatementMetricDto> Metrics { get; set; } = new List<StartEndRestatementMetricDto>();
        public bool Start { get; set; }
    }

    public class StartEndRestatementMetricDto
    {
        public long MetricId { get; set; }
        public string? MetricCode { get; set; }
    }
}
