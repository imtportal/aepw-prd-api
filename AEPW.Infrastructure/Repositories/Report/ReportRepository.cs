using System.Text.Json;
using AEPW.Application.DTOs.Report.Summary;
using AEPW.Application.DTOs.Report.ReviewMetricTracker;
using AEPW.Application.Interfaces.Repositories.Report;
using AEPW.Application.DTOs.Report.ProjectReportCpm;
using AEPW.Application.DTOs.Report.PerTopLevelMetric;
using AEPW.Application.DTOs.Report.ProjectsOnly;
using AEPW.Application.DTOs.Report.AnnualPortfolio;
using System.Data;
using Microsoft.Data.SqlClient;
using AEPW.Infrastructure.Data;
using AEPW.Domain.Constants;
using AEPW.Application.Report.PerTopLevelMetric;
using AEPW.Application.DTOs.Report;

namespace AEPW.Infrastructure.Repositories.Report
{
  public class ReportRepository : IReportRepository
  {
    private readonly Data.ISqlConnectionFactory _sqlConnectionFactory;

    public ReportRepository(ISqlConnectionFactory sqlConnectionFactory)
    {
      _sqlConnectionFactory = sqlConnectionFactory;
    }


    // SUMMARY
    public async Task<DataSet> GetSummaryReportAsync(
 SummaryReportRequestDto request, CancellationToken cancellationToken)
    {
      await using var connection =
         await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken); // Canclellation token

      await using var command = new SqlCommand(
        StoredProcedures.Reports.GetSummarySinceInceptionReport,
        connection);

      command.CommandType = CommandType.StoredProcedure;


      //command.Parameters.Add(
      //    "@ProjectPIN",
      //    SqlDbType.NVarChar,
      //    -1
      //).Value = JsonSerializer.Serialize(
      //    request.ProjectPIN ?? []);

      command.Parameters.Add(
       "@L1MetricIds",
       SqlDbType.NVarChar,
       -1
   ).Value = JsonSerializer.Serialize(
       request.L1MetricIds ?? []);

      command.Parameters.Add(
          "@ProjectNameIds",
          SqlDbType.NVarChar,
          -1
      ).Value = JsonSerializer.Serialize(
          request.ProjectNameIds ?? []);

      command.Parameters.Add(
          "@CpmIds",
          SqlDbType.NVarChar,
          -1
      ).Value = JsonSerializer.Serialize(
          request.CpmIds ?? []);

      //command.Parameters.Add(
      //    "@FundingRecipientIds",
      //    SqlDbType.NVarChar,
      //    -1
      //).Value = JsonSerializer.Serialize(
      //    request.FundingRecipientIds ?? []);

      command.Parameters.Add(
          "@ProgramIds",
          SqlDbType.NVarChar,
          -1
      ).Value = JsonSerializer.Serialize(
          request.ProgramIds ?? []);

      command.Parameters.Add(
          "@CountryCityIds",
          SqlDbType.NVarChar,
          -1
      ).Value = JsonSerializer.Serialize(
          request.CountryCityIds ?? []);

      command.Parameters.Add(
          "@ProjectStatusIds",
          SqlDbType.NVarChar,
          -1
      ).Value = JsonSerializer.Serialize(
          request.ProjectStatusIds ?? []);

      command.Parameters.Add(
          "@ProjectCategoryIds",
          SqlDbType.NVarChar,
          -1
      ).Value = JsonSerializer.Serialize(
          request.ProjectCategoryIds ?? []);

      command.Parameters.Add(
          "@SourceTypeIds",
          SqlDbType.NVarChar,
          -1
      ).Value = JsonSerializer.Serialize(
          request.SourceTypeIds ?? []);

      command.Parameters.Add(
          "@RecordStatusIds",
          SqlDbType.NVarChar,
          -1
      ).Value = JsonSerializer.Serialize(
          request.RecordStatusIds ?? []);

      command.Parameters.Add(
        "@UserId",
        SqlDbType.NVarChar,
        100
    ).Value = request.UserId;


      //await connection.OpenAsync();

      var dataSet = new DataSet();

      using var adapter = new SqlDataAdapter(command);

      adapter.Fill(dataSet);

      return dataSet;
    }

    // Review Metric 

