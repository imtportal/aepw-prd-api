using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class BaselineRemainderDto
    {
        public string Baseline { get; set; }
        public long ReviewId { get; set; }
        public long MetricId { get; set; }
        public string MetricCode { get; set; }
        public long ImpactMasterId { get; set; }
        public byte? Quarter { get; set; }
        public string ReviewIdMetricIdQuarter { get; set; }
        public decimal? BaselineSum { get; set; }
        public decimal? ImmBaseline { get; set; }
    }
}
