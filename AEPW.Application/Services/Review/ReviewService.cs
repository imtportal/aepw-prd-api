using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs;
using AEPW.Application.DTOs.Lookup;
using AEPW.Application.DTOs.Metric;
using AEPW.Application.DTOs.Notification;
using AEPW.Application.DTOs.Review;
using AEPW.Application.Interfaces.Repositories.Metric;
using AEPW.Application.Interfaces.Repositories.Review;
using AEPW.Application.Interfaces.Services.AzureBlobService;
using AEPW.Application.Interfaces.Services.ExcelImportExportService;
using AEPW.Application.Interfaces.Services.ExcelParser;
using AEPW.Application.Interfaces.Services.Notification;
using AEPW.Application.Interfaces.Services.Review;
using AEPW.Application.Interfaces.Services.SharepointService;
using AEPW.Application.Services.AzureBlobService;
using AEPW.Application.Services.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.DeviceManagement.ManagedDevices.Item.Retire;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Graph.Models.Security;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using static AEPW.Domain.Constants.StoredProcedures;
namespace AEPW.Application.Services.Review
{
    public sealed class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IDataCollectionExcelGeneratorService _excelGeneratorService;
        private readonly ISharepointService _sharepointService;
        private readonly IExcelParserService _excelParserService;
        private readonly IServiceBusService _serviceBusService;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IAzureBlobService _azureBlobService;

        public ReviewService(
            IReviewRepository reviewRepository
            ,
            ISharepointService sharepointService
            ,
            IDataCollectionExcelGeneratorService excelGeneratorService
            ,
            IExcelParserService excelParserService
            ,
            IServiceBusService serviceBusService
            ,
            IConfiguration configuration
            ,
            IAzureBlobService azureBlobService
        )
        {
            _reviewRepository = reviewRepository;
            _sharepointService = sharepointService;
            _excelGeneratorService = excelGeneratorService;
            _excelParserService = excelParserService;
            _serviceBusService = serviceBusService;
            _azureBlobService = azureBlobService;

            _configuration = configuration;
            Console.WriteLine(configuration);
        }

        public async Task<IReadOnlyList<string>> SaveReview(CreateReviewRequestDto input)
        {
            IReadOnlyList<string> repoResponse = await _reviewRepository.SaveReview(input);

            long reviewId = long.Parse(repoResponse[0]);

            // calling the async method WITHOUT WAITING for Excel Generation, Sharepoint Upload, and DB update for storing the Sharepoint url in DB
            const string dataCollectionStageName = "Data Collection";
            List<string> launchStageNames = new List<string>();
            launchStageNames.Add(dataCollectionStageName);
            launchStageNames.Add(dataCollectionStageName.ToLower());
            launchStageNames.Add(dataCollectionStageName.ToUpper());
            launchStageNames.Add(dataCollectionStageName.Replace(" ", ""));
            launchStageNames.Add(dataCollectionStageName.Replace(" ", "").ToLower());
            launchStageNames.Add(dataCollectionStageName.Replace(" ", "").ToUpper());

            if (input.ReviewId != null && launchStageNames.Contains(input.Stage))
            {
                try
                {
                    //GenerateExcelAndSendNotification(reviewId, input.UserId);
                }
                catch (Exception ex)
                {
                    //_logger
                }
            }


            return repoResponse;
        }
        public async Task<IReadOnlyList<ReviewPeriodDto>> GetReviewPeriods(long projectId, int fromPresent)
        {
            return await _reviewRepository.GetReviewPeriods(projectId, fromPresent);
        }

        public async Task<IReadOnlyList<ReviewStageLookupDto>> GetReviewStages()
        {
            return await _reviewRepository.GetReviewStages();
        }