    public async Task<DataSet> GetReviewMetricTrackerReportAsync(
    ReviewMetricTrackerFiltersDto request,
    CancellationToken cancellationToken)
    {
      await using var connection =
          await _sqlConnectionFactory.CreateOpenConnectionAsync(
              cancellationToken);

      await using var command = new SqlCommand(
          StoredProcedures.Reports.GetReviewMetricTrackerReport,
          connection);

      command.CommandType = CommandType.StoredProcedure;

      command.Parameters.Add("@ReviewPeriodCodes", SqlDbType.NVarChar, -1).Value =
          request.ReviewPeriodCodes.Any()
              ? JsonSerializer.Serialize(request.ReviewPeriodCodes)
              : DBNull.Value;

      command.Parameters.Add("@TopicIds", SqlDbType.NVarChar, -1).Value =
          request.TopicIds.Any()
              ? JsonSerializer.Serialize(request.TopicIds)
              : DBNull.Value;

      command.Parameters.Add("@TopLevelMetricIds", SqlDbType.NVarChar, -1).Value =
          request.TopLevelMetricIds.Any()
              ? JsonSerializer.Serialize(request.TopLevelMetricIds)
              : DBNull.Value;

      //command.Parameters.Add("@L1MetricIds", SqlDbType.NVarChar, -1).Value =
      //    request.L1MetricIds.Any()
      //        ? JsonSerializer.Serialize(request.L1MetricIds)
      //        : DBNull.Value;

      command.Parameters.Add("@ProjectIds", SqlDbType.NVarChar, -1).Value =
          request.ProjectIds.Any()
              ? JsonSerializer.Serialize(request.ProjectIds)
              : DBNull.Value;

      command.Parameters.Add("@ProjectNameIds", SqlDbType.NVarChar, -1).Value =
          request.ProjectNameIds.Any()
              ? JsonSerializer.Serialize(request.ProjectNameIds)
              : DBNull.Value;

      command.Parameters.Add("@CpmIds", SqlDbType.NVarChar, -1).Value =
          request.CpmIds.Any()
              ? JsonSerializer.Serialize(request.CpmIds)
              : DBNull.Value;

      command.Parameters.Add("@ProgramLeaderIds", SqlDbType.NVarChar, -1).Value =
          request.ProgramLeaderIds.Any()
              ? JsonSerializer.Serialize(request.ProgramLeaderIds)
              : DBNull.Value;

      command.Parameters.Add("@GlobalExecutionLeaderIds", SqlDbType.NVarChar, -1).Value =
          request.GlobalExecutionLeaderIds.Any()
              ? JsonSerializer.Serialize(request.GlobalExecutionLeaderIds)
              : DBNull.Value;

      command.Parameters.Add("@FundingRecipientIds", SqlDbType.NVarChar, -1).Value =
          request.FundingRecipientIds.Any()
              ? JsonSerializer.Serialize(request.FundingRecipientIds)
              : DBNull.Value;

      command.Parameters.Add("@ProgramIds", SqlDbType.NVarChar, -1).Value =
          request.ProgramIds.Any()
              ? JsonSerializer.Serialize(request.ProgramIds)
              : DBNull.Value;

      command.Parameters.Add("@CountryCityIds", SqlDbType.NVarChar, -1).Value =
          request.CountryCityIds.Any()
              ? JsonSerializer.Serialize(request.CountryCityIds)
              : DBNull.Value;

      command.Parameters.Add("@ProjectStatusIds", SqlDbType.NVarChar, -1).Value =
          request.ProjectStatusIds.Any()
              ? JsonSerializer.Serialize(request.ProjectStatusIds)
              : DBNull.Value;

      command.Parameters.Add("@ProjectCategoryIds", SqlDbType.NVarChar, -1).Value =
       request.ProjectCategoryIds.Any()
           ? JsonSerializer.Serialize(request.ProjectCategoryIds)
           : DBNull.Value;


      command.Parameters.Add("@RecordStatusIds", SqlDbType.NVarChar, -1).Value =
          request.RecordStatusIds.Any()
              ? JsonSerializer.Serialize(request.RecordStatusIds)
              : DBNull.Value;

      command.Parameters.Add("@SourceTypeIds", SqlDbType.NVarChar, -1).Value =
          request.SourceTypeIds.Any()
              ? JsonSerializer.Serialize(request.SourceTypeIds)
              : DBNull.Value;

      command.Parameters.Add("@StartDate", SqlDbType.Date).Value =
          request.StartDate.HasValue
              ? request.StartDate.Value.Date
              : DBNull.Value;

        command.Parameters.Add("@EndDate", SqlDbType.Date).Value =
            request.EndDate.HasValue
                ? request.EndDate.Value.Date
                : DBNull.Value;

          command.Parameters.Add(
            "@UserId",
            SqlDbType.NVarChar,
            100
        ).Value = request.UserId;

      var dataSet = new DataSet();

      using var adapter = new SqlDataAdapter(command);

      adapter.Fill(dataSet);

      return dataSet;
    }

