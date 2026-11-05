using AEPW.Application.DTOs.ImpactMaster;
using AEPW.Application.Contracts.Responses;
using AEPW.Application.Interfaces.Repositories.ImpactMaster;
using AEPW.Domain.Constants;
using AEPW.Domain.Models.Metric;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AEPW.Infrastructure.Repositories.ImpactMaster
{
    public sealed class ImpactMasterRepository : IImpactMasterRepository
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public ImpactMasterRepository(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }
        //Get ALL Impact Masters with Pagination and Filtering
        public async Task<PagedResponse<ImpactMasterListDto>> GetAllImpactMastersAsync(
            GetImpactMastersRequestDto request,
            CancellationToken cancellationToken = default)
        {

            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                 StoredProcedures.ImpactMaster.GetAll,
                connection);

            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue(
    "@PageNumber",
    request.PageNumber);

            command.Parameters.AddWithValue(
                "@PageSize",
                request.PageSize);

            command.Parameters.AddWithValue(
                "@SearchText",
                (object?)request.SearchText ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@ProgramId",
                (object?)request.ProgramId ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@ImpactMasterId",
                (object?)request.ImpactMasterId ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@FundingRecipientId",
                (object?)request.FundingRecipientId ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@StatusId",
                (object?)request.StatusId ?? DBNull.Value);
            command.Parameters.AddWithValue(
    "@L1UserId",
    (object?)request.L1UserId ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@RecordTypeId",
                (object?)request.RecordTypeId ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@PhaseId",
                (object?)request.PhaseId ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@CategoryId",
                (object?)request.CategoryId ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@MetricCategories",
                (object?)request.MetricCategories ?? DBNull.Value);


            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var impactMasters = new List<ImpactMasterListDto>();

            int totalRecords = 0;
            while (await reader.ReadAsync(cancellationToken))
            {
                totalRecords =
    reader.GetInt32(reader.GetOrdinal("TotalRecords"));
                impactMasters.Add(new ImpactMasterListDto
                {
                    ImpactMasterId = long.Parse(reader["ImpactMasterId"].ToString()!),

                    Pin = reader.IsDBNull(reader.GetOrdinal("Pin"))
    ? null
    : reader.GetString(reader.GetOrdinal("Pin")),

                    Title = reader["Title"]?.ToString() ?? string.Empty,

                    ProgramId = reader.IsDBNull(reader.GetOrdinal("ProgramId"))
    ? (long?)null
    : reader.GetInt64(reader.GetOrdinal("ProgramId")),

                    ProgramName = reader["ProgramName"]?.ToString() ?? string.Empty,

                    FundingRecipientNames =
    reader["FundingRecipientNames"]?.ToString() ?? string.Empty,
                    RecordTypeId =
        reader.GetInt64(reader.GetOrdinal("RecordTypeId")),

                    RecordTypeName =
        reader["RecordTypeName"]?.ToString() ?? string.Empty,

                    CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId"))
        ? null
        : reader.GetInt64(reader.GetOrdinal("CategoryId")),

                    CategoryCode = reader["CategoryCode"] as string,

                    CategoryName = reader["CategoryName"] as string,

                    PhaseId = reader.IsDBNull(reader.GetOrdinal("PhaseId"))
        ? null
        : reader.GetInt64(reader.GetOrdinal("PhaseId")),

                    PhaseCode = reader["PhaseCode"] as string,

                    PhaseName = reader["PhaseName"] as string,
                    L1UserId = reader.IsDBNull(reader.GetOrdinal("L1UserId"))
    ? null
    : reader.GetInt64(reader.GetOrdinal("L1UserId")),

                    L1UserName =
        reader["L1UserName"]?.ToString() ?? string.Empty,

                    StatusId = reader.IsDBNull(reader.GetOrdinal("StatusId"))
    ? null
    : reader.GetInt64(reader.GetOrdinal("StatusId")),

                    StatusName = reader["StatusName"]?.ToString() ?? string.Empty,

                    MetricCategories = reader["MetricCategories"]?.ToString() ?? string.Empty,

                    CreatedOnUTC =
        reader.GetDateTime(reader.GetOrdinal("CreatedOnUTC")),

                    UpdatedOnUTC = reader.IsDBNull(reader.GetOrdinal("UpdatedOnUTC"))
        ? null
        : reader.GetDateTime(reader.GetOrdinal("UpdatedOnUTC"))
                });
            }

            return new PagedResponse<ImpactMasterListDto>
            {
                Items = impactMasters,
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        //GET BY IMPACT MASTER ID
        public async Task<ImpactMasterDetailDto?> GetImpactMasterByIdAsync(
            long impactMasterId,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.ImpactMaster.GetById,
                connection);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@ImpactMasterId", SqlDbType.BigInt) { Value = impactMasterId });

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            
            if (!await reader.ReadAsync(cancellationToken))
                return null;

            var dto = new ImpactMasterDetailDto
            {
                ImpactMasterId = reader["ImpactMasterId"].ToString() ?? string.Empty,
                Pin = reader["Pin"].ToString() ?? string.Empty,
                Title = reader["Title"].ToString() ?? string.Empty,

                Description = reader["Description"] as string,

                ProgramId = reader["ProgramId"].ToString() ?? string.Empty,
                ProgramName = reader["ProgramName"].ToString() ?? string.Empty,

                L1UserId = reader["L1UserId"].ToString() ?? string.Empty,
                L1UserName = reader["L1UserName"].ToString() ?? string.Empty,

                L2ApproverUserId = reader.IsDBNull(reader.GetOrdinal("L2ApproverUserId"))
          ? null
          : reader["L2ApproverUserId"].ToString(),

                L2ApproverUserName = reader.IsDBNull(reader.GetOrdinal("L2ApproverUserName"))
          ? null
          : reader["L2ApproverUserName"].ToString(),

                L3ApproverUserId = reader.IsDBNull(reader.GetOrdinal("L3ApproverUserId"))
          ? null
          : reader["L3ApproverUserId"].ToString(),

                L3ApproverUserName = reader.IsDBNull(reader.GetOrdinal("L3ApproverUserName"))
          ? null
          : reader["L3ApproverUserName"].ToString(),

          //      CorporateApproverUserId = reader.IsDBNull(reader.GetOrdinal("CorporateApproverUserId"))
          //? null
          //: reader["CorporateApproverUserId"].ToString(),

          //      CorporateApproverUserName = reader.IsDBNull(reader.GetOrdinal("CorporateApproverUserName"))
          //? null
          //: reader["CorporateApproverUserName"].ToString(),

                RecordTypeId = reader["RecordTypeId"].ToString() ?? string.Empty,
                RecordTypeName = reader["RecordTypeName"].ToString() ?? string.Empty,

                PhaseId = reader.IsDBNull(reader.GetOrdinal("PhaseId"))
          ? null
          : reader["PhaseId"].ToString(),

                PhaseCode = reader.IsDBNull(reader.GetOrdinal("PhaseCode"))
          ? null
          : reader["PhaseCode"].ToString(),

                PhaseName = reader.IsDBNull(reader.GetOrdinal("PhaseName"))
          ? null
          : reader["PhaseName"].ToString(),

                CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId"))
          ? null
          : reader["CategoryId"].ToString(),

                CategoryCode = reader.IsDBNull(reader.GetOrdinal("CategoryCode"))
          ? null
          : reader["CategoryCode"].ToString(),

                CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName"))
          ? null
          : reader["CategoryName"].ToString(),

                StartDate = reader.IsDBNull(reader.GetOrdinal("StartDate"))
          ? null
          : reader.GetDateTime(reader.GetOrdinal("StartDate")),

                EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate"))
          ? null
          : reader.GetDateTime(reader.GetOrdinal("EndDate")),

                ActualCompletionDate = reader.IsDBNull(reader.GetOrdinal("ActualCompletionDate"))
          ? null
          : reader.GetDateTime(reader.GetOrdinal("ActualCompletionDate")),

                TotalFunding = reader.IsDBNull(reader.GetOrdinal("TotalFunding"))
          ? null
          : reader.GetDecimal(reader.GetOrdinal("TotalFunding")),

                InternalReferences = reader["InternalReferences"] as string,
                FieldCheckReport = reader["FieldCheckReport"] as string,
                LastFieldCheckDate = reader["LastFieldCheckDate"] as DateTime?,

                StatusId = reader["StatusId"].ToString() ?? string.Empty,
                StatusCode = reader["StatusCode"].ToString() ?? string.Empty,
                StatusName = reader["StatusName"].ToString() ?? string.Empty,

                CreatedBy = reader["CreatedBy"].ToString() ?? string.Empty,
                CreatedOnUTC = reader.GetDateTime(reader.GetOrdinal("CreatedOnUTC")),

                UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy"))
          ? null
          : reader["UpdatedBy"].ToString(),

                UpdatedOnUTC = reader.IsDBNull(reader.GetOrdinal("UpdatedOnUTC"))
          ? null
          : reader.GetDateTime(reader.GetOrdinal("UpdatedOnUTC")),

                DataCollectionFolderUrl = reader.IsDBNull(reader.GetOrdinal("DataCollectionFolderUrl"))
          ? null
          : reader["DataCollectionFolderUrl"].ToString(),



            };
            await reader.NextResultAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                dto.Cities.Add(new ImpactMasterCityDto
                {
                    ImpactMasterCityId = reader["ImpactMasterCityId"].ToString() ?? string.Empty,
                    CityId = reader["CityId"].ToString() ?? string.Empty,
                    CityName = reader["CityName"].ToString() ?? string.Empty,
                    CountryName = reader["CountryName"].ToString() ?? string.Empty
                });
            }
            //FundingRecipient 
            await reader.NextResultAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                dto.FundingRecipients.Add(new ImpactMasterFundingRecipientDto
                {
                    FundingRecipientId =
                        reader["FundingRecipientId"].ToString() ?? string.Empty,

                    FundingRecipientName =
                        reader["FundingRecipientName"].ToString() ?? string.Empty,

                    FundingRecipientOrgName =
                        reader["FundingRecipientOrgName"].ToString() ?? string.Empty
                });
            }

            return dto;
        }

        public async Task<List<long>> GetReviewIds(
            long impactMasterId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.ImpactMaster.GetReviewIds,
                connection);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(
                new SqlParameter("@ImpactMasterId", SqlDbType.BigInt)
                {
                    Value = impactMasterId
                });

            command.Parameters.Add(
                new SqlParameter("@StartDate", SqlDbType.Date)
                {
                    Value = startDate
                });

            command.Parameters.Add(
                new SqlParameter("@EndDate", SqlDbType.Date)
                {
                    Value = endDate
                });

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var reviewIds = new List<long>();
            while (await reader.ReadAsync(cancellationToken))
            {
                reviewIds.Add(Convert.ToInt64(reader[0]));
            }

            return reviewIds;
        }

        public async Task<CreateImpactMasterResponseDto> CreateImpactMasterAsync(
    CreateImpactMasterRequestDto request,
    CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.ImpactMaster.Insert,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            var citiesTable = new DataTable();
            citiesTable.Columns.Add("CityId", typeof(long));
            //cities
            if (request.CityIds != null)
            {
                foreach (var cityId in request.CityIds)
                {
                    citiesTable.Rows.Add(long.Parse(cityId));
                }
            }
            //FundingRecipient
            var fundingRecipientsTable = new DataTable();
            fundingRecipientsTable.Columns.Add("FundingRecipientId", typeof(long));
            if (request.FundingRecipientIds != null)
            {
                foreach (var fundingRecipientId in request.FundingRecipientIds)
                {
                    fundingRecipientsTable.Rows.Add(long.Parse(fundingRecipientId));
                }
            }


            command.Parameters.Add(new SqlParameter("@Pin", SqlDbType.VarChar, 24)
            {
                Value = string.IsNullOrWhiteSpace(request.Pin)
               ? DBNull.Value
               : request.Pin
            });

            command.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 240)
            {
                Value = request.Title
            });

            command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, -1)
            {
                Value = (object?)request.Description ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter("@ProgramId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.ProgramId)
             ? DBNull.Value
             : long.Parse(request.ProgramId)
            });

            command.Parameters.Add(new SqlParameter("@L1UserId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.L1UserId)
    ? DBNull.Value
    : Convert.ToInt64(request.L1UserId)
            });

            command.Parameters.Add(new SqlParameter("@L2ApproverUserId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.L2ApproverUserId)
                    ? DBNull.Value
                    : long.Parse(request.L2ApproverUserId)
            });

            command.Parameters.Add(new SqlParameter("@L3ApproverUserId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.L3ApproverUserId)
                    ? DBNull.Value
                    : long.Parse(request.L3ApproverUserId)
            });

            command.Parameters.Add(new SqlParameter("@RecordTypeId", SqlDbType.BigInt)
            {
                Value = long.Parse(request.RecordTypeId)
            });

            command.Parameters.Add(new SqlParameter("@PhaseId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.PhaseId)
                    ? DBNull.Value
                    : long.Parse(request.PhaseId)
            });

            command.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.CategoryId)
                    ? DBNull.Value
                    : long.Parse(request.CategoryId)
            });
            command.Parameters.Add(new SqlParameter("@StatusId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.StatusId)
                    ? DBNull.Value
                    : long.Parse(request.StatusId)
            });
            command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime2)
            {
                Value = (object?)request.StartDate ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime2)
            {
                Value = (object?)request.EndDate ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter("@ActualCompletionDate", SqlDbType.DateTime2)
            {
                Value = (object?)request.ActualCompletionDate ?? DBNull.Value
            });



            command.Parameters.Add(new SqlParameter("@TotalFunding", SqlDbType.Decimal)
            {
                Precision = 18,
                Scale = 2,
                Value = (object?)request.TotalFunding ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter("@InternalReferences", SqlDbType.NVarChar, 2048)
            {
                Value = (object?)request.InternalReferences ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter("@FieldCheckReport", SqlDbType.NVarChar, 2048)
            {
                Value = (object?)request.FieldCheckReport ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter("@LastFieldCheckDate", SqlDbType.DateTime2)
            {
                Value = (object?)request.LastFieldCheckDate ?? DBNull.Value
            });




            command.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.BigInt)
            {
                Value = request.CreatedBy
            });
            command.Parameters.Add(new SqlParameter("@Cities", SqlDbType.Structured)
            {
                TypeName = "dbo.ImpactMasterCityModel",
                Value = citiesTable
            });
            command.Parameters.Add(new SqlParameter("@FundingRecipients", SqlDbType.Structured)
            {
                TypeName = "dbo.ImpactMasterFundingRecipientModel",
                Value = fundingRecipientsTable
            });
            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            await reader.ReadAsync(cancellationToken);

            return new CreateImpactMasterResponseDto
            {
                ImpactMasterId = long.Parse(reader["ImpactMasterId"].ToString()!),
                Pin = reader["Pin"].ToString()!
            };
        }

        public async Task<IReadOnlyList<ImpactMasterMetricHierarchyModel>> GetMetricsForImpactMasterAsync(
            string? impactMasterId,
            string? recordTypeId,
            CancellationToken cancellationToken = default)
        {
            var metrics = new List<ImpactMasterMetricHierarchyModel>();

            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.ImpactMasterMetric.GetMetricsForImpactMaster,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@ImpactMasterId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(impactMasterId)
                    ? DBNull.Value
                    : (object)long.Parse(impactMasterId)
            });
            command.Parameters.Add(new SqlParameter("@RecordTypeId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(recordTypeId)
        ? DBNull.Value
        : (object)long.Parse(recordTypeId)
            });

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                metrics.Add(new ImpactMasterMetricHierarchyModel
                {
                    CategoryId =
                        reader["CategoryId"].ToString()!,

                    CategoryCode =
                        reader["CategoryCode"]?.ToString()
                        ?? string.Empty,

                    CategoryName =
                        reader["CategoryName"]?.ToString()
                        ?? string.Empty,

                    CategoryColor =
                        reader["CategoryColor"] as string,
                    //metric
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

                    ImpactMasterMetricId =
                        reader.IsDBNull(
                            reader.GetOrdinal("ImpactMasterMetricId"))
                            ? null
                            : reader["ImpactMasterMetricId"].ToString(),
                    RiskPct =
    reader.IsDBNull(
        reader.GetOrdinal("RiskPct"))
        ? null
        : reader.GetDecimal(
            reader.GetOrdinal("RiskPct")),

                    BaselinePct =
    reader.IsDBNull(
        reader.GetOrdinal("BaselinePct"))
        ? null
        : reader.GetDecimal(
            reader.GetOrdinal("BaselinePct")),
                    

                    IsRiskAdjustmentRequired =
                    reader.GetBoolean(
        reader.GetOrdinal("IsRiskAdjustmentRequired"))
                });
            }
            // Temporary debugging: verify parent and child rows from SQL.
            //var metric36 = metrics.FirstOrDefault(x => x.MetricId == "36");
            //var metric38 = metrics.FirstOrDefault(x => x.MetricId == "38");
            //var rowCount = metrics.Count;
            return metrics;
        }

        public async Task SaveImpactMasterMetricsAsync(
            SaveImpactMasterMetricsRequest request,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.ImpactMasterMetric.UpsertImpactMasterMetrics,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@ImpactMasterId", SqlDbType.BigInt)
            {
                Value = long.Parse(request.ImpactMasterId)
            });

            DataTable metricsTable = BuildMetricsDataTable(request.Metrics);

            command.Parameters.Add(new SqlParameter("@Metrics", SqlDbType.Structured)
            {
                TypeName = "dbo.ImpactMasterMetricType",
                Value = metricsTable
            });

            command.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.BigInt)
            {
                Value = request.UpdatedBy
            });

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private static DataTable BuildMetricsDataTable(
     List<ImpactMasterMetricItemRequest> items)
        {
            var table = new DataTable();

            table.Columns.Add("MetricId", typeof(long));
            table.Columns.Add("RiskPct", typeof(decimal));
            table.Columns.Add("BaselinePct", typeof(decimal));
            table.Columns.Add("IsActive", typeof(bool));

            foreach (var item in items)
            {
                DataRow row = table.NewRow();

                row["MetricId"] = long.Parse(item.MetricId);

                row["RiskPct"] =
                    item.RiskPct.HasValue
                        ? item.RiskPct.Value
                        : DBNull.Value;

                row["BaselinePct"] =
                    item.BaselinePct.HasValue
                        ? item.BaselinePct.Value
                        : DBNull.Value;

                row["IsActive"] = item.IsActive;

                table.Rows.Add(row);
            }

            return table;
        }

        public async Task<bool> UpdateImpactMasterByIdAsync(
    UpdateImpactMasterRequestDto request,
    CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(
                    cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.ImpactMaster.UpdateById,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@ImpactMasterId", SqlDbType.BigInt)
            {
                Value = long.Parse(request.ImpactMasterId)
            });

            command.Parameters.Add(new SqlParameter("@Pin", SqlDbType.NVarChar, 50)
            {
                Value = (object?)request.Pin ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 240)
            {
                Value = request.Title
            });

            command.Parameters.Add(new SqlParameter("@ProgramId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.ProgramId)
             ? DBNull.Value
             : long.Parse(request.ProgramId)
            });

            command.Parameters.Add(new SqlParameter("@L1UserId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.L1UserId)
? DBNull.Value
: Convert.ToInt64(request.L1UserId)
            });

            command.Parameters.Add(new SqlParameter("@L2ApproverUserId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.L2ApproverUserId)
                    ? DBNull.Value
                    : (object)long.Parse(request.L2ApproverUserId)
            });

            command.Parameters.Add(new SqlParameter("@L3ApproverUserId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.L3ApproverUserId)
                    ? DBNull.Value
                    : (object)long.Parse(request.L3ApproverUserId)
            });


            command.Parameters.Add(new SqlParameter("@RecordTypeId", SqlDbType.BigInt)
            {
                Value = long.Parse(request.RecordTypeId)
            });

            command.Parameters.Add(new SqlParameter("@PhaseId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.PhaseId)
                    ? DBNull.Value
                    : (object)long.Parse(request.PhaseId)
            });

            command.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.CategoryId)
                    ? DBNull.Value
                    : (object)long.Parse(request.CategoryId)
            });
            command.Parameters.Add(new SqlParameter("@StatusId", SqlDbType.BigInt)
            {
                Value = string.IsNullOrWhiteSpace(request.StatusId)
                    ? DBNull.Value
                    : (object)long.Parse(request.StatusId)
            });
            command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar)
            {
                Value = (object?)request.Description ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date)
            {
                Value = (object?)request.StartDate ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date)
            {
                Value = (object?)request.EndDate ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter("@ActualCompletionDate", SqlDbType.Date)
            {
                Value = (object?)request.ActualCompletionDate ?? DBNull.Value
            });



            command.Parameters.Add(new SqlParameter("@TotalFunding", SqlDbType.Decimal)
            {
                Precision = 18,
                Scale = 2,
                Value = (object?)request.TotalFunding ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter("@InternalReferences", SqlDbType.NVarChar, 2048)
            {
                Value = (object?)request.InternalReferences ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter("@FieldCheckReport", SqlDbType.NVarChar, 2048)
            {
                Value = (object?)request.FieldCheckReport ?? DBNull.Value
            });

            command.Parameters.Add(new SqlParameter("@LastFieldCheckDate", SqlDbType.DateTime2)
            {
                Value = (object?)request.LastFieldCheckDate ?? DBNull.Value
            });



            command.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.BigInt)
            {
                Value = request.UpdatedBy
            });
            DataTable citiesTable = BuildCitiesDataTable(request.CityIds);

            command.Parameters.Add(new SqlParameter("@Cities", SqlDbType.Structured)
            {
                TypeName = "dbo.ImpactMasterCityModel",
                Value = citiesTable
            });

            DataTable fundingRecipientsTable =
    BuildFundingRecipientsDataTable(request.FundingRecipientIds);

            command.Parameters.Add(new SqlParameter("@FundingRecipients", SqlDbType.Structured)
            {
                TypeName = "dbo.ImpactMasterFundingRecipientModel",
                Value = fundingRecipientsTable
            });

            await command.ExecuteNonQueryAsync(cancellationToken);

            return true;
        }
        //helper methods
        private static DataTable BuildCitiesDataTable(List<string> cityIds)
        {
            var table = new DataTable();

            table.Columns.Add("CityId", typeof(long));

            foreach (var cityId in cityIds)
            {
                table.Rows.Add(long.Parse(cityId));
            }

            return table;
        }
        private static DataTable BuildFundingRecipientsDataTable(List<string> fundingRecipientIds)
        {
            var table = new DataTable();

            table.Columns.Add("FundingRecipientId", typeof(long));

            foreach (var fundingRecipientId in fundingRecipientIds)
            {
                table.Rows.Add(long.Parse(fundingRecipientId));
            }

            return table;
        }

        public async Task<DeleteImpactMasterResponseDto> DeleteImpactMasterAsync(
    DeleteImpactMasterRequestDto request,
    CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.ImpactMaster.Delete,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter("@ImpactMasterId", SqlDbType.BigInt)
                {
                    Value = Convert.ToInt64(request.ImpactMasterId)
                });

            command.Parameters.Add(
                new SqlParameter("@UpdatedBy", SqlDbType.BigInt)
                {
                    Value = Convert.ToInt64(request.UpdatedBy)
                });

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return new DeleteImpactMasterResponseDto
                {
                    ImpactMasterId = request.ImpactMasterId,
                    IsDeleted = false,
                    HasPendingReviews = false,
                    PendingReviewIds = null
                };
            }

            return new DeleteImpactMasterResponseDto
            {
                ImpactMasterId = request.ImpactMasterId,

                IsDeleted =
                    reader["IsDeleted"] != DBNull.Value &&
                    Convert.ToBoolean(reader["IsDeleted"]),

                HasPendingReviews =
                    reader["HasPendingReviews"] != DBNull.Value &&
                    Convert.ToBoolean(reader["HasPendingReviews"]),

                PendingReviewIds =
                    reader["PendingReviewIds"] != DBNull.Value
                        ? reader["PendingReviewIds"].ToString()
                        : null
            };
        }

        public async Task<CopyImpactMasterResponseDto> CopyImpactMasterAsync(
        CopyImpactMasterRequestDto request,
        CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(
                    cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.ImpactMaster.Copy,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter("@ImpactMasterId", SqlDbType.BigInt)
                {
                    Value = Convert.ToInt64(request.ImpactMasterId)
                });

            command.Parameters.Add(
                new SqlParameter("@CreatedBy", SqlDbType.BigInt)
                {
                    Value = Convert.ToInt64(request.CreatedBy)
                });

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "ImpactMaster copy operation did not return a new ImpactMasterId.");
            }

            return new CopyImpactMasterResponseDto
            {
                ImpactMasterId = Convert.ToInt64(
                    reader["ImpactMasterId"]
                ).ToString()
            };
        }
        public async Task<ImpactMasterFilterOptionsDto> ImpactMasterFilterOptions(
   ImpactMasterFilterRequestDto request,
   CancellationToken cancellationToken = default)
        {


            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(
                    cancellationToken);


            await using var command = new SqlCommand(
                StoredProcedures.ImpactMaster.FilterOptions,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            // =========================================================
            // PARAMETERS
            // =========================================================

            command.Parameters.Add(
                new SqlParameter("@ProgramId", SqlDbType.BigInt)
                {
                    Value = string.IsNullOrWhiteSpace(request.ProgramId)
                        ? DBNull.Value
                        : Convert.ToInt64(request.ProgramId)
                });

            command.Parameters.Add(
                new SqlParameter("@RecordTypeId", SqlDbType.BigInt)
                {
                    Value = string.IsNullOrWhiteSpace(request.RecordTypeId)
                        ? DBNull.Value
                        : Convert.ToInt64(request.RecordTypeId)
                });

            command.Parameters.Add(
                new SqlParameter("@StatusId", SqlDbType.BigInt)
                {
                    Value = string.IsNullOrWhiteSpace(request.StatusId)
                        ? DBNull.Value
                        : Convert.ToInt64(request.StatusId)
                });

            Console.WriteLine("------ COMMAND ------");
            Console.WriteLine($"SP Name : {command.CommandText}");

            Console.WriteLine("------ PARAMETERS ------");

            foreach (SqlParameter parameter in command.Parameters)
            {
                Console.WriteLine(
                    $"{parameter.ParameterName} = {parameter.Value}");
            }

            // =========================================================
            // EXECUTE SP
            // =========================================================

            Console.WriteLine("------ EXECUTING SP ------");

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            Console.WriteLine("SP executed successfully.");

            var result = new ImpactMasterFilterOptionsDto();


            try
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    Console.WriteLine("Reading Program row...");

                    result.ProgramOptions.Add(new ProgramFilterOptions
                    {
                        ProgramId = reader["ProgramId"] == DBNull.Value
                            ? null
                            : reader["ProgramId"].ToString(),

                        ProgramCode = reader["ProgramCode"] == DBNull.Value
                            ? null
                            : reader["ProgramCode"].ToString(),

                        ProgramName = reader["ProgramName"] == DBNull.Value
                            ? null
                            : reader["ProgramName"].ToString()
                    });
                }

                Console.WriteLine(
                    $"Program count : {result.ProgramOptions.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!! ERROR IN PROGRAM RESULT SET !!!");
                Console.WriteLine($"Message : {ex.Message}");
                Console.WriteLine($"Type    : {ex.GetType().FullName}");
                Console.WriteLine($"Stack   : {ex.StackTrace}");

                throw;
            }


            bool hasResult2 =
                await reader.NextResultAsync(cancellationToken);


            if (hasResult2)
            {

                try
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {


                        result.RecordTypeOptions.Add(
                            new RecordTypeFilterOptions
                            {
                                RecordTypeId =
                                    reader["RecordTypeId"] == DBNull.Value
                                        ? null
                                        : reader["RecordTypeId"].ToString(),

                                RecordTypeName =
                                    reader["RecordTypeName"] == DBNull.Value
                                        ? null
                                        : reader["RecordTypeName"].ToString()
                            });
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine("!!! ERROR IN RECORD TYPE RESULT SET !!!");
                    Console.WriteLine($"Message : {ex.Message}");
                    Console.WriteLine($"Type    : {ex.GetType().FullName}");
                    Console.WriteLine($"Stack   : {ex.StackTrace}");

                    throw;
                }
            }

            // =========================================================
            // RESULT SET 3 - STATUS


            bool hasResult3 =
                await reader.NextResultAsync(cancellationToken);

            Console.WriteLine($"NextResult : {hasResult3}");

            if (hasResult3)
            {


                try
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        Console.WriteLine("Reading Status row...");

                        result.StatusOptions.Add(
                            new StatusFilterOptions
                            {
                                StatusId =
                                    reader["StatusId"] == DBNull.Value
                                        ? null
                                        : reader["StatusId"].ToString(),

                                StatusCode =
                                    reader["StatusCode"] == DBNull.Value
                                        ? null
                                        : reader["StatusCode"].ToString(),

                                StatusName =
                                    reader["StatusName"] == DBNull.Value
                                        ? null
                                        : reader["StatusName"].ToString()
                            });
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine("!!! ERROR IN STATUS RESULT SET !!!");
                    Console.WriteLine($"Message : {ex.Message}");
                    Console.WriteLine($"Type    : {ex.GetType().FullName}");
                    Console.WriteLine($"Stack   : {ex.StackTrace}");

                    throw;
                }
            }

            // =========================================================
            // FINAL
            // =========================================================


            return result;
        }

    }
}
