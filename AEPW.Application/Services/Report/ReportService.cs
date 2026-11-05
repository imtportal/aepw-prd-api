using System.Text.Json;
using AEPW.Application.DTOs.Report;
using AEPW.Application.DTOs.Report.Summary;
using AEPW.Application.DTOs.Report.ReviewMetricTracker;
using AEPW.Application.DTOs.Report.ProjectReportCpm;
using AEPW.Application.DTOs.Report.PerTopLevelMetric;
using AEPW.Application.DTOs.Report.ProjectsOnly;
using AEPW.Application.Interfaces.Repositories;
using AEPW.Application.Interfaces.Services;
using AEPW.Application.DTOs.Report.AnnualPortfolio;
using AEPW.Application.Interfaces.Services.Report;
using AEPW.Application.Interfaces.Repositories.Report;
using System.Data;
using AEPW.Application.Contracts.Responses;
using AEPW.Application.Report.PerTopLevelMetric;
using System.Text.RegularExpressions;

namespace AEPW.Application.Services.Report
{
  public class ReportService : IReportService
  {
    private readonly IReportRepository _reportRepository;

    private readonly SummaryExcelGeneratorService
        _summaryExcelGeneratorService;

    private readonly ReviewMetricTrackerExcelGeneratorService
        _reviewMetricTrackerExcelGeneratorService;

    private readonly ProjectReportCpmExcelGeneratorService
        _projectReportCpmExcelGeneratorService;


    private readonly PerTopLevelMetricExcelGeneratorService
        _perTopLevelMetricExcelGeneratorService;

    private readonly ProjectsOnlyExcelGeneratorService
         _projectsOnlyExcelGeneratorService;

    private readonly AnnualPortfolioExcelGeneratorService
_annualPortfolioExcelGeneratorService;

    public ReportService(
IReportRepository reportRepository,
SummaryExcelGeneratorService summaryExcelGeneratorService,
ReviewMetricTrackerExcelGeneratorService reviewMetricTrackerExcelGeneratorService,
ProjectReportCpmExcelGeneratorService projectReportCpmExcelGeneratorService,
PerTopLevelMetricExcelGeneratorService perTopLevelMetricExcelGeneratorService, ProjectsOnlyExcelGeneratorService projectsOnlyExcelGeneratorService,
AnnualPortfolioExcelGeneratorService annualPortfolioExcelGeneratorService)
    {
      _reportRepository = reportRepository;

      _summaryExcelGeneratorService =
          summaryExcelGeneratorService;

      _reviewMetricTrackerExcelGeneratorService =
          reviewMetricTrackerExcelGeneratorService;

      _projectReportCpmExcelGeneratorService =
          projectReportCpmExcelGeneratorService;

      _perTopLevelMetricExcelGeneratorService =
          perTopLevelMetricExcelGeneratorService;
      _projectsOnlyExcelGeneratorService =
          projectsOnlyExcelGeneratorService;

      _annualPortfolioExcelGeneratorService =
          annualPortfolioExcelGeneratorService;

    }


    // SUMMARY

    public async Task<SummaryReportDto> GetSummaryReportAsync(
        SummaryReportRequestDto request, CancellationToken cancellationToken)
    {
      var dataSet =
          await _reportRepository
              .GetSummaryReportAsync(request , cancellationToken);

      return MapSummaryReport(
          dataSet,
          request);
    }

    //public async Task<byte[]> GenerateSummaryExcelAsync(
    //    SummaryReportRequestDto request)
    //{
    //    var report =
    //        await _reportRepository
    //            .GetSummaryReportAsync(request);

    //    using var stream =
    //        _summaryExcelGeneratorService
    //            .GenerateSummaryExcel(report);

    //    return stream.ToArray();
    //}

    public async Task<byte[]> GenerateSummaryExcelAsync(
    SummaryReportRequestDto request  , CancellationToken cancellationToken)
    {
      var dataSet =
          await _reportRepository
              .GetSummaryReportAsync(request, cancellationToken);

      var report =
          MapSummaryReport(
              dataSet,
              request);

      using var stream =
          _summaryExcelGeneratorService
              .GenerateSummaryExcel(report);

      return stream.ToArray();
    }

