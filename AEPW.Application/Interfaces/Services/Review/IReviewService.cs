using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs;
using AEPW.Application.DTOs.Lookup;
using AEPW.Application.DTOs.Metric;
using AEPW.Application.DTOs.Review;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.Interfaces.Services.Review
{
    public interface IReviewService
    {
        Task<IReadOnlyList<string>> SaveReview(CreateReviewRequestDto input);
        Task<IReadOnlyList<ReviewPeriodDto>> GetReviewPeriods(long projectId, int fromPresent);
        Task<IReadOnlyList<ReviewStageLookupDto>> GetReviewStages();
        Task<IReadOnlyList<ReviewDto>> GetReviews(GetReviewFilters filters);
        Task<IReadOnlyList<ReviewDto>> GetReviewById(long reviewId, string sourcePage, long userId, int? stage);
        Task<ApiResponse<Int64>> SaveRiskAdjustmentAction(Int64 reviewId, List<MetricRiskResponseJSON> metricRiskResponseJSON, int userId);
        Task<byte[]> GenerateReviewExcelAsync(Int64 reviewId);
        Task GenerateExcelAndSendNotification(List<NewReviewsDto> reviews, long userId);
        Task<List<ReviewExcelUploadDto>> GetReviewExcelUploads(Int64 reviewId);
        Task<string> DownloadFileAsync(string fileName, string accessToken);
        Task<List<ReviewCommentDto>> AddReviewComments(List<AddReviewCommentDto> reqBody);
        Task<ApiResponse<List<ReviewExcelImportDto>>> ParseReviewExcelAsync(IFormFile file, long reviewId, long userId);
        Task UpdateTemplateExcelForReview(long reviewId, IFormFile file);
        Task<List<string>> SaveReviewStages(
        List<SaveReviewStageRequest> reviewStageJSON,
        long userId);
        Task<List<LaunchableProjectDto>> GetLaunchableImpacts(string quarter, string status, string impactName, int pageNo, int skippedRows, int itemsPerPage);
        Task<List<NewReviewsDto>> BulkLaunchReviews(List<long> impactIds, long? quarter, long userId);
        Task<bool> UpdateBaselineForMetric(long reviewId, List<UpdateReviewMetricsValuesDto> input, long UserUId);
        Task<bool> RestatementUpdateBaselineForMetric(long reviewId, List<UpdateReviewMetricsValuesDto> input, long UserUId);
        Task<List<ApiResponse<long>>> AddReviewMetricEvidence(long? reviewId, long? metricId, long? userId, long? evidenceTypeId, List<IFormFile> files);
        Task<List<ApiResponse<string>>> SaveReviewMetricConfig(long reviewId, List<string> metricIds, long userId);
        Task<List<ApiResponse<long>>> DeleteReviewMetricEvidence(string itemId, long userId);
        Task<List<BaselineRemainderDto>> GetBaselineRemainder(long reviewId);
        Task<ReviewStagePromotionResultDto> ApproveRejectReviewMetric(List<ReviewMetricApproveRejectDto> metricActionJSON, long userId);
        Task<List<ReviewApprovalProgressDto>> GetReviewApprovalByMetricCode(long reviewId, string metricCode, long userId);
        Task<ReviewApprovalChangesPreviewDto> GetApprovalChangesAsync(
            IReadOnlyList<ReviewStageApprovalChangeRequestDto> stageChanges,
            CancellationToken cancellationToken = default);
        Task<bool> UpdateReviewApprovalAssignee(UpdateReviewApprovalAssigneeDto input);

        Task<List<TopLevelMetricAggregationDto>> GetToplevelAggregation(string topicName, string periodCode, string applicablestages, long userId);
        Task<List<ProjectLevelAggregationDto>> GetProjectMetricAggregation(string topicName, string periodCode, string applicablestages, long userId);
        Task<List<ReviewMetricCodeForAdminL4ApprovalRejectionDto>> GetReviewMetricCodesForAdminOrL4Approval(string topicName, string periodCode, List<ProjectPinParentCodeDto> projectPinParentCodes);
        Task<List<RecallTopLevelMetricResultDto>> RecallToplevelMetric(RecallTopLevelMetricInput input);
        Task<List<ApiResponse<long>>> IssueRestatementForReview(StartEndRestatementDto input);

        Task<List<ApiResponse<long>>> UpdateReviewMetricFRComment(UpdateReviewMetricFRCommentDto input);
        Task SyncMetricFoldersForReview(long reviewId);
        Task<List<LaunchableProjectDto>> GetImpactsWithActiveReviews(int pageNo, int skippedRows, int itemsPerPage, string quarter, string status, string impactName);
        Task<List<ApiResponse<long>>> DeleteReviewByIds(List<long> reviewIdsInput, long userId);
        //Task<List<ApiResponse<long>>> SaveReviewMetricEvidences(long reviewId, List<ReviewMetricEvidencesJsonDto> reviewMetricEvidencesJson, long userId);
        Task<List<ValorisationTypeDto>> GetValorisationTypes();

        Task<List<ApiResponse<long?>>> UpdateValorisationTypeForMetric(long reviewId, string metricCode, int valorisationTypeId);

        Task<List<ApiResponse<int?>>> UpdateReportedLimitAcknowledgedBy(long reviewId, string metricCode, long userId, long? metricId);
        public void CreateMetricFoldersInSharepoint(List<NewReviewsDto> reviews, long userId);
        Task<List<Dictionary<string, object?>>> GetCurrentMetricStages(List<long> reviewIds, int? stageId, long? metricId);
    }
}