        public async Task GenerateExcelAndSendNotification(List<NewReviewsDto> reviews, long userId)
        {
            List<NewReviewsDto> corpReviews = new List<NewReviewsDto>();
            //throw new Exception();
            List <ReviewUploadExcelJSON> reviewUploadExcelJSON = new List<ReviewUploadExcelJSON>();
            // saving quarterFolderUrls
            List<ReviewDataCollectionUrlUpdateDto> dataCollectionList = new List<ReviewDataCollectionUrlUpdateDto>();

            List<long> reviewIds = new List<long>();
            foreach (NewReviewsDto review in reviews)
            {
                reviewIds.Add(review.ReviewId);
            }

          
            foreach (NewReviewsDto review in reviews)
            {
                // do this only for Project type records, skip for Corporate
                if (String.IsNullOrEmpty(review.ProjectPin))
                {
                    continue;
                }
                if (review.RecordType?.ToUpper() == "CORPORATE")
                {
                    corpReviews.Add(review);
                    continue;
                }

                //review.
                //string dataCollectionUrl = await _sharepointService.CreateLaunchFoldersInSharepoint(folderParam);
                SharepointHierarchyFolderCreationDto folderParam = await _reviewRepository.GetTopicWiseMetricsForReview(review.ReviewId);
                string dataCollectionUrl = await _sharepointService.CreateDataCollectionFolderInSharepoint(folderParam);
                if (!String.IsNullOrEmpty(dataCollectionUrl))
                {
                    dataCollectionList.Add(new ReviewDataCollectionUrlUpdateDto()
                    {
                        ReviewId = review.ReviewId,
                        DataCollectionUrl = dataCollectionUrl
                    });
                }

                var bytes = await GenerateReviewExcelAsync((int)review.ReviewId);
                //Thread.Sleep(2000);

                // creating folder for the project and quarter associated with this review
                //review.ProjectPin + "/" + review.Quarter + "/" + "Data Collection";


                MemoryStream stream = new MemoryStream(bytes);
                stream.Position = 0;
                string fileName = review.ProjectTitle + "_" + review.Quarter + "_" + review.ReviewId + ".xlsx";

                //string folderPath = _configuration["SharepointReviewRootDirectoryUrl"] + "/" + review.ProjectPin + "/" + review.Quarter + "/" + "Data Collection";
                string folderPath = review.ProjectPin + "/" + review.Quarter + "/" + "Data Collection";

                try
                {
                    DriveItem? sharePointResponse = await _sharepointService.UploadFileAsync(stream, fileName, folderPath);


                    ReviewUploadExcelJSON reviewUploadExcelJSONElem = new ReviewUploadExcelJSON();

                    reviewUploadExcelJSONElem.ReviewId = review.ReviewId;
                    reviewUploadExcelJSONElem.FileName = fileName;
                    reviewUploadExcelJSONElem.ViewUri = "";
                    reviewUploadExcelJSONElem.DownloadUri = "";
                    reviewUploadExcelJSONElem.ParseStatus = "QUEUED"; // Initial Status
                    reviewUploadExcelJSONElem.ParseLogJson = "";
                    reviewUploadExcelJSONElem.StagedValuesJson = "";


                    if (sharePointResponse != null)
                    {
                        bool valid = false;
                        if (!String.IsNullOrEmpty(sharePointResponse.WebUrl))
                        {
                            reviewUploadExcelJSONElem.ViewUri = sharePointResponse.WebUrl;
                            valid = true;

                            //if (reviewId != null)
                            //{
                            //    NotificationMessageDto notificationMsg = new NotificationMessageDto();
                            //    notificationMsg.NotificationType = "LaunchReview";
                            //    notificationMsg.EntityId = ((long)reviewId);
                            //    notificationMsg.EntityId = ((long)reviewId);
                            //    notificationMsg.DocumentId = ((long)documentId);

                            //    _serviceBusService.PublishNotificationAsync(notificationMsg);
                            //}
                        }

                        string downloadUrl = String.Empty;

                        try
                        {
                            downloadUrl = (string)sharePointResponse.AdditionalData["@microsoft.graph.downloadUrl"];

                        }
                        catch (Exception ex)
                        {
                        }

                        if (!String.IsNullOrEmpty(downloadUrl))
                        {
                            reviewUploadExcelJSONElem.DownloadUri = downloadUrl;
                            valid = true;
                        }

                        if (valid)
                        {
                            reviewUploadExcelJSON.Add(reviewUploadExcelJSONElem);
                        }
                    }


                }
                catch (Exception ex)
                {
                }
            }

            List<ApiResponse<long>> response = await _reviewRepository.SaveReviewUploadDocuments(reviewUploadExcelJSON, userId);

            await _reviewRepository.SaveDataCollectionUrls(dataCollectionList, userId);

            NotificationMessageDto notificationMsg = new NotificationMessageDto();
            notificationMsg.NotificationType = "LAUNCHREVIEW";
            notificationMsg.ReviewIds = String.Join(", ", reviewIds);
            await _serviceBusService.PublishNotificationAsync(notificationMsg);


            // for project
            CreateMetricFoldersInSharepoint(reviews, userId);
            // for corporate
            //CreateMetricFoldersInSharepoint(corpReviews, userId);

            //_serviceBusService.PublishNotificationAsync(notificationMsg);

        }


