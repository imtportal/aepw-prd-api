using System;
using System.Collections.Generic;
using System.Text;
using AEPW.Application.Interfaces.Repositories.Dashboard;
using AEPW.Domain.Constants;
using AEPW.Domain.Models.Metric;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;
using AEPW.Application.DTOs.Dashboard;
using System.Text.Json;

public sealed class DashboardRepository : IDashboardRepository
{
  private readonly ISqlConnectionFactory _sqlConnectionFactory;

  public DashboardRepository(
      ISqlConnectionFactory sqlConnectionFactory)
  {
    _sqlConnectionFactory = sqlConnectionFactory;
  }

  public async Task<DashboardPortfolioListDto> GetDashboardPortfolioListAsync(
      DashboardPortfolioListRequestDto request,
      CancellationToken cancellationToken = default)
  {
    var result = new DashboardPortfolioListDto();

    await using var connection =
        await _sqlConnectionFactory
            .CreateOpenConnectionAsync(cancellationToken);

    await using var command = new SqlCommand(
        StoredProcedures.Dashboard.GetPortfolioList,
        connection);

    command.CommandType = CommandType.StoredProcedure;

    command.Parameters.Add(
     new SqlParameter("@ReviewPeriodCodes", SqlDbType.NVarChar, -1)
     {
       Value = request.ReviewPeriodCodes != null &&
                 request.ReviewPeriodCodes.Any()
             ? JsonSerializer.Serialize(request.ReviewPeriodCodes)
             : "[]"
     });

    command.Parameters.Add(
        new SqlParameter("@CategoryCode", SqlDbType.VarChar, 50)
        {
          Value = string.IsNullOrWhiteSpace(request.CategoryCode)
                ? DBNull.Value
                : request.CategoryCode.Trim()
        });

    command.Parameters.Add(
        new SqlParameter("@ProgramId", SqlDbType.BigInt)
        {
          Value = string.IsNullOrWhiteSpace(request.ProgramId)
                ? DBNull.Value
                : Convert.ToInt64(request.ProgramId)
        });

    command.Parameters.Add(
        new SqlParameter("@StatusId", SqlDbType.BigInt)
        {
          Value = string.IsNullOrWhiteSpace(request.StatusId)
                ? DBNull.Value
                : Convert.ToInt64(request.StatusId)
        });

    command.Parameters.Add(
        new SqlParameter("@RecordTypeId", SqlDbType.BigInt)
        {
          Value = string.IsNullOrWhiteSpace(request.RecordTypeId)
                ? DBNull.Value
                : Convert.ToInt64(request.RecordTypeId)
        });

    command.Parameters.Add(
        new SqlParameter("@SearchTerm", SqlDbType.NVarChar, 200)
        {
          Value = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? DBNull.Value
                : request.SearchTerm.Trim()
        });

    command.Parameters.Add(
        new SqlParameter("@PageNumber", SqlDbType.Int)
        {
          Value = request.PageNumber
        });

    command.Parameters.Add(
        new SqlParameter("@PageSize", SqlDbType.Int)
        {
          Value = request.PageSize
        });

    command.Parameters.Add(
        new SqlParameter("@SinceInception", SqlDbType.Bit)
        {
          Value = request.SinceInception
        });

    await using var reader =
        await command.ExecuteReaderAsync(cancellationToken);

    var metricDefinitions =
    new List<DashboardMetricDto>();

    // RESULT SET 1 - Metric definitions

    while (await reader.ReadAsync(cancellationToken))
    {
      metricDefinitions.Add(new DashboardMetricDto
      {
        MetricId =
              reader["MetricId"]?.ToString(),

        MetricCode =
              reader["Code"]?.ToString(),

        MetricName =
              reader["Name"]?.ToString(),

        CategoryCode =
              reader["CategoryCode"]?.ToString(),

        Color =
              reader["Color"]?.ToString()
      });
    }

    result.Metrics = metricDefinitions;

    //Create a temporary dictionary:
    var programs =
    new Dictionary<string, DashboardProgramDto>();

    // RESULT SET 2 - Programs

    if (await reader.NextResultAsync(cancellationToken))
    {
      while (await reader.ReadAsync(cancellationToken))
      {
        var programId =
            reader["ProgramId"]?.ToString();

        if (string.IsNullOrWhiteSpace(programId))
          continue;

        programs[programId] = new DashboardProgramDto
        {
          ProgramId = programId,

          ProgramCode =
                reader["ProgramCode"]?.ToString(),

          ProgramName =
                reader["ProgramName"]?.ToString(),

          ProjectCount =
                reader["ProjectCount"]?.ToString()
        };
      }
    }

    // RESULT SET 3 - Program metrics

    if (await reader.NextResultAsync(cancellationToken))
    {
      while (await reader.ReadAsync(cancellationToken))
      {
        var programId =
            reader["ProgramId"]?.ToString();

        if (string.IsNullOrWhiteSpace(programId))
          continue;

        if (!programs.TryGetValue(programId, out var program))
          continue;

        program.Metrics.Add(new DashboardProgramMetricDto
        {
          MetricId =
                reader["MetricId"]?.ToString(),

          MetricCode = reader["Code"].ToString(),

          RiskAdjustment =
                reader["RiskAdjustment"]?.ToString()
        });
      }
    }
    //Create a dictionary:
    var impacts =
    new Dictionary<string, DashboardProgramImpactDto>();
    // RESULT SET 4 - Program impacts

    if (await reader.NextResultAsync(cancellationToken))
    {
      while (await reader.ReadAsync(cancellationToken))
      {
        var programId =
            reader["ProgramId"]?.ToString();

        var impactMasterId =
            reader["ImpactMasterId"]?.ToString();

        if (string.IsNullOrWhiteSpace(programId) ||
            string.IsNullOrWhiteSpace(impactMasterId))
          continue;

        var key = $"{programId}_{impactMasterId}";

        var impact = new DashboardProgramImpactDto
        {
          ImpactMasterId = impactMasterId,

          Pin =
                reader["Pin"]?.ToString(),

          ImpactName =
                reader["ImpactName"]?.ToString(),

          //CategoryId =
          //      reader["CategoryId"]?.ToString(),

          //CategoryCode =
          //      reader["CategoryCode"]?.ToString(),

          //CategoryName =
          //      reader["CategoryName"]?.ToString(),

          //Color =
          //      reader["CategoryColor"]?.ToString(),

          StatusId =
                reader["StatusId"]?.ToString(),

          StatusCode =
                reader["StatusCode"]?.ToString(),

          StatusName =
                reader["StatusName"]?.ToString(),

          RecordTypeId =
                reader["RecordTypeId"]?.ToString(),

          RecordTypeName =
                reader["RecordTypeName"]?.ToString(),

          FundingRecipients =
                reader["FundingRecipients"]?.ToString()

        };


        impacts[key] = impact;

        if (programs.TryGetValue(programId, out var program))
        {
          program.Impacts.Add(impact);
        }
      }
    }

    // RESULT SET 5 - Impact metrics

    if (await reader.NextResultAsync(cancellationToken))
    {
      while (await reader.ReadAsync(cancellationToken))
      {
        var impactMasterId =
            reader["ImpactMasterId"]?.ToString();

        if (string.IsNullOrWhiteSpace(impactMasterId))
          continue;

        var impact =
            impacts.Values.FirstOrDefault(
                x => x.ImpactMasterId == impactMasterId);

        if (impact is null)
          continue;

        impact.Metrics.Add(new DashboardImpactMetricDto
        {
          MetricId =
                reader["MetricId"]?.ToString(),

          MetricCode = reader["Code"].ToString(),

          RiskAdjustment =
                reader["RiskAdjustment"]?.ToString()
        });
      }
    }

    // RESULT SET 6 - Total records

    if (await reader.NextResultAsync(cancellationToken))
    {
      if (await reader.ReadAsync(cancellationToken))
      {
        result.TotalRecord =
            reader["TotalProgramCount"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["TotalProgramCount"]);
      }
    }

 

    result.Programs = programs.Values.ToList();

    return result;
  }


