using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Metric;
using AEPW.Application.Interfaces.Repositories.Metric;
using AEPW.Domain.Constants;
using AEPW.Domain.Models.Metric;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
namespace AEPW.Infrastructure.Repositories.Metric
{
    public sealed class MetricRepository : IMetricRepository
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public MetricRepository(
            ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<MetricHierarchyResultModel>
            GetMetricHierarchyAsync(
                CancellationToken cancellationToken = default)
        {
            var metrics = new List<MetricHierarchyModel>();

            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                "dbo.uspMetricGetAllMetrics",
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                metrics.Add(new MetricHierarchyModel
                {
                    CategoryId =
                        reader["CategoryId"].ToString()!,

                    CategoryCode =
                        reader["CategoryCode"]?.ToString()
                        ?? string.Empty,

                    CategoryName =
                        reader["CategoryName"]?.ToString()
                        ?? string.Empty,

                    CategoryDescription =
                        reader["CategoryDescription"]?.ToString()
                        ?? string.Empty,

                    CategoryColor =
                        reader["CategoryColor"] as string,
                    MetricId =
                        reader["MetricId"].ToString()!,

                    MetricCode =
                        reader["MetricCode"]?.ToString()
                        ?? string.Empty,

                    MetricName =
                        reader["MetricName"]?.ToString()
                        ?? string.Empty,

                    Description =
                        reader["Description"] as string,

                    MetricHeader =
                     reader["MetricHeader"] as string,

                    IsTopLevel =
                        reader.GetBoolean(
                            reader.GetOrdinal("IsTopLevel")),
                    ParentMetricId =
                        reader.IsDBNull(
                            reader.GetOrdinal("ParentMetricId"))
                            ? null
                            : reader["ParentMetricId"].ToString(),

                    ParentMetricName =
                        reader["ParentMetricName"] as string,


                    IsActive =
                        reader.GetBoolean(
                            reader.GetOrdinal("IsActive")),

                    MetricUnitId =
                        reader.IsDBNull(
                            reader.GetOrdinal("MetricUnitId"))
                            ? null
                            : reader["MetricUnitId"].ToString(),

                    UnitName =
                        reader["UnitName"] as string,

                    MetricSourceId =
    reader.IsDBNull(reader.GetOrdinal("MetricSourceId"))
        ? null
        : reader["MetricSourceId"].ToString(),

                    MetricSourceCode =
    reader["MetricSourceCode"]?.ToString(),

                    MetricSourceName =
    reader["MetricSourceName"]?.ToString(),

                    IsRiskAdjustmentRequired =
    reader.GetBoolean(
        reader.GetOrdinal("IsRiskAdjustmentRequired")),

                    IsICPRequired =
    reader.GetBoolean(
        reader.GetOrdinal("IsICPRequired")),

                    ICPApproverUserId =
    reader.IsDBNull(reader.GetOrdinal("ICPApproverUserId"))
        ? null
        : reader["ICPApproverUserId"].ToString(),

                    ICPApproverName =
    reader["ICPApproverName"] as string,

                    L4ApproverUserId =
    reader.IsDBNull(reader.GetOrdinal("L4ApproverUserId"))
        ? null
        : reader["L4ApproverUserId"].ToString(),

                    L4ApproverName =
    reader["L4ApproverName"] as string,


                });
            }
            await reader.NextResultAsync(cancellationToken);

            var evidenceTypes = new List<MetricEvidenceTypeModel>();

            while (await reader.ReadAsync(cancellationToken))
            {
                evidenceTypes.Add(new MetricEvidenceTypeModel
                {
                    MetricId = reader["MetricId"].ToString()!,

                    EvidenceTypeId =
                        reader["EvidenceTypeId"].ToString()!,

                    Code =
                        reader["Code"].ToString()!,

                    Description =
                        reader["Description"].ToString()!,

                    IsRequired =
                        reader.GetBoolean(
                            reader.GetOrdinal("IsRequired"))
                });
            }


            return new MetricHierarchyResultModel
            {
                Metrics = metrics,
                EvidenceTypes = evidenceTypes
            };
        }
        //SAVE
        public async Task SaveMetricConfigurationAsync(
            MetricSaveConfigurationModel model,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                "dbo.uspMetricSaveMetricConfiguration",
                connection);

