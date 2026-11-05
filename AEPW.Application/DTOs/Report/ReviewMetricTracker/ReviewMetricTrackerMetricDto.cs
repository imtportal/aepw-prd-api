namespace AEPW.Application.DTOs.Report.ReviewMetricTracker
{
    public class ReviewMetricTrackerMetricDto
    {
        public int ReviewMetricId { get; set; }

        public int MetricId { get; set; }

        public string MetricCode { get; set; } = string.Empty;

        public string MetricName { get; set; } = string.Empty;

        public string Topic { get; set; } = string.Empty;
    }
}
