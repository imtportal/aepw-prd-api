using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class MetricRiskResponseJSON
    {
        public int MetricId { get; set; }
        public decimal? RiskPctProposed { get; set; }
        public decimal? RiskPctEffective { get; set; }
        public string? RAReason { get; set; }
        public int? Approved { get; set; }
    }
}