            command.CommandType =
      System.Data.CommandType.StoredProcedure;

            command.Parameters.Add(
                MetricRepositoryHelper.CreateMetricCategoryTvp(model.MetricCategories));

            command.Parameters.Add(
                MetricRepositoryHelper.CreateMetricTvp(model.Metrics));

            command.Parameters.AddWithValue(
                "@UpdatedBy",
                model.UserId); // TODO: Resolve from logged-in user

            await command.ExecuteNonQueryAsync(cancellationToken);
        }





        public async Task<IReadOnlyList<MetricDto>>GetAllMetricsAsync(

        CancellationToken cancellationToken = default)

        {

            var metrics = new List<MetricDto>();

            await using var connection =

                await _sqlConnectionFactory

                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(

                "dbo.uspMetricGetAllMetrics",

                connection);

            command.CommandType =

                System.Data.CommandType.StoredProcedure;

            await using var reader =

                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))

            {

                metrics.Add(new MetricDto

                {

                    MetricId = reader["MetricId"].ToString(),
                    CategoryId = int.Parse(reader["CategoryId"].ToString()),
                    MetricCode = reader["MetricCode"].ToString(),
                    MetricName = reader["MetricName"].ToString(),
                    Description = reader["Description"].ToString(),

                    IsTopLevel = (bool)reader["IsTopLevel"],

                    ParentMetricId = reader["ParentMetricId"].ToString(),
                    ParentMetricName = reader["ParentMetricName"].ToString(),

                    MetricSourceId = reader["MetricSourceId"].ToString(),
                    MetricSourceCode = reader["MetricSourceCode"].ToString(),
                    MetricSourceName = reader["MetricSourceName"].ToString(),


                    IsRiskAdjustmentRequired = (bool)reader["IsRiskAdjustmentRequired"],

                    IsICPRequired = (bool)reader["IsICPRequired"],

                    ICPApproverUserId = reader["ICPApproverUserId"] == DBNull.Value
        ? null
        : reader["ICPApproverUserId"].ToString(),

                    ICPApproverName = reader["ICPApproverName"] == DBNull.Value
        ? null
        : reader["ICPApproverName"].ToString(),

               

                    IsActive = (bool)reader["IsActive"],

          
                    MetricUnitId = reader["MetricUnitId"].ToString(),

                    UnitName = reader["UnitName"].ToString()

                });

            }

            return metrics;

        }
