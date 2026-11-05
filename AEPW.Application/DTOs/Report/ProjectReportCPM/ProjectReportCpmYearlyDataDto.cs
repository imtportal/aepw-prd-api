namespace AEPW.Application.DTOs.Report.ProjectReportCpm
{
    public class ProjectReportCpmYearlyDataDto
    {
        public int Year { get; set; }

        public ProjectReportCpmQuarterDataDto ReportedData { get; set; }
            = new();

        public ProjectReportCpmQuarterDataDto RiskAdjustedData { get; set; }
            = new();
    }
}
