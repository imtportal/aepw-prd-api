using AEPW.Application.Contracts.Responses;
using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs;
using AEPW.Application.DTOs.Evidence;
using AEPW.Application.DTOs.ImpactMaster;
using AEPW.Application.DTOs.Lookup;
using AEPW.Application.DTOs.Notification;
using AEPW.Application.DTOs.Review;
using AEPW.Application.Interfaces.Services.Evidence;
using AEPW.Application.Interfaces.Services.Notification;
using AEPW.Application.Interfaces.Services.Review;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.DeviceManagement.ManagedDevices.Item.RequestRemoteAssistance;
using System.Buffers.Text;
using System.Text.Json;

namespace AEPW.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public sealed class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        private readonly IServiceBusService _serviceBusService;

        private readonly IEvidenceReviewService _evidenceReviewService;

        public ReviewController(
            IReviewService reviewService,
            IServiceBusService serviceBusService,
            IEvidenceReviewService evidenceReviewService)
        {
            _reviewService = reviewService;
            _serviceBusService = serviceBusService;
            _evidenceReviewService = evidenceReviewService;
        }


        [HttpGet]
        public async Task<IActionResult> GetReviews([FromQuery] GetReviewFilters filters)
        {
            var response =
                await _reviewService.GetReviews(filters);

            return Ok(ApiResponse<IReadOnlyList<ReviewDto>>.Ok(response, "Success"));
        }

        /// <summary>
        /// Returns the full review payload for one Evidence item, shaped to match the
        /// Angular evidence-review page. Binds evidenceId from the route or the query
        /// string (whichever the caller supplies).
        /// </summary>
        [Authorize]
        [HttpGet("evidence/{evidenceId:long?}")]
        public async Task<IActionResult> GetEvidenceReview(
            long? evidenceId,
            [FromQuery] long userId,
            CancellationToken cancellationToken)
        {
            var response = await _evidenceReviewService.GetReviewDetailsAsync(
                evidenceId ?? 0,
                userId,
                cancellationToken);

            return Ok(ApiResponse<EvidenceReviewResponseDto>.Ok(response));
        }

        /// <summary>Saves edited scalar extracted fields for an Evidence Review.</summary>
        [Authorize]
        [HttpPost("evidence/{evidenceId:long}/extracted-fields")]
        public async Task<IActionResult> SaveEvidenceReviewExtractedFields(
       long evidenceId,
       [FromBody] SaveEvidenceReviewExtractedFieldsRequestDto request,
       CancellationToken cancellationToken)
        {
            var saved = await _evidenceReviewService.SaveExtractedFieldsAsync(
                evidenceId,
                request,
                cancellationToken);

            return Ok(ApiResponse<bool>.Ok(saved));
        }

        [HttpGet("GetReviewById/{reviewId}")]
        public async Task<IActionResult> GetReviewById(string reviewId)
        {
            long userId = long.Parse(Request.Headers["userId"].ToString());
            string sourcePage = Request.Headers["SourcePage"].ToString();

            int? stage = null;

            try
            {
                if (Request.Headers["stage"] != "null" && Request.Headers["stage"] != "")
                {
                    stage = int.Parse(Request.Headers["stage"]);
                }
            }catch
            {

            }

            var response =
                await _reviewService.GetReviewById(Int64.Parse(reviewId), sourcePage, userId, stage);

            return Ok(ApiResponse<IReadOnlyList<ReviewDto>>.Ok(response, "Success"));
        }


        [HttpGet("GetReviewPeriods")]
        public async Task<IActionResult> GetReviewPeriods()
        {
            long projectId = int.Parse(Request.Headers["projectId"].ToString() ?? "0");

            int fromPresent = 0;

            try
            {
                fromPresent = int.Parse(Request.Headers["fromPresent"].ToString() ?? "0");

            }
            catch (Exception e)
            {
                fromPresent = 0;
            }
            var response =
                await _reviewService.GetReviewPeriods(projectId, fromPresent);

            return Ok(ApiResponse<IReadOnlyList<ReviewPeriodDto>>.Ok(response, "Success"));
        }

        [HttpGet("GetReviewStages")]
        public async Task<IActionResult> GetReviewStages()
        {
            IReadOnlyList<ReviewStageLookupDto> response =
                await _reviewService.GetReviewStages();

            return Ok(ApiResponse<IReadOnlyList<ReviewStageLookupDto>>.Ok(response, "Success"));
        }

        [HttpPost("SaveReview")]
        public async Task<IActionResult> SaveReview([FromBody] CreateReviewRequestDto input)
        {
            var response =
                await _reviewService.SaveReview(input);

            long? reviewId = Int64.Parse(response[0]);
            string message = response[1];

            //if (reviewId != null)
            //{
            //    NotificationMessageDto notificationMsg = new NotificationMessageDto();
            //    notificationMsg.NotificationType = "LaunchReview";
            //    notificationMsg.EntityId = ((long) reviewId);

            //    _serviceBusService.PublishNotificationAsync(notificationMsg);
            //}

            return Ok(ApiResponse<long?>.Ok(reviewId, message));
        }

        [HttpPost("SaveReviewStages")]
        public async Task<IActionResult> SaveReviewStages(
            [FromBody] List<SaveReviewStageRequest> reviewStageJSON,
            [FromQuery] long userId)
        {
            if (reviewStageJSON == null || reviewStageJSON.Count == 0)
            {
                return BadRequest(
                    ApiResponse<int>.Ok(0, "No review stages were provided."));
            }

            List<string> response = await _reviewService.SaveReviewStages(
                reviewStageJSON,
                userId);

            int data = int.Parse(response[0]);
            string message = response[1];

            if (data == 0)
            {
                return BadRequest(ApiResponse<int>.Ok(data, message));
            }

            return Ok(ApiResponse<int>.Ok(data, message));
        }

        /// <summary>Previews approval changes caused by enabling or disabling review stages.</summary>
        [HttpPost("GetApprovalChanges")]
        [ProducesResponseType(typeof(ApiResponse<ReviewApprovalChangesPreviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetApprovalChanges(
            [FromBody] List<ReviewStageApprovalChangeRequestDto>? stageChanges,
            CancellationToken cancellationToken)
        {
            if (stageChanges is null || stageChanges.Count == 0)
            {
                return BadRequest(ApiResponse<object>.Fail("At least one stage change is required."));
            }

            if (stageChanges.GroupBy(change => change.StageCode).Any(group => group.Count() > 1))
            {
                return BadRequest(ApiResponse<object>.Fail("StageCode values must be unique."));
            }

            var response = await _reviewService.GetApprovalChangesAsync(
                stageChanges,
                cancellationToken);

            return Ok(ApiResponse<ReviewApprovalChangesPreviewDto>.Ok(response));
        }

        [HttpPost("SaveRiskAdjustmentAction/{reviewId}/{userId}")]
        public async Task<IActionResult> SaveRiskAdjustmentAction([FromBody] List<MetricRiskResponseJSON> input, string reviewId, string userId)
        {
            ApiResponse<Int64> response =
            await _reviewService.SaveRiskAdjustmentAction(Int64.Parse(reviewId), input, int.Parse(userId));

            return Ok(ApiResponse<Int64>.Ok(response.Data, response.Message));

        }


        [HttpPost("GenerateExcelAndSendNotification")]
        public async Task<IActionResult> GenerateExcelAndSendNotification([FromBody] List<NewReviewsDto> newReviews)
        {
            long? userId;
            try
            {
                userId = long.Parse(
                    Request.Headers["userId"]
                    .ToString()
                );
            }
            catch (Exception e)
            {
                return BadRequest("Invalid UserId");
            }

            await _reviewService.GenerateExcelAndSendNotification(newReviews, (long)userId);


            return Ok(ApiResponse<bool>.Ok(true, "Message"));
        }

        [AllowAnonymous]
        [HttpPost("GenerateExcelAndSendNotificationForLogicApp")]
        public async Task<IActionResult> GenerateExcelAndSendNotificationForLogicApp([FromBody] List<NewReviewsDto> newReviews)
        {
            long? userId;
            try
            {
                userId = long.Parse(
                    Request.Headers["userId"]
                    .ToString()
                );
            }
            catch (Exception e)
            {
                return BadRequest("Invalid UserId");
            }

            await _reviewService.GenerateExcelAndSendNotification(newReviews, (long)userId);

            return Ok(ApiResponse<bool>.Ok(true, "Message"));
        }

        [HttpGet("{reviewId}/excel")]
        public async Task<IActionResult> GenerateExcel(Int64 reviewId)
        {
            var stream =
                await _reviewService
                    .GenerateReviewExcelAsync(reviewId);

            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Review_{reviewId}.xlsx");
        }

        [HttpGet("GetReviewExcelUploads")]
        public async Task<IActionResult> GetReviewExcelUploads()
        {
            string reviewIdStr = Request.Headers["ReviewId"].ToString();

            Int64 reviewId = 0;

            Int64.TryParse(reviewIdStr, out reviewId);

            //try
            //{
            //    reviewId = Int64.TryParse(reviewIdStr);

            //}
            //catch (Exception ex) 
            //{
            //    reviewId = 0;
            //}


            List<ReviewExcelUploadDto> result =
                await _reviewService
                    .GetReviewExcelUploads(reviewId);

            return Ok(ApiResponse<List<ReviewExcelUploadDto>>.Ok(result, ""));
        }

        [HttpGet("DownloadReviewExcel")]
        public async Task<IActionResult> DownloadReviewExcel()
        {
            string fileName = Request.Headers["fileName"].ToString();

            string accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer", "");


            string base64 = await _reviewService.DownloadFileAsync(fileName, accessToken);

            return Ok(ApiResponse<string>.Ok(base64));
        }

        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment([FromBody] List<AddReviewCommentDto> reqBody)
        {
            List<ReviewCommentDto> reviewComments = await _reviewService.AddReviewComments(reqBody);
            string base64 = "";
            return Ok(ApiResponse<List<ReviewCommentDto>>.Ok(reviewComments));
        }

        [HttpGet("GetLaunchableImpacts")]
        public async Task<IActionResult> GetLaunchableImpacts()
        {
            string quarter = Request.Headers["Quarter"];

            string status = Request.Headers["status"].ToString();
            string impactName = Request.Headers["impactName"].ToString();

            int pageNo = int.Parse(Request.Headers["pageNo"].ToString());
            int skippedRows = int.Parse(Request.Headers["skippedRows"].ToString());
            int itemsPerPage = int.Parse(Request.Headers["itemsPerPage"].ToString());


            if (String.IsNullOrEmpty(status))
            {
                status = String.Empty;
            }

            if (String.IsNullOrEmpty(impactName))
            {
                impactName = String.Empty;
            }


            List<LaunchableProjectDto> projects = await _reviewService.GetLaunchableImpacts(quarter, status, impactName, pageNo, skippedRows, itemsPerPage);
            return Ok(ApiResponse<List<LaunchableProjectDto>>.Ok(projects));
        }

        [HttpPost("BulkLaunchReviews")]
        public async Task<IActionResult> BulkLaunchReviews([FromBody] List<long> reqBody)
        {
            string userId = Request.Headers["userId"].ToString();

            long? quarter = null;

            string qtrStr = Request.Headers["quarter"].ToString().Trim();
            if (!String.IsNullOrEmpty(qtrStr))
            {
                try
                {
                    quarter = long.Parse(qtrStr);
                }
                catch(Exception ex)
                {
                    quarter = null;
                }
            }

            List<NewReviewsDto> reviewComments = await _reviewService.BulkLaunchReviews(reqBody, quarter, long.Parse(userId));
            string base64 = "";
            return Ok(ApiResponse<List<NewReviewsDto>>.Ok(reviewComments));
        }

        [AllowAnonymous]
        [HttpPost("BulkLaunchReviewsForLogicApp")]
        public async Task<IActionResult> BulkLaunchReviewsForLogicApp([FromBody] List<long> reqBody)
        {
            string userId = Request.Headers["userId"].ToString();
            List<NewReviewsDto> reviewComments = await _reviewService.BulkLaunchReviews(reqBody, null, long.Parse(userId));
            string base64 = "";
            return Ok(ApiResponse<List<NewReviewsDto>>.Ok(reviewComments));
        }
    
        //import logic
        [HttpPost("{reviewId}/upload")]
        public async Task<IActionResult> UploadExcel(long reviewId, IFormFile file)
        {
            string userId = Request.Headers["UserId"].ToString();
            var result = await _reviewService.ParseReviewExcelAsync(file, reviewId, long.Parse(userId));

            if (result.Success)
            {
                await _reviewService.UpdateTemplateExcelForReview(reviewId, file);
            }

            return Ok(result);
        }

        [HttpPost("UpdateBaselineForMetric/{reviewId}")]
        public async Task<IActionResult> UpdateBaselineForMetric(long reviewId, [FromBody] List<UpdateReviewMetricsValuesDto> input)
        {
            try
            {
                long userId = long.Parse(Request.Headers["userId"].ToString());

                var result = await _reviewService.UpdateBaselineForMetric(reviewId, input, userId);

                return Ok(result);
            }

            catch(Exception ex)
            {
                return BadRequest();
            }

        }

        [HttpPost("RestatementUpdateBaselineForMetric/{reviewId}")]
        public async Task<IActionResult> RestatementUpdateBaselineForMetric(long reviewId, [FromBody] List<UpdateReviewMetricsValuesDto> input)
        {
            try
            {
                long userId = long.Parse(Request.Headers["userId"].ToString());

                var result = await _reviewService.RestatementUpdateBaselineForMetric(reviewId, input, userId);

                return Ok(result);
            }

            catch(Exception ex)
            {
                return BadRequest();
            }

        }


        [HttpPost("AddReviewMetricEvidence/{reviewId}/{metricId}")]
        public async Task<IActionResult> AddReviewMetricEvidence(long? reviewId, long? metricId, List<IFormFile> files)
        {
            long? userId = null;

            long? evidenceTypeId = null;

            try
            {
                userId = long.Parse(Request.Headers["UserId"].ToString());
            }
            catch (Exception e)
            {
            }

            try
            {
                evidenceTypeId = long.Parse(Request.Headers["evidenceTypeId"].ToString());
            }
            catch (Exception e)
            {
            }

            await _reviewService.AddReviewMetricEvidence(reviewId, metricId, userId, evidenceTypeId, files);

            return Ok(true);
        }

        [HttpPost("SaveReviewMetricConfig/{reviewId}")]
        public async Task<IActionResult> SaveReviewMetricConfig(long reviewId, [FromBody] List<string> metricIds)
        {

            long userId = long.Parse(Request.Headers["UserId"].ToString());
            var result = await _reviewService.SaveReviewMetricConfig(reviewId, metricIds, userId);

            return Ok(result);
        }

        [HttpDelete("DeleteReviewMetricEvidence/{itemId}")]
        public async Task<IActionResult> DeleteReviewMetricEvidence(string itemId)
        {

            //long userId = long.Parse(Request.Headers["UserId"].ToString());
            long? userId = null;
            try
            {
                userId = long.Parse(Request.Headers["UserId"].ToString());
            }
            catch (Exception e)
            {
                return BadRequest();
            }


            var result = await _reviewService.DeleteReviewMetricEvidence(itemId, (long) userId);

            return Ok(result);
        }

        [HttpGet("GetBaselineRemainder/{reviewId}")]
        public async Task<IActionResult> GetBaselineRemainder(long reviewId)
        {

            //long userId = long.Parse(Request.Headers["UserId"].ToString());

            var result = await _reviewService.GetBaselineRemainder(reviewId);

            return Ok(ApiResponse<List<BaselineRemainderDto>>.Ok(result));
        }



        [HttpPut("ReviewMetricAction")]
        //List<ReviewMetricApproveRejectDto>> 
        public async Task<IActionResult> ApproveRejectReviewMetric([FromBody] List<ReviewMetricApproveRejectDto> input)
        {

            //long userId = long.Parse(Request.Headers["UserId"].ToString());
            long userId;

            try
            {

                userId = long.Parse(Request.Headers["UserId"].ToString());
                var result = await _reviewService.ApproveRejectReviewMetric(input, userId);

                return Ok(ApiResponse<ReviewStagePromotionResultDto>.Ok(result));
            }
            catch (FormatException e)
            {
                return BadRequest("User could not be authenticated");
            }
            catch (Exception e)
            {
                return BadRequest("Unknown error occured");
            }
        }


        [HttpGet("GetReviewApprovalByMetricCode/{reviewId}/{metricCode}")]
        //List<ReviewMetricApproveRejectDto>> 
        public async Task<IActionResult> GetReviewApprovalByMetricCode(long reviewId, string metricCode)
        {

            try
            {
                long userId = long.Parse(Request.Headers["UserId"].ToString());
                var result = await _reviewService.GetReviewApprovalByMetricCode(reviewId, metricCode, userId);
                return Ok(ApiResponse<List<ReviewApprovalProgressDto>>.Ok(result));
            }
            catch (Exception e)
            {
                return BadRequest("Unknown error occured");
            }
        }

        [HttpPut("UpdateReviewApprovalAssignee")]
        public async Task<IActionResult> UpdateReviewApprovalAssignee([FromBody] UpdateReviewApprovalAssigneeDto input)
        {
            try
            {
                var result = await _reviewService.UpdateReviewApprovalAssignee(input);
                return Ok(ApiResponse<bool>.Ok(result));
            }
            catch (Exception e)
            {
                return BadRequest("Unknown error occured");
            }
        }

        [HttpGet("GetToplevelAggregation/{topicName}/{periodCode}/{role}/{userId}")]
        public async Task<IActionResult> GetToplevelAggregation(string topicName, string periodCode, string role, long userId)
        {
            try
            {
                if (role.ToLower() == "seniorleadership")
                {
                    role = "L4";
                }
                List<TopLevelMetricAggregationDto> result = await _reviewService.GetToplevelAggregation(topicName, periodCode, role, userId);
                return Ok(ApiResponse<List<TopLevelMetricAggregationDto>>.Ok(result));
            }
            catch (Exception e)
            {
                return BadRequest("Unknown error occured");
            }
        }

        [HttpGet("GetProjectLevelAggregationDto/{topicName}/{periodCode}/{role}/{userId}")]
        public async Task<IActionResult> GetProjectLevelAggregationDto(string topicName, string periodCode, string role, long userId)
        {
            try
            {
                if (role.ToLower() == "seniorleadership")
                {
                    role = "L4";
                }

                List<ProjectLevelAggregationDto> result = await _reviewService.GetProjectMetricAggregation(topicName, periodCode, role, userId);
                return Ok(ApiResponse<List<ProjectLevelAggregationDto>>.Ok(result));
            }
            catch (Exception e)
            {
                return BadRequest("Unknown error occured");
            }
        }

        [HttpGet("GetReviewMetricCodesForAdminOrL4Approval/{topicName}/{periodCode}")]
        public async Task<IActionResult> GetReviewMetricCodesForAdminOrL4Approval(string topicName, string periodCode)
        {
            try
            {
                List<ProjectPinParentCodeDto> projectPinParentCodes = JsonSerializer.Deserialize<List<ProjectPinParentCodeDto>>(Request.Headers["projectPinParentCodes"].ToString());
                List<ReviewMetricCodeForAdminL4ApprovalRejectionDto> result = await _reviewService.GetReviewMetricCodesForAdminOrL4Approval(topicName, periodCode, projectPinParentCodes);
                return Ok(ApiResponse<List<ReviewMetricCodeForAdminL4ApprovalRejectionDto>>.Ok(result));
            }
            catch (Exception e)
            {
                return BadRequest("Unknown error occured");
            }
        }

        [HttpPut("RecallTopLevelMetric")]
        public async Task<IActionResult> RecallToplevelMetric(RecallTopLevelMetricInput input)
        {
            try
            {
                List<RecallTopLevelMetricResultDto> result = await _reviewService.RecallToplevelMetric(input);
                return Ok(ApiResponse<List<RecallTopLevelMetricResultDto>>.Ok(result));
            }
            catch (Exception e)
            {
                return BadRequest("Unknown error occured");
            }
        }

        [HttpPut("IssueRestatementForReview")]
        public async Task<IActionResult> RecallToplevelMetric([FromBody] StartEndRestatementDto input)
        {
            try
            {
                List<ApiResponse<long>> result = await _reviewService.IssueRestatementForReview(input);
                return Ok(ApiResponse<List<ApiResponse<long>>>.Ok(result));
            }
            catch (Exception e)
            {
                return BadRequest("Unknown error occured");
            }
        }

        [HttpPut("UpdateReviewMetricFRComment")]
        public async Task<IActionResult> UpdateReviewMetricFRComment([FromBody] UpdateReviewMetricFRCommentDto input)
        {
            try
            {
                List<ApiResponse<long>> result = await _reviewService.UpdateReviewMetricFRComment(input);
                return Ok(ApiResponse<List<ApiResponse<long>>>.Ok(result));
            }
            catch (Exception e)
            {
                return BadRequest("Unknown error occured");
            }
        }

        [HttpGet("GetImpactsWithActiveReviews")]
        public async Task<IActionResult> GetImpactsWithActiveReviews()
        {
            int pageNo = int.Parse(Request.Headers["pageNo"].ToString());
            int skippedRows = int.Parse(Request.Headers["skippedRows"].ToString());
            int itemsPerPage = int.Parse(Request.Headers["itemsPerPage"].ToString());
            string quarter = (Request.Headers["period"].ToString());
            string status = Request.Headers["status"].ToString();
            string impactName = Request.Headers["impactName"].ToString();

            
            List <LaunchableProjectDto> projects = await _reviewService.GetImpactsWithActiveReviews(pageNo, skippedRows, itemsPerPage, quarter, status, impactName);
            return Ok(ApiResponse<List<LaunchableProjectDto>>.Ok(projects));
        }

        [HttpDelete("DeleteReviewByIds")]
        public async Task<IActionResult> DeleteReviewByIds()
        {
            string inputStr = Request.Headers["ReviewIds"];
            long userId = long.Parse(Request.Headers["UserId"].ToString());
            List<long> ReviewIds = inputStr
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(long.Parse)
                .ToList();
 

            List <ApiResponse<long>> result_data = await _reviewService.DeleteReviewByIds(ReviewIds, userId);

            ApiResponse<List<ApiResponse<long>>> result = new ApiResponse<List<ApiResponse<long>>>() { Data = result_data };

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpDelete("DeleteReviewByIdsForLogicApp")]
        public async Task<IActionResult> DeleteReviewByIdsForLogicApp()
        {
            string inputStr = Request.Headers["ReviewIds"];
            long userId = long.Parse(Request.Headers["UserId"].ToString());
            List<long> ReviewIds = inputStr
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(long.Parse)
                .ToList();
 

            List <ApiResponse<long>> result_data = await _reviewService.DeleteReviewByIds(ReviewIds, userId);

            ApiResponse<List<ApiResponse<long>>> result = new ApiResponse<List<ApiResponse<long>>>() { Data = result_data };

            return Ok(result);
        }

        [HttpGet("GetValorisationTypes")]
        public async Task<IActionResult> GetValorisationTypes()
        {
            List<ValorisationTypeDto> result = await _reviewService.GetValorisationTypes();

            ApiResponse<List<ValorisationTypeDto>> apiResponse = new ApiResponse<List<ValorisationTypeDto>>() { Data = result };
            return Ok(apiResponse);
        }

        [HttpPut("UpdateValorisationTypeForMetric")]
        public async Task<IActionResult> UpdateValorisationTypeForMetric()
        {
            long reviewId;
            string metricCode;
            int valorisationTypeId;
            try
            {
                reviewId = long.Parse(Request.Headers["reviewId"]);
                metricCode = (Request.Headers["metricCode"]);
                valorisationTypeId = int.Parse(Request.Headers["valorisationTypeId"]);

            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
            List<ApiResponse<long?>> data = await _reviewService.UpdateValorisationTypeForMetric(reviewId, metricCode, valorisationTypeId);
            ApiResponse<List<ApiResponse<long?>>> result = new ApiResponse<List<ApiResponse<long?>>>() { Data = data };
            return Ok(result);
        }

        [HttpPut("UpdateReportedLimitAcknowledgedBy")]
        public async Task<IActionResult> UpdateReportedLimitAcknowledgedBy()
        {
            long reviewId;
            string metricCode;
            long? metricId;
            long userId;
            try
            {
                reviewId = long.Parse(Request.Headers["reviewId"]);
                metricCode = (Request.Headers["metricCode"]);
                userId = long.Parse(Request.Headers["userId"]);

            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }

            try
            {
                metricId = long.Parse(Request.Headers["metricID"].ToString());
            }
            catch(Exception e) { metricId = null; }

            List<ApiResponse<int?>> data = await _reviewService.UpdateReportedLimitAcknowledgedBy(reviewId, metricCode, userId, metricId);
            ApiResponse<List<ApiResponse<int?>>> result = new ApiResponse<List<ApiResponse<int?>>>() { Data = data };
            return Ok(result);
        }

        [HttpGet("ReviewGetCurrentMetricStages")]
        public async Task<IActionResult> ReviewGetCurrentMetricStages(
            [FromQuery(Name = "ReviewIds")] string? reviewIds,
            [FromQuery(Name = "StageId")] int? stageId,
            [FromQuery(Name = "MetricId")] long? metricId)
        {
            if (string.IsNullOrWhiteSpace(reviewIds))
            {
                return BadRequest("ReviewIds is required");
            }

            var ids = new List<long>();
            foreach (var part in reviewIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!long.TryParse(part, out var id))
                {
                    return BadRequest($"Invalid ReviewId: {part}");
                }
                ids.Add(id);
            }

            var result = await _reviewService.GetCurrentMetricStages(ids, stageId, metricId);
            return Ok(ApiResponse<List<Dictionary<string, object?>>>.Ok(result));
        }

    }
}