    // Project Report CPM
    public async Task<DataSet> GetProjectReportCpmReportAsync(
    ProjectReportCpmFiltersDto request , CancellationToken cancellationToken)
    {
      await using var connection =
          await _sqlConnectionFactory
              .CreateOpenConnectionAsync(cancellationToken);

      await using var command =
          new SqlCommand(
              StoredProcedures.Reports.GetProjectReportCPM,
              connection);

      command.CommandType =
          CommandType.StoredProcedure;

      // ---------------------------------------------------------
      // Filter JSON
      // ---------------------------------------------------------

      var filterJson =
          JsonSerializer.Serialize(
              request,
              new JsonSerializerOptions
              {
                PropertyNamingPolicy =
                      JsonNamingPolicy.CamelCase
              });

      command.Parameters.Add(
          "@FilterJson",
          SqlDbType.NVarChar,
          -1).Value = filterJson;


      // ---------------------------------------------------------
      // Execute SP
      // ---------------------------------------------------------

      //await using var reader =
      //    await command.ExecuteReaderAsync();

      var dataSet = new DataSet();

      using var adapter = new SqlDataAdapter(command);

      adapter.Fill(dataSet);

      return dataSet;
  
    }



    // PER TOP-LEVEL METRIC
    public async Task<DataSet> GetPerTopLevelMetricReportAsync(
  PerTopLevelMetricRequestDto request, CancellationToken cancellationToken)
    {
      await using var connection =
          await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

      await using var command = new SqlCommand(
          StoredProcedures.Reports.GetPerTopLevelMetric,
          connection);

      command.CommandType = CommandType.StoredProcedure;

      var filterJson = JsonSerializer.Serialize(
          request,
          new JsonSerializerOptions
          {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
          });

      command.Parameters.Add(
          "@FilterJson",
          SqlDbType.NVarChar,
          -1).Value = filterJson;

      // ---------------------------------------------------------
      // Execute
      // ---------------------------------------------------------

      var dataSet = new DataSet();

      using var adapter = new SqlDataAdapter(command);

      adapter.Fill(dataSet);

      return dataSet;
    }
    //For project only


    public async Task<DataSet> GetProjectsOnlyReportAsync(
    ProjectsOnlyRequestDto request ,CancellationToken cancellationToken)
    {
      await using var connection =
          await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

      await using var command = new SqlCommand(
          StoredProcedures.Reports.GetProjectsOnlyReport,
          connection);

      command.CommandType = CommandType.StoredProcedure;

      // ---------------------------------------------------------
      // Filters
      // ---------------------------------------------------------

      command.Parameters.Add(
          "@ReviewPeriodCodes",
          SqlDbType.NVarChar,
          -1).Value =
          request.Filters.ReviewPeriodCodes.Any()
              ? JsonSerializer.Serialize(
                  request.Filters.ReviewPeriodCodes)
              : DBNull.Value;


      command.Parameters.Add(
          "@ProjectIds",
          SqlDbType.NVarChar,
          -1).Value =
          request.Filters.ProjectIds.Any()
              ? JsonSerializer.Serialize(
                  request.Filters.ProjectIds)
              : DBNull.Value;


      command.Parameters.Add(
          "@CpmIds",
          SqlDbType.NVarChar,
          -1).Value =
          request.Filters.CpmIds.Any()
              ? JsonSerializer.Serialize(
                  request.Filters.CpmIds)
              : DBNull.Value;


      command.Parameters.Add(
          "@ProgramLeaderIds",
          SqlDbType.NVarChar,
          -1).Value =
          request.Filters.ProgramLeaderIds.Any()
              ? JsonSerializer.Serialize(
                  request.Filters.ProgramLeaderIds)
              : DBNull.Value;


      command.Parameters.Add(
          "@GlobalExecutionLeaderIds",
          SqlDbType.NVarChar,
          -1).Value =
          request.Filters.GlobalExecutionLeaderIds.Any()
              ? JsonSerializer.Serialize(
                  request.Filters.GlobalExecutionLeaderIds)
              : DBNull.Value;


      command.Parameters.Add(
          "@FundingRecipientIds",
          SqlDbType.NVarChar,
          -1).Value =
          request.Filters.FundingRecipientIds.Any()
              ? JsonSerializer.Serialize(
                  request.Filters.FundingRecipientIds)
              : DBNull.Value;


      command.Parameters.Add(
          "@ProgramIds",
          SqlDbType.NVarChar,
          -1).Value =
          request.Filters.ProgramIds.Any()
              ? JsonSerializer.Serialize(
                  request.Filters.ProgramIds)
              : DBNull.Value;


      command.Parameters.Add(
          "@CountryCityIds",
          SqlDbType.NVarChar,
          -1).Value =
          request.Filters.CountryCityIds.Any()
              ? JsonSerializer.Serialize(
                  request.Filters.CountryCityIds)
              : DBNull.Value;


      command.Parameters.Add(
          "@ProjectStatusIds",
          SqlDbType.NVarChar,
          -1).Value =
          request.Filters.ProjectStatusIds.Any()
              ? JsonSerializer.Serialize(
                  request.Filters.ProjectStatusIds)
              : DBNull.Value;

      command.Parameters.Add(
        "@ProjectCategoryIds",
        SqlDbType.NVarChar,
        -1).Value =
        request.Filters.ProjectCategoryIds.Any()
            ? JsonSerializer.Serialize(
                request.Filters.ProjectCategoryIds)
            : DBNull.Value;



      command.Parameters.Add(
          "@RecordStatusIds",
          SqlDbType.NVarChar,
          -1).Value =
          request.Filters.RecordStatusIds.Any()
              ? JsonSerializer.Serialize(
                  request.Filters.RecordStatusIds)
              : DBNull.Value;

      command.Parameters.Add(
          "@UserId",
          SqlDbType.NVarChar,
          100).Value =
          string.IsNullOrWhiteSpace(request.Filters.UserId)
              ? DBNull.Value
              : request.Filters.UserId;


      // ---------------------------------------------------------
      // Execute
      // ---------------------------------------------------------

      var dataSet = new DataSet();

    using var adapter = new SqlDataAdapter(command);

    adapter.Fill(dataSet);

    return dataSet;
    }