  public async Task<IReadOnlyList<DashboardPortfolioAdjustmentByCategoriesDto>>
     GetDashboardPortfolioAdjustmentByCategoriesAsync(
         DashboardPortfolioByCategoriesRequestDto request,
         CancellationToken cancellationToken = default)
  {
    var result =
        new List<DashboardPortfolioAdjustmentByCategoriesDto>();

    await using var connection =
        await _sqlConnectionFactory
            .CreateOpenConnectionAsync(cancellationToken);

    await using var command = new SqlCommand(
        StoredProcedures.Dashboard.GetPortfolioByCategories,
        connection);

    command.CommandType = CommandType.StoredProcedure;

    // ---------------------------------------------------------
    // ReviewPeriodCodes
    // ---------------------------------------------------------
    command.Parameters.Add(
        new SqlParameter("@ReviewPeriodCodes", SqlDbType.NVarChar, -1)
        {
          Value = request.ReviewPeriodCodes != null &&
                    request.ReviewPeriodCodes.Any()
                ? JsonSerializer.Serialize(request.ReviewPeriodCodes)
                : "[]"
        });

    // ---------------------------------------------------------
    // SinceInception
    // ---------------------------------------------------------
    command.Parameters.Add(
        new SqlParameter("@SinceInception", SqlDbType.Bit)
        {
          Value = request.SinceInception
        });

    // ---------------------------------------------------------
    // CategoryCode
    // ---------------------------------------------------------
    command.Parameters.Add(
        new SqlParameter("@CategoryCode", SqlDbType.VarChar, 50)
        {
          Value = string.IsNullOrWhiteSpace(request.CategoryCode)
                ? DBNull.Value
                : request.CategoryCode.Trim()
        });

    await using var reader =
        await command.ExecuteReaderAsync(cancellationToken);


    // =========================================================
    // RESULT SET 1
    // Dashboard Portfolio Adjustment
    // =========================================================

    while (await reader.ReadAsync(cancellationToken))
    {
      result.Add(new DashboardPortfolioAdjustmentByCategoriesDto
      {
        Code =
              reader["Code"] == DBNull.Value
                  ? string.Empty
                  : reader["Code"]?.ToString() ?? string.Empty,

        Name =
              reader["Name"] == DBNull.Value
                  ? string.Empty
                  : reader["Name"]?.ToString() ?? string.Empty,

        MetricId =
              reader["MetricId"] == DBNull.Value
                  ? string.Empty
                  : reader["MetricId"]?.ToString() ?? string.Empty,

        RiskAdjustment =
              reader["RiskAdjustment"] == DBNull.Value
                  ? string.Empty
                  : reader["RiskAdjustment"]?.ToString() ?? string.Empty,

        Color =
              reader["Color"] == DBNull.Value
                  ? string.Empty
                  : reader["Color"]?.ToString() ?? string.Empty
      });
    }


    // =========================================================
    // RESULT SET 2
    // Growth
    //
    // MetricId
    // Code
    // QuarterlyGrowth
    // YearlyGrowth
    // =========================================================

    if (await reader.NextResultAsync(cancellationToken))
    {
      while (await reader.ReadAsync(cancellationToken))
      {
        var metricId =
            reader["MetricId"] == DBNull.Value
                ? string.Empty
                : reader["MetricId"]?.ToString() ?? string.Empty;

        var item =
            result.FirstOrDefault(x =>
                x.MetricId == metricId);

        if (item == null)
          continue;


        item.QuarterlyGrowth =
            reader["QuarterlyGrowth"] == DBNull.Value
                ? "7"
                : reader["QuarterlyGrowth"]?.ToString() ?? "7";


        item.AnnualGrowth =
            reader["YearlyGrowth"] == DBNull.Value
                ? "-8"
                : reader["YearlyGrowth"]?.ToString() ?? "-8";
      }
    }


    return result;
  }

}