//------------------------------------------------------------------------
        public async Task<IReadOnlyList<MetricDto>>
            GetMetricsForProject(
                int ProjectId,
                CancellationToken cancellationToken = default)
        {

            var metrics = new List<MetricDto>();

            await using var connection =

                await _sqlConnectionFactory

                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(

                "dbo.uspProjectGetMetricsForProject",

                connection);

            command.CommandType =

                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@ProjectId", ProjectId);

            await using var reader =

                await command.ExecuteReaderAsync(cancellationToken);

            string metricId = "";
            while (await reader.ReadAsync(cancellationToken)) 
            {
                metricId = reader["MetricId"].ToString();
                try 
                {
                    metrics.Add(new MetricDto

                    {

                        MetricId = reader["MetricId"].ToString(),

                        CategoryId = int.Parse(reader["CategoryId"].ToString()),

                        MetricCode = reader["MetricCode"].ToString(),

                        MetricName = reader["MetricName"].ToString(),
                        Description = reader["Description"].ToString(),
                        IsTopLevel = (bool)reader["IsTopLevel"],
                        ParentMetricId = reader["ParentMetricId"].ToString(),
                        ParentMetricName = reader["ParentMetricName"].ToString(),
              

                        IsActive = reader["IsActive"].ToString() == "True",

              
                        MetricUnitId = reader["MetricUnitId"].ToString(),

                        UnitName = reader["UnitName"].ToString()

                    });

                }
                catch(Exception ex)
                {
                     
                }

            }

            return metrics;

        }


        public async Task<IReadOnlyList<ImpactMasterMetricHierarchyModel>>
            GetAssignableMetricsForReview(
                long reviewId,
                CancellationToken cancellationToken = default)
        {

            var metrics = new List<ImpactMasterMetricHierarchyModel>();

            await using var connection =

                await _sqlConnectionFactory

                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(

                "dbo.uspReviewGetAssignableMetricsForReview",

                connection);

            command.CommandType =

                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@reviewId", reviewId);

            await using var reader =

                await command.ExecuteReaderAsync(cancellationToken);

            string metricId = "";
            while (await reader.ReadAsync(cancellationToken)) 
            {
                metricId = reader["MetricId"].ToString();
                try 
                {
                    metrics.Add(new ImpactMasterMetricHierarchyModel

                    {

                        MetricId = reader["MetricId"].ToString(),

                        CategoryId = (reader["CategoryId"].ToString()),
                        CategoryCode = reader["CategoryCode"].ToString(),
                        CategoryName = reader["CategoryName"].ToString(),
                        CategoryColor = reader["CategoryColor"].ToString(),

                        MetricCode = reader["MetricCode"].ToString(),

                        MetricName = reader["MetricName"].ToString(),
                        Description = reader["Description"].ToString(),
                        IsTopLevel = (bool)reader["IsTopLevel"],
                        ParentMetricId = reader["ParentMetricId"].ToString(),
                        ParentMetricName = reader["ParentMetricName"].ToString(),
              

                        IsActive = reader["IsActive"].ToString() == "True",

              
                        MetricUnitId = reader["MetricUnitId"].ToString(),

                        UnitName = reader["UnitName"].ToString()

                    });

                }
                catch(Exception ex)
                {
                     
                }

            }

            return metrics;

        }


        public async Task<IReadOnlyList<MetricDto>>

            GetMetricsForReview(

                int ReviewId,

                CancellationToken cancellationToken = default)

        {

            var metrics = new List<MetricDto>();

            await using var connection =

                await _sqlConnectionFactory

                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(

                "dbo.uspReviewGetMetricsForReview",

                connection);

            command.CommandType =

                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@ReviewId", ReviewId);

            await using var reader =

                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken)) 
            {

                metrics.Add(new MetricDto

                {

                    MetricId = reader["MetricId"].ToString(),

                    CategoryId = int.Parse(reader["CategoryId"].ToString()),

                    MetricCode = reader["MetricCode"].ToString(),

                    MetricName = reader["MetricName"].ToString(),

                    Description = reader["Description"].ToString(),

                    IsTopLevel = reader["IsTopLevel"].ToString() == "1",

                    ParentMetricId = reader["ParentMetricId"].ToString(),

                    ParentMetricName = reader["ParentMetricName"].ToString(),

                 
                    //EvidenceRequiredMin = (int)reader["EvidenceRequiredMin"],

                    IsActive = reader["IsActive"].ToString() == "True",

   

                    MetricUnitId = reader["MetricUnitId"].ToString(),

                    UnitName = reader["UnitName"].ToString()

                });

            }

            return metrics;

        }

        public async Task<List<MetricEvidenceTypeDto>> GetEvidenceTypeForMetric(string metricCodes, string metricIds)
        {
            var result = new List<MetricEvidenceTypeDto>();

            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Metric.GetEvidenceTypeForMetric,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@MetricCodes", metricCodes);
            command.Parameters.AddWithValue("@MetricIds", metricIds);

            //command.Parameters.AddWithValue("quarter", quarter);

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRowCollection rows = ds.Tables[0].Rows;

                    foreach (DataRow row in rows)
                    {
                        result.Add(new MetricEvidenceTypeDto()
                        {
                            EvidenceTypeId = row.Field<string>("EvidenceTypeId"),
                            Code = row.Field<string>("Code"),
                            Description = row.Field<string>("Description"),
                        });
                    }
                }


            });

            return result;
        }

    }
}