    //Annual Report
    public async Task<DataSet> GetAnnualPortfolioRiskAdjustedReportAsync(
    AnnualPortfolioRequestDto request , CancellationToken cancellationToken)
    {
      await using var connection =
          await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

      await using var command = new SqlCommand(
          StoredProcedures.Reports.GetAnnualPortfolioRiskAdjusted,
          connection);

      command.CommandType = CommandType.StoredProcedure;

      // ---------------------------------------------------------
      // Serialize request as JSON
      // ---------------------------------------------------------

      var filterJson = JsonSerializer.Serialize(
          request,
          new JsonSerializerOptions
          {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
          });

      command.Parameters.Add(
          "@FilterJson",
          SqlDbType.NVarChar,
          -1).Value = filterJson;

      // ---------------------------------------------------------
      // Execute SP
      //
      // Result Set 1 -> Metric Categories
      // Result Set 2 -> Project Metric Data
      // Result Set 3 -> Grand Totals
      // ---------------------------------------------------------

      var dataSet = new DataSet();

      using var adapter = new SqlDataAdapter(command);

      adapter.Fill(dataSet);

      return dataSet;
    }


    //-------------------------------------------------------------- below is Filter options ---------------------------------------------------------------



    public async Task<DataSet> ReportMetricFilterOptions(
       MetricFilterRequestDto request,
       CancellationToken cancellationToken = default)
    {
      await using var connection =
          await _sqlConnectionFactory.CreateOpenConnectionAsync(
              cancellationToken);

      await using var command = new SqlCommand(
          StoredProcedures.Reports.MetricFilterOptions,
          connection)
      {
        CommandType = CommandType.StoredProcedure
      };

      // =========================================================
      // PARAMETERS
      // =========================================================

      command.Parameters.Add(
          "@TopicIds",
          SqlDbType.NVarChar,
          -1
      ).Value =
          request.TopicIds?.Any() == true
              ? JsonSerializer.Serialize(request.TopicIds)
              : DBNull.Value;

      command.Parameters.Add(
          "@TopLevelMetricIds",
          SqlDbType.NVarChar,
          -1
      ).Value =
          request.TopLevelMetricIds?.Any() == true
              ? JsonSerializer.Serialize(request.TopLevelMetricIds)
              : DBNull.Value;

      command.Parameters.Add(
          "@L1MetricIds",
          SqlDbType.NVarChar,
          -1
      ).Value =
          request.L1MetricIds?.Any() == true
              ? JsonSerializer.Serialize(request.L1MetricIds)
              : DBNull.Value;



      // =========================================================
      // EXECUTE SP
      // =========================================================

      var dataSet = new DataSet();

      using var adapter = new SqlDataAdapter(command);

      adapter.Fill(dataSet);

      return dataSet;
    }

