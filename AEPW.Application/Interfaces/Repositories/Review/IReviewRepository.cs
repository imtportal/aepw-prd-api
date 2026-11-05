using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs;
using AEPW.Application.DTOs.Lookup;
using AEPW.Application.DTOs.Review;
using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.Interfaces.Repositories.Review
{
    public interface IReviewRepository
    {
        Task<IReadOnlyList<string>>
            SaveReview(CreateReviewRequestDto input,
                CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReviewPeriodDto>>
            GetReviewPeriods(
                long periodId
                , int fromPresent
                , CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReviewDto>>
            GetReviews(GetReviewFilters filters,
                CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReviewDto>> GetReviewById(
         long ReviewId,
         long UserId,
         string sourcePage,
         int? stage,
         CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReviewStageLookupDto>> GetReviewStages(
         CancellationToken cancellationToken = default
        );

        Task<IReadOnlyList<string>> SaveRiskAdjustmentAction(
            Int64 ReviewId,
            List<MetricRiskResponseJSON> metricRiskResponseJSON,
            int userId,
            CancellationToken cancellationToken = default
        );
        Task<List<ReviewExcelExportDto>> GetReviewDataAsync(Int64 reviewId);
        Task UpdateReviewExcelAsync(long userId, string jsonData);
        Task<List<ApiResponse<int>>> SaveMetricDataForReview(long userId, string projectPin, string jsonData);
        Task<List<ApiResponse<long>>> SaveReviewUploadDocuments(List<ReviewUploadExcelJSON> reviewUploadExcelJSON, long userObjectId);
        Task<List<ReviewExcelUploadDto>> GetReviewExcelUploads(Int64 reviewId);
        Task<List<ReviewCommentDto>> AddReviewComments(List<AddReviewCommentDto> reqBody);
        Task<List<string>> SaveReviewStages(
            List<SaveReviewStageRequest> reviewStageJSON,
            long userId);
        Task<List<LaunchableProjectDto>> GetLaunchableImpacts(string quarter, string status, string impactName, int pageNo, int skippedRows, int itemsPerPage);
        Task<List<NewReviewsDto>> BulkLaunchReviews(List<long> impactIds, long? quarter, long userObjectId);
        Task<SharepointHierarchyFolderCreationDto> GetTopicWiseMetricsForReview(long reviewId);

        Task<bool> SaveDataCollectionUrls(List<ReviewDataCollectionUrlUpdateDto> dataCollectionUrls, long userId);
        Task<bool> UpdateBaselineForMetric(long reviewId, List<UpdateReviewMetricsValuesDto> input, long UserUId);
        Task<bool> RestatementUpdateBaselineForMetric(long reviewId, List<UpdateReviewMetricsValuesDto> input, long UserUId);
        Task<ReviewMetricDetailsForEvidenceUploadDto> GetReviewMetricDetailsForUpload(long reviewId, long metricId, long? evidenceTypeId);
        Task<List<ApiResponse<long>>> SaveReviewMetricEvidences(long? reviewId, long? userId, List<ReviewMetricEvidencesJsonDto>? reviewMetricEvidencesJsonDto);
        Task<List<ApiResponse<string>>> SaveReviewMetricConfig(long reviewId, List<string> metricIds, long userId, string deletedItemIdKeyword);
        Task<List<ApiResponse<long>>> DeleteReviewMetricEvidence(string itemId, long userId);
        Task<List<BaselineRemainderDto>> GetBaselineRemainder(long reviewId);
        Task<ReviewStagePromotionResultDto> ApproveRejectReviewMetric(List<ReviewMetricApproveRejectDto> metricActionJSON, long userId);
        Task<List<ReviewApprovalProgressDto>> GetReviewApprovalByMetricCode(long reviewId, string metricCode, long userId);
        Task<(IReadOnlyList<ReviewApprovalChangeRepositoryRowDto> DisablePreview, IReadOnlyList<ReviewApprovalChangeRepositoryRowDto> EnablePreview)>
            GetApprovalChangesAsync(
                IReadOnlyList<ReviewStageApprovalChangeRequestDto> stageChanges,
                CancellationToken cancellationToken = default);
        Task<long> UpdateReviewApprovalAssignee(UpdateReviewApprovalAssigneeDto input);
        Task<List<TopLevelMetricAggregationDto>> GetToplevelAggregation(string topicName, string periodCode, string applicablestages, long userId);
        Task<List<ProjectLevelAggregationDto>> GetProjectMetricAggregation(string topicName, string periodCode, string applicablestages, long userId);
        Task<List<ReviewMetricCodeForAdminL4ApprovalRejectionDto>> GetReviewMetricCodesForAdminOrL4Approval(string topicName, string periodCode, List<ProjectPinParentCodeDto> projectPinParentCodes);
        Task<List<RecallTopLevelMetricResultDto>> RecallToplevelMetric(RecallTopLevelMetricInput input);
        Task<List<ApiResponse<long>>> IssueRestatementForReview(StartEndRestatementDto input);
        Task<List<ApiResponse<long>>> UpdateReviewMetricFRComment(UpdateReviewMetricFRCommentDto input);
        Task<List<LaunchableProjectDto>> GetImpactsWithActiveReviews(int pageNo, int skippedRows, int itemsPerPage, string quarter, string status, string impactName);
        Task<List<string>> GetAllMetricsWithValorisationRequired();
        Task<List<ApiResponse<long>>> DeleteReviewByIds(List<long> reviewIdsInput, long userId);
        Task<List<ValorisationTypeDto>> GetValorisationTypes();
        Task<List<ApiResponse<long?>>> UpdateValorisationTypeForMetric(long reviewId, string metricCode, int valorisationTypeId);
        Task<List<ApiResponse<int?>>> UpdateReportedLimitAcknowledgedBy(long reviewId, string metricCode, long userId, long? metricId);
        Task<List<NewReviewsDto>> GetReviewImpactByReviewId(List<long> reviewIds);
        Task<List<Dictionary<string, object?>>> GetCurrentMetricStages(List<long> reviewIds, int? stageId, long? metricId);
    }
}
