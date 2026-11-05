namespace AEPW.Application.DTOs.Report.ReviewMetricTracker
{
    public class ReviewMetricTrackerDataDto
    {
        public long ReviewId { get; set; }

        public long? ReviewMetricId { get; set; }

        public long? MetricId { get; set; }

        public string IMId { get; set; } = string.Empty;

        public string PIN { get; set; } = string.Empty;

        public string ProjectTitle { get; set; } = string.Empty;

        public string ReportingPeriod { get; set; } = string.Empty;

        public string ProjectStatus { get; set; } = string.Empty;

        public string Topic { get; set; } = string.Empty;

        public string TopLevelMetric { get; set; } = string.Empty;

        public string L1Metric { get; set; } = string.Empty;

        public string MetricCode { get; set; } = string.Empty;

        public string MetricName { get; set; } = string.Empty;

        public string RecordStatus { get; set; } = string.Empty;

        public ReviewMetricTrackerApprovalDto? Approval { get; set; }
    }
}