    public async Task<DataSet> ReportFilterProjectCascadingOptions(
    ReportFilterProjectCascadingRequestDto request,
    CancellationToken cancellationToken = default)
    {
      await using var connection =
          await _sqlConnectionFactory.CreateOpenConnectionAsync(
              cancellationToken);

      await using var command = new SqlCommand(
          StoredProcedures.Reports.ReportFilterProjectCascadingOptions,
          connection);

      command.CommandType = CommandType.StoredProcedure;

      // =========================================================
      // PARAMETERS
      // =========================================================

      command.Parameters.Add("@ProjectIds", SqlDbType.NVarChar, -1).Value =
      request.ProjectIds != null && request.ProjectIds.Any()
          ? JsonSerializer.Serialize(request.ProjectIds)
          : DBNull.Value;

      command.Parameters.Add("@CPMUserIds", SqlDbType.NVarChar, -1).Value =
          request.CPMUserIds != null && request.CPMUserIds.Any()
              ? JsonSerializer.Serialize(request.CPMUserIds)
              : DBNull.Value;

      command.Parameters.Add("@GELUserIds", SqlDbType.NVarChar, -1).Value =
          request.GELUserIds != null && request.GELUserIds.Any()
              ? JsonSerializer.Serialize(request.GELUserIds)
              : DBNull.Value;

      command.Parameters.Add("@PLUserIds", SqlDbType.NVarChar, -1).Value =
          request.PLUserIds != null && request.PLUserIds.Any()
              ? JsonSerializer.Serialize(request.PLUserIds)
              : DBNull.Value;

      command.Parameters.Add("@FundingRecipientIds", SqlDbType.NVarChar, -1).Value =
          request.FundingRecipientIds != null && request.FundingRecipientIds.Any()
              ? JsonSerializer.Serialize(request.FundingRecipientIds)
              : DBNull.Value;

      command.Parameters.Add("@ProgramIds", SqlDbType.NVarChar, -1).Value =
          request.ProgramIds != null && request.ProgramIds.Any()
              ? JsonSerializer.Serialize(request.ProgramIds)
              : DBNull.Value;

      command.Parameters.Add("@CountryCodes", SqlDbType.NVarChar, -1).Value =
          request.CountryCodes != null && request.CountryCodes.Any()
              ? JsonSerializer.Serialize(request.CountryCodes)
              : DBNull.Value;

      command.Parameters.Add("@CityIds", SqlDbType.NVarChar, -1).Value =
          request.CityIds != null && request.CityIds.Any()
              ? JsonSerializer.Serialize(request.CityIds)
              : DBNull.Value;

      command.Parameters.Add("@ReportingYears", SqlDbType.NVarChar, -1).Value =
      request.ReportingYears != null && request.ReportingYears.Any()
        ? JsonSerializer.Serialize(request.ReportingYears)
        : DBNull.Value;

      command.Parameters.Add("@QuarterCodes", SqlDbType.NVarChar, -1).Value =
      request.QuarterCodes != null && request.QuarterCodes.Any()
       ? JsonSerializer.Serialize(request.QuarterCodes)
       : DBNull.Value;

      command.Parameters.Add("@SelectedReportType", SqlDbType.NVarChar, 100).Value =
      !string.IsNullOrWhiteSpace(request.SelectedReportType)
          ? request.SelectedReportType
          : DBNull.Value;

      // =========================================================
      // EXECUTE SP
      // =========================================================


      var dataSet = new DataSet();

      using var adapter = new SqlDataAdapter(command);

      adapter.Fill(dataSet);

      return dataSet;
    }


  

    public async Task<DataSet> ReportIndependentFilterOptions(
    List<string>? projectIds,
    CancellationToken cancellationToken = default)
    {
      await using var connection =
          await _sqlConnectionFactory.CreateOpenConnectionAsync(
              cancellationToken);

      await using var command = new SqlCommand(
          StoredProcedures.Reports.ReportIndependentFilterOptions,
          connection)
      {
        CommandType = CommandType.StoredProcedure
      };

      var projectIdsJson =
          projectIds != null && projectIds.Count > 0
              ? JsonSerializer.Serialize(projectIds)
              : null;

      command.Parameters.Add(
          "@ProjectIds",
          SqlDbType.NVarChar,
          -1
      ).Value = (object?)projectIdsJson ?? DBNull.Value;

      var dataSet = new DataSet();

      using var adapter = new SqlDataAdapter(command);

      adapter.Fill(dataSet);

      return dataSet;
    }



  }
}
