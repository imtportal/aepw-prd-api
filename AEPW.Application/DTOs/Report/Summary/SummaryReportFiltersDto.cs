namespace AEPW.Application.DTOs.Report.Summary
{
    public class SummaryReportFiltersDto
    {
        public List<string> ProjectPIN { get; set; } = [];

        public List<string> ProjectName { get; set; } = [];

      public List<string> Phase { get; set; } = [];
      public List<string> L1MetricIds { get; set; } = [];

    public List<string> ContractPerformanceManager { get; set; } = [];

        public List<string> ProgramLeader { get; set; } = [];

        public List<string> GlobalExecutionLeader { get; set; } = [];

        public List<string> FundingRecipient { get; set; } = [];

        public List<string> Program { get; set; } = [];

        public List<string> CountryCity { get; set; } = [];

        public List<string> ProjectStatus { get; set; } = [];
    public List<string> ProjectCategoryIds { get; set; } = [];

    public List<string> SourceType { get; set; } = [];

    public List<string>? RecordStatusIds { get; set; } = [];

    public string GeneratedByUser { get; set; } = string.Empty;
    public DateTime GenerationDate { get; set; } 
  }
}