        public async void CreateMetricFoldersInSharepoint(List<NewReviewsDto> reviews, long userId)
        {

            foreach(NewReviewsDto review in reviews)
            {
                SharepointHierarchyFolderCreationDto folderParam = await _reviewRepository.GetTopicWiseMetricsForReview(review.ReviewId);
                await _sharepointService.CreateLaunchFoldersInSharepoint(folderParam, review.RecordType.ToLower());


                //if (!String.IsNullOrEmpty(dataCollectionUrl))
                //{
                //    dataCollectionList.Add(new ReviewDataCollectionUrlUpdateDto()
                //    {
                //        ReviewId = review.ReviewId,
                //        DataCollectionUrl = dataCollectionUrl
                //    });
                //}

            }

            //await _reviewRepository.SaveDataCollectionUrls(dataCollectionList, userId);

        }

        public async Task<IReadOnlyList<ReviewDto>> GetReviews(GetReviewFilters filters)
        {
            if ((filters.PM != null) && filters.PM.ToLower() == "all")
            {
                filters.PM = String.Empty;
            }
            if ((filters.Stage != null) && filters.Stage.ToLower() == "all")
            {
                filters.Stage = String.Empty;
            }
            if ((filters.ReviewPeriodId != null) && filters.ReviewPeriodId.ToLower() == "all")
            {
                filters.ReviewPeriodId = String.Empty;
            }
            return await _reviewRepository.GetReviews(filters);
        }

        public async Task<IReadOnlyList<ReviewDto>> GetReviewById(long reviewId, string sourcePage, long userId, int? stage)
        {
            return await _reviewRepository.GetReviewById(reviewId, userId, sourcePage, stage);
        }

        public async Task<ApiResponse<Int64>> SaveRiskAdjustmentAction(Int64 reviewId, List<MetricRiskResponseJSON> metricRiskResponseJSON, int userId)
        {
            var response = await _reviewRepository.SaveRiskAdjustmentAction(reviewId, metricRiskResponseJSON, userId);

            if (response != null && response.Count > 0)
            {
                return new ApiResponse<Int64> { Success = true, Data = Int64.Parse(response[0]), Message = response[1] };
            }

            return new ApiResponse<Int64> { Success = false, Data = 0, Message = "Some Error Occured" };
        }

        public async Task<byte[]> GenerateReviewExcelAsync(Int64 reviewId)
        {
            var rows =
                await _reviewRepository
                    .GetReviewDataAsync(
                        reviewId);

            List<string> metricCodes = await _reviewRepository.GetAllMetricsWithValorisationRequired();

            List<string> valorisationTypeNames = new List<string>();
            List<ValorisationTypeDto> valorisationTypes = await _reviewRepository.GetValorisationTypes();

            foreach (ValorisationTypeDto valorisationType in valorisationTypes)
            {
                valorisationTypeNames.Add(valorisationType.Description);
            }

            Dictionary<string, bool> metricCodesMap = new Dictionary<string, bool>();
            foreach (string metricCode in metricCodes)
            {
                metricCodesMap.Add(metricCode, true);
            }

            byte[] bytes = await _excelGeneratorService.GenerateWorkbook(rows, metricCodesMap, string.Join(",", valorisationTypeNames));

            Stream stream = new MemoryStream(bytes);
            stream.Position = 0;


            return bytes;
        }