    private SummaryReportDto MapSummaryReport(
    DataSet dataSet,
    SummaryReportRequestDto request)
    {
      var ds = dataSet;
      var report = new SummaryReportDto
      {
        ReportType = "SummarySinceInception",
        ReportName = "Summary Since Inception",

        Filters = new SummaryReportFiltersDto
        {
          ProjectPIN = [],
          ProjectName = [],
          Phase = [],

          ContractPerformanceManager = [],
          ProgramLeader = [],
          GlobalExecutionLeader = [],

          FundingRecipient = [],
          Program = [],
          CountryCity = [],
          ProjectStatus = [],
          SourceType = [],
          GeneratedByUser = string.Empty,
          GenerationDate = DateTime.MinValue
        },
        Data = []
      };

      // ============================================================
      // TABLE 0 - FILTERED PROJECTS
      // ============================================================

      if (dataSet.Tables.Count > 0)
      {
        var projectTable = dataSet.Tables[0];

        report.Filters.ProjectPIN = projectTable.AsEnumerable()
            .Where(row => !row.IsNull("ProjectPIN"))
            .Select(row => row.Field<string>("ProjectPIN"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        report.Filters.ProjectName = projectTable.AsEnumerable()
            .Where(row => !row.IsNull("ProjectName"))
            .Select(row => row.Field<string>("ProjectName"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        if (projectTable.Rows.Count > 0 &&
       projectTable.Columns.Contains("GeneratedByUser"))
        {
          report.Filters.GeneratedByUser =
              projectTable.Rows[0].Field<string>("GeneratedByUser")
              ?? string.Empty;
        }

        if (projectTable.Rows.Count > 0 &&
      projectTable.Columns.Contains("GenerationDate"))
        {
          report.Filters.GenerationDate =
              projectTable.Rows[0].Field<DateTime>("GenerationDate");
        }

      }

      // TABLE 1 - METRIC DATA


      var metricTable = dataSet.Tables[1];

      report.Data =
          MapSummaryData(metricTable);


      return report;
    }

    private List<int> ParseProjectPins(
    List<string>? projectPins)
    {
      if (projectPins == null)
        return [];

      return projectPins
          .Where(x => int.TryParse(x, out _))
          .Select(int.Parse)
          .ToList();
    }

    private List<SummaryDataDto> MapSummaryData(
    DataTable table)
    {
      var result = new List<SummaryDataDto>();

      var rows = table.AsEnumerable();


      /* ============================================================
         GROUP BY CATEGORY + TOP LEVEL METRIC
         ============================================================ */

      var groups = rows
          .GroupBy(row => new
          {
            CategoryId =
                  row.Field<long>("CategoryId"),

            CategoryCode =
                  row.Field<string>("CategoryCode") ?? string.Empty,

            CategoryName =
                  row.Field<string>("CategoryName") ?? string.Empty,

            TopLevelMetricId =
                  row.Field<long>("TopLevelMetricId"),

            TopLevelMetricCode =
                  row.Field<string>("TopLevelMetricCode") ?? string.Empty,

            TopLevelMetricName =
                  row.Field<string>("TopLevelMetricName") ?? string.Empty,

            TopLevelMetricUnit =
                  row.Field<string>("TopLevelMetricUnit") ?? string.Empty,

            AggregationType =
                  row.Field<string?>("AggregationType")
          });


      foreach (var group in groups)
      {
        var firstRow = group.First();

        var dto = new SummaryDataDto
        {
          CategoryCode =
                group.Key.CategoryCode,

          CategoryName =
                group.Key.CategoryName,

          TopLevelMetric = new SummaryMetricDto
          {
            MetricCode =
                    group.Key.TopLevelMetricCode,

            MetricName =
                    group.Key.TopLevelMetricName,

            Unit =
                    group.Key.TopLevelMetricUnit
          },

          AggregationType =
                group.Key.AggregationType
        };


        /* ========================================================
           MC
           ======================================================== */

        if (group.Key.CategoryCode == "MC")
        {
          MapMcMetric(
              dto,
              group);
        }


        /* ========================================================
           NORMAL METRIC
           ======================================================== */

        else
        {
          MapNormalMetric(
              dto,
              group);
        }


        result.Add(dto);
      }


      return result;
    }

    private void MapNormalMetric(
    SummaryDataDto dto,
    IEnumerable<DataRow> rows)
    {
      var yearlyValues = rows
          .Where(row =>
              !row.IsNull("Year"))
          .GroupBy(row =>
              row.Field<int>("Year"))
          .Select(group => new YearlyValueDto
          {
            Year = group.Key,

            Value = group.Sum(row =>
              row.Field<decimal?>(
                  "YearlyRiskAdjustedValue"
              ) ?? 0)
          })
          .OrderBy(x => x.Year)
          .ToList();


      dto.YearlyRiskAdjustedValues =
          yearlyValues;


      var total =
          rows
              .Select(row =>
                  row.Field<decimal?>(
                      "TotalSinceInception"))
              .FirstOrDefault(x => x.HasValue);


      dto.TotalSinceInception = total;
    }

    private void MapMcMetric(
    SummaryDataDto dto,
    IEnumerable<DataRow> rows)
    {
      /* ============================================================
         L1 METRICS
         ============================================================ */

      var l1Groups = rows
          .Where(row =>
              !row.IsNull("L1MetricId"))
          .GroupBy(row => new
          {
            MetricId =
                  row.Field<long>("L1MetricId"),

            MetricCode =
                  row.Field<string>("L1MetricCode")
                  ?? string.Empty,

            MetricName =
                  row.Field<string>("L1MetricName")
                  ?? string.Empty,

            Unit =
                  row.Field<string>("L1MetricUnit")
                  ?? string.Empty
          });


      foreach (var l1Group in l1Groups)
      {
        var l1 = new SummaryL1MetricDto
        {
          MetricCode =
                l1Group.Key.MetricCode,

          MetricName =
                l1Group.Key.MetricName,

          Unit =
                l1Group.Key.Unit,

          TotalSinceInception =
                l1Group
                    .Select(row =>
                        row.Field<decimal?>(
                            "TotalSinceInception"))
                    .FirstOrDefault(x => x.HasValue),

          YearlyRiskAdjustedValues =
                l1Group
                    .Where(row =>
                        !row.IsNull("Year"))
                    .GroupBy(row =>
                        row.Field<int>("Year"))
                    .Select(yearGroup =>
                        new YearlyValueDto
                        {
                          Year =
                                yearGroup.Key,

                          Value =
                                yearGroup.Sum(row =>
                                    row.Field<decimal?>(
                                        "YearlyRiskAdjustedValue"
                                    ) ?? 0)
                        })
                    .OrderBy(x => x.Year)
                    .ToList()
        };


        dto.L1Metrics.Add(l1);
      }


      /* ============================================================
         SUMMARY
         ============================================================ */

      dto.Summary =
          BuildMcSummary(dto.L1Metrics);
    }

    private SummaryMetricSummaryDto BuildMcSummary(
    List<SummaryL1MetricDto> l1Metrics)
    {
      var summary = new SummaryMetricSummaryDto();


      /* ============================================================
         TOTAL
         ============================================================ */

      if (l1Metrics.Any(x =>
          x.TotalSinceInception.HasValue))
      {
        summary.TotalSinceInception =
            l1Metrics
                .Where(x =>
                    x.TotalSinceInception.HasValue)
                .Sum(x =>
                    x.TotalSinceInception!.Value);
      }


      /* ============================================================
         YEARLY
         ============================================================ */

      summary.YearlyRiskAdjustedValues =
          l1Metrics
              .SelectMany(x =>
                  x.YearlyRiskAdjustedValues)
              .GroupBy(x => x.Year)
              .Select(group =>
                  new YearlyValueDto
                  {
                    Year = group.Key,

                    Value = group.Sum(x =>
                      x.Value)
                  })
              .OrderBy(x => x.Year)
              .ToList();


      return summary;
    }

    // REVIEW METRIC TRACKER


    //  public async Task<byte[]> GenerateReviewMetricTrackerExcelAsync(
    //          ReviewMetricTrackerFiltersDto request , CancellationToken cancellationToken)
    //  {
    //    var report =
    //await _reportRepository
    //    .GetReviewMetricTrackerReportAsync(request , cancellationToken);

    //    //var json = JsonSerializer.Serialize(
    //    //    report,
    //    //    new JsonSerializerOptions
    //    //    {
    //    //      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    //    //    });

    //    using var stream =
    //        _reviewMetricTrackerExcelGeneratorService
    //            .GenerateReviewMetricTrackerExcel(report);

    //    return stream.ToArray();
    //  }

    public async Task<byte[]> GenerateReviewMetricTrackerExcelAsync(
    ReviewMetricTrackerFiltersDto request,
    CancellationToken cancellationToken)
    {
      var dataSet =
          await _reportRepository.GetReviewMetricTrackerReportAsync(
              request,
              cancellationToken);

      var report = MapReviewMetricTrackerReport(dataSet, request);

      using var stream =
          _reviewMetricTrackerExcelGeneratorService
              .GenerateReviewMetricTrackerExcel(report);

      return stream.ToArray();
    }

    private ReviewMetricTrackerReportDto MapReviewMetricTrackerReport(
    DataSet dataSet,
    ReviewMetricTrackerFiltersDto request)
    {

      var report = new ReviewMetricTrackerReportDto
      {
        ReportType = "ReviewMetricTracker",
        ReportName = "Review / Metric Tracker",
        Filters = new ReviewMetricTrackerFiltersDto
        {
          ContractPerformanceManager = [],
          ProgramLeader = [],
          GlobalExecutionLeader = [],
          Program = [],
          StartDate = null,
          EndDate = null,
          ReportingYears = [],
          Quarters = [],
          GeneratedByUser = string.Empty,
          GenerationDate = DateTime.MinValue,
          

        },
        Data = []
      };

      if (dataSet == null || dataSet.Tables.Count == 0)
        return report;

      var data = new List<ReviewMetricTrackerDataDto>();

      // Result Set 1
      var dataTable = dataSet.Tables[0];

      // ================================
      // FILTER MAPPING
      // ================================

      report.Filters.ContractPerformanceManager =
          dataTable.AsEnumerable()
              .Select(row => Convert.ToString(
                  row["ContractPerformanceManager"]))
              .Where(x => !string.IsNullOrWhiteSpace(x))
              .Distinct()
              .ToList();

      report.Filters.ProgramLeader =
          dataTable.AsEnumerable()
              .Select(row => Convert.ToString(
                  row["ProgramLeader"]))
              .Where(x => !string.IsNullOrWhiteSpace(x))
              .Distinct()
              .ToList();

      report.Filters.GlobalExecutionLeader =
          dataTable.AsEnumerable()
              .Select(row => Convert.ToString(
                  row["GlobalExecutionLeader"]))
              .Where(x => !string.IsNullOrWhiteSpace(x))
              .Distinct()
              .ToList();

      report.Filters.Program =
          dataTable.AsEnumerable()
              .Select(row => Convert.ToString(
                  row["Program"]))
              .Where(x => !string.IsNullOrWhiteSpace(x))
              .Distinct()
              .ToList();


      report.Filters.StartDate = dataTable.AsEnumerable()
  .Select(row => row["StartDate"])
  .Where(x => x != DBNull.Value)
  .Select(Convert.ToDateTime)
  .Select(x => x == DateTime.MinValue ? (DateTime?)null : x)
  .FirstOrDefault();

      report.Filters.EndDate = dataTable.AsEnumerable()
          .Select(row => row["EndDate"])
          .Where(x => x != DBNull.Value)
          .Select(Convert.ToDateTime)
          .Select(x => x == DateTime.MinValue ? (DateTime?)null : x)
          .FirstOrDefault();

    // Result Set 3 - Report Generation Metadata
    if (dataSet.Tables.Count > 2)
          {
            var metadataTable = dataSet.Tables[2];

            if (metadataTable.Rows.Count > 0)
            {
              var row = metadataTable.Rows[0];

              report.Filters.ReportingYears =
                  row["ReportingYears"] == DBNull.Value
                      ? []
                      : Convert.ToString(row["ReportingYears"])!
                          .Split(',', StringSplitOptions.RemoveEmptyEntries |
                                      StringSplitOptions.TrimEntries)
                          .ToList();

              report.Filters.Quarters =
                  row["Quarters"] == DBNull.Value
                      ? []
                      : Convert.ToString(row["Quarters"])!
                          .Split(',', StringSplitOptions.RemoveEmptyEntries |
                                      StringSplitOptions.TrimEntries)
                          .ToList();

              report.Filters.GeneratedByUser =
                  row["GeneratedByUser"] == DBNull.Value
                      ? string.Empty
                      : Convert.ToString(row["GeneratedByUser"])!;

              report.Filters.GenerationDate =
                  row["GenerationDate"] == DBNull.Value
                      ? DateTime.MinValue
                      : Convert.ToDateTime(row["GenerationDate"]);
            }
          }




      foreach (DataRow row in dataTable.Rows)
      {
        data.Add(new ReviewMetricTrackerDataDto
        {
          ReviewId =
            Convert.ToInt64(row["ReviewId"]),

          ReviewMetricId =
            row["ReviewMetricId"] == DBNull.Value
                ? null
                : Convert.ToInt64(row["ReviewMetricId"]),

          MetricId =
            row["MetricId"] == DBNull.Value
                ? null
                : Convert.ToInt64(row["MetricId"]),

          IMId =
            row["ImpactMasterId"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(row["ImpactMasterId"])!,

          PIN =
            row["Pin"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(row["Pin"])!,

          ProjectTitle =
            row["ProjectTitle"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(row["ProjectTitle"])!,

          ReportingPeriod =
            row["ReportingPeriod"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(row["ReportingPeriod"])!,

          ProjectStatus =
            row["ProjectStatus"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(row["ProjectStatus"])!,

          Topic =
            row["Topic"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(row["Topic"])!,

          TopLevelMetric =
            row["TopLevelMetric"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(row["TopLevelMetric"])!,

          L1Metric =
            row["L1Metric"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(row["L1Metric"])!,

          MetricCode =
            row["MetricCode"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(row["MetricCode"])!,

          MetricName =
            row["MetricName"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(row["MetricName"])!,

          RecordStatus =
            row["RecordStatus"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(row["RecordStatus"])!
        });
      }


      // ADD THIS
      var dataLookup = data
          .Where(x => x.MetricId.HasValue)
          .ToDictionary(
              x => (x.ReviewId, x.MetricId!.Value));

      // Result Set 2
      if (dataSet.Tables.Count > 1)
      {
        var approvalTable = dataSet.Tables[1];

        foreach (DataRow row in approvalTable.Rows)
        {
          var reviewId = row.Field<long>("ReviewId");
          var metricId = row.Field<long>("MetricId");

          //var reportRow = data.FirstOrDefault(x =>
          //    x.ReviewId == reviewId &&
          //    x.MetricId == metricId);

          if (!dataLookup.TryGetValue(
          (reviewId, metricId),
          out var reportRow))
          {
            continue;
          }

          reportRow.Approval = new ReviewMetricTrackerApprovalDto
          {
            ReviewApprovalId =
        row["ReviewApprovalId"] == DBNull.Value
            ? null
            : Convert.ToInt64(row["ReviewApprovalId"]),

            ReviewStageCode =
        row["ReviewStageCode"] == DBNull.Value
            ? null
            : Convert.ToInt32(row["ReviewStageCode"]),

            ReviewStageName =
        row["ReviewStageName"] == DBNull.Value
            ? null
            : Convert.ToString(row["ReviewStageName"]),

            StepNo =
        row["StepNo"] == DBNull.Value
            ? null
            : Convert.ToInt32(row["StepNo"]),

            AttemptNo =
        row["AttemptNo"] == DBNull.Value
            ? null
            : Convert.ToInt32(row["AttemptNo"]),

            Status =
        row["Status"] == DBNull.Value
            ? null
            : Convert.ToString(row["Status"]),

            AssignedUserId =
        row["AssigneeUserId"] == DBNull.Value
            ? null
            : Convert.ToInt64(row["AssigneeUserId"]),

            AssignedUtc =
        row["AssignedUtc"] == DBNull.Value
            ? null
            : Convert.ToDateTime(row["AssignedUtc"]),

            DecisionByUserId =
        row["DecisionByUserId"] == DBNull.Value
            ? null
            : Convert.ToInt64(row["DecisionByUserId"]),

            DecisionUtc =
        row["DecisionUtc"] == DBNull.Value
            ? null
            : Convert.ToDateTime(row["DecisionUtc"]),

            Comment =
        row["Comment"] == DBNull.Value
            ? null
            : Convert.ToString(row["Comment"]),

            IsCurrent =
        row["IsCurrent"] != DBNull.Value &&
        Convert.ToBoolean(row["IsCurrent"])
          };
        }
      }

      report.Data = data;

      return report;
    }


    //////////////////////////////////////////////////////////// PROJECT REPORT CPM//////////////////////////////////////////////////////////////

    
   public async Task<byte[]> GenerateProjectReportCpmExcelAsync(
    ProjectReportCpmFiltersDto request, CancellationToken cancellationToken)
    {
      var filterRequest = request;

      // ---------------------------------------------------------
      // Get 3 result sets from SP
      // ---------------------------------------------------------

      var dataSet =
          await _reportRepository
              .GetProjectReportCpmReportAsync(filterRequest, cancellationToken);

      // ---------------------------------------------------------
      // Map DataSet -> Report DTO
      // ---------------------------------------------------------

      var report =
          MapProjectReportCpm(
              dataSet,
              filterRequest);

      // ---------------------------------------------------------
      // Generate Excel
      // ---------------------------------------------------------

      return _projectReportCpmExcelGeneratorService
          .GenerateProjectReportCpmExcel(report);
    }


    private ProjectReportCpmReportDto MapProjectReportCpm(
        DataSet dataSet,
        ProjectReportCpmFiltersDto request)
    {
      if (dataSet == null || dataSet.Tables.Count < 3)
      {
        return new ProjectReportCpmReportDto
        {
          ReportType = "ProjectReportCpm",
          ReportName = "Project Report CPM",
          Filters = new ProjectReportCpmFiltersDto
          {
            Pin = [],
            ProjectTitle = [],
            CPM = [],
            Program = [],
            Topics = []
          },
          Projects = [],
          GenerationData = null
        };
      }

      // Map Result Set 1
      var projectRows = MapProjectRowsCPM(dataSet.Tables[0]);

      // Map Result Set 2
      var topicRows = MapTopicRows(dataSet.Tables[1]);

      // Map Result Set 3
      var metricRows = MapMetricRows(dataSet.Tables[2]);

      // Build project hierarchy
      var projects = BuildProjectHierarchy(
          projectRows,
          topicRows,
          metricRows);

      // Map Result Set 4: Report Generation Metadata
      ReportGenerationDataDto? generationData = null;

      if (dataSet.Tables.Count > 3 &&
          dataSet.Tables[3].Rows.Count > 0)
      {
        var row = dataSet.Tables[3].Rows[0];

        generationData = new ReportGenerationDataDto
        {
          GeneratedByUser = row["GeneratedByUser"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(row["GeneratedByUser"]) ?? string.Empty,

          GenerationDate = row["GenerationDate"] == DBNull.Value
                ? DateTime.MinValue
                : Convert.ToDateTime(row["GenerationDate"])
        };
      }

      // Map display names for selected filters
      var filterNames = new ProjectReportCpmFiltersDto
      {
        Pin = projectRows
              .Where(x => !string.IsNullOrWhiteSpace(x.PIN))
              .Select(x => x.PIN)
              .Distinct()
              .OrderBy(x => x)
              .ToList(),

        ProjectTitle = projectRows
              .Where(x => !string.IsNullOrWhiteSpace(x.ProjectTitle))
              .Select(x => x.ProjectTitle)
              .Distinct()
              .OrderBy(x => x)
              .ToList(),

        CPM = projectRows
              .Where(x => !string.IsNullOrWhiteSpace(x.ContractPerformanceManager))
              .Select(x => x.ContractPerformanceManager)
              .Distinct()
              .OrderBy(x => x)
              .ToList(),

        Program = projectRows
              .Where(x => !string.IsNullOrWhiteSpace(x.Program))
              .Select(x => x.Program)
              .Distinct()
              .OrderBy(x => x)
              .ToList(),

        // Topic code and name from Result Set 2
        Topics = topicRows
              .Where(x => !string.IsNullOrWhiteSpace(x.TopicCode)
                       || !string.IsNullOrWhiteSpace(x.TopicName))
              .OrderBy(x => x.CategoryDisplayOrder)
              .Select(x => $"{x.TopicCode} - {x.TopicName}")
              .Distinct()
              .ToList()
      };

      return new ProjectReportCpmReportDto
      {
        ReportType = "ProjectReportCpm",
        ReportName = "Project Report CPM",
        Filters = filterNames,
        Projects = projects,
        GenerationData = generationData
      };
    }




    private List<ProjectRow> MapProjectRowsCPM(DataTable table)
    {
      return table.AsEnumerable()
          .Select(row => new ProjectRow
          {
            ImpactMasterId = row.Field<long>("ImpactMasterId"),

            PIN = row.Field<string>("PIN") ?? string.Empty,

            ProjectTitle =
                  row.Field<string>("ProjectTitle") ?? string.Empty,

            ContractPerformanceManager =
                  row.Field<string>("ContractPerformanceManager")
                  ?? string.Empty,

            ProgramLeader =
                  row.Field<string>("ProgramLeader")
                  ?? string.Empty,

            GlobalExecutionLeader =
                  row.Field<string>("GlobalExecutionLeader")
                  ?? string.Empty,

            FundingRecipient =
                  row.Field<string>("FundingRecipient")
                  ?? string.Empty,

            Program =
                  row.Field<string>("Program")
                  ?? string.Empty,

            CountryCity =
                  row.Field<string>("CountryCity")
                  ?? string.Empty
          })
          .ToList();
    }

    private List<TopicRow> MapTopicRows(DataTable table)
    {
      return table.AsEnumerable()
          .Select(row => new TopicRow
          {
            ImpactMasterId =
                  row.Field<long>("ImpactMasterId"),

            CategoryId =
                  row.Field<long>("CategoryId"),

            TopicCode =
                  row.Field<string>("TopicCode")
                  ?? string.Empty,

            TopicName =
                  row.Field<string>("TopicName")
                  ?? string.Empty,

            CategoryDisplayOrder =
                  row.Field<int>("CategoryDisplayOrder")
          })
          .ToList();
    }

    private List<MetricRow> MapMetricRows(DataTable table)
    {
      return table.AsEnumerable()
          .Select(row => new MetricRow
          {
            ImpactMasterId =
                  row.Field<long>("ImpactMasterId"),

            CategoryId =
                  row.Field<long>("CategoryId"),

            TopicCode =
                  row.Field<string>("TopicCode")
                  ?? string.Empty,

            TopicName =
                  row.Field<string>("TopicName")
                  ?? string.Empty,

            TopLevelMetricId =
                  row.Field<long>("TopLevelMetricId"),

            TopLevelMetricCode =
                  row.Field<string>("TopLevelMetricCode")
                  ?? string.Empty,

            TopLevelMetricName =
                  row.Field<string>("TopLevelMetricName")
                  ?? string.Empty,

            L1MetricId =
                  row.IsNull("L1MetricId")
                      ? null
                      : row.Field<long>("L1MetricId"),

            L1MetricCode =
                  row.IsNull("L1MetricCode")
                      ? null
                      : row.Field<string>("L1MetricCode"),

            L1MetricName =
                  row.IsNull("L1MetricName")
                      ? null
                      : row.Field<string>("L1MetricName"),

            MetricId =
                  row.Field<long>("MetricId"),

            MetricLevel =
                  row.Field<string>("MetricLevel")
                  ?? string.Empty,

            Year =
                  row.Field<int>("Year"),

            Q1ReportedData =
                  GetDecimal(row, "Q1ReportedData"),

            Q2ReportedData =
                  GetDecimal(row, "Q2ReportedData"),

            Q3ReportedData =
                  GetDecimal(row, "Q3ReportedData"),

            Q4ReportedData =
                  GetDecimal(row, "Q4ReportedData"),

            TotalReportedData =
                  GetDecimal(row, "TotalReportedData"),

            Q1RiskAdjustedData =
                  GetDecimal(row, "Q1RiskAdjustedData"),

            Q2RiskAdjustedData =
                  GetDecimal(row, "Q2RiskAdjustedData"),

            Q3RiskAdjustedData =
                  GetDecimal(row, "Q3RiskAdjustedData"),

            Q4RiskAdjustedData =
                  GetDecimal(row, "Q4RiskAdjustedData"),

            TotalRiskAdjustedData =
                  GetDecimal(row, "TotalRiskAdjustedData")
          })
          .ToList();
    }

    private List<ProjectReportCpmProjectDto> BuildProjectHierarchy(
     List<ProjectRow> projectRows,
     List<TopicRow> topicRows,
     List<MetricRow> metricRows)
    {
      var projects = new List<ProjectReportCpmProjectDto>();

      foreach (var project in projectRows
                   .GroupBy(x => x.ImpactMasterId)
                   .Select(x => x.First()))
      {
        var projectDto = new ProjectReportCpmProjectDto
        {
          ImpactMasterId = project.ImpactMasterId,
          PIN = project.PIN,
          ProjectTitle = project.ProjectTitle,
          ContractPerformanceManager =
                project.ContractPerformanceManager,
          ProgramLeader = project.ProgramLeader,
          GlobalExecutionLeader =
                project.GlobalExecutionLeader,
          FundingRecipient = project.FundingRecipient,
          Program = project.Program,
          CountryCity = project.CountryCity,
          Topics = []
        };

        // Get topics for THIS project
        var projectTopics = topicRows
            .Where(x =>
                x.ImpactMasterId == project.ImpactMasterId)
            .GroupBy(x => new
            {
              x.CategoryId,
              x.TopicCode
            })
            .Select(x => x.First())
            .OrderBy(x => x.CategoryDisplayOrder)
            .ThenBy(x => x.CategoryId)
            .ToList();

        foreach (var topic in projectTopics)
        {
          var topicDto = new ProjectReportCpmTopicDto
          {
            CategoryId = topic.CategoryId,
            TopicCode = topic.TopicCode,
            TopicName = topic.TopicName,
            DisplayOrder = topic.CategoryDisplayOrder,
            TopLevelMetrics = []
          };

          // Get metrics for THIS project + THIS topic
          var topicMetrics = metricRows
              .Where(x =>
                  x.ImpactMasterId == project.ImpactMasterId &&
                  x.CategoryId == topic.CategoryId)
              .ToList();

          BuildTopLevelMetrics(
              topicDto,
              topicMetrics);

          projectDto.Topics.Add(topicDto);
        }

        projects.Add(projectDto);
      }

      return projects;
    }

    private void BuildTopLevelMetrics(
     ProjectReportCpmTopicDto topicDto,
     List<MetricRow> metricRows)
    {
      // =========================================================
      // TOP LEVEL METRICS
      // =========================================================

      var topLevelRows = metricRows
          .Where(x =>
              x.L1MetricId == null)
          .GroupBy(x => new
          {
            x.TopLevelMetricId,
            x.TopLevelMetricCode
          })
          .Select(g => g.First())
          .OrderBy(x => x.TopLevelMetricId)
          .ToList();

      foreach (var topRow in topLevelRows)
      {
        // =====================================================
        // CREATE TOP LEVEL
        // =====================================================

        var topMetric = new ProjectReportCpmMetricDto
        {
          MetricId = topRow.TopLevelMetricId,

          MetricCode =
                topRow.TopLevelMetricCode,

          MetricName =
                topRow.TopLevelMetricName,

          Unit = string.Empty,

          DisplayOrder = 0,

          L1Metrics = [],

          YearlyData = BuildYearlyData(
                metricRows
                    .Where(x =>
                        x.L1MetricId == null &&
                        x.TopLevelMetricId ==
                            topRow.TopLevelMetricId)
                    .ToList())
        };

        // =====================================================
        // L1 METRICS
        // =====================================================

        var l1Rows = metricRows
            .Where(x =>
                x.L1MetricId.HasValue
                &&
                x.TopLevelMetricId ==
                    topRow.TopLevelMetricId)
            .GroupBy(x => new
            {
              x.L1MetricId,
              x.L1MetricCode
            })
            .Select(g => g.First())
            .OrderBy(x => x.L1MetricId)
            .ToList();

        // =====================================================
        // ADD EACH L1
        // =====================================================

        foreach (var l1Row in l1Rows)
        {
          var l1MetricId =
              l1Row.L1MetricId!.Value;

          var l1Metric =
              new ProjectReportCpmMetricDto
              {
                MetricId = l1MetricId,

                MetricCode =
                      l1Row.L1MetricCode
                      ?? string.Empty,

                MetricName =
                      l1Row.L1MetricName
                      ?? string.Empty,

                Unit = string.Empty,

                DisplayOrder = 0,

                L1Metrics = [],

                YearlyData = BuildYearlyData(
                      metricRows
                          .Where(x =>
                              x.L1MetricId.HasValue
                              &&
                              x.L1MetricId.Value ==
                                  l1MetricId
                              &&
                              x.TopLevelMetricId ==
                                  topRow.TopLevelMetricId)
                          .ToList())
              };

          topMetric.L1Metrics.Add(l1Metric);
        }

        // =====================================================
        // ADD TOP LEVEL TO TOPIC
        // =====================================================

        topicDto.TopLevelMetrics.Add(topMetric);
      }
    }

    private List<ProjectReportCpmYearlyDataDto> BuildYearlyData(
      List<MetricRow> rows)
    {
      return rows
          .GroupBy(x => x.Year)
          .OrderByDescending(x => x.Key)
          .Select(yearGroup =>
          {
            var row = yearGroup.First();

            return new ProjectReportCpmYearlyDataDto
            {
              Year = yearGroup.Key,

              ReportedData =
                  new ProjectReportCpmQuarterDataDto
                      {
                        Q1 = row.Q1ReportedData,
                        Q2 = row.Q2ReportedData,
                        Q3 = row.Q3ReportedData,
                        Q4 = row.Q4ReportedData,
                        Total = row.TotalReportedData
                      },

              RiskAdjustedData =
                  new ProjectReportCpmQuarterDataDto
                      {
                        Q1 = row.Q1RiskAdjustedData,
                        Q2 = row.Q2RiskAdjustedData,
                        Q3 = row.Q3RiskAdjustedData,
                        Q4 = row.Q4RiskAdjustedData,
                        Total = row.TotalRiskAdjustedData
                      }
            };
          })
          .ToList();
    }





    private static int GetMetricDisplayOrder(
    string metricCode)
    {
      if (string.IsNullOrWhiteSpace(metricCode))
      {
        return int.MaxValue;
      }

      var numericPart =
          new string(
              metricCode
                  .SkipWhile(c => !char.IsDigit(c))
                  .ToArray());

      if (string.IsNullOrWhiteSpace(numericPart))
      {
        return int.MaxValue;
      }

      return int.TryParse(
          numericPart.Replace(".", ""),
          out var order)
          ? order
          : int.MaxValue;
    }
    private static string GetString(
    DataRow row,
    string columnName)
    {
      if (!row.Table.Columns.Contains(columnName) ||
          row[columnName] == DBNull.Value)
      {
        return string.Empty;
      }

      return Convert.ToString(row[columnName])
             ?? string.Empty;
    }

    private static int GetInt(
        DataRow row,
        string columnName)
    {
      if (!row.Table.Columns.Contains(columnName) ||
          row[columnName] == DBNull.Value)
      {
        return 0;
      }

      return Convert.ToInt32(row[columnName]);
    }

    private static long GetLong(
        DataRow row,
        string columnName)
    {
      if (!row.Table.Columns.Contains(columnName) ||
          row[columnName] == DBNull.Value)
      {
        return 0;
      }

      return Convert.ToInt64(row[columnName]);
    }

    private static long? GetNullableLong(
        DataRow row,
        string columnName)
    {
      if (!row.Table.Columns.Contains(columnName) ||
          row[columnName] == DBNull.Value)
      {
        return null;
      }

      return Convert.ToInt64(row[columnName]);
    }

  

    private static int? GetNullableInt(
    DataRow row,
    string columnName)
    {
      if (!row.Table.Columns.Contains(
              columnName))
      {
        return null;
      }


      if (row[columnName] == DBNull.Value)
      {
        return null;
      }


      return Convert.ToInt32(
          row[columnName]);
    }

    ////////////////////////////////////////////////////////////////////////////////////// PER TOP-LEVEL METRIC/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //    public async Task<byte[]> GeneratePerTopLevelMetricExcelAsync(
    //PerTopLevelMetricRequestDto request)
    //    {
    //      var report =
    //          await _reportRepository
    //              .GetPerTopLevelMetricReportAsync(request);

    //      if (report == null)
    //      {
    //        throw new InvalidOperationException(
    //            "Per Top-Level Metric report data is null.");
    //      }

    //      var stream =
    //          _perTopLevelMetricExcelGeneratorService
    //              .GeneratePerTopLevelMetricExcel(report);

    //      using (stream)
    //      {
    //        return stream.ToArray();
    //      }
    //    }
    public async Task<byte[]> GeneratePerTopLevelMetricExcelAsync(
         PerTopLevelMetricRequestDto request , CancellationToken cancellationToken)
    {
      var dataSet =
          await _reportRepository
              .GetPerTopLevelMetricReportAsync(request , cancellationToken);

      if (dataSet == null || dataSet.Tables.Count == 0)
      {
        throw new InvalidOperationException(
            "Per Top-Level Metric report data is empty.");
      }

      var report =
          MapPerTopLevelMetricReport(dataSet);

      var stream =
          _perTopLevelMetricExcelGeneratorService
              .GeneratePerTopLevelMetricExcel(report);

      using (stream)
      {
        return stream.ToArray();
      }
    }


    private PerTopLevelMetricReportDto MapPerTopLevelMetricReport(
    DataSet dataSet)
    {
      var report = new PerTopLevelMetricReportDto
      {
        ReportType = "PerTopLevelMetric",
        ReportName = "Per Top-Level Metric"
      };

      var projects =
          new Dictionary<long, PerTopLevelMetricProjectDto>();

      // Result Set 1
      MapProjects(
          dataSet.Tables[0],
          projects);

      // Result Set 2
      MapProjectMetrics(
          dataSet.Tables[1],
          projects);

      // Result Set 3
      MapAggregations(
          dataSet.Tables[2],
          report);


      // Result Set 4 - Generation Data
      if (dataSet.Tables.Count > 3 &&
          dataSet.Tables[3].Rows.Count > 0)
      {
        var row = dataSet.Tables[3].Rows[0];

        report.GenerationData = new ReportGenerationDataDto
        {
          GeneratedByUser =
                row["GeneratedByUser"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(row["GeneratedByUser"])
                        ?? string.Empty,

          GenerationDate =
                row["GenerationDate"] == DBNull.Value
                    ? DateTime.MinValue
                    : Convert.ToDateTime(row["GenerationDate"])
        };
      }

      // Selected metrics
      report.SelectedMetrics =
          projects.Values
              .SelectMany(x => x.Metrics)
              .Where(x =>
                  !string.IsNullOrWhiteSpace(x.MetricCode))
              .GroupBy(
                  x => x.MetricCode!,
                  StringComparer.OrdinalIgnoreCase)
              .Select(g =>
              {
                var metric = g.First();

                return new PerTopLevelMetricSelectedMetricDto
                {
                  MetricCode = metric.MetricCode!,
                  MetricName = metric.MetricName ?? string.Empty,
                  TotalRiskAdjusted = metric.RiskAdjusted
                };
              })
              .OrderBy(x => GetMetricSortKey(x.MetricCode).CategoryOrder)
              .ThenBy(x => GetMetricSortKey(x.MetricCode).MetricCode)
              .ToList();

      report.Projects =
          projects.Values.ToList();

      return report;
    }

    private static void MapProjects(
    DataTable table,
    Dictionary<long, PerTopLevelMetricProjectDto> projects)
    {
      foreach (DataRow row in table.Rows)
      {
        var impactMasterId =
            row["ImpactMasterId"] == DBNull.Value
                ? 0L
                : Convert.ToInt64(row["ImpactMasterId"]);

        if (impactMasterId == 0)
          continue;

        var project = new PerTopLevelMetricProjectDto
        {
          ImId = impactMasterId.ToString(),

          Pin =
                row["Pin"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(row["Pin"])!,

          ProjectTitle =
                row["Title"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(row["Title"])!
        };

        projects[impactMasterId] = project;
      }
    }

    private static void MapProjectMetrics(
    DataTable table,
    Dictionary<long, PerTopLevelMetricProjectDto> projects)
    {
      var projectMetrics =
          new Dictionary<
              long,
              Dictionary<string, PerTopLevelMetricDataDto>>();

      foreach (DataRow row in table.Rows)
      {
        var impactMasterId =
            row["ImpactMasterId"] == DBNull.Value
                ? 0L
                : Convert.ToInt64(row["ImpactMasterId"]);

        if (impactMasterId == 0)
          continue;

        if (!projects.TryGetValue(
                impactMasterId,
                out var project))
        {
          continue;
        }

        var metricLevel =
            row["MetricLevel"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(row["MetricLevel"])!;

        var metricCode =
            metricLevel == "L1"
                ? Convert.ToString(row["L1MetricCode"])!
                : Convert.ToString(row["TopLevelMetricCode"])!;

        if (string.IsNullOrWhiteSpace(metricCode))
          continue;

        var metricName =
            metricLevel == "L1"
                ? Convert.ToString(row["L1MetricName"])!
                : Convert.ToString(row["TopLevelMetricName"])!;

        if (!projectMetrics.TryGetValue(
                impactMasterId,
                out var metrics))
        {
          metrics =
              new Dictionary<
                  string,
                  PerTopLevelMetricDataDto>(
                  StringComparer.OrdinalIgnoreCase);

          projectMetrics.Add(
              impactMasterId,
              metrics);
        }

        if (!metrics.TryGetValue(
                metricCode,
                out var metric))
        {
          metric = new PerTopLevelMetricDataDto
          {
            MetricCode = metricCode,
            MetricName = metricName
          };

          metrics.Add(metricCode, metric);

          project.Metrics.Add(metric);
        }

        var year =
            row["Year"] == DBNull.Value
                ? 0
                : Convert.ToInt32(row["Year"]);

        if (year == 0)
          continue;

        var yearly =
            metric.YearlyData?
                .FirstOrDefault(x => x.Year == year);

        if (yearly == null)
        {
          yearly =
              new PerTopLevelMetricYearlyDataDto
              {
                Year = year
              };

          metric.YearlyData ??=
              new List<PerTopLevelMetricYearlyDataDto>();

          metric.YearlyData.Add(yearly);
        }

        // Original
        yearly.OriginalData.Q1 =
            GetNullableDecimal(row, "Q1Raw");

        yearly.OriginalData.Q2 =
            GetNullableDecimal(row, "Q2Raw");

        yearly.OriginalData.Q3 =
            GetNullableDecimal(row, "Q3Raw");

        yearly.OriginalData.Q4 =
            GetNullableDecimal(row, "Q4Raw");

        // Risk adjusted
        yearly.RiskAdjustedData.Q1 =
            GetNullableDecimal(row, "Q1Adjusted");

        yearly.RiskAdjustedData.Q2 =
            GetNullableDecimal(row, "Q2Adjusted");

        yearly.RiskAdjustedData.Q3 =
            GetNullableDecimal(row, "Q3Adjusted");

        yearly.RiskAdjustedData.Q4 =
            GetNullableDecimal(row, "Q4Adjusted");

        // Annual
        var annualRaw =
            GetNullableDecimal(
                row,
                "AnnualRawValue");

        var annualRiskAdjusted =
            GetNullableDecimal(
                row,
                "AnnualAdjustedValue");

        yearly.EvidenceRating = GetNullableDecimal(row, "EvidenceRating");
        yearly.Baseline = GetNullableDecimal(row, "Baseline");
        yearly.ContributionPercentage = GetNullableDecimal(row, "ContributionPercentage");

        // Year-specific annual values
        yearly.OriginalData.Total =
            annualRaw;

        yearly.RiskAdjustedData.Total =
            annualRiskAdjusted;

        // Keep metric-level values only if required
        metric.Annual =
            annualRaw;

        metric.RiskAdjusted =
            annualRiskAdjusted;
      }

      // Sorting

      foreach (var project in projects.Values)
      {
        foreach (var metric in project.Metrics)
        {
          metric.YearlyData =
              metric.YearlyData?
                  .OrderBy(x => x.Year)
                  .ToList();
        }

        project.Metrics = project.Metrics
      .OrderBy(x => GetMetricSortKey(x.MetricCode).CategoryOrder)
      .ThenBy(x => GetMetricSortKey(x.MetricCode).MetricCode)
      .ToList();
      }
    }

    private static decimal? GetNullableDecimal(
    DataRow row,
    string columnName)
    {
      return row[columnName] == DBNull.Value
          ? null
          : Convert.ToDecimal(row[columnName]);
    }

    private static void MapAggregations(
    DataTable table,
    PerTopLevelMetricReportDto report)
    {
      var aggregations =
          new Dictionary<
              string,
              PerTopLevelMetricAggregationDto>(
              StringComparer.OrdinalIgnoreCase);

      foreach (DataRow row in table.Rows)
      {
        var metricCode =
            row["TopLevelMetricCode"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(
                    row["TopLevelMetricCode"])!;

        if (string.IsNullOrWhiteSpace(metricCode))
          continue;

        var metricName =
            row["TopLevelMetricName"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(
                    row["TopLevelMetricName"])!;

        if (!aggregations.TryGetValue(
                metricCode,
                out var aggregation))
        {
          aggregation =
              new PerTopLevelMetricAggregationDto
              {
                MetricCode = metricCode,
                MetricName = metricName
              };

          aggregations.Add(
              metricCode,
              aggregation);
        }

        var year =
            row["Year"] == DBNull.Value
                ? 0
                : Convert.ToInt32(row["Year"]);

        if (year == 0)
          continue;

        var yearly =
            aggregation.YearlyData
                .FirstOrDefault(x => x.Year == year);

        if (yearly == null)
        {
          yearly =
              new PerTopLevelMetricAggregationYearlyDataDto
              {
                Year = year
              };

          aggregation.YearlyData.Add(yearly);
        }

        yearly.OriginalData ??=
            new PerTopLevelMetricQuarterDataDto();

        yearly.OriginalData.Q1 =
            GetNullableDecimal(row, "Q1Raw");

        yearly.OriginalData.Q2 =
            GetNullableDecimal(row, "Q2Raw");

        yearly.OriginalData.Q3 =
            GetNullableDecimal(row, "Q3Raw");

        yearly.OriginalData.Q4 =
            GetNullableDecimal(row, "Q4Raw");

        yearly.RiskAdjustedData ??=
            new PerTopLevelMetricQuarterDataDto();

        yearly.RiskAdjustedData.Q1 =
            GetNullableDecimal(row, "Q1Adjusted");

        yearly.RiskAdjustedData.Q2 =
            GetNullableDecimal(row, "Q2Adjusted");

        yearly.RiskAdjustedData.Q3 =
            GetNullableDecimal(row, "Q3Adjusted");

        yearly.RiskAdjustedData.Q4 =
            GetNullableDecimal(row, "Q4Adjusted");

        yearly.Annual =
            GetNullableDecimal(
                row,
                "AnnualRawValue");

        yearly.RiskAdjusted =
            GetNullableDecimal(
                row,
                "AnnualAdjustedValue");

        yearly.OriginalData.Total =
            yearly.Annual;

        yearly.RiskAdjustedData.Total =
            yearly.RiskAdjusted;
      }
      report.Aggregations = aggregations.Values
    .OrderBy(x => GetMetricSortKey(x.MetricCode).CategoryOrder)
    .ThenBy(x => GetMetricSortKey(x.MetricCode).MetricCode)
    .ToList();

      foreach (var aggregation in report.Aggregations)
      {
        aggregation.YearlyData =
            aggregation.YearlyData
                .OrderBy(x => x.Year)
                .ToList();
      }
    }

    private static (int CategoryOrder, string MetricCode) GetMetricSortKey(
       string? metricCode)
    {
      if (string.IsNullOrWhiteSpace(metricCode))
        return (999, string.Empty);

      metricCode = metricCode.Trim().ToUpperInvariant();

      var categoryOrder =
          metricCode.StartsWith("MW") ? 1 :
          metricCode.StartsWith("CV") ? 2 :
          metricCode.StartsWith("EE") ? 3 :
          metricCode.StartsWith("MC") ? 4 :
          metricCode.StartsWith("SB") ? 5 :
          metricCode.StartsWith("CI") ? 6 :
          999;

      return (categoryOrder, metricCode);
    }




    //////////////////////////////////////////////////////////////////////PROJECTS ONLY/////////////////////////////////////////////////////////////
    private async Task<byte[]> GenerateProjectsOnlyExcelAsync(
        ProjectsOnlyRequestDto request , CancellationToken cancellationToken)
    {
      var report = await BuildProjectsOnlyReportAsync(request , cancellationToken);


      var excelBytes = _projectsOnlyExcelGeneratorService
          .GenerateProjectsOnlyExcel(report);

      return excelBytes;
    }

    private async Task<ProjectsOnlyReportDto> BuildProjectsOnlyReportAsync(
      ProjectsOnlyRequestDto request ,CancellationToken cancellationToken)
    {
      var dataSet =
          await _reportRepository
              .GetProjectsOnlyReportAsync(request , cancellationToken);

      var report = new ProjectsOnlyReportDto
      {
        ReportType = "ProjectsOnly",
        ReportName = "Projects Only",
        Filters = request.Filters
      };

      if (dataSet == null || dataSet.Tables.Count == 0)
      {
        report.Projects = new List<ProjectsOnlyProjectDto>();
        report.MetricCodes = new List<string>();

        return report;
      }


      // ---------------------------------------------------------
      // RESULT SET 1
      // Projects
      // ---------------------------------------------------------

      var projects =
          new Dictionary<string, ProjectsOnlyProjectDto>();

      var projectTable = dataSet.Tables[0]; //filterd project

      foreach (DataRow row in projectTable.Rows)
      {
        var imId =
            row["ImpactMasterId"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(
                    row["ImpactMasterId"])!;


        var reportingPeriod =
            row["ReviewPeriodCode"] == DBNull.Value
                ? string.Empty
                : Convert.ToString(
                    row["ReviewPeriodCode"])!;


        // Same project can exist for multiple periods.
        var projectKey =
            $"{imId}_{reportingPeriod}";


        var project = new ProjectsOnlyProjectDto
        {
          ImId = imId,

          Pin =
                row["PIN"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(
                        row["PIN"])!,

          ProjectTitle =
                row["ProjectTitle"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(
                        row["ProjectTitle"])!,

          ReportingPeriod =
                reportingPeriod,

          ContractPerformanceManager =
                row["ContractPerformanceManager"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(
                        row["ContractPerformanceManager"])!,

          Program =
                row["Program"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(
                        row["Program"])!
        };


        projects[projectKey] = project;
      }


      // ---------------------------------------------------------
      // RESULT SET 2
      // Metrics
      // ---------------------------------------------------------

      if (dataSet.Tables.Count > 1)
      {
        var metricTable = dataSet.Tables[1];

        foreach (DataRow row in metricTable.Rows)
        {
          var imId =
              row["ImpactMasterId"] == DBNull.Value
                  ? string.Empty
                  : Convert.ToString(
                      row["ImpactMasterId"])!;


          var reportingPeriod =
              row["ReviewPeriodCode"] == DBNull.Value
                  ? string.Empty
                  : Convert.ToString(
                      row["ReviewPeriodCode"])!;


          // Same project + reporting period
          var projectKey =
              $"{imId}_{reportingPeriod}";


          if (!projects.TryGetValue(
                  projectKey,
                  out var project))
          {
            continue;
          }


          // -------------------------------------------------
          // Metric Code
          // -------------------------------------------------

          var metricCode =
              row["MetricCode"] == DBNull.Value
                  ? string.Empty
                  : Convert.ToString(
                      row["MetricCode"])!;


          if (string.IsNullOrWhiteSpace(metricCode))
          {
            continue;
          }


          // -------------------------------------------------
          // Metric Name
          // -------------------------------------------------

          var metricName =
              row["MetricName"] == DBNull.Value
                  ? string.Empty
                  : Convert.ToString(
                      row["MetricName"])!;


          // -------------------------------------------------
          // Original Metric
          // -------------------------------------------------

          decimal? originalValue =
              row["OriginalMetric"] == DBNull.Value
                  ? null
                  : Convert.ToDecimal(
                      row["OriginalMetric"]);


          // -------------------------------------------------
          // Risk Adjusted Metric
          // -------------------------------------------------

          decimal? riskAdjustedValue =
              row["RiskAdjustedMetric"] == DBNull.Value
                  ? null
                  : Convert.ToDecimal(
                      row["RiskAdjustedMetric"]);


          // -------------------------------------------------
          // Dictionaries
          // -------------------------------------------------

          project.OriginalMetrics[metricCode] =
              originalValue;

          project.RiskAdjustedMetrics[metricCode] =
              riskAdjustedValue;


          // -------------------------------------------------
          // Metrics List
          // -------------------------------------------------

          project.Metrics.Add(
              new ProjectsOnlyMetricDto
              {
                MetricCode = metricCode,

                MetricName = metricName,

                OriginalData =
                      new ProjectsOnlyQuarterDataDto
                      {
                        Total = originalValue
                      },

                RiskAdjustedData =
                      new ProjectsOnlyQuarterDataDto
                      {
                        Total = riskAdjustedValue
                      }
              });
        }
      }

      // ---------------------------------------------------------
      // RESULT SET 3
      // Report Generation Metadata
      // ---------------------------------------------------------

      if (dataSet.Tables.Count > 2)
      {
        var metadataTable = dataSet.Tables[2];

        if (metadataTable.Rows.Count > 0)
        {
          var row = metadataTable.Rows[0];

          report.ReportGenerationData = new ReportGenerationDataDto
          {
            GeneratedByUser =
                  row["GeneratedByUser"] == DBNull.Value
                      ? string.Empty
                      : Convert.ToString(row["GeneratedByUser"])!,

            GenerationDate =
                  row["GenerationDate"] == DBNull.Value
                      ? DateTime.MinValue
                      : Convert.ToDateTime(row["GenerationDate"])
          };
        }
      }


      // ---------------------------------------------------------
      // Final Report
      // ---------------------------------------------------------

      report.Projects =
          projects.Values.ToList();


      // ---------------------------------------------------------
      // Unique Metric Codes
      // ---------------------------------------------------------

      report.MetricCodes =
          report.Projects
              .SelectMany(
                  x => x.OriginalMetrics.Keys)
              .Distinct()
              .OrderBy(x => x)
              .ToList();


      return report;
    }


    //Annual Report or Risk Adjusted Data

    private async Task<AnnualPortfolioRiskAdjustedReportDto>
        BuildAnnualPortfolioRiskAdjustedReportAsync(
            AnnualPortfolioRequestDto request , CancellationToken cancellationToken)
    {
      var dataSet =
          await _reportRepository
              .GetAnnualPortfolioRiskAdjustedReportAsync(request , cancellationToken);

      if (dataSet == null || dataSet.Tables.Count < 3)
      {
        throw new InvalidOperationException(
            "Annual Portfolio Risk-adjusted report returned invalid result sets.");
      }

      return MapAnnualPortfolioRiskAdjustedReport(dataSet);
    }

    private async Task<byte[]> GenerateAnnualPortfolioExcelAsync(
AnnualPortfolioRequestDto request , CancellationToken cancellationToken)
    {
      var report =
          await BuildAnnualPortfolioRiskAdjustedReportAsync(request , cancellationToken);

      return _annualPortfolioExcelGeneratorService
 .GenerateAnnualPortfolioExcel(report);
    }



    private static AnnualPortfolioRiskAdjustedReportDto
     MapAnnualPortfolioRiskAdjustedReport(
         DataSet dataSet)
    {
      if (dataSet == null ||
          dataSet.Tables.Count < 3)
      {
        throw new InvalidOperationException(
            "Annual Portfolio Risk-adjusted report must return 3 result sets."
        );
      }

      var report =
          new AnnualPortfolioRiskAdjustedReportDto();

      // =========================================================
      // RESULT SET 1
      // METRIC CATEGORIES + ALL ACTIVE METRICS
      // =========================================================

      var categoryTable = dataSet.Tables[0];

      foreach (DataRow row in categoryTable.Rows)
      {
        var categoryCode =
            GetString(row, "CategoryCode");

        if (string.IsNullOrWhiteSpace(categoryCode))
        {
          continue;
        }

        var category =
            report.MetricCategories
                .FirstOrDefault(x =>
                    string.Equals(
                        x.CategoryCode,
                        categoryCode,
                        StringComparison.OrdinalIgnoreCase
                    ));

        if (category == null)
        {
          category =
              new AnnualPortfolioMetricCategoryDto
              {
                CategoryCode = categoryCode,

                CategoryName =
                      GetString(
                          row,
                          "CategoryName"
                      ),

                Color =
                      GetString(
                          row,
                          "Color"
                      ),

                DisplayOrder =
                      GetInt(
                          row,
                          "DisplayOrder"
                      )
              };

          report.MetricCategories.Add(category);
        }

        var metricCode =
            GetString(row, "MetricCode");

        if (string.IsNullOrWhiteSpace(metricCode))
        {
          continue;
        }

        // Prevent duplicate metrics
        if (!category.Metrics.Any(x =>
                string.Equals(
                    x.MetricCode,
                    metricCode,
                    StringComparison.OrdinalIgnoreCase
                )))
        {
          category.Metrics.Add(
              new AnnualPortfolioMetricDto
              {
                MetricCode = metricCode
              }
          );
        }
      }

      // =========================================================
      // SORT CATEGORY
      // =========================================================

      report.MetricCategories =
          report.MetricCategories
              .OrderBy(x => x.DisplayOrder)
              .ToList();


      // =========================================================
      // RESULT SET 2
      //
      // ONE PROJECT OBJECT =
      //
      // ImpactMasterId + Year
      //
      // Same project in two years:
      //
      // 1350 + 2024
      // 1350 + 2025
      //
      // => TWO PROJECT OBJECTS
      // =========================================================

      var projectTable = dataSet.Tables[1];

      var projectLookup =
          new Dictionary<string, AnnualPortfolioProjectDto>();

      foreach (DataRow row in projectTable.Rows)
      {
        var impactMasterId =
            GetLong(row, "ImpactMasterId");

        var year =
            GetString(row, "ReportYear");

        if (string.IsNullOrWhiteSpace(year))
        {
          year =
              GetString(row, "Year");
        }

        if (impactMasterId <= 0 ||
            string.IsNullOrWhiteSpace(year))
        {
          continue;
        }

        // =====================================================
        // IMPORTANT
        //
        // Internal unique key:
        //
        // 2024_1350
        // 2025_1350
        //
        // But DTO.ImId will contain ONLY:
        //
        // 1350
        // =====================================================

        var projectKey =
            $"{year}_{impactMasterId}";

        if (!projectLookup.TryGetValue(
                projectKey,
                out var project))
        {
          project =
              new AnnualPortfolioProjectDto
              {
                ImId =
                      impactMasterId.ToString(),

                Year =
                      year,

                Pin =
                      GetString(
                          row,
                          "Pin"
                      ),

                ProjectTitle =
                      GetString(
                          row,
                          "ProjectTitle"
                      ),

                ProjectManager =
                      GetString(
                          row,
                          "ProjectManager"
                      ),

                // =================================================
                // NEW FROM SP
                // =================================================

                FundingRecipient =
                      GetString(
                          row,
                          "FundingRecipient"
                      ),

                City =
                      GetString(
                          row,
                          "City"
                      ),

                Program =
                      GetString(
                          row,
                          "ProgramName"
                      ),

                ProjectStatus =
                      GetString(
                          row,
                          "ProjectStatus"
                      ),

                ProjectCategory =
                      GetString(
                          row,
                          "ProjectCategory"
                      )



              };

          projectLookup.Add(
              projectKey,
              project
          );
        }

        // =====================================================
        // METRIC
        // =====================================================

        var metricCode =
            GetString(
                row,
                "MetricCode"
            );

        if (string.IsNullOrWhiteSpace(metricCode))
        {
          continue;
        }

        var riskAdjusted =
            GetNullableDecimal(
                row,
                "AnnualRiskAdjusted"
            );

        var contribution =
            GetNullableDecimal(
                row,
                "ContributionPercentage"
            );

        // =====================================================
        // IMPORTANT
        //
        // Every metric goes into MetricValues:
        //
        // MW1.1
        // MW1.11
        // MW1.12
        // MW1.2
        // ...
        //
        // Top-level:
        // RiskAdjusted + ContributionPercentage
        //
        // L1:
        // RiskAdjusted
        // ContributionPercentage = NULL
        // =====================================================

        project.MetricValues[metricCode] =
            new AnnualPortfolioMetricValueDto
            {
              RiskAdjusted =
                    riskAdjusted,

              ContributionPercentage =
                    contribution
            };
      }

      // =========================================================
      // PROJECT ORDER
      //
      // Year first
      // then PIN
      // then IM ID
      // =========================================================

      report.Projects = projectLookup.Values
       .OrderBy(x => x.Pin)
       .ThenBy(x => x.ImId)
       .ThenByDescending(x => x.Year)
       .ToList();


      // =========================================================
      // RESULT SET 3
      //
      // GRAND TOTAL
      //
      // Structure:
      //
      // Year
      //    MetricCode
      //        GrandTotalRiskAdjusted
      // =========================================================

      var totalTable = dataSet.Tables[2];

      foreach (DataRow row in totalTable.Rows)
      {
        var metricCode = GetString(row, "MetricCode");

        if (string.IsNullOrWhiteSpace(metricCode))
          continue;

        report.Totals[metricCode] =
            new AnnualPortfolioMetricValueDto
            {
              RiskAdjusted =
                    GetNullableDecimal(
                        row,
                        "GrandTotalRiskAdjusted"
                    ),

              ContributionPercentage =
                    GetNullableDecimal(
                        row,
                        "ContributionPercentage"
                    )
            };
      }

      // =========================================================
      // RESULT SET 4
      // REPORT GENERATION METADATA
      // =========================================================

      var metadataTable = dataSet.Tables[3];

      if (metadataTable.Rows.Count > 0)
      {
        var row = metadataTable.Rows[0];

        report.GenerationDataDto =
            new ReportGenerationDataDto
            {
              GeneratedByUser =
                    GetString(row, "GeneratedByUser")
                    ?? string.Empty,

              GenerationDate =
                    row["GenerationDate"] != DBNull.Value
                        ? Convert.ToDateTime(row["GenerationDate"])
                        : DateTime.MinValue
            };
      }

      return report;
    }

    private static decimal? GetDecimal(
    DataRow row,
    string columnName)
    {
      if (!row.Table.Columns.Contains(columnName))
      {
        return null;
      }

      if (row[columnName] == DBNull.Value)
      {
        return null;
      }

      return Convert.ToDecimal(row[columnName]);
    }


    /// ////////////////////////////////COMMON REPORT ENDPOINT///////////////////////////////////////////////


    public async Task<ReportFileResultDto> GenerateReportAsync(
        ReportRequestDto request, CancellationToken cancellationToken)
    {
      var options = new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      };

      switch (request.ReportType)
      {

        // SUMMARY


        case "SummarySinceInception":
          {
            var summaryRequest =
                request.Filters.Deserialize<SummaryReportRequestDto>(
                    options);

            if (summaryRequest == null)
            {
              throw new ArgumentException(
                  "Invalid Summary report filters.");
            }

            // Set UserId from the main request DTO
            summaryRequest.UserId = request.UserId;


            var excel =
                await GenerateSummaryExcelAsync(
                    summaryRequest, cancellationToken);

            return new ReportFileResultDto
            {
              Excel = excel,

              FileName =
                    "Summary_Since_Inception.xlsx",

              ContentType =
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
          }


        // REVIEW METRIC TRACKER


        case "ReviewMetricTracker":
          {
            var trackerRequest =
                request.Filters.Deserialize<ReviewMetricTrackerFiltersDto>(
                    options);

            if (trackerRequest == null)
            {
              throw new ArgumentException(
                  "Invalid Review Metric Tracker filters.");
            }

            // Set UserId from the main request DTO
            trackerRequest.UserId = request.UserId;

            var excel =
                await GenerateReviewMetricTrackerExcelAsync(
                    trackerRequest , cancellationToken);

            return new ReportFileResultDto
            {
              Excel = excel,

              FileName =
                    "ReviewMetricTracker.xlsx",

              ContentType =
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
          }


        // PROJECT REPORT CPM


        case "ProjectReportCPM":
          {
            var projectReportRequest =
                request.Filters.Deserialize<ProjectReportCpmFiltersDto>(
                    options);

            if (projectReportRequest == null)
            {
              throw new ArgumentException(
                  "Invalid Project Report CPM filters.");
            }

            // Set UserId from the main request DTO
            projectReportRequest.UserId = request.UserId;

            var excel =
                await GenerateProjectReportCpmExcelAsync(
                    projectReportRequest, cancellationToken);

            return new ReportFileResultDto
            {
              Excel = excel,

              FileName =
                    "ProjectReport_CPM.xlsx",

              ContentType =
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
          }


        // PER TOP-LEVEL METRIC


        case "TopLevelMetric":
          {
            var perTopLevelMetricRequest =
                request.Filters.Deserialize<PerTopLevelMetricRequestDto>(
                    options);

            if (perTopLevelMetricRequest == null)
            {
              throw new ArgumentException(
                  "Invalid  Top-Level Metric filters.");
            }

            // Set UserId from the main request DTO
            perTopLevelMetricRequest.UserId = request.UserId;

            var excel =
                await GeneratePerTopLevelMetricExcelAsync(
                    perTopLevelMetricRequest , cancellationToken);

            return new ReportFileResultDto
            {
              Excel = excel,

              FileName =
                    "Top-level Metric (Qtr).xlsx",

              ContentType =
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
          }

        // PROJECTS ONLY


        case "ProjectsPortfolio":
          {
            //var projectsOnlyRequest =
            //    request.Filters.Deserialize<ProjectsOnlyRequestDto>(
            //        options);

            var projectsOnlyRequest = new ProjectsOnlyRequestDto
            {
              Filters = request.Filters.Deserialize<ProjectsOnlyFiltersDto>(
             options) ?? new ProjectsOnlyFiltersDto()
            };



            if (projectsOnlyRequest == null)
            {
              throw new ArgumentException(
                  "Invalid Projects Portfolio filters.");
            }

            // Set UserId from the main request DTO
            projectsOnlyRequest.Filters.UserId = request.UserId;

            var excel =
                await GenerateProjectsOnlyExcelAsync(
                    projectsOnlyRequest, cancellationToken);

            return new ReportFileResultDto
            {
              Excel = excel,

              FileName =
                    "Projects Portfolio.xlsx",

              ContentType =
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
          }
        // Risk Adjusted Portfolio (Before it was Annual Portfolio)



        case "RiskAdjustedPortfolio":
          {
            var annualPortfolioRequest =
                request.Filters.Deserialize<AnnualPortfolioRequestDto>(
                    options);

            // Set UserId from the main request DTO
            annualPortfolioRequest.UserId = request.UserId;

            if (annualPortfolioRequest == null)
            {
              throw new ArgumentException(
                  "Invalid Risk Adjusted  filters.");
            }

            var excel =
      await GenerateAnnualPortfolioExcelAsync(
      annualPortfolioRequest , cancellationToken);

            return new ReportFileResultDto
            {
              Excel = excel,

              FileName =
                    "Risk-adjusted Portfolio.xlsx",

              ContentType =
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
          }

        // INVALID REPORT TYPE


        default:
          throw new ArgumentException(
              $"Unsupported report type: {request.ReportType}");
      }

    }

    //----------------filter options--------------------------

    //public async Task<ApiResponse<MetricFilterOptionsDto>> ReportMetricFilterOptions(
    //  MetricFilterRequestDto request,
    //  CancellationToken cancellationToken = default)
    //{
    //  var result =
    //      await _reportRepository.ReportMetricFilterOptions(
    //          request,
    //          cancellationToken);

    //  return ApiResponse<MetricFilterOptionsDto>.Ok(result);
    //}

    public async Task<ApiResponse<MetricFilterOptionsDto>>
    ReportMetricFilterOptions(
        MetricFilterRequestDto request,
        CancellationToken cancellationToken = default)
    {
      var dataSet =
          await _reportRepository.ReportMetricFilterOptions(
              request,
              cancellationToken);

      var response = new MetricFilterOptionsDto();

      // =========================================================
      // TOPIC
      // =========================================================

      if (dataSet.Tables.Count > 0)
      {
        response.TopicMetricOptions =
            dataSet.Tables[0]
                .AsEnumerable()
                .Select(row => new MetricOptionsDto
                {
                  Id = row["Id"] == DBNull.Value
                        ? string.Empty
                        : row["Id"].ToString() ?? string.Empty,

                  Code = row["Code"] == DBNull.Value
                        ? string.Empty
                        : row["Code"].ToString() ?? string.Empty,

                  Name = row["Name"] == DBNull.Value
                        ? string.Empty
                        : row["Name"].ToString() ?? string.Empty,

                  OptionName = row["OptionName"] == DBNull.Value
                        ? string.Empty
                        : row["OptionName"].ToString() ?? string.Empty
                })
                .ToList();
      }

      // =========================================================
      // TOP LEVEL
      // =========================================================

      if (dataSet.Tables.Count > 1)
      {
        response.ToplevelMetricOptions =
            dataSet.Tables[1]
                .AsEnumerable()
                .Select(row => new MetricOptionsDto
                {
                  Id = row["Id"] == DBNull.Value
                        ? string.Empty
                        : row["Id"].ToString() ?? string.Empty,

                  Code = row["Code"] == DBNull.Value
                        ? string.Empty
                        : row["Code"].ToString() ?? string.Empty,

                  Name = row["Name"] == DBNull.Value
                        ? string.Empty
                        : row["Name"].ToString() ?? string.Empty,

                  OptionName = row["OptionName"] == DBNull.Value
                        ? string.Empty
                        : row["OptionName"].ToString() ?? string.Empty
                })
                .ToList();
      }

      // =========================================================
      // L1
      // =========================================================

      if (dataSet.Tables.Count > 2)
      {
        response.L1MetricOptions =
            dataSet.Tables[2]
                .AsEnumerable()
                .Select(row => new MetricOptionsDto
                {
                  Id = row["Id"] == DBNull.Value
                        ? string.Empty
                        : row["Id"].ToString() ?? string.Empty,

                  Code = row["Code"] == DBNull.Value
                        ? string.Empty
                        : row["Code"].ToString() ?? string.Empty,

                  Name = row["Name"] == DBNull.Value
                        ? string.Empty
                        : row["Name"].ToString() ?? string.Empty,

                  OptionName = row["OptionName"] == DBNull.Value
                        ? string.Empty
                        : row["OptionName"].ToString() ?? string.Empty
                })
                .ToList();
      }

      return ApiResponse<MetricFilterOptionsDto>.Ok(response);
    }

    public async Task<ApiResponse<ReportFilterProjectCascadingOptionsDto>>
    ReportFilterProjectCascadingOptions(
        ReportFilterProjectCascadingRequestDto request,
        CancellationToken cancellationToken = default)
    {
      var dataSet =
          await _reportRepository.ReportFilterProjectCascadingOptions(
              request,
              cancellationToken);

      var response = new ReportFilterProjectCascadingOptionsDto();

      // =========================================================
      // RESULT SET 1 - PROJECT
      // =========================================================

      if (dataSet.Tables.Count > 0)
      {
        response.ProjectOptions =
            MapProjectOptions(dataSet.Tables[0]);
      }

      // =========================================================
      // RESULT SET 2 - CPM
      // =========================================================

      if (dataSet.Tables.Count > 1)
      {
        response.CPMOptions =
            MapOptions(dataSet.Tables[1]);
      }

      // =========================================================
      // RESULT SET 3 - GEL
      // =========================================================

      if (dataSet.Tables.Count > 2)
      {
        response.GELOptions =
            MapOptions(dataSet.Tables[2]);
      }

      // =========================================================
      // RESULT SET 4 - PL
      // =========================================================

      if (dataSet.Tables.Count > 3)
      {
        response.PLOptions =
            MapOptions(dataSet.Tables[3]);
      }

      // =========================================================
      // RESULT SET 5 - FUNDING RECIPIENT
      // =========================================================

      if (dataSet.Tables.Count > 4)
      {
        response.FundingRecipientOptions =
            MapOptions(dataSet.Tables[4]);
      }

      // =========================================================
      // RESULT SET 6 - PROGRAM
      // =========================================================

      if (dataSet.Tables.Count > 5)
      {
        response.ProgramOptions =
            MapOptions(dataSet.Tables[5]);
      }

      // =========================================================
      // RESULT SET 7 - COUNTRY CITY
      // =========================================================

      if (dataSet.Tables.Count > 6)
      {
        response.CountryCityOptions =
            MapOptions(dataSet.Tables[6]);
      }

  
      return ApiResponse<ReportFilterProjectCascadingOptionsDto>.Ok(
          response);
    }

    private static List<ReportFilterProjectOptionDto> MapProjectOptions(
    DataTable table)
    {
      return table.AsEnumerable()
          .Select(row => new ReportFilterProjectOptionDto
          {
            ProjectId = row["ProjectId"] == DBNull.Value
                  ? string.Empty
                  : row["ProjectId"].ToString() ?? string.Empty,

            Pin = row["Pin"] == DBNull.Value
                  ? string.Empty
                  : row["Pin"].ToString() ?? string.Empty,

            ProjectName = row["ProjectName"] == DBNull.Value
                  ? string.Empty
                  : row["ProjectName"].ToString() ?? string.Empty
          })
          .ToList();
    }

    private static List<ReportFilterOptionDto> MapOptions(
    DataTable table)
    {
      return table.AsEnumerable()
          .Select(row => new ReportFilterOptionDto
          {
            Id = row["Id"] == DBNull.Value
                  ? string.Empty
                  : row["Id"].ToString() ?? string.Empty,

            Code = row["Code"] == DBNull.Value
                  ? string.Empty
                  : row["Code"].ToString() ?? string.Empty,

            Name = row["Name"] == DBNull.Value
                  ? string.Empty
                  : row["Name"].ToString() ?? string.Empty
          })
          .ToList();
    }


    public async Task<ApiResponse<ReportIndependentFilterOptionsDto>>
    ReportIndependentFilterOptions(
        List<string>? projectIds,
        CancellationToken cancellationToken = default)
    {
      var dataSet =
          await _reportRepository.ReportIndependentFilterOptions(projectIds,
              cancellationToken);

      var response = new ReportIndependentFilterOptionsDto();

      // =========================================================
      // RESULT SET 1 - RECORD TYPE
      // =========================================================

      if (dataSet.Tables.Count > 0)
      {
        response.ProjectStatusOptions =
            MapOptions(dataSet.Tables[0]);
      }

      // =========================================================
      // RESULT SET 2 - STATUS
      // =========================================================

      if (dataSet.Tables.Count > 1)
      {
        response.SourceTypeOptions =
            MapOptions(dataSet.Tables[1]);
      }

      // =========================================================
      // RESULT SET 3 - REPORTING YEAR
      // =========================================================

      if (dataSet.Tables.Count > 2)
      {
        response.ReportingYearOptions =
            MapOptions(dataSet.Tables[2]);
      }

      // =========================================================
      // RESULT SET 4 - REPORTING QUARTER
      // =========================================================

      if (dataSet.Tables.Count > 3)
      {
        response.ReportingQuarterOptions =
            MapOptions(dataSet.Tables[3]);
      }

      // =========================================================
      // RESULT SET 5 - Record Status
      // =========================================================

      if (dataSet.Tables.Count > 4)
      {
        response.RecordStatusOptions =
            MapOptions(dataSet.Tables[4]);
      }

      // =========================================================
      // RESULT SET 6- Project  Category
      // =========================================================

      if (dataSet.Tables.Count > 5)
      {
        response.ProjectCategoryOptions =
            MapOptions(dataSet.Tables[5]);
      }

      return ApiResponse<ReportIndependentFilterOptionsDto>.Ok(
          response);


    }





  }
}

