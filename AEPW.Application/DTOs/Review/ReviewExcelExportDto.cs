using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class ReviewExcelExportDto
    {
        public long ReviewId { get; set; }

        public string Pin { get; set; }

        public string ProjectTitle { get; set; }

        public string? PeriodCode { get; set; }

        public int Year { get; set; }

        public int? Quarter { get; set; }

        public string FundingRecipientName { get; set; }

        public string Email { get; set; }

        public DateTime StartUtc { get; set; } 
        public DateTime EndUtc { get; set; }
        public string? CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public long MetricId { get; set; }

        public bool IsTopLevel { get; set; }
        public string ParentMetricCode { get; set; }

        public string ParentMetricName { get; set; }

        public long? ParentMetricId { get; set; }
        public string SubMetricCode { get; set; }

        public string SubMetricName { get; set; }

        public string MetricDescription { get; set; }

        public string UnitName { get; set; }

        public long? ReviewMetricId { get; set; }

        public long? EvidenceTypeId { get; set; }

        public string EvidenceTypeName { get; set; }

        public bool IsRequired { get; set; }

        public string? CategoryDescription { get; set; }

        public string? MetricHeader { get; set; }

        public string? ValidationChecks { get; set; }
        public int CategoryDisplayOrder { get; set; }
        public string? TopLevelTableHeader { get; set; }


    }
}
