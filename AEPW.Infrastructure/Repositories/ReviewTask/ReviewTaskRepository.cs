using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.ReviewTask;
using AEPW.Application.Interfaces.Repositories.ReviewTask;
using AEPW.Domain.Constants;
using AEPW.Infrastructure.Data;
using Azure.AI.Projects.Evaluation;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.Graph.Models;
using System.Data;

namespace AEPW.Infrastructure.Repositories.ReviewTask;

public sealed class ReviewTaskRepository : IReviewTaskRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public ReviewTaskRepository(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<ApiResponse<PagedResponse<ReviewTaskDto>>> GetReviewApprovalTasksAsync(
        GetReviewTasksRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(
                cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.ReviewTask.GetReviewApprovalTasks,
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@PageNumber", request.PageNumber);
        command.Parameters.AddWithValue("@PageSize", request.PageSize);
        command.Parameters.AddWithValue("@StageCode", request.StageCode);
        command.Parameters.AddWithValue("@PeriodCode", request.PeriodCode);
        command.Parameters.AddWithValue("@UserId", request.UserId);

        //await using var reader =
        //    await command.ExecuteReaderAsync(cancellationToken);

        var tasks = new List<ReviewTaskDto>();
        List<string> periodCodesHavingData = new List<string>();
        string periodsHavingData = String.Empty;
        int totalRecords = 0;

        DataSet ds = new DataSet();
        await Task.Run(() =>
        {
            using var adapter = new SqlDataAdapter(command);
            adapter.Fill(ds);

            DataTable tasksDataTable = ds.Tables[0];
            DataTable dataPresentInTable = ds.Tables[1];
            
            
            DataRowCollection taskDataRows;
            if (tasksDataTable != null)
            {
                taskDataRows = tasksDataTable.Rows;
            }

            DataRowCollection dataPresentInRows = tasksDataTable.Rows;
            if (dataPresentInTable != null)
            {
                dataPresentInRows = dataPresentInTable.Rows;
            }

          
            foreach (DataRow row in tasksDataTable.Rows)
            {
                tasks.Add(
                    new ReviewTaskDto()
                    {
                        ReviewId = row.Field<long>("ReviewId"),
                        ImpactMasterId = row.Field<long>("ImpactMasterId"),
                        Pin = row.Field<string>("Pin") ?? string.Empty,
                        Title = row.Field<string>("Title") ?? string.Empty,
                        RecordTypeId = row.Field<long>("RecordTypeId"),
                        RecordTypeName = row.Field<string>("RecordTypeName") ?? string.Empty,
                        StatusId = row.Field<long>("StatusId"),
                        StatusName = row.Field<string>("StatusName") ?? string.Empty,

                        PeriodCode = row.Field<string?>("PeriodCode"),

                        Stage = row.Field<int>("Stage"),
                        StageName = row.Field<string>("StageName") ?? string.Empty,
                        StageDisplayValue = row.Field<string>("StageDisplayValue") ?? string.Empty,

                        MetricCategories = row.Field<string?>("MetricCategories"),

                        CreatedOnUTC = row.Field<DateTime>("CreatedOnUTC"),

                        UpdatedOnUTC = row.IsNull("UpdatedOnUTC")
                            ? (DateTime?)null
                            : row.Field<DateTime>("UpdatedOnUTC")
                    }
                );
            }
           


            foreach (DataRow row in dataPresentInTable.Rows)
            {
                periodCodesHavingData.Add(row.Field<string>("PeriodCode"));
            }
        });

        //while (await reader.ReadAsync(cancellationToken))
        //{
        //    if (totalRecords == 0)
        //    {
        //        totalRecords = reader.GetInt32(
        //            reader.GetOrdinal("TotalRecords"));
        //    }

        //tasks.Add(new ReviewTaskDto
        //{
        //    ReviewId = reader.GetInt64(reader.GetOrdinal("ReviewId")),
        //    ImpactMasterId = reader.GetInt64(reader.GetOrdinal("ImpactMasterId")),
        //    Pin = reader["Pin"]?.ToString() ?? string.Empty,
        //    Title = reader["Title"]?.ToString() ?? string.Empty,
        //    RecordTypeId = reader.GetInt64(reader.GetOrdinal("RecordTypeId")),
        //    RecordTypeName = reader["RecordTypeName"]?.ToString() ?? string.Empty,
        //    StatusId = reader.GetInt64(reader.GetOrdinal("StatusId")),
        //    StatusName = reader["StatusName"]?.ToString() ?? string.Empty,
        //    PeriodCode = reader.IsDBNull(reader.GetOrdinal("PeriodCode"))
        //        ? null
        //        : reader["PeriodCode"]?.ToString(),
        //    Stage = reader.GetInt32(reader.GetOrdinal("Stage")),
        //    StageName = reader["StageName"]?.ToString() ?? string.Empty,
        //    StageDisplayValue = reader["StageDisplayValue"]?.ToString() ?? string.Empty,

        //    MetricCategories = reader.IsDBNull(reader.GetOrdinal("MetricCategories"))
        //        ? null
        //        : reader["MetricCategories"]?.ToString(),
        //    CreatedOnUTC = reader.GetDateTime(reader.GetOrdinal("CreatedOnUTC")),
        //    UpdatedOnUTC = reader.IsDBNull(reader.GetOrdinal("UpdatedOnUTC"))
        //        ? null
        //        : reader.GetDateTime(reader.GetOrdinal("UpdatedOnUTC"))
        //});
        //}
        periodsHavingData = String.Join(", ", periodCodesHavingData);

        return new ApiResponse<PagedResponse<ReviewTaskDto>>()
        {
            Data = new PagedResponse<ReviewTaskDto>()
            {
                Items = tasks,
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            },
            Success = true,
            Message = periodsHavingData
        };
            
    }
}