        public async Task<List<ReviewExcelUploadDto>> GetReviewExcelUploads(Int64 reviewId)
        {
            return await _reviewRepository.GetReviewExcelUploads(reviewId);

        }

        public async Task<string> DownloadFileAsync(string fileName, string accessToken)

        {
            return await _sharepointService.DownloadFileAsync(fileName, String.Empty);
        }

        public async Task<bool> UpdateBaselineForMetric(long reviewId, List<UpdateReviewMetricsValuesDto> input, long UserId)
        {
            return await _reviewRepository.UpdateBaselineForMetric(reviewId, input, UserId);
        }

        public async Task<bool> RestatementUpdateBaselineForMetric(long reviewId, List<UpdateReviewMetricsValuesDto> input, long UserId)
        {
            return await _reviewRepository.RestatementUpdateBaselineForMetric(reviewId, input, UserId);
        }

        public async Task<List<ReviewCommentDto>> AddReviewComments(List<AddReviewCommentDto> reqBody)
        {
            var result = await _reviewRepository.AddReviewComments(reqBody);
            List<UserMentionedEmailNotificationInputDto> emailInput = new List<UserMentionedEmailNotificationInputDto>();
            List<long> reviewIds = new List<long>();

            foreach(AddReviewCommentDto commentDto in reqBody)
            {
                reviewIds.Add(commentDto.ReviewId);

                foreach(var mentionedUserId in commentDto.MentionedUsers)
                {
                    emailInput.Add(new UserMentionedEmailNotificationInputDto()
                    {
                        ReviewId = commentDto.ReviewId
                        , CommentBody = commentDto.Body
                        , CommentedByUserId = commentDto.UserId
                        , TaggedUserId = mentionedUserId
                        , Stage = (commentDto.Stage).ToString()
                    });

                }

            }

            string reviewMetrics = JsonSerializer.Serialize(emailInput);

            NotificationMessageDto notificationMsg = new NotificationMessageDto();
            notificationMsg.NotificationType = "USERMENTIONED";
            notificationMsg.ReviewIds = String.Join(", ", reviewIds);
            notificationMsg.ReviewMetrics = reviewMetrics;
            await _serviceBusService.PublishNotificationAsync(notificationMsg);



            return result;
        }

        public async Task<List<string>> SaveReviewStages(
      List<SaveReviewStageRequest> reviewStageJson,
      long userId)
        {
            return await _reviewRepository.SaveReviewStages(
                reviewStageJson,
                userId);
        }

        public async Task<List<LaunchableProjectDto>> GetLaunchableImpacts(string quarter, string status, string impactName, int pageNo, int skippedRows, int ietmsPerPage)
        {
            return await _reviewRepository.GetLaunchableImpacts(quarter, status, impactName, pageNo, skippedRows, ietmsPerPage);
        }

        public async Task<List<NewReviewsDto>> BulkLaunchReviews(List<long> impactIds, long? quarter, long userId)
        {
            List<NewReviewsDto> newReviews = await _reviewRepository.BulkLaunchReviews(impactIds, quarter, userId);

            //GenerateExcelAndSendNotification(newReviews, userId);

            return newReviews;
        }

        public static decimal? ParseMetricValue(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return null;

            var cleaned = new string(
                rawValue
                    .Where(c => char.IsDigit(c) || c == '.')
                    .ToArray());

            return decimal.TryParse(cleaned, out var result)
                ? result
                : null;
        }

        public async Task<ApiResponse<List<ReviewExcelImportDto>>> ParseReviewExcelAsync(IFormFile file, long reviewId, long userId)
        {
            using var stream = file.OpenReadStream();
            string projectPin = String.Empty;

            var parsedData = _excelParserService.ParseWorkbook(
                stream,
                reviewId,
                out projectPin);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };


            //Console.WriteLine(jsonData);

            foreach (var reviewMetricValue in parsedData)
            {
                if (reviewMetricValue.RawValue != null)
                {
                    reviewMetricValue.RawValue = (ParseMetricValue(reviewMetricValue.RawValue)).ToString();
                }
            }


            var jsonData = JsonSerializer.Serialize(parsedData, options);

