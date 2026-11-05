namespace AEPW.Application.DTOs.Report.PerTopLevelMetric
{
    public class PerTopLevelMetricFiltersDto
    {
        public List<int> ReportingYears { get; set; } = new();

        public List<string> L1Metrics { get; set; } = new();

        public List<string> ProjectPIN { get; set; } = new();

        public List<string> ProjectName { get; set; } = new();

        public List<string> ContractPerformanceManager { get; set; } = new();

        public List<string> ProgramLeader { get; set; } = new();

        public List<string> GlobalExecutionLeader { get; set; } = new();

        public List<string> Program { get; set; } = new();

        public List<string> ProjectStatus { get; set; } = new();

        public List<string> RecordStatus { get; set; } = new();
    }
}
