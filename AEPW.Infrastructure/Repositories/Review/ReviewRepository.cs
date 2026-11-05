using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs;
using AEPW.Application.DTOs.Evidence;
using AEPW.Application.DTOs.Lookup;
using AEPW.Application.DTOs.Metric;
using AEPW.Application.DTOs.Review;
using AEPW.Application.Interfaces.Repositories.Review;
using AEPW.Domain.Constants;
using AEPW.Infrastructure.Data;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Communications.OnlineMeetings.GetAllRecordingsmeetingOrganizerUserIdMeetingOrganizerUserIdWithStartDateTimeWithEndDateTime;
using Microsoft.Graph.Models;
 using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using static AEPW.Domain.Constants.StoredProcedures;

namespace AEPW.Infrastructure.Repositories.Review
{
    public sealed class ReviewRepository : IReviewRepository
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly IConfiguration _configuration;


        public ReviewRepository(
            ISqlConnectionFactory sqlConnectionFactory
            , IConfiguration configuration)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
            _configuration = configuration;
        }

        public async Task<IReadOnlyList<string>>
            SaveReview(
            CreateReviewRequestDto input,
                CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.Review.SaveReview,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@ReviewId", input.ReviewId);
            command.Parameters.AddWithValue("@ProjectId", input.ProjectId);
            command.Parameters.AddWithValue("@ReviewPeriodId", input.ReviewPeriodId);
            command.Parameters.AddWithValue("@StageName", input.Stage);
            command.Parameters.AddWithValue("@UserId", input.UserId);
            string ReviewMetricJSON = JsonSerializer.Serialize(input.ReviewMetrics);
            command.Parameters.AddWithValue("@ReviewMetricJSON", ReviewMetricJSON);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            string reviewId = "0";
            string message = "";

            while (await reader.ReadAsync(cancellationToken))
            {
                if (reader["Data"] != null)
                {
                    reviewId = reader["Data"].ToString();
                }
                if (reader["Message"] != null)
                {
                    message = reader["Message"].ToString();
                }
            }

            return new List<string>() { reviewId, message };
        }

        public async Task<IReadOnlyList<ReviewPeriodDto>>
            GetReviewPeriods(
                long projectId
                , int fromPresent
                , CancellationToken cancellationToken = default)
        {
            List<ReviewPeriodDto> reviewPeriods = new List<ReviewPeriodDto>();
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetReviewPeriods,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@ProjectId", projectId);
            command.Parameters.AddWithValue("@fromPresent", fromPresent);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                reviewPeriods.Add(
                    new ReviewPeriodDto()
                    {
                        ReviewPeriodId = int.Parse((reader["ReviewPeriodId"]).ToString()),
                        Year = int.Parse((reader["Year"]).ToString()),
                        Quarter = reader["Quarter"] == DBNull.Value
                            ? (int?)null
                            : Convert.ToInt32(reader["Quarter"]),

                        PeriodCode = reader["PeriodCode"].ToString(),
                        StartUtc = (DateTime)reader["StartUtc"],
                        EndUtc = (DateTime)reader["EndUtc"],
                        SubmissionDueUtc = (DateTime)reader["SubmissionDueUtc"],
                        CreatedBy = int.Parse((reader["CreatedBy"]).ToString()),
                        CreatedOnUTC = (DateTime)reader["CreatedOnUTC"],
                        UpdatedBy = (int?)((reader["UpdatedBy"] == DBNull.Value) ? null : reader["UpdatedBy"]),
                        UpdatedOnUTC = (DateTime?)((reader["UpdatedBy"] == DBNull.Value) ? null : reader["UpdatedBy"]),

                        IsCurrent = (bool) reader["IsCurrent"]
                    }
                );
            }

            return reviewPeriods;
        }
        public async Task<IReadOnlyList<ReviewStageLookupDto>>
            GetReviewStages(
                CancellationToken cancellationToken = default)
        {
            List<ReviewStageLookupDto> reviewStages = new List<ReviewStageLookupDto>();
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.Lookup.GetReviewStages,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dataTable = ds.Tables[0];

                    foreach (DataRow row in dataTable.Rows)
                    {
                        reviewStages.Add(
                            new ReviewStageLookupDto()
                            {
                                ReviewStageId = row.Field<Int64>("ReviewStageId"),
                                Code = row.Field<string>("Code"),
                                Name = row.Field<string>("Name"),
                                Description = row.Field<string>("Description"),
                                DefaultSequence = row.Field<short>("DefaultSequence"),
                                IsActive = (row.Field<bool>("IsActive") == true) ? 1 : 0,
                                CreatedBy = row.Field<long?>("CreatedBy"),
                                CreatedOnUTC = row.Field<DateTime>("CreatedOnUTC"),
                                UpdatedBy = row.Field<long?>("UpdatedBy"),
                                UpdatedOnUTC = row.Field<DateTime?>("UpdatedOnUTC"),
                                IsApprovalRequired = row.Field<int>("IsApprovalRequired"),
                            }
                        );
                    }

                }
            });

            return reviewStages;
        }

        public async Task<IReadOnlyList<ReviewDto>> GetReviews(GetReviewFilters filters, CancellationToken cancellationToken = default)
        {
            List<ReviewDto> reviews = new List<ReviewDto>();
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetReviews,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            if (filters.Project != null)
            {
                command.Parameters.AddWithValue("@Project", filters.Project);
            }
            if (filters.PM != null)
            {
                command.Parameters.AddWithValue("@PM", filters.PM);
            }
            if (filters.Stage != null)
            {
                command.Parameters.AddWithValue("@Stage", filters.Stage);
            }
            if (filters.ReviewPeriodId != null)
            {
                command.Parameters.AddWithValue("@ReviewPeriodId", Int64.Parse(filters.ReviewPeriodId));
            }

            if (!String.IsNullOrEmpty(filters.SortBy))
            {
                command.Parameters.AddWithValue("@SortBy", filters.SortBy);
            }
            if (filters.SortDesc != null)
            {
                command.Parameters.AddWithValue("@SortDesc", filters.SortDesc);
            }

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);


            while (await reader.ReadAsync(cancellationToken))
            {
                string updatedOnStr = String.Empty;

                if (reader["updatedOnStr"] != null)
                {
                    updatedOnStr = reader["updatedOnStr"].ToString();
                }

                reviews.Add(
                    new ReviewDto()
                    {
                        ReviewId = Int64.Parse((reader["ReviewId"]).ToString()),
                        ImpactMasterId = int.Parse((reader["ImpactMasterId"]).ToString()),
                        ProjectName = reader["ProjectName"].ToString(),

                        FundingRecipientId = (int)reader["FundingRecipientId"],
                        FundingRecipientName = reader["FundingRecipientName"].ToString(),

                        L1UserId = (int)reader["L1UserId"],
                        PMName = reader["PMName"].ToString(),

                        ReviewPeriodId = int.Parse((reader["ReviewPeriodId"]).ToString()),
                        Quarter = (byte?)reader["Quarter"],
                        Stage = reader["Stage"].ToString(),
                        StageDisplayValue = reader["StageDisplayValue"].ToString(),
                        LaunchedUtc = (DateTime?)((reader["LaunchedUtc"] == DBNull.Value) ? null : reader["LaunchedUtc"]),
                        SubmittedUtc = (DateTime?)((reader["SubmittedUtc"] == DBNull.Value) ? null : reader["SubmittedUtc"]),

                        DataCompletionPct = (decimal)reader["DataCompletionPct"],
                        EvidenceConfirmedPct = (decimal)reader["EvidenceConfirmedPct"],

                        ClosedUtc = (DateTime?)((reader["ClosedUtc"] == DBNull.Value) ? null : reader["ClosedUtc"]),
                        //CreatedBy = int.Parse((reader["CreatedBy"]).ToString()),
                        CreatedByName = reader["CreatedByName"].ToString(),
                        CreatedOnUTC = (DateTime)reader["CreatedOnUTC"],
                        //UpdatedBy = (int?)((reader["UpdatedBy"] == DBNull.Value) ? null : reader["UpdatedBy"]),
                        UpdatedByName = reader["UpdatedByName"].ToString(),
                        UpdatedOnUTC = (DateTime?)((reader["UpdatedOnUTC"] == DBNull.Value) ? null : reader["UpdatedOnUTC"]),

                        DataCollectionFolderUrl = reader["DataCollectionFolderUrl"]?.ToString(),

                        UpdatedOnStr = updatedOnStr
                    }
                );
            }

            return reviews;
        }


        public async Task<IReadOnlyList<ReviewDto>>
            GetReviewById(
                long reviewId,
                long userId,
                string sourcePage,
                int? stage,
                CancellationToken cancellationToken = default)
        {
            List<ReviewDto> reviews = new List<ReviewDto>();
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetReviewById,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@ReviewId", reviewId);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@SourcePage", sourcePage);

            if (stage != null)
            {
                command.Parameters.AddWithValue("@stage", stage);
            }


            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                //ds.Tables[0].Rows[0].Field
                DataTable reviewsTable = ds.Tables[0];
                DataTable metricsTable = ds.Tables[1];
                DataTable commentsTable = ds.Tables[2];
                DataTable evidencesTable = ds.Tables[3];

                List<ReviewMetricDto> reviewMetrics = new List<ReviewMetricDto>();
                List<ReviewCommentDto> reviewComments = new List<ReviewCommentDto>();
                Dictionary<long, List<EvidenceDto>> evidences = new Dictionary<long, List<EvidenceDto>>();


                long reviewMetricIdOfEvidence = 0;
                foreach (DataRow row in evidencesTable.Rows)
                {
                    reviewMetricIdOfEvidence = row.Field<long>("ReviewMetricId");

                    List<EvidenceDto> existingEvidences = new List<EvidenceDto>();

                    if (evidences.ContainsKey(reviewMetricIdOfEvidence))
                    {
                        evidences.TryGetValue(reviewMetricIdOfEvidence, out existingEvidences);
                    }

                    existingEvidences.Add(new EvidenceDto()
                    {
                        EvidenceId = row.Field<long>("EvidenceId"),
                        ReviewMetricId = row.Field<long>("ReviewMetricId"),
                        ItemId = row.Field<string>("ItemId"),
                        FileName = row.Field<string>("FileName"),
                        FileSizeBytes = row.Field<long?>("FileSizeBytes"),
                        FileUriView = row.Field<string>("FileUriView"),
                        FileUriDownload = row.Field<string>("FileUriDownload"),
                        SiteTag = row.Field<string?>("SiteTag"),
                        Period = row.Field<string?>("Period"),
                        Language = row.Field<string?>("Language"),
                        UploadedByUserId = row.Field<long>("UploadedByUserId"),
                        UploadedUtc = row.Field<DateTime>("UploadedUtc"),
                        Status = row.Field<int>("Status"),
                        IsLocked = row.Field<bool>("IsLocked"),
                        CreatedBy = row.Field<long>("CreatedBy"),
                        CreatedOnUTC = row.Field<DateTime>("CreatedOnUTC"),
                        UpdatedBy = row.Field<long?>("UpdatedBy"),
                        UpdatedOnUTC = row.Field<DateTime?>("UpdatedOnUTC"),

                        StatusDisplayValue = row.Field<string>("StatusDisplayValue"),
                        StatusBadgeColor = row.Field<string?>("StatusBadgeColor") ?? "",
                        StatusTextColor = row.Field<string?>("StatusTextColor") ?? "",

                        EvidenceTypeId = row.Field<long?>("EvidenceTypeId"),
                        EvidenceTypeCode = row.Field<string?>("EvidenceTypeCode"),
                        EvidenceTypeDescription = row.Field<string?>("EvidenceTypeDescription"),

                        EvidencedValue = row.Field<string>("EvidencedValue"),
                        Contribution = row.Field<decimal?>("contribution")


                    });


                    if (evidences.ContainsKey(reviewMetricIdOfEvidence))
                    {
                        evidences.Remove(reviewMetricIdOfEvidence);
                    }

                    evidences.Add(
                        reviewMetricIdOfEvidence,
                        existingEvidences
                    );

                }

                foreach (DataRow row in metricsTable.Rows)
                {
                    long? reviewMetricId = row.Field<long?>("ReviewMetricId");
                    List<EvidenceDto> evidencesForReviewMetric = new List<EvidenceDto>();
                    //evidencesForReviewMetric.Clear();


                    if (reviewMetricId > 0)
                    {
                        if (evidences.ContainsKey((long)reviewMetricId))
                        {
                            evidences.TryGetValue((long)reviewMetricId, out evidencesForReviewMetric);
                        }
                    }

                    reviewMetrics.Add(
                        new ReviewMetricDto()
                        {
                            ReviewMetricId = row.Field<long?>("ReviewMetricId"),
                            ReviewId = row.Field<Int64?>("ReviewId"),
                            MetricId = row.Field<Int64?>("MetricId"),
                            MetricName = row.Field<string>("MetricName"),
                            MetricCode = row.Field<string?>("MetricCode"),
                            MetricDescription = row.Field<string>("MetricDescription"),
                            ParentMetricId = row.Field<long?>("ParentMetricId"),
                            FRComment = row.Field<string?>("FRComment"),

                            CategoryId = row.Field<long?>("CategoryId"),
                            CategoryCode = row.Field<string?>("CategoryCode"),
                            CategoryColor = row.Field<string?>("CategoryColor"),
                            CategoryName = row.Field<string?>("CategoryName"),

                            IsTopLevel = row.Field<bool>("IsTopLevel") ? 1 : 0,
                            IsRequired = row.Field<int?>("IsRequired"),
                            Baseline = row.Field<decimal?>("Baseline"),
                            RiskOriginal = row.Field<decimal?>("RiskOriginal"),
                            Status = row.Field<string?>("Status"),
                            RiskPctProposed = row.Field<decimal?>("RiskPctProposed"),
                            RiskPctEffective = (row.Field<decimal?>("RiskPctEffective")),
                            RAStatus = row.Field<string?>("RAStatus"),
                            RAApprovedByUserId = row.Field<int?>("RAApprovedByUserId"),
                            RAApprovedUtc = row.Field<DateTime?>("RAApprovedUtc") != null ? row.Field<DateTime?>("RAApprovedUtc").ToString() : null,
                            RAReason = row.Field<string?>("RAReason"),
                            RAReviewComment = row.Field<string?>("RAReviewComment"),
                            ValidationStatus = row.Field<string?>("ValidationStatus"),
                            //EvidenceCount = row.Field<int?>("EvidenceCount"),
                            //EvidenceUploadCount = row.Field<int?>("EvidenceUploadCount"),
                            VarianceAbs = row.Field<decimal?>("VarianceAbs"),
                            VariancePct = row.Field<decimal?>("VariancePct"),

                            RawValue = row.Field<decimal?>("RawValue"),
                            AdjustedValue = row.Field<decimal?>("AdjustedValue"),

                            LastValueSource = row.Field<string?>("LastValueSource"),
                            LastValueEnteredByUserId = row.Field<int?>("LastValueEnteredByUserId"),
                            LastValueEnteredUtc = row.Field<string?>("LastValueEnteredUtc"),
                            ValorisationTypeId = row.Field<int?>("ValorisationTypeId"),
                            IsValorisationRequired = row.Field<bool>("IsValorisationRequired"),
                            IsRiskAdjustmentRequired = row.Field<bool?>("IsRiskAdjustmentRequired") == true ? true : false,

                            ReportedLimitAcknowledgedBy = row.Field<long?>("ReportedLimitAcknowledgedBy"),
                            ReportedLimit = row.Field<decimal?>("ReportedLimit"),
                            ReportedLimitMsg = row.Field<string>("ReportedLimitMsg"),

                            RejectedInSubsequentStages = row.Field<int>("RejectedInSubsequentStages"),

                            StageDescription = row.Field<string?>("StageDescription") ?? "",
                            CurrentStage = row.Field<int?>("CurrentStage"),
                            Evidenced = row.Field<decimal?>("Evidenced"),
                            EvidencedPer = row.Field<decimal?>("EvidencedPer"),

                            AllowAction = row.Field<bool>("AllowAction"),
                            AllowRejection = row.Field<bool?>("AllowRejection") == true ? true : false,
                            AllowRecall = row.Field<bool?>("AllowRecall") == true ? true : false,
                            //AllowRestatement = row.Field<bool?>("AllowRestatement") == true ? true : false,

                            Evidences = evidencesForReviewMetric
                        }
                    );
                }

                foreach (DataRow row in commentsTable.Rows)
                {
                    reviewComments.Add(
                        new ReviewCommentDto()
                        {
                            ReviewCommentId = row.Field<Int64>("ReviewCommentId"),
                            ReviewId = row.Field<Int64>("ReviewId"),
                            AuthorUserId = row.Field<Int64>("AuthorUserId"),
                            AuthorName = row.Field<string>("AuthorName"),
                            //Role = row.Field<string?>("Role"),
                            Body = row.Field<string>("Body"),
                            CreatedUtc = row.Field<DateTime>("CreatedUtc"),
                            CreatedBy = row.Field<Int64>("CreatedBy"),
                            CreatedOnUtc = row.Field<DateTime>("CreatedOnUtc"),

                            IsActive = row.Field<bool>("IsActive") ? 1 : 0,

                            UpdatedBy = row.Field<int?>("UpdatedBy"),
                            UpdatedOnUtc = row.Field<DateTime?>("UpdatedOnUtc"),

                        }
                    );
                }




                foreach (DataRow row in reviewsTable.Rows)
                {
                    reviews.Add(
                        new ReviewDto()
                        {
                            ReviewId = row.Field<Int64>("ReviewId"),
                            ImpactMasterId = row.Field<Int64>("ImpactMasterId"),
                            ProjectName = row.Field<string>("ProjectName").ToString(),
                            RecordTypeName = row.Field<string?>("RecordTypeName"),

                            ReviewPeriodCode = row.Field<string?>("ReviewPeriodCode"),
                            //FundingRecipientId = row.Field<Int64>("FundingRecipientId"),
                            //FundingRecipientName = row.Field<string>("FundingRecipientName").ToString(),

                            L1UserId = row.Field<Int64>("L1UserId"),
                            PMName = row.Field<string>("PMName").ToString(),
                            AssigneeName = row.Field<string>("AssigneeName")?.ToString(),

                            ReviewPeriodId = row.Field<Int64>("ReviewPeriodId"),
                            Stage = row.Field<int>("Stage").ToString(),
                            StageName = row.Field<string>("StageName"),
                            StageDisplayValue = row.Field<string>("StageDisplayValue"),
                            LaunchedUtc = row.Field<DateTime?>("LaunchedUtc"),
                            SubmittedUtc = row.Field<DateTime?>("LaunchedUtc"),

                            DataCompletionPct = (decimal)row.Field<dynamic>("DataCompletionPct"),
                            EvidenceConfirmedPct = (decimal)row.Field<dynamic>("EvidenceConfirmedPct"),

                            DataCollectionFolderUrl = row.Field<string?>("DataCollectionFolderUrl"),

                            AllowRestatement = row.Field<bool?>("AllowRestatement") == true ? true : false,

                            ClosedUtc = row.Field<DateTime?>("LaunchedUtc"),
                            //CreatedBy = int.Parse((row.Field<dynamic>("CreatedBy")).ToString()),
                            CreatedByName = row.Field<string>("CreatedByName").ToString(),
                            CreatedOnUTC = (DateTime)row.Field<dynamic>("CreatedOnUTC"),
                            //UpdatedBy = (int?)((row.Field<dynamic>("UpdatedBy") == DBNull.Value) ? null : row.Field<dynamic>("UpdatedBy")),
                            UpdatedByName = row.Field<string>("UpdatedByName").ToString(),
                            UpdatedOnUTC = row.Field<DateTime?>("LaunchedUtc"),

                            AtLeastOneMetricAssigned = row.Field<bool>("AtLeastOneMetricAssigned"),

                            ReviewMetrics = reviewMetrics,
                            Comments = reviewComments
                        }
                    );
                }

            }, cancellationToken);


            return reviews;
        }


        public async Task<IReadOnlyList<string>> SaveRiskAdjustmentAction(Int64 ReviewId, List<MetricRiskResponseJSON> metricRiskResponseJSON, int userId, CancellationToken cancellationToken = default)
        {
            string reviewId = "0";
            string message = String.Empty;

            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.Review.SaveRiskAdjustmentAction,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@ReviewId", ReviewId);
            command.Parameters.AddWithValue("@MetricRiskResponseJSON", JsonSerializer.Serialize(metricRiskResponseJSON));
            command.Parameters.AddWithValue("@UserId", userId);

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables != null && ds.Tables.Count > 0)
                {

                    DataTable resultsTable = ds.Tables[0];



                    foreach (DataRow row in resultsTable.Rows)
                    {
                        reviewId = row.Field<Int64>("Data").ToString();
                        message = row.Field<string>("Message") ?? String.Empty;
                    }
                }

                return new List<string>() { reviewId, message };


            }, cancellationToken);


            return new List<string>() { reviewId, message };
        }


        public async Task<List<ReviewExcelExportDto>>
            GetReviewDataAsync(
                long reviewId)
        {
            var result =
                new List<ReviewExcelExportDto>();

            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            using var command =
                new SqlCommand(
                    "usp_GetReviewExcelData",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@ReviewId",
                reviewId);

            using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(
                    new ReviewExcelExportDto
                    {
                        ReviewMetricId = reader["ReviewMetricId"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["ReviewMetricId"]),

                        Pin =
                            reader["Pin"]?.ToString(),

                        ProjectTitle =
                            reader["ProjectTitle"]?.ToString(),

                        FundingRecipientName =
                            reader["FundingRecipientName"]?.ToString(),

                        Email =
                            reader["Email"]?.ToString(),

                        Year =
                            Convert.ToInt32(
                                reader["Year"]),

                        Quarter =
                            reader["Quarter"] == DBNull.Value
                            ? null
                            : Convert.ToInt32(
                                reader["Quarter"]),

                        PeriodCode = reader["PeriodCode"]?.ToString(),
                        StartUtc = Convert.ToDateTime(reader["StartUtc"]),

                        EndUtc = Convert.ToDateTime(reader["EndUtc"]),
                        CategoryCode =
                            reader["CategoryCode"]?.ToString(),
                        CategoryName =
                            reader["CategoryName"]?.ToString(),

                        ParentMetricCode =
                            reader["ParentMetricCode"]?.ToString(),

                        CategoryDescription = reader["CategoryDescription"]?.ToString(),

                        MetricHeader = reader["MetricHeader"]?.ToString(),

                        ValidationChecks = reader["ValidationChecks"]?.ToString(),

                        ParentMetricName =
    reader["ParentMetricName"]?.ToString(),

                        TopLevelTableHeader =
    reader["TopLevelTableHeader"] == DBNull.Value
        ? null
        : reader["TopLevelTableHeader"]?.ToString(),

                        ParentMetricId =
    reader["ParentMetricId"] == DBNull.Value
        ? null
        : Convert.ToInt64(reader["ParentMetricId"]),
                        MetricId = reader["MetricId"] == DBNull.Value
    ? 0
    : Convert.ToInt64(reader["MetricId"]),
                        SubMetricCode =
                            reader["SubMetricCode"]?.ToString(),

                        SubMetricName =
                            reader["SubMetricName"]?.ToString(),

                        MetricDescription =
                            reader["MetricDescription"]?.ToString(),

                        UnitName =
                            reader["UnitName"]?.ToString(),


                        EvidenceTypeId =
                            reader["EvidenceTypeId"] == DBNull.Value
                            ? null
                            : Convert.ToInt64(
                                reader["EvidenceTypeId"]),

                        EvidenceTypeName = reader["EvidenceTypeName"]?.ToString(),

                        IsTopLevel = Convert.ToBoolean(reader["IsTopLevel"]),

                        IsRequired = Convert.ToBoolean(reader["IsRequired"])
                    });
            }

            return result;


        }

        public async Task<List<ApiResponse<long>>> SaveReviewUploadDocuments(List<ReviewUploadExcelJSON> reviewUploadExcelJSON, long userObjectId)
        {
            List<ApiResponse<long>> result = new List<ApiResponse<long>>();

            await using var connection =
               await _sqlConnectionFactory
                   .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.SaveReviewUploadDocuments,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            //command.Parameters.AddWithValue("@ReviewId", reviewId);
            command.Parameters.AddWithValue("@ReviewUploadExcelJSON", JsonSerializer.Serialize(reviewUploadExcelJSON));
            command.Parameters.AddWithValue("@UserId", userObjectId);

            await using var reader =
                await command.ExecuteReaderAsync();


            while (await reader.ReadAsync())
            {
                result.Add(
                    new ApiResponse<long>
                    {
                        Data = (long)reader["Data"],
                        Message = reader["Message"].ToString(),

                    }
                );

            }

            return result;
        }

        public async Task<List<ReviewExcelUploadDto>> GetReviewExcelUploads(Int64 reviewId)
        {
            List<ReviewExcelUploadDto> reviewExcelUploads = new List<ReviewExcelUploadDto>();
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetReviewExcelUploads,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dataTable = ds.Tables[0];

                    DataRowCollection rows = dataTable.Rows;

                    foreach (DataRow row in rows)
                    {
                        reviewExcelUploads.Add(
                            new ReviewExcelUploadDto()
                            {
                                DocumentId = row.Field<Int64>("DocumentId"),
                                ReviewId = row.Field<Int64>("ReviewId"),
                                ViewUri = row.Field<string>("ViewUri"),
                                DownloadUri = row.Field<string?>("ViewUri") ?? "",
                                FileName = row.Field<string>("FileName"),
                                UploadedByUserId = row.Field<Int64>("UploadedByUserId"),
                                UploadedUtc = row.Field<DateTime>("UploadedUtc"),
                                ParseStatus = row.Field<Int64>("ParseStatus"),
                                ParseLogJson = row.Field<string>("ParseLogJson"),
                                StagedValuesJson = row.Field<string>("StagedValuesJson"),
                                CreatedBy = row.Field<Int64>("CreatedBy"),
                                CreatedOnUTC = row.Field<DateTime>("CreatedOnUTC"),
                                UpdatedBy = row.Field<Int64?>("UpdatedBy"),
                                UpdatedOnUTC = row.Field<DateTime?>("UpdatedOnUTC"),

                            }
                        );
                    }
                }
            });


            return reviewExcelUploads;

        }

        public async Task<List<ReviewCommentDto>> AddReviewComments(List<AddReviewCommentDto> reqBody)
        {
            List<ReviewCommentDto> reviewComments = new List<ReviewCommentDto>();
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.AddReviewComment,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@commentsJSON", JsonSerializer.Serialize(reqBody));

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dataTable = ds.Tables[0];

                    DataRowCollection rows = dataTable.Rows;

                    foreach (DataRow row in rows)
                    {
                        reviewComments.Add(
                            new ReviewCommentDto()
                            {
                                ReviewCommentId = row.Field<Int64>("ReviewCommentId"),
                                ReviewId = row.Field<Int64>("ReviewId"),
                                AuthorUserId = row.Field<Int64>("AuthorUserId"),
                                AuthorName = row.Field<string>("AuthorName"),
                                Body = row.Field<string>("Body"),
                                CreatedUtc = row.Field<DateTime>("CreatedUtc"),
                                CreatedBy = row.Field<Int64>("CreatedBy"),
                                CreatedOnUtc = row.Field<DateTime>("CreatedOnUtc"),
                                IsActive = row.Field<bool>("IsActive") ? 1 : 0,
                                UpdatedBy = row.Field<int?>("UpdatedBy"),
                                UpdatedOnUtc = row.Field<DateTime?>("UpdatedOnUtc")
                            }
                        );
                    }
                }
            });

            return reviewComments;
        }

        //SaveMetricDataForReview
        public async Task<List<ApiResponse<int>>> SaveMetricDataForReview(long userId, string projectPin, string jsonData)
        {
            int success = 0;
            string message = String.Empty;
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.SaveMetricDataForReview,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@json", jsonData);
            command.Parameters.AddWithValue("@ProjectPin", projectPin);
            command.Parameters.AddWithValue("@UserId", userId);

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                DataTable dataTable = ds.Tables[0];

                DataRow row = dataTable.Rows[0];

                success = row.Field<int>("Data");
                message = row.Field<string>("Message");
            });

            return new List<ApiResponse<int>>() { new ApiResponse<int>() { Data = success, Success = (success == 1), Message = message } };
            //using var reader = await command.ExecuteReaderAsync();

            //if (await reader.ReadAsync())
            //{
            //    Console.WriteLine($"Data = {reader["Data"]}");
            //    Console.WriteLine($"Message = {reader["Message"]}");
            //}

            ////Console.WriteLine(jsonData);
            ////Console.WriteLine(connectionString);
        }

        public async Task UpdateReviewExcelAsync(long userId, string jsonData)
        {
            var connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            using var connection =
                new SqlConnection(connectionString);

            using var command = new SqlCommand(
                StoredProcedures.Review.SaveMetricEvidences,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@json", jsonData);
            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();

            //using var reader = await command.ExecuteReaderAsync();

            //if (await reader.ReadAsync())
            //{
            //    Console.WriteLine($"Data = {reader["Data"]}");
            //    Console.WriteLine($"Message = {reader["Message"]}");
            //}

            ////Console.WriteLine(jsonData);
            ////Console.WriteLine(connectionString);
        }


        public async Task<List<string>> SaveReviewStages(
            List<SaveReviewStageRequest> reviewStageJSON,
            long userId)
        {
            var result = new List<string>();

            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.SaveReviewStages,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                "@ReviewStageJSON",
                SqlDbType.NVarChar,
                -1
            ).Value = JsonSerializer.Serialize(reviewStageJSON);

            command.Parameters.Add(
                "@UserId",
                SqlDbType.BigInt
            ).Value = userId;

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                result.Add(reader.GetInt32(reader.GetOrdinal("Data")).ToString());

                result.Add(
                    reader.IsDBNull(reader.GetOrdinal("Message"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("Message"))
                );
            }

            return result;
        }


        public async Task<List<LaunchableProjectDto>> GetLaunchableImpacts(string quarter, string status, string impactName, int pageNo, int skippedRows, int itemsPerPage)
        {
            //GetTopicWiseMetricsForReview(2); //TODO: remove asap
            List<LaunchableProjectDto> result = new List<LaunchableProjectDto>();
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetImpactsForLaunch,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@quarter", quarter);
            command.Parameters.AddWithValue("@status", status);
            command.Parameters.AddWithValue("@impactName", impactName);
            command.Parameters.AddWithValue("@pageNo", pageNo);
            command.Parameters.AddWithValue("@skippedRows", skippedRows);
            command.Parameters.AddWithValue("@itemsPerPage", itemsPerPage);

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dataTable = ds.Tables[0];

                    DataRowCollection rows = dataTable.Rows;

                    if (rows.Count > 0)
                    {
                        foreach (DataRow row in rows)
                        {
                            result.Add
                            (
                                new LaunchableProjectDto()
                                {
                                    ImpactMasterId = row.Field<long?>("ImpactMasterId"),
                                    Pin = row.Field<string>("Pin"),
                                    Title = row.Field<string>("Title"),
                                    L1UserId = row.Field<long?>("L1UserId"),
                                    L1UserName = row.Field<string>("L1UserName"),
                                    RegionId = row.Field<long?>("RegionId"),
                                    ProgramId = row.Field<long?>("ProgramId"),
                                    RegionName = row.Field<string?>("RegionName"),
                                    ProgramName = row.Field<string?>("ProgramName"),
                                    StatusId = row.Field<long?>("StatusId"),
                                    StatusCode = row.Field<string>("StatusCode"),
                                    StatusName = row.Field<string>("StatusName"),
                                    LastReviewStage = row.Field<string>("LastReviewStage"),

                                    //FundingRecipientId = row.Field<long>("FundingRecipientId"),
                                    //FundingRecipientName = row.Field<string>("FundingRecipientName"),
                                    //FundingRecipientOrgName = row.Field<string>("FundingRecipientOrgName"),

                                    //FundingRecipientCountryName = row.Field<string>("FundingRecipientCountryName"),
                                    //FundingRecipientCountryId = row.Field<long>("FundingRecipientCountryId"),

                                    StartDate = row.Field<string>("StartDate"),
                                    EndDate = row.Field<string>("EndDate"),

                                    ReportingFrequency = row.Field<string>("ReportingFrequency"),


                                    TotalMetrics = row.Field<long?>("TotalMetrics") ?? 0,
                                    PageNo = row.Field<int>("PageNo"),
                                    SkippedRows = row.Field<int>("SkippedRows"),
                                    TotalRows = row.Field<int>("TotalRows"),
                                    ItemsPerPage = row.Field<int>("ItemsPerPage"),

                                    //MetricCategories = row.Field<string>("MetricCategories"),
                                    //MetricCategoryList = row.Field<string>("MetricCategoryList"),
                                    //StartDate = row.Field<string>("StartDate"),
                                    //EndDate = row.Field<string>("EndDate"),
                                    //ReportingFrequency = row.Field<string>("ReportingFrequency"),
                                    //TotalFunding = row.Field<long>("TotalFunding"),
                                    //CreatedBy = row.Field<long>("CreatedBy"),
                                    //CreatedOnUtc = row.Field<string>("CreatedOnUtc"),
                                    //UpdatedBy = row.Field<long>("UpdatedBy"),
                                    //UpdatedOnUtc = row.Field<string>("UpdatedOnUtc"),
                                }
                            );
                        }

                        List<MetricDto> metrics = new List<MetricDto>();
                        if (ds.Tables.Count > 1)
                        {
                            DataRowCollection metricRows = ds.Tables[1].Rows;

                            foreach (DataRow row in metricRows)
                            {
                                metrics.Add(new MetricDto()
                                {
                                    MetricId = row.Field<long>("MetricId").ToString(),
                                    MetricCode = row.Field<string>("MetricCode"),
                                    MetricName = row.Field<string>("MetricName"),
                                    IsTopLevel = row.Field<bool>("IsTopLevel"),
                                    ParentMetricId = row.Field<string>("ParentMetricId"),
                                    //CategoryColor = row.Field<string?>("CategoryColor") ?? String.Empty,
                                    //CategoryCode = row.Field<string?>("CategoryCode") ?? String.Empty,
                                    //CategoryName = row.Field<string?>("CategoryCode") ?? String.Empty,
                                    Description = row.Field<string>("Description"),
                                });
                            }
                        }

                        result[0].Metrics = metrics;
                    }
                }
            });

            return result;
        }

        public async Task<List<NewReviewsDto>> BulkLaunchReviews(List<long> impactIds, long? quarter, long userId)
        {
            List<NewReviewsDto> result = new List<NewReviewsDto>();
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.LaunchReviews,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;


            string impactIdsStr = String.Empty;

            foreach (long impactId in impactIds)
            {
                if (impactIdsStr != "")
                {
                    impactIdsStr += ", ";
                }

                impactIdsStr += impactId.ToString().Trim();
            }

            command.Parameters.AddWithValue("@ImpactMasterIds", impactIdsStr);
            command.Parameters.AddWithValue("@UserId", userId);

            if (quarter != null)
            {
                command.Parameters.AddWithValue("@Quarter", quarter);
            }

            //command.Parameters.AddWithValue("quarter", quarter);

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataRowCollection rows = ds.Tables[0].Rows;

                    foreach (DataRow row in rows)
                    {
                        try
                        {
                            result.Add(
                                new NewReviewsDto()
                                {
                                    ReviewId = row.Field<long>("ReviewId"),
                                    ImpactMasterId = row.Field<long>("ImpactMasterId"),
                                    ProjectTitle = row.Field<string>("ProjectTitle"),
                                    ProjectPin = row.Field<string>("ProjectPin"),
                                    Quarter = row.Field<string>("Quarter"),
                                    RecordType = row.Field<string>("RecordType")
                                }
                            );

                        }

                        catch(Exception e) {
                            result.Add(
                              new NewReviewsDto()
                              {
                                  ProjectTitle = row.Field<string>("Message"),
                              });
                        }
                    }
                }
            });

            return result;
        }

        public async Task<SharepointHierarchyFolderCreationDto> GetTopicWiseMetricsForReview(long reviewId)
        {
            List<SharepointHierarchyFolderCreationDto> result = new List<SharepointHierarchyFolderCreationDto>();
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetTopicWiseMetricsForReview,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;


            string impactIdsStr = String.Empty;

            command.Parameters.AddWithValue("@ReviewId", reviewId);
            //command.Parameters.AddWithValue("quarter", quarter);

            Dictionary<string, List<string>> topicMetricsMap = new Dictionary<string, List<string>>();
            Dictionary<string, SharepointHierarchyFolderCreationL1TopTopic> L1MetricCodes = new Dictionary<string, SharepointHierarchyFolderCreationL1TopTopic>();
            List<string> allCategoryCodes = new List<string>();
            long? impactMasterId = null;
            string title = String.Empty;
            string projectPin = String.Empty;
            string quarter = String.Empty;


            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables == null || ds.Tables.Count <= 0)
                {
                    return;
                }

                DataTable dataTable = ds.Tables[0];
                if (dataTable == null)
                {
                    return;
                }

                string categoryCode = String.Empty;
                List<string> metricCodes = new List<string>();

                DataRowCollection rows = dataTable.Rows;
                if (rows == null || rows.Count <= 0)
                {
                    return;
                }

                bool categoryExists = false;


                Dictionary<string, L1MetricCodeEvidenceTypeCodeDto> l1MetricCodeEvidenceTypeCodes = new Dictionary<string, L1MetricCodeEvidenceTypeCodeDto>();
                
                foreach (DataRow row in rows)
                {
                    string metricCode=  row.Field<string>("MetricCode");
                    string parentMetricCode =  row.Field<string>("ParentMetricCode");
                    string evidenceTypeCode =  row.Field<string>("EvidenceTypeCode");

                    L1MetricCodeEvidenceTypeCodeDto obj = new L1MetricCodeEvidenceTypeCodeDto() { MetricCode = metricCode, ParentMetricCode = parentMetricCode, EvidenceTypeCodes = new List<string>() };

                    if (l1MetricCodeEvidenceTypeCodes.ContainsKey(metricCode))
                    {
                        obj = l1MetricCodeEvidenceTypeCodes[metricCode];
                    }
                    if (parentMetricCode != null && l1MetricCodeEvidenceTypeCodes.ContainsKey(parentMetricCode))
                    {
                        obj = l1MetricCodeEvidenceTypeCodes[parentMetricCode];
                    }

                    obj.EvidenceTypeCodes.Add(evidenceTypeCode);

                    l1MetricCodeEvidenceTypeCodes[metricCode] = obj;
                }



                foreach (DataRow row in rows)
                {
                    categoryCode = row.Field<string>("CategoryCode");
                    string metricCode = row.Field<string>("metricCode");

                    if (impactMasterId == null)
                    {
                        impactMasterId = row.Field<long>("ImpactMasterId");
                    }

                    if (String.IsNullOrEmpty(title))
                    {
                        title = row.Field<string>("Title");
                    }

                    if (String.IsNullOrEmpty(projectPin))
                    {
                        projectPin = row.Field<string>("ProjectPin");
                    }

                    if (String.IsNullOrEmpty(quarter))
                    {
                        quarter = row.Field<string>("quarter");
                    }

                    if (topicMetricsMap.ContainsKey(categoryCode))
                    {
                        categoryExists = true;
                        metricCodes = topicMetricsMap.GetValueOrDefault(categoryCode);
                    }
                    else
                    {
                        categoryExists = false;
                        metricCodes = new List<string>();
                    }

                    metricCodes.Add(metricCode);



                    // Top-L1
                    if (row.Field<bool>("IsTopLevel"))
                    {
                        // Top level
                        string parentMetricCode = row.Field<string>("MetricCode");
                        if (!L1MetricCodes.ContainsKey(parentMetricCode))
                        {
                            L1MetricCodes.Add(parentMetricCode, new SharepointHierarchyFolderCreationL1TopTopic() { ParentMetricCode = parentMetricCode, CategoryCode = row.Field<string> ("CategoryCode") });
                        }
                    }
                    else
                    {
                        // L1
                        string parentMetricCode = row.Field<string>("ParentMetricCode");
                        L1MetricCodeEvidenceTypeCodeDto l1MetricCode = new L1MetricCodeEvidenceTypeCodeDto()
                        {
                            MetricCode = row.Field<string>("MetricCode")
                            , ParentMetricCode = row.Field<string>("ParentMetricCode") ?? ""
                            , EvidenceTypeCodes = l1MetricCodeEvidenceTypeCodes[metricCode]?.EvidenceTypeCodes ?? l1MetricCodeEvidenceTypeCodes[parentMetricCode]?.EvidenceTypeCodes ?? new List<string>()
                        };
                        
                        List<L1MetricCodeEvidenceTypeCodeDto> l1MetricCodesList = new List<L1MetricCodeEvidenceTypeCodeDto>();

                        if (L1MetricCodes.ContainsKey(parentMetricCode))
                        {
                            l1MetricCodesList = L1MetricCodes[parentMetricCode].L1MetricCodes;
                        }

                        l1MetricCodesList.Add(l1MetricCode);

                        L1MetricCodes.Remove(parentMetricCode);

                        L1MetricCodes.Add(parentMetricCode, new SharepointHierarchyFolderCreationL1TopTopic() { ParentMetricCode = parentMetricCode, CategoryCode = row.Field<string>("CategoryCode"), L1MetricCodes = l1MetricCodesList });
                    }




                    if (categoryExists)
                    {
                        topicMetricsMap.Remove(categoryCode);
                    }

                    topicMetricsMap.Add(categoryCode, metricCodes);
                }

                allCategoryCodes = topicMetricsMap.Keys.ToList();
            });


            List<string> metricCodes = new List<string>();
            List<SharepointHierarchyFolderCreationTopicDto> topics = new List<SharepointHierarchyFolderCreationTopicDto>();

            foreach (string categoryCode in allCategoryCodes)
            {
                try
                {
                    topicMetricsMap.TryGetValue(categoryCode, out metricCodes);
                    topics.Add(
                        new SharepointHierarchyFolderCreationTopicDto()
                        {
                            TopicName = categoryCode
                             ,
                            TopLevelMetricCodes = metricCodes
                        }
                    );
                }
                catch (Exception e)
                {

                }
            }

            return new SharepointHierarchyFolderCreationDto()
            {
                ImpactMasterId = impactMasterId ?? 0
                ,
                ProjectPin = projectPin
                ,
                Quarter = quarter
                ,
                Topics = topics
                , 
                Title = title
                ,
                L1MetricCodes = L1MetricCodes
            };
        }

        public async Task<bool> SaveDataCollectionUrls(List<ReviewDataCollectionUrlUpdateDto> dataCollectionUrls, long userId)
        {
            bool success = false;
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.SaveDataCollectionUrls,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;



            command.Parameters.AddWithValue("@DataCollectionUrlsJson", JsonSerializer.Serialize(dataCollectionUrls));
            command.Parameters.AddWithValue("@UserId", userId);
            //command.Parameters.AddWithValue("quarter", quarter);

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];

                    if (row.Field<int?>("Data")! > 0)
                    {
                        success = true;
                    }
                }
            });

            return success;
        }

        public async Task<bool> UpdateBaselineForMetric(long reviewId, List<UpdateReviewMetricsValuesDto> input, long UserId)
        {
            bool success = false;
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.UpdateBaselineForReviewMetric,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;


            string updateReviewMetricJSON = JsonSerializer.Serialize(input);
            command.Parameters.AddWithValue("@ReviewId", reviewId);
            command.Parameters.AddWithValue("@UserId", UserId);
            command.Parameters.AddWithValue("@updateReviewMetricJSON", updateReviewMetricJSON);
            //command.Parameters.AddWithValue("quarter", quarter);

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];

                    if (row.Field<int?>("Data")! > 0)
                    {
                        success = true;
                    }
                }
            });

            return success;
        }

        public async Task<bool> RestatementUpdateBaselineForMetric(long reviewId, List<UpdateReviewMetricsValuesDto> input, long UserId)
        {
            bool success = false;
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.RestatementUpdateBaselineForReviewMetric,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;



            command.Parameters.AddWithValue("@ReviewId", reviewId);
            command.Parameters.AddWithValue("@UserId", UserId);
            command.Parameters.AddWithValue("@updateReviewMetricJSON", JsonSerializer.Serialize(input));
            //command.Parameters.AddWithValue("quarter", quarter);

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];

                    if (row.Field<int?>("Data")! > 0)
                    {
                        success = true;
                    }
                }
            });

            return success;
        }

        public async Task<ReviewMetricDetailsForEvidenceUploadDto> GetReviewMetricDetailsForUpload(long reviewId, long metricId, long? evidenceTypeId)
        {
            ReviewMetricDetailsForEvidenceUploadDto result = new ReviewMetricDetailsForEvidenceUploadDto();
            bool success = false;
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetReviewMetricDetailsForUpload,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;


            command.Parameters.AddWithValue("@ReviewId", reviewId);
            command.Parameters.AddWithValue("@MetricId", metricId);
            command.Parameters.AddWithValue("@EvidenceTypeId", evidenceTypeId);
            //command.Parameters.AddWithValue("quarter", quarter);

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow metricDetailsRow = ds.Tables[0].Rows[0];

                    result = new ReviewMetricDetailsForEvidenceUploadDto()
                    {
                        ProjectPin = metricDetailsRow.Field<string>("ProjectPin"),
                        Quarter = metricDetailsRow.Field<string>("Quarter"),
                        CategoryCode = metricDetailsRow.Field<string>("CategoryCode"),
                        EvidenceTypeCode = metricDetailsRow.Field<string?>("EvidenceTypeCode"),
                        MetricCode = metricDetailsRow.Field<string>("MetricCode"),
                        ParentMetricCode = metricDetailsRow.Field<string>("ParentMetricCode"),
                    };
                }
            });

            return result;
        } 

        public async Task<List<ApiResponse<long>>> SaveReviewMetricEvidences(long? reviewId, long? userId, List<ReviewMetricEvidencesJsonDto> reviewMetricEvidencesJsonInput)
        {
            List<ApiResponse<long>> result = new List<ApiResponse<long>>();
            bool success = false;
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.SaveReviewMetricEvidences,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            if (reviewId != null)
            {
                command.Parameters.AddWithValue("@ReviewId", reviewId);
            }

            if (userId != null)
            {
                command.Parameters.AddWithValue("@UserId", userId);
            }

            if (reviewMetricEvidencesJsonInput != null)
            {
                string reviewMetricEvidencesJson = JsonSerializer.Serialize(reviewMetricEvidencesJsonInput);
                command.Parameters.AddWithValue("@reviewMetricEvidencesJson", reviewMetricEvidencesJson);
            }

            //command.Parameters.AddWithValue("quarter", quarter);

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];

                    result.Add(
                        new ApiResponse<long>()
                        {
                            Data = row.Field<long>("Data"),
                            Message = row.Field<string>("Message")
                        }
                    );
                }
            });

            return result;
        }

        public async Task<List<ApiResponse<string>>> SaveReviewMetricConfig(long reviewId, List<string> metricIds, long userId, string deletedItemIdKeyword)
        {
            List<ApiResponse<string>> result = new List<ApiResponse<string>>();
            bool success = false;
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.SaveReviewMetricConfig,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            string metricIdStr = String.Empty;

            foreach (string metricId in metricIds)
            {
                if (!String.IsNullOrEmpty(metricIdStr.Trim()) && !String.IsNullOrEmpty(metricId.Trim()))
                {
                    metricIdStr += ", ";
                }

                metricIdStr += metricId;
            }


            command.Parameters.AddWithValue("@ReviewId", reviewId);
            command.Parameters.AddWithValue("@metricIds", (metricIdStr));
            command.Parameters.AddWithValue("@UserId", userId);
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
                        try
                        {
                            string data = row.Field<long>("Data").ToString();
                            result.Add(
                                new ApiResponse<string>()
                                {
                                    Data = data,
                                    Message = row.Field<string>("Message")
                                }
                            );
                        }
                        catch (Exception e)
                        {
                            result.Add(
                                new ApiResponse<string>()
                                {
                                    Data = "0",
                                    Message = row.Field<string>("Message")
                                }
                            );

                        }
                    }

                    if (ds.Tables[1] != null)
                    {
                        DataRowCollection deleteItemIdRows = ds.Tables[1].Rows;
                        //const string /*deletedItemIdKeyword*/ = "deleteFromSharepoint";

                        foreach(DataRow deleteItemIdRow in deleteItemIdRows)
                        {
                            result.Add(
                                new ApiResponse<string>()
                                {
                                    Data = deleteItemIdRow.Field<string>("ItemId"),
                                    Message = deletedItemIdKeyword
                                }    
                            );
                        }
                    }
                }
            });

            return result;
        }

        public async Task<List<ApiResponse<long>>> DeleteReviewMetricEvidence(string itemId, long userId)
        {
            List<ApiResponse<long>> result = new List<ApiResponse<long>>();
            bool success = false;
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.DeleteReviewMetricEvidence,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            string metricIdStr = String.Empty;


            command.Parameters.AddWithValue("@ItemId", itemId);
            command.Parameters.AddWithValue("@UserId", userId);
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
                        try
                        {
                            result.Add(
                                new ApiResponse<long>()
                                {
                                    Data = row.Field<dynamic>("Data") == 1 ? 1 : 0,
                                    Message = row.Field<string>("Message"),
                                    Success = true
                                });
                        }
                        catch (Exception e)
                        {
                            result.Add(
                                new ApiResponse<long>()
                                {
                                    Data = 0,
                                    Message = row.Field<string>("Message"),
                                    Success = !true
                                });

                        }
                    }
                }
            });

            return result;
        }

        public async Task<List<BaselineRemainderDto>> GetBaselineRemainder(long reviewId)
        {
            List<BaselineRemainderDto> result = new List<BaselineRemainderDto>();
            bool success = false;
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetBaselineRemainder,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            string metricIdStr = String.Empty;


            command.Parameters.AddWithValue("@ReviewId", reviewId);
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
                        try
                        {
                            result.Add(
                                new BaselineRemainderDto()
                                {
                                    Baseline = row.Field<string>("Baseline"),
                                    ReviewId = row.Field<long>("ReviewId"),
                                    MetricId = row.Field<long>("MetricId"),
                                    MetricCode = row.Field<string>("MetricCode"),
                                    ImpactMasterId = row.Field<long>("ImpactMasterId"),
                                    Quarter = row.Field<byte?>("Quarter"),
                                    ReviewIdMetricIdQuarter = row.Field<string>("ReviewIdMetricIdQuarter"),
                                    BaselineSum = row.Field<decimal?>("BaselineSum"),
                                    ImmBaseline = row.Field<decimal?>("ImmBaseline"),

                                });
                        }
                        catch (Exception e)
                        {


                        }
                    }
                }
            });

            return result;
        }

        public async Task<ReviewStagePromotionResultDto> ApproveRejectReviewMetric(List<ReviewMetricApproveRejectDto> metricActionJSONInput, long userId)
        {
            List<StagePromotionDetailDto> stagePromotionsDetails = new List<StagePromotionDetailDto>();
            List<ReviewPromotionNextStageDetailsDto> reviewPromotionNextStageDetails = new List<ReviewPromotionNextStageDetailsDto>();


            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.ApproveRejectReviewMetric,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            string metricIdStr = String.Empty;

            string metricActionJSON = JsonSerializer.Serialize(metricActionJSONInput);
            command.Parameters.AddWithValue("@metricActionJSON", metricActionJSON);
            command.Parameters.AddWithValue("@UserId", userId);
            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataRowCollection approvedCountRows = ds.Tables[0].Rows;

                        foreach (DataRow row in approvedCountRows)
                        {
                            try
                            {
                                stagePromotionsDetails.Add(
                                    new StagePromotionDetailDto()
                                    {
                                        ReviewId = row.Field<long>("ReviewId"),
                                        //ApprovedCount = row.Field<long>("ApprovedCount"),
                                        //TotalCount = row.Field<long>("TotalCount"),
                                        //PromotionEligible = row.Field<bool>("PromotionEligible"),
                                        //Decision = row.Field<string>("Decision"),

                                        //StatusId = row.Field<long?>("StatusId"),
                                        //Action = row.Field<string?>("Action"),
                                    }
                                );
                            }

                            catch (Exception e)
                            {

                            }

                        }

                    }

                    if (ds.Tables.Count > 1)
                    {
                        DataRowCollection reviewPromotionNextStageDetailsRows = ds.Tables[1].Rows;
                        foreach (DataRow row in reviewPromotionNextStageDetailsRows)
                        {
                            reviewPromotionNextStageDetails.Add(
                                new ReviewPromotionNextStageDetailsDto()
                                {
                                    ReviewId = row.Field<long>("ReviewId"),
                                    PrevStageCode = row.Field<int?>("PrevStageCode"),
                                    PrevStageName = row.Field<string?>("PrevStageName"),
                                    CurrentStageCode = row.Field<int?>("CurrentReviewHeaderStageCode"),
                                    NextStageCode = row.Field<int?>("ToBePromotedToStageCode"),
                                    NextStageName = row.Field<string?>("NextStageName"),
                                    PromotionEligible = row.Field<bool>("PromotionEligible"),
                                }
                            );
                        }
                    }
                }
            });

            return new ReviewStagePromotionResultDto()
            {
                stagePromotionDetails = stagePromotionsDetails
                ,
                reviewPromotionNextStageDetails = reviewPromotionNextStageDetails
            };
        }

        public async Task<List<ReviewApprovalProgressDto>> GetReviewApprovalByMetricCode(long reviewId, string metricCode, long userId)
        {
            List<ReviewApprovalProgressDto> result = new List<ReviewApprovalProgressDto>();


            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetReviewApprovalByMetricCode,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            string metricIdStr = String.Empty;


            command.Parameters.AddWithValue("@reviewId", (reviewId));
            command.Parameters.AddWithValue("@metricCode", metricCode);
            command.Parameters.AddWithValue("@UserId", userId);
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
                        try
                        {
                            result.Add(
                                new ReviewApprovalProgressDto()
                                {
                                    ReviewId = row.Field<long?>("ReviewId"),
                                    MetricCode = row.Field<string?>("MetricCode"),
                                    Stage = row.Field<int?>("Stage").ToString(),
                                    StageDisplayName = row.Field<string?>("StageDisplayName"),
                                    Status = row.Field<string?>("Status"),

                                    ApprovedBy = row.Field<string?>("ApprovedBy"),
                                    ApprovedOnUTC = row.Field<string?>("ApprovedOnUTC"),
                                    ApprovalComment = row.Field<string?>("ApprovalComment"),

                                    RejectedBy = row.Field<string?>("RejectedByName"),
                                    RejectedOnUTC = row.Field<string?>("RejectedOnUTC"),
                                    RejectionComment = row.Field<string?>("RejectionComment"),

                                    AssigneeUserId = row.Field<long?>("AssigneeUserId"),
                                    AssigneeUserName = row.Field<string?>("AssigneeUserName"),


                                    RecallDoneById = row.Field<long?>("RecallDoneById"),
                                    RecallDoneByName = row.Field<string?>("RecallDoneByName"),
                                    RecallComment = row.Field<string?>("RecallComment"),
                                    RecallOnUtc = row.Field<string?>("RecallOnUtc"),
                                    //TaskReassignedBy = row.Field<long?>("TaskReassignedBy"),
                                    //TaskReassignedByName = row.Field<string?>("TaskReassignedByName"),
                                    //TaskReassignedOn = row.Field<string?>("TaskReassignedOn") ?? "",

                                    //UpdatedOnUtc = row.Field<string?>("UpdatedOnUtc") ?? "",
                                    //Comment = row.Field<string?>("Comment"),
                                }
                            );
                        }

                        catch (Exception e)
                        {

                        }

                    }
                }
            });

            return result;
        }

        public async Task<List<TopLevelMetricAggregationDto>> GetToplevelAggregation(string topicName, string periodCode, string applicablestages, long userId)
        {
            List<TopLevelMetricAggregationDto> result = new List<TopLevelMetricAggregationDto>();


            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetToplevelAggregation,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            string metricIdStr = String.Empty;


            command.Parameters.AddWithValue("@topicName", topicName);
            command.Parameters.AddWithValue("@periodCode", periodCode);
            command.Parameters.AddWithValue("@breakdowntype", "toplevel");
            command.Parameters.AddWithValue("@applicablestages", applicablestages);
            command.Parameters.AddWithValue("@UserId", userId);
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
                        try
                        {
                            result.Add(
                                new TopLevelMetricAggregationDto()
                                {
                                    ParentCode = row.Field<string>("ParentCode"),
                                    ParentName = row.Field<string>("ParentName"),
                                    RawValue = row.Field<decimal>("RawValue"),
                                    Evidenced = row.Field<decimal>("Evidenced"),
                                    AdjustedValue = row.Field<decimal>("AdjustedValue"),
                                    Unit = row.Field<string>("Unit"),
                                }
                            );
                        }

                        catch (Exception e)
                        {

                        }

                    }
                }
            });

            return result;
        }

        public async Task<List<ProjectLevelAggregationDto>> GetProjectMetricAggregation(string topicName, string periodCode, string applicablestages, long userId)
        {
            List<ProjectLevelAggregationDto> result = new List<ProjectLevelAggregationDto>();


            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetToplevelAggregation,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            string metricIdStr = String.Empty;


            command.Parameters.AddWithValue("@topicName", topicName);
            command.Parameters.AddWithValue("@periodCode", periodCode);
            command.Parameters.AddWithValue("@breakdowntype", "project");
            command.Parameters.AddWithValue("@applicablestages", applicablestages);
            command.Parameters.AddWithValue("@UserId", userId);
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
                        try
                        {
                            result.Add(
                                new ProjectLevelAggregationDto()
                                {
                                    Title = row.Field<string>("Title"),
                                    ImpactMasterId = row.Field<long>("ImpactMasterId"),
                                    RecordTypeId = row.Field<long>("RecordTypeId"),
                                    ParentCode = row.Field<string>("ParentCode"),
                                    ReviewId = row.Field<long>("ReviewId"),
                                    Pin = row.Field<string>("Pin"),
                                    RawValue = row.Field<decimal>("RawValue"),
                                    AdjustedValue = row.Field<decimal>("AdjustedValue"),
                                    Evidenced = row.Field<decimal>("Evidenced"),
                                    Breakdown = row.Field<string>("Breakdown"),
                                    Total = row.Field<int>("Total"),
                                }
                            );
                        }

                        catch (Exception e)
                        {

                        }

                    }
                }
            });

            return result;
        }

        public async Task<List<ReviewMetricCodeForAdminL4ApprovalRejectionDto>> GetReviewMetricCodesForAdminOrL4Approval(string topicName, string periodCode, List<ProjectPinParentCodeDto> projectPinParentCodes)
        {
            List<ReviewMetricCodeForAdminL4ApprovalRejectionDto> result = new List<ReviewMetricCodeForAdminL4ApprovalRejectionDto>();


            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetToplevelAggregation,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            string metricIdStr = String.Empty;


            command.Parameters.AddWithValue("@topicName", topicName);
            command.Parameters.AddWithValue("@periodCode", periodCode);
            command.Parameters.AddWithValue("@breakdowntype", "reviewmetriccode");
            command.Parameters.AddWithValue("@projectPinParentCodes", JsonSerializer.Serialize(projectPinParentCodes));
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
                        try
                        {
                            result.Add(
                                new ReviewMetricCodeForAdminL4ApprovalRejectionDto()
                                {
                                    ReviewId = row.Field<long>("ReviewId"),
                                    MetricCode = row.Field<string>("MetricCode"),
                                    ParentCode = row.Field<string>("ParentCode"),
                                    Total = row.Field<int>("Total"),
                                }
                            );
                        }

                        catch (Exception e)
                        {

                        }

                    }
                }
            });

            return result;
        }

        public async Task<(IReadOnlyList<ReviewApprovalChangeRepositoryRowDto> DisablePreview, IReadOnlyList<ReviewApprovalChangeRepositoryRowDto> EnablePreview)>
            GetApprovalChangesAsync(
                IReadOnlyList<ReviewStageApprovalChangeRequestDto> stageChanges,
                CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(stageChanges.Select(change => new
            {
                stageCode = change.StageCode,
                isActive = change.IsActive
            }));

            var disablePreview = new List<ReviewApprovalChangeRepositoryRowDto>();
            var enablePreview = new List<ReviewApprovalChangeRepositoryRowDto>();

            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetApprovalChanges,
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@StageCodesJson", SqlDbType.NVarChar, -1).Value = json;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                disablePreview.Add(ReadApprovalChangeRow(reader, hasStatusId: true));
            }

            if (!await reader.NextResultAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "Review stage approval preview did not return the enable result set.");
            }

            while (await reader.ReadAsync(cancellationToken))
            {
                enablePreview.Add(ReadApprovalChangeRow(reader, hasStatusId: false));
            }

            return (disablePreview, enablePreview);
        }

        private static ReviewApprovalChangeRepositoryRowDto ReadApprovalChangeRow(
            SqlDataReader reader,
            bool hasStatusId)
        {
            return new ReviewApprovalChangeRepositoryRowDto(
                reader.GetInt32(reader.GetOrdinal("StageCode")),
                reader.GetString(reader.GetOrdinal("StageName")),
                GetNullableInt64(reader, "ReviewApprovalId"),
                GetInt64(reader, "ReviewId"),
                reader.GetString(reader.GetOrdinal("ReviewTitle")),
                GetInt64(reader, "MetricId"),
                reader.GetString(reader.GetOrdinal("MetricCode")),
                reader.GetString(reader.GetOrdinal("MetricName")),
                hasStatusId ? GetNullableInt32(reader, "StatusId") : null,
                hasStatusId ? GetNullableString(reader, "StatusCode") : null,
                GetNullableString(reader, "CurrentStatus"),
                GetNullableInt64(reader, "AssigneeUserId"),
                GetNullableInt32(reader, "StepNo"),
                GetNullableInt32(reader, "AttemptNo"),
                reader.GetString(reader.GetOrdinal("Action")));
        }

        private static long GetInt64(SqlDataReader reader, string column)
        {
            return Convert.ToInt64(reader.GetValue(reader.GetOrdinal(column)));
        }

        private static long? GetNullableInt64(SqlDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? null : Convert.ToInt64(reader.GetValue(ordinal));
        }

        private static int? GetNullableInt32(SqlDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? null : Convert.ToInt32(reader.GetValue(ordinal));
        }

        private static string? GetNullableString(SqlDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        private static bool? GetNullableBoolean(SqlDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? null : reader.GetBoolean(ordinal);
        }

        public async Task<long> UpdateReviewApprovalAssignee(
          UpdateReviewApprovalAssigneeDto input)
        {
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.UpdateReviewApprovalAssignee,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@ReviewId", input.ReviewId);
            command.Parameters.AddWithValue("@ReviewStageCode", input.ReviewStageCode);
            command.Parameters.AddWithValue("@MetricCode", input.MetricCode);
            command.Parameters.AddWithValue("@CurrentAssigneeUserId", input.CurrentAssigneeUserId);
            command.Parameters.AddWithValue("@NewAssigneeUserId", input.NewAssigneeUserId);
            command.Parameters.AddWithValue("@UpdatedBy", input.UpdatedBy);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                throw new InvalidOperationException(
                    "Review approval reassignment did not return the old assignee.");
            }

            return reader.GetInt64(
                reader.GetOrdinal("OldAssigneeUserId"));
        }

        public async Task<List<RecallTopLevelMetricResultDto>> RecallToplevelMetric(RecallTopLevelMetricInput input)
        {
            List<RecallTopLevelMetricResultDto> result = new List<RecallTopLevelMetricResultDto>();


            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.RecallTopLevelMetric,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            string metricIdStr = String.Empty;


            command.Parameters.AddWithValue("@topLevelMetricCode", input.TopLevelMetricCode);
            command.Parameters.AddWithValue("@ReviewId", input.ReviewId);
            command.Parameters.AddWithValue("@RecallComment", input.Recallcomment);
            command.Parameters.AddWithValue("@UserId", input.UserId);
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
                        result.Add(
                            new RecallTopLevelMetricResultDto()
                            {
                                PreviousStageCode = row.Field<long?>("PreviousStageCode"),
                                NewStageCode = row.Field<long?>("NewStageCode"),
                                Message = row.Field<string>("Message")
                            }

                        );
                    }
                }
            });

            return result;
        }

        public async Task<List<ApiResponse<long>>> IssueRestatementForReview(StartEndRestatementDto input)
        {
            List<ApiResponse<long>> result = new List<ApiResponse<long>>();
            bool success = false;
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.IssueRestatementForReview,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            if (input.ReviewId != null)
            {
                command.Parameters.AddWithValue("@ReviewId", input.ReviewId);
            }

            if (input.UserId != null)
            {
                command.Parameters.AddWithValue("@UserId", input.UserId);
            }

            command.Parameters.AddWithValue("@Start", input.Start);
            command.Parameters.AddWithValue("@Comment", input.Comment);
            command.Parameters.AddWithValue("@Metrics", JsonSerializer.Serialize(input.Metrics));

            //command.Parameters.AddWithValue("quarter", quarter);

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];

                    result.Add(
                        new ApiResponse<long>()
                        {
                            Data = row.Field<long>("Data"),
                            Message = row.Field<string>("Message")
                        }
                    );
                }
            });

            return result;
        }


        public async Task<List<ApiResponse<long>>> UpdateReviewMetricFRComment(UpdateReviewMetricFRCommentDto input)
        {
            List<ApiResponse<long>> result = new List<ApiResponse<long>>();
            bool success = false;
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.UpdateReviewMetricFRComment,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@ReviewId", input.ReviewId);
            command.Parameters.AddWithValue("@UserId", input.UserId);
            command.Parameters.AddWithValue("@Comment", input.Comment);

            if (input.MetricCode != null)
            {
                command.Parameters.AddWithValue("@MetricCode", input.MetricCode);
            }

            if (input.MetricId != null)
            {
                command.Parameters.AddWithValue("@MetricId", input.MetricId);
            }


            //command.Parameters.AddWithValue("quarter", quarter);

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];

                    result.Add(
                        new ApiResponse<long>()
                        {
                            Data = row.Field<long>("Data"),
                            Message = row.Field<string>("Message")
                        }
                    );
                }
            });

            return result;
        }

        public async Task<List<LaunchableProjectDto>> GetImpactsWithActiveReviews(int pageNo, int skippedRows, int itemsPerPage, string quarter, string status, string impactName)
        {
            List<LaunchableProjectDto> result = new List<LaunchableProjectDto>();
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetImpactsWithActiveReviews,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@pageNo", pageNo);
            command.Parameters.AddWithValue("@skippedRows", skippedRows);
            command.Parameters.AddWithValue("@itemsPerPage", itemsPerPage);
            command.Parameters.AddWithValue("@status", status);
            command.Parameters.AddWithValue("@impactName", impactName);

            if (!String.IsNullOrEmpty(quarter))
            {
                try
                {
                    command.Parameters.AddWithValue("@quarter", int.Parse(quarter));
                }
                catch(Exception e)
                {

                }

            }

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dataTable = ds.Tables[0];

                    DataRowCollection rows = dataTable.Rows;

                    if (rows.Count > 0)
                    {
                        foreach (DataRow row in rows)
                        {
                            result.Add
                            (
                                new LaunchableProjectDto()
                                {
                                    ReviewId = row.Field<long?>("ReviewId"),
                                    ImpactMasterId = row.Field<long?>("ImpactMasterId"),
                                    Pin = row.Field<string>("Pin"),
                                    Title = row.Field<string>("Title"),
                                    L1UserId = row.Field<long?>("L1UserId"),
                                    L1UserName = row.Field<string>("L1UserName"),
                                    RegionId = row.Field<long?>("RegionId"),
                                    ProgramId = row.Field<long?>("ProgramId"),
                                    RegionName = row.Field<string?>("RegionName"),
                                    ProgramName = row.Field<string?>("ProgramName"),
                                    StatusId = row.Field<long?>("StatusId"),
                                    StatusCode = row.Field<string>("StatusCode"),
                                    StatusName = row.Field<string>("StatusName"),
                                    LastReviewStage = row.Field<string>("LastReviewStage"),
                                    ReviewStatus = row.Field<string?>("ReviewStatus"),

                                    AllowDeletion = row.Field<bool?>("AllowDeletion") ?? false,
                                    DeletionPreventionReason = row.Field<string?>("DeletionPreventionReason") ?? "",
                                    //FundingRecipientId = row.Field<long>("FundingRecipientId"),
                                    //FundingRecipientName = row.Field<string>("FundingRecipientName"),
                                    //FundingRecipientOrgName = row.Field<string>("FundingRecipientOrgName"),

                                    //FundingRecipientCountryName = row.Field<string>("FundingRecipientCountryName"),
                                    //FundingRecipientCountryId = row.Field<long>("FundingRecipientCountryId"),

                                    StartDate = row.Field<string>("StartDate"),
                                    EndDate = row.Field<string>("EndDate"),

                                    ReportingFrequency = row.Field<string>("ReportingFrequency"),


                                    TotalMetrics = row.Field<int?>("TotalMetrics") ?? 0,
                                    PageNo = row.Field<int>("PageNo"),
                                    SkippedRows = row.Field<int>("SkippedRows"),
                                    TotalRows = row.Field<int>("TotalRows"),
                                    ItemsPerPage = row.Field<int>("ItemsPerPage"),

                                    //MetricCategories = row.Field<string>("MetricCategories"),
                                    //MetricCategoryList = row.Field<string>("MetricCategoryList"),
                                    //StartDate = row.Field<string>("StartDate"),
                                    //EndDate = row.Field<string>("EndDate"),
                                    //ReportingFrequency = row.Field<string>("ReportingFrequency"),
                                    //TotalFunding = row.Field<long>("TotalFunding"),
                                    //CreatedBy = row.Field<long>("CreatedBy"),
                                    //CreatedOnUtc = row.Field<string>("CreatedOnUtc"),
                                    //UpdatedBy = row.Field<long>("UpdatedBy"),
                                    //UpdatedOnUtc = row.Field<string>("UpdatedOnUtc"),
                                }
                            );
                        }

                        List<MetricDto> metrics = new List<MetricDto>();
                        if (ds.Tables.Count > 1)
                        {
                            DataRowCollection metricRows = ds.Tables[1].Rows;

                            foreach (DataRow row in metricRows)
                            {
                                metrics.Add(new MetricDto()
                                {
                                    MetricId = row.Field<long>("MetricId").ToString(),
                                    MetricCode = row.Field<string>("MetricCode"),
                                    MetricName = row.Field<string>("MetricName"),
                                    IsTopLevel = row.Field<bool>("IsTopLevel"),
                                    ParentMetricId = row.Field<string>("ParentMetricId"),
                                    //CategoryColor = row.Field<string?>("CategoryColor") ?? String.Empty,
                                    //CategoryCode = row.Field<string?>("CategoryCode") ?? String.Empty,
                                    //CategoryName = row.Field<string?>("CategoryCode") ?? String.Empty,
                                    Description = row.Field<string>("Description"),
                                });
                            }
                        }

                        result[0].Metrics = metrics;
                    }
                }
            });

            return result;
        }

        public async Task<List<string>> GetAllMetricsWithValorisationRequired()
        {
            var result = new List<string>();

            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetAllMetricsWithValorisationRequired,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;


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
                        result.Add(row.Field<string>("MetricCode"));
                    }

                }
            });

            return result;
        }

        public async Task<List<ApiResponse<long>>> DeleteReviewByIds(List<long> reviewIdsInput, long userId)
        {
            List<ApiResponse<long>> result = new List<ApiResponse<long>>();
            bool success = false;
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.DeleteReviewByIds,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            List<string> reviewIds = new List<string>();

            foreach (long reviewId in reviewIdsInput)
            {
                reviewIds.Add(reviewId.ToString());
            }

            string reviewStr = string.Join(",", reviewIdsInput);
            command.Parameters.AddWithValue("@ReviewIds", reviewStr);
            command.Parameters.AddWithValue("@UserId", userId);

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
                        try
                        {
                            
                            result.Add(
                                  new ApiResponse<long>()
                                  {

                                      Data = row.Field<long>("Data"),

                                      Message = row.Field<string>("Message")

                                  }
                            );                            
                        }
                        catch(Exception ex)
                        {
                            result.Add(
                                  new ApiResponse<long>()
                                  {
                                      Message = row.Field<string>("Message")
                                  }
                            );
                        }
                    }
                }
            });

            return result;
        }

        public async Task<List<ValorisationTypeDto>> GetValorisationTypes()
        {
            var result = new List<ValorisationTypeDto>();

            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetValorisationTypes,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;


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
                        result.Add(new ValorisationTypeDto()
                        {
                            Id = row.Field<int>("Id"),
                            Description = row.Field<string>("Description"),
                        });
                    }

                }
            });

            return result;
        }

        public async Task<List<ApiResponse<long?>>> UpdateValorisationTypeForMetric(long reviewId, string metricCode, int valorisationTypeId)
        {
            var result = new List<ApiResponse<long?>>();

            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.UpdateValorisationTypeForMetric,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@ReviewId", reviewId);
            command.Parameters.AddWithValue("@MetricCode", metricCode);
            command.Parameters.AddWithValue("@ValorisationTypeId", valorisationTypeId);


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
                        result.Add(new ApiResponse<long?>() 
                        { 
                            Data = row.Field<long?>("Data"),
                            Message = row.Field<string>("Message")
                        });
                    }
                }


                });

            return result;
        }

        public async Task<List<ApiResponse<int?>>> UpdateReportedLimitAcknowledgedBy(long reviewId, string metricCode, long userId, long? metricId)
        {
            var result = new List<ApiResponse<int?>>();

            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.UpdateReportedLimitAcknowledgedBy,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@ReviewId", reviewId);
            command.Parameters.AddWithValue("@MetricCode", metricCode);
            command.Parameters.AddWithValue("@MetricId", metricId);
            command.Parameters.AddWithValue("@UserId", userId);


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
                        result.Add(new ApiResponse<int?>()
                        {
                            Data = row.Field<int?>("Data"),
                            Message = row.Field<string>("Message")
                        });
                    }
                }


            });

            return result;
        }
        public async Task<List<NewReviewsDto>> GetReviewImpactByReviewId(List<long> reviewIdsInput)
        {
            var result = new List<NewReviewsDto>();

            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetReviewImpactByReviewId,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            string reviewIds = String.Join(",", reviewIdsInput);
            command.Parameters.AddWithValue("@ReviewIds", reviewIds);  

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
                        result.Add(new NewReviewsDto()
                        {
                            ReviewId = row.Field<long>("ReviewId"),
                            ProjectTitle = row.Field<string>("ProjectTitle"),
                            ProjectPin = row.Field<string?>("ProjectPin"),
                            Quarter = row.Field<string>("Quarter"),
                            RecordType = row.Field<string>("RecordType"),
                        });
                    }
                }


            });

            return result;
        }

        public async Task<List<Dictionary<string, object?>>> GetCurrentMetricStages(List<long> reviewIds, int? stageId, long? metricId)
        {
            var result = new List<Dictionary<string, object?>>();

            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync();

            await using var command = new SqlCommand(
                StoredProcedures.Review.GetCurrentMetricStages,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@ReviewIds", string.Join(",", reviewIds));
            command.Parameters.AddWithValue("@Stage", (object?)stageId ?? DBNull.Value);
            command.Parameters.AddWithValue("@MetricId", (object?)metricId ?? DBNull.Value);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>(reader.FieldCount);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                result.Add(row);
            }

            return result;
        }

    }
}