            List<ApiResponse<int>>  response = await _reviewRepository.SaveMetricDataForReview(
            userId,
            projectPin,
            jsonData);

            ApiResponse<int> firstResponse = response[0];

            ApiResponse<List<ReviewExcelImportDto>> result = new ApiResponse<List<ReviewExcelImportDto>>()
            {
                 Data = parsedData,
                 Success = firstResponse.Success,
                 Message = firstResponse.Message
            };


            // Upload back to Data Collection folder, overwriting the existing one
            //hereherhere

            return result;
        }

        public async Task<List<ApiResponse<long>>> AddReviewMetricEvidence(long? reviewId, long? metricId, long? userId, long? evidenceTypeId, List<IFormFile> files)
        {
            ReviewMetricDetailsForEvidenceUploadDto reviewMetricDetails = null;

            if (metricId != null && reviewId != null)
            {
                reviewMetricDetails = await _reviewRepository.GetReviewMetricDetailsForUpload((long)reviewId, (long)metricId, evidenceTypeId);
            }

            List<ReviewMetricEvidencesJsonDto> evidences = new List<ReviewMetricEvidencesJsonDto>();
            foreach (IFormFile file in files)
            {
                Stream stream = file.OpenReadStream();

                string evidenceTypeFolderName = String.Empty;

                string filePath = $"{reviewMetricDetails.ProjectPin}/{reviewMetricDetails.Quarter}/{reviewMetricDetails.CategoryCode}/{reviewMetricDetails.ParentMetricCode}/{reviewMetricDetails.MetricCode}";

                if (!String.IsNullOrEmpty(reviewMetricDetails.EvidenceTypeCode))
                {
                    filePath = (filePath + $"/{reviewMetricDetails.EvidenceTypeCode}");
                }

                DriveItem? sharepointResponse = await _sharepointService.UploadFileAsync(stream, file.FileName, filePath);

                if (sharepointResponse != null && metricId != null)
                {
                    string downloadUrl = (string)sharepointResponse.AdditionalData["@microsoft.graph.downloadUrl"];

                    ReviewMetricEvidencesJsonDto evidence = new ReviewMetricEvidencesJsonDto()
                    {
                        MetricId = (long)metricId
                            ,
                        FileName = file.FileName
                            ,
                        ItemId = sharepointResponse.Id
                            ,
                        FileSizeBytes = file.Length
                            ,
                        FileUriView = sharepointResponse.WebDavUrl ?? sharepointResponse.WebUrl
                            ,
                        FileUriDownload = downloadUrl
                            
                    };

                    if (evidenceTypeId != null)
                    {
                        evidence.EvidenceTypeId = evidenceTypeId;
                    }    

                    evidences.Add(
                        evidence
                    );

                }

            } 
            return await _reviewRepository.SaveReviewMetricEvidences(reviewId, userId, evidences);


            //return new List<ApiResponse<long>>() { new ApiResponse<long>() { Data = 0, Message = "No response from Sharepoint" } };
        }
        //public async Task<List<ApiResponse<long>>> SaveReviewMetricEvidences(long reviewId, List<ReviewMetricEvidencesJsonDto> evidences, long userId)
        //{
        //    return await _reviewRepository.SaveReviewMetricEvidences(reviewId, evidences, userId);

        //}

        public async Task<List<ApiResponse<string>>> SaveReviewMetricConfig(long reviewId, List<string> metricIds, long userId)
        {
            List<ApiResponse<string>> result = new List<ApiResponse<string>>();
            List<string> itemIdsToDelete = new List<string>();
            const string deletedItemIdKeyword = "deleteFromSharepoint";
            var resultFromRepo = await _reviewRepository.SaveReviewMetricConfig(reviewId, metricIds, userId, deletedItemIdKeyword);

            foreach(var elem in resultFromRepo)
            {
                if (elem.Message == deletedItemIdKeyword)
                {
                    itemIdsToDelete.Add(elem.Data);
                    continue;
                }

                result.Add(elem);
            }

            foreach(string itemId in itemIdsToDelete)
            {
                _sharepointService.DeleteFileAsync(itemId);
            }

            return result;
        }

        public async Task<List<ApiResponse<long>>> DeleteReviewMetricEvidence(string itemId, long userId)
        {
            bool deletionSuccessful = await _sharepointService.DeleteFileAsync(itemId);
            if (deletionSuccessful)
            {
                return await _reviewRepository.DeleteReviewMetricEvidence(itemId, userId);
            }

            return new List<ApiResponse<long>>() { new ApiResponse<long>() { Data = 0, Message = "Some error occured", Success = false } };
        }

        public async Task<List<BaselineRemainderDto>> GetBaselineRemainder(long reviewId)
        {

            return await _reviewRepository.GetBaselineRemainder(reviewId);
        }


        public async Task<List<TopLevelMetricAggregationDto>> GetToplevelAggregation(string topicName, string periodCode, string applicablestages, long userId)
        {
            return await _reviewRepository.GetToplevelAggregation(topicName, periodCode, applicablestages, userId);
        }

        public async Task<List<ProjectLevelAggregationDto>> GetProjectMetricAggregation(string topicName, string periodCode, string applicablestages, long userId)
        {
            return await _reviewRepository.GetProjectMetricAggregation(topicName, periodCode, applicablestages, userId);
        }

        public async Task<List<ReviewMetricCodeForAdminL4ApprovalRejectionDto>> GetReviewMetricCodesForAdminOrL4Approval(string topicName, string periodCode, List<ProjectPinParentCodeDto> projectPinParentCodes)
        {
            return await _reviewRepository.GetReviewMetricCodesForAdminOrL4Approval(topicName, periodCode, projectPinParentCodes);
        }

        public async Task<ReviewStagePromotionResultDto> ApproveRejectReviewMetric(List<ReviewMetricApproveRejectDto> metricActionJSON, long userId)
        {
            ReviewStagePromotionResultDto result = await _reviewRepository.ApproveRejectReviewMetric(metricActionJSON, userId);

            List<long> reviewIds = new List<long>();

            NotificationMessageDto emailInput = new NotificationMessageDto();

            bool atleastOneReviewPromoted = false;

            // calling ServiceBus for L3 to Admin
            // foreach (StagePromotionDetailDto promDetail in result.stagePromotionDetails)
            // {
                //if (promDetail.PromotionEligible)
                //{
                    //reviewIds.Add(promDetail.ReviewId);
                    //atleastOneReviewPromoted = true;
                //}
            // }

            // for rejections
            NotificationMessageDto notifmsg = new NotificationMessageDto();
            List<ReviewMetricApproveRejectDto> ReviewMetrics = new List<ReviewMetricApproveRejectDto>();

            foreach (ReviewMetricApproveRejectDto action in metricActionJSON)
            {
                if (action.Decision.ToLower() == "rejected" || action.Decision.ToLower() == "reject")
                {
                    //var ob = new 
                    //{
                    //    ReviewId = action.ReviewId,
                    //    MetricCode = action.MetricCode.Trim(),
                    //    Decision = "Rejected"
                    //};

                    ReviewMetrics.Add(action);

                }
            }
            notifmsg.NotificationType = "METRICSREJECTION";
            notifmsg.ReviewIds = null;
            notifmsg.ReviewMetrics = JsonSerializer.Serialize(ReviewMetrics);


            var k = await _serviceBusService.PublishNotificationAsync(notifmsg);


            List<ReviewMetricApproveRejectDto> metricActionjsonForClosedNotifcation = new List<ReviewMetricApproveRejectDto>();


            Dictionary<long, bool> closedReviews = new Dictionary<long, bool>();
            foreach (ReviewPromotionNextStageDetailsDto newReviewStage in result.reviewPromotionNextStageDetails)
            {
                if (newReviewStage.NextStageName?.ToLower() == "closed")
                {
                    closedReviews.Add(newReviewStage.ReviewId, true);
                }


            }
            foreach (var metric in metricActionJSON)
            {
                if (closedReviews.ContainsKey(metric.ReviewId))
                {
                    metricActionjsonForClosedNotifcation.Add(metric);
                }
            }
            string reviewMetricsJson = JsonSerializer.Serialize(metricActionJSON);
            if (metricActionjsonForClosedNotifcation.Count > 0)
            {
                emailInput = new NotificationMessageDto()
                {
                    ReviewIds = "",
                    NotificationType = "REVIEWCLOSED",
                    ReviewMetrics = reviewMetricsJson
                };
                _serviceBusService.PublishNotificationAsync(emailInput);
            }


            if (atleastOneReviewPromoted == true)
            {
                emailInput = new NotificationMessageDto()
                {
                    ReviewIds = String.Join(",", reviewIds),
                    NotificationType = "STAGEPROMOTION"
                };

                _serviceBusService.PublishNotificationAsync(emailInput);
            }


            // toplevel
            emailInput = new NotificationMessageDto()
            {
                ReviewIds = String.Empty,
                NotificationType = "STAGEPROMOTION",
                ReviewMetrics = reviewMetricsJson
            };

            _serviceBusService.PublishNotificationAsync(emailInput);

            return result;
        }

        public async Task<List<ReviewApprovalProgressDto>> GetReviewApprovalByMetricCode(long reviewId, string metricCode, long userId)
        {
            return await _reviewRepository.GetReviewApprovalByMetricCode(reviewId, metricCode, userId);
        }

        public async Task<ReviewApprovalChangesPreviewDto> GetApprovalChangesAsync(
            IReadOnlyList<ReviewStageApprovalChangeRequestDto> stageChanges,
            CancellationToken cancellationToken = default)
        {
            var (disableRows, enableRows) =
                await _reviewRepository.GetApprovalChangesAsync(stageChanges, cancellationToken);

            return new ReviewApprovalChangesPreviewDto(
                GroupApprovalChanges(disableRows),
                GroupApprovalChanges(enableRows));
        }

        private static IReadOnlyList<ReviewStageApprovalChangesDto> GroupApprovalChanges(
            IReadOnlyList<ReviewApprovalChangeRepositoryRowDto> rows)
        {
            return rows
                .GroupBy(row => new { row.StageCode, row.StageName })
                .Select(group => new ReviewStageApprovalChangesDto(
                    group.Key.StageCode,
                    group.Key.StageName,
                    group.Select(row => new ReviewApprovalChangeRecordDto(
                        row.ReviewApprovalId,
                        row.ReviewId,
                        row.ReviewTitle,
                        row.MetricId,
                        row.MetricCode,
                        row.MetricName,
                        row.StatusId,
                        row.StatusCode,
                        row.CurrentStatus,
                        row.AssigneeUserId,
                        row.StepNo,
                        row.AttemptNo,
                        row.Action)).ToList()))
                .ToList();
        }

   
