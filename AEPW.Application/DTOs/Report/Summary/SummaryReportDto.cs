namespace AEPW.Application.DTOs.Report.Summary
{
    public class SummaryReportDto
    {
        public string ReportType { get; set; } = string.Empty;

        public string ReportName { get; set; } = string.Empty;

        public SummaryReportFiltersDto Filters { get; set; }
            = new();

        public List<SummaryDataDto> Data { get; set; }
            = new();
    }
}