public async Task<bool> UpdateReviewApprovalAssignee(
    UpdateReviewApprovalAssigneeDto input)
        {
            var oldAssigneeUserId =
                await _reviewRepository.UpdateReviewApprovalAssignee(input);

            var notificationData = new[]
{
    new
    {
        ReviewIds = input.ReviewId,
        StageCode = input.ReviewStageCode,
        MetricCode = input.MetricCode,
        OldAssigneeId = oldAssigneeUserId,
        NewAssigneeId = input.NewAssigneeUserId,
        ReassignedByUserId = input.UpdatedBy
    }
};

            var notification = new NotificationMessageDto
            {
                NotificationType = "TaskReassignment",
                ReviewIds = input.ReviewId.ToString(),
                ReviewMetrics = JsonSerializer.Serialize(notificationData)
            };

            var publishResult =
                await _serviceBusService.PublishNotificationAsync(notification);

            if (!publishResult.Success || publishResult.Data != true)
            {
                throw new InvalidOperationException(
                    publishResult.Message);
            }

            return true;
        }

        public async Task<List<RecallTopLevelMetricResultDto>> RecallToplevelMetric(RecallTopLevelMetricInput input)
        {
            return await _reviewRepository.RecallToplevelMetric(input);
        }


        
public async Task<List<ApiResponse<long>>> IssueRestatementForReview(
    StartEndRestatementDto input)
        {
            // Execute the database operation first.
            var result = await _reviewRepository.IssueRestatementForReview(input);

            // Publish notification only if the database operation succeeded.
            var operationSucceeded = result.Any(r =>
                r.Data != 0 &&
                r.Message == "Operation Successful");

            if (operationSucceeded)
            {
                var notification = new NotificationMessageDto
                {
                    NotificationType = "Restatement",
                    ReviewIds = input.ReviewId.ToString() ?? string.Empty,
                    ReviewMetrics = JsonSerializer.Serialize(input.Metrics)
                };

                var publishResult =
                    await _serviceBusService.PublishNotificationAsync(notification);

                if (!publishResult.Success || publishResult.Data != true)
                {
                    throw new InvalidOperationException(publishResult.Message);
                }
            }

            return result;
        }

        public async Task<List<ApiResponse<long>>> UpdateReviewMetricFRComment(UpdateReviewMetricFRCommentDto input)
        {
            return await _reviewRepository.UpdateReviewMetricFRComment(input);
        }

        public async Task<List<LaunchableProjectDto>> GetImpactsWithActiveReviews(int pageNo, int skippedRows, int itemsPerPage, string quarter, string status, string impactName)
        {
            return await _reviewRepository.GetImpactsWithActiveReviews(pageNo, skippedRows, itemsPerPage, quarter, status, impactName);
        }

        public async Task<List<ApiResponse<long>>> DeleteReviewByIds (List<long> reviewIdsInput, long userId)
        {
            List<string> deleteFolders = new List<string>();
            List<NewReviewsDto> deletedReviewsMetaData = await _reviewRepository.GetReviewImpactByReviewId(reviewIdsInput);

            foreach(NewReviewsDto reviewMetaData in deletedReviewsMetaData)
            {
                deleteFolders.Add(reviewMetaData.ProjectPin);
            }

            _sharepointService.DeleteFoldersAsync(deleteFolders);

            return await _reviewRepository.DeleteReviewByIds(reviewIdsInput, userId);
        }

        public async Task SyncMetricFoldersForReview(long reviewId)
        {

        }
        
        public async Task<List<ValorisationTypeDto>> GetValorisationTypes()
        {
            return await _reviewRepository.GetValorisationTypes();
        }

        public async Task<List<ApiResponse<long?>>> UpdateValorisationTypeForMetric(long reviewId, string metricCode, int valorisationTypeId)
        {
            return await _reviewRepository.UpdateValorisationTypeForMetric(reviewId, metricCode, valorisationTypeId);
        }

        public async Task<List<ApiResponse<int?>>> UpdateReportedLimitAcknowledgedBy(long reviewId, string metricCode, long userId, long? metricId)
        {
            return await _reviewRepository.UpdateReportedLimitAcknowledgedBy(reviewId, metricCode, userId, metricId);
        }

        public async Task<List<Dictionary<string, object?>>> GetCurrentMetricStages(List<long> reviewIds, int? stageId, long? metricId)
        {
            var result = await _reviewRepository.GetCurrentMetricStages(reviewIds, stageId, metricId);

            var notification = new NotificationMessageDto
            {
                NotificationType = "RECALL",
                ReviewIds = string.Join(",", reviewIds),
                ReviewMetrics = JsonSerializer.Serialize(result)
            };

            // Convert the JSON string to JsonElement for the final payload
            var notificationJson = JsonSerializer.Serialize(new
            {
                notification.NotificationType,
                notification.ReviewIds,
                ReviewMetrics = JsonDocument.Parse(notification.ReviewMetrics).RootElement
                    .Clone(),
                notification.StageCode,
                notification.MetricCode,
                notification.OldAssigneeId,
                notification.NewAssigneeId
            });

            await _serviceBusService.PublishNotificationAsync(notification);

            return result;
        }

        public async Task UpdateTemplateExcelForReview(long reviewId, IFormFile file)
        {
            SharepointHierarchyFolderCreationDto folderParam = await _reviewRepository.GetTopicWiseMetricsForReview(reviewId);
            
            string folderPath = folderParam.ProjectPin + "/" + folderParam.Quarter + "/" + "Data Collection";

            await _sharepointService.UpdateTemplateExcelForReview(folderPath, file);
        }

    }
}
