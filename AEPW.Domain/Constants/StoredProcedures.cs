namespace AEPW.Domain.Constants;

public static class StoredProcedures
{
  public static class Logging
  {
    public const string InsertExceptionLog = "dbo.uspExceptionLogInsert";
  }
  public static class User
  {
    // READ procedures
    public const string GetById = "dbo.uspAppUserGetAppUserById";
    public const string GetByEmail = "uspUserGetUserByEmail";
    public const string GetAll = "dbo.uspAppUserGetAllAppUsers";
    public const string GetPaged = "usp_User_GetPaged";
    public const string GetRoleSummary = "dbo.uspAppUserGetAppUserRoleSummary";

    // CUD procedures 
    public const string Create = "dbo.uspAppUserInsertAppUser";
    public const string Update = "dbo.uspAppUserUpdateAppUser";
    public const string Delete = "dbo.uspAppUserDeleteAppUser";
    public const string BulkImport = "dbo.uspAppUserBulkUpsert";
  }

  public static class ImpactMaster
  {
    public const string GetAll = "dbo.uspImpactMasterGetAllImpactMasters";
    public const string GetById = "dbo.uspImpactMasterGetImpactMasterById";
    public const string GetReviewIds = "dbo.useGetImpactMasterReviewIds";
    public const string Insert = "dbo.uspImpactMasterSaveImpactMaster";
    public const string UpdateById = "dbo.uspImpactMasterUpdateImpactMasterById";
    public const string Delete = "dbo.uspDeleteImpactMaster";
    public const string Copy = "dbo.uspImpactMasterCopy";
    public const string FilterOptions = "dbo.uspImpactMasterFilterOptions";
  }
  public static class ImpactMasterMetric
  {
    public const string GetMetricsForImpactMaster = "dbo.uspImpactMasterGetMetricsForImpactMaster";
    public const string UpsertImpactMasterMetrics = "dbo.uspImpactMasterMetricUpsert";
  }
  public static class Program
  {
    public const string GetAll = "dbo.uspProgramGetAllPrograms";
    public const string GetById = "dbo.uspProgramGetProgramById";
    public const string Insert = "dbo.uspProgramInsertProgram";
    public const string UpdateById = "dbo.uspProgramUpdateProgram";
  }


  public static class Contact
  {
    public const string GetAll = "dbo.uspContactGetAllContacts";
    public const string Insert = "dbo.uspContactInsertContact";
    public const string Update = "dbo.uspContactUpdateContactsById";
  }
  public static class FundingRecipient
  {
    public const string GetAll = "dbo.uspFundingRecipientGetAllFundingRecipients";
    public const string Insert = "dbo.uspFundingRecipientInsertFundingRecipient";
    public const string Update = "dbo.uspFundingRecipientUpdateFundingRecipientById";
    public const string GetById = "dbo.uspFundingRecipientGetFundingRecipientById";
    public const string InsertContact =
"dbo.uspFundingRecipientInsertContact";
  }
  public static class Lookup
  {
    //Common Lookup fields
    public const string GetAllAppUsers = "dbo.uspGetAppUserLookup";
    public const string GetAllCities = "dbo.uspGetAllCities";
    public const string GetAllRecordTypes = "dbo.uspGetAllRecordTypesLookup";
    public const string GetImpactCategoryLookup = "dbo.uspGetImpactCategoryLookup";
    public const string GetImpactMasterLookup = "dbo.uspGetImpactMasterLookup";
    public const string GetPhases = "dbo.uspGetPhaseLookup";
    public const string GetAllPrograms = "dbo.uspGetProgramLookup";
    public const string GetAllRegions = "dbo.uspGetRegionLookup";
    public const string GetAllStatus = "dbo.uspGetMasterStatusByModule";
    public const string GetAllCountries = "dbo.uspGetCountryLookup";
    public const string GetAllFundingRecipients = "dbo.uspGetFundingRecipientLookup";
    public const string GetProjectTypeahead = "dbo.uspGetProjectLookup";
    public const string GetProjectManagers = "dbo.uspGetProjectManagers";
    public const string GetAllTimeZones = "dbo.uspGetTimeZonesLookup";

    //Metrics
    public const string GetAllMetricSources = "dbo.uspGetMetricSourceLookup";
    public const string GetAllMetricCategories = "dbo.uspMetricGetMetricCategories";
    public const string GetAllMetricUnits = "dbo.uspMetricGetMetricUnits";
    public const string GetAllParentMetrics = "dbo.uspGetParentMetrics";
    //System Roles
    public const string GetAllSystemRoles = "dbo.uspGetSystemRoleLookup";
    //Business Roles
    public const string GetBusinessRoles = "dbo.uspGetBusinessRolesLookup";
    public const string GetBusinessRoleUsers = "dbo.uspGetBusinessRoleUsersLookup";
    public const string GetBusinessRoleMappings = "dbo.uspGetBusinessRoleMappings";
    public const string DeleteBusinessRoleMapping = "dbo.uspDeleteBusinessRoleMapping";
    public const string SaveBusinessRoleMapping =
"dbo.uspSaveBusinessRoleMapping";
    public const string GetBusinessRoleMappingById =
"dbo.uspGetBusinessRoleMappingById";

    public const string GetReviewStages = "dbo.uspReviewGetReviewStages";
    public const string GetReviewStagesByUserId = "dbo.uspReviewGetReviewStagesByUserId";

  }
  public static class Review
  {

    //public const string SaveReview = "dbo.uspReviewSaveReview";
    public const string GetReviewPeriods = "dbo.uspReviewGetReviewPeriods";
    public const string GetReviews = "dbo.uspReviewGetReviews";
    //public const string GetReviewById = "dbo.uspReviewGetReviewById";
    //public const string SaveRiskAdjustmentAction = "dbo.uspReviewSaveRiskAdjustmentAction";
    public const string GetReviewExcelUploads = "dbo.uspReviewGetExcelUploads";
    //public const string SaveReviewUploadExcel = "dbo.uspRevie";

    public const string SaveReview = "dbo.uspReviewSaveReview";
    //public const string GetReviews = "dbo.uspReviewGetReviews_ToDelete";
    public const string GetReviewById = "dbo.uspReviewGetReviewById";
    //public const string GetReviewPeriods = "dbo.uspReviewGetReviewPeriods_ToDelete";
    public const string SaveReviewUploadDocuments = "dbo.uspReviewSaveReviewUploadDocument";
    //public const string GetReviewExcelUploads = "dbo.uspReviewGetExcelUploads_ToDelete";
    public const string SaveRiskAdjustmentAction = "dbo.uspReviewSaveRiskAdjustmentAction_ToDelete";


    public const string AddReviewComment = "dbo.uspReviewAddReviewComments";
    public const string SaveMetricEvidences = "dbo.uspReviewSaveMetricEvidences";
    public const string SaveMetricDataForReview = "dbo.uspReviewSaveMetricDataForReview";
    public const string SaveReviewStages = "dbo.uspReviewToggleReviewStages";
    public const string GetImpactsForLaunch = "dbo.uspReviewGetImpactsForLaunch";
    public const string LaunchReviews = "dbo.uspReviewLaunchReviews";
    public const string GetTopicWiseMetricsForReview = "dbo.uspReviewGetTopicWiseMetricsForReview";
    public const string SaveDataCollectionUrls = "dbo.uspReviewSaveDataCollectionUrls";
    public const string UpdateBaselineForReviewMetric = "dbo.uspReviewUpdateBaselineForReviewMetric";
    public const string RestatementUpdateBaselineForReviewMetric = "dbo.uspRestatementUpdateBaselineForReviewMetric";
    public const string GetReviewMetricDetailsForUpload = "dbo.uspReviewGetReviewMetricDetailsById";
    public const string SaveReviewMetricEvidences = "dbo.uspReviewSaveReviewMetricEvidences";
    public const string SaveReviewMetricConfig = "dbo.uspReviewSaveReviewMetricConfig";
    public const string DeleteReviewMetricEvidence = "dbo.uspReviewDeleteReviewMetricEvidence";
    public const string GetBaselineRemainder = "dbo.uspReviewGetBaselineRemainder";
    public const string ApproveRejectReviewMetric = "dbo.uspReviewApproveRejectReviewMetric";
    public const string GetReviewApprovalByMetricCode = "dbo.uspReviewGetReviewApprovalByMetricCode";
    public const string GetApprovalChanges = "dbo.uspReviewStageGetApprovalChanges";
    public const string UpdateReviewApprovalAssignee = "dbo.uspReviewApprovalUpdateAssignee";
    public const string GetToplevelAggregation = "dbo.uspReviewGetToplevelAggregation";
    public const string RecallTopLevelMetric = "dbo.uspReviewRecallToplevelMetric";
    public const string IssueRestatementForReview = "dbo.uspReviewIssueRestatementForReview";
    public const string UpdateReviewMetricFRComment = "dbo.uspReviewUpdateReviewMetricFRComment";
    public const string GetImpactsWithActiveReviews = "dbo.uspReviewGetImpactsWithActiveReviews";
    public const string GetAllMetricsWithValorisationRequired = "dbo.uspReviewGetAllMetricsWithValorisationRequired";
    public const string GetValorisationTypes = "dbo.uspReviewGetValorisationtypes";

    public const string DeleteReviewByIds = "dbo.uspReviewDeleteReview";
    public const string UpdateValorisationTypeForMetric = "dbo.uspReviewUpdateValorisationTypeForMetric";
    public const string UpdateReportedLimitAcknowledgedBy = "dbo.uspReviewUpdateReportedLimitAcknowledgedBy";
    public const string GetReviewImpactByReviewId = "dbo.uspReviewGetReviewImpactByReviewId";
    public const string GetCurrentMetricStages = "dbo.uspReviewGetCurrentMetricStages";
}

    public static class Metric
    {
        public const string GetEvidenceTypeForMetric = "dbo.uspMetricGetEvidenceTypeForMetric";
    }

  public static class EmailNotificationTemplate
  {
    public const string GetEmailNotificationData = "dbo.uspGetEmailNotificationData_NEW";
    public const string GetAllInterpolationKeywords ="dbo.uspEmailNotificationTemplateGetAllInterpolationKeywords";
    public const string GetAllEmailNotificationTemplates =
      "dbo.uspEmailNotificationTemplateGetAllEmailNotificationTemplates";
    public const string SaveEmailNotificationTemplate =
      "dbo.uspEmailNotificationTemplateSave";
  }

  public static class Authorization
  {
    public const string GetLoginContext = "dbo.uspAuthorizationGetLoginContext";
    public const string GetAppUserPageAccess = "dbo.uspGetAppUserPageAccess";
    public const string GetAppUserActionAccess = "dbo.uspGetAppUserActionAccess";
  }

  public static class RoleAccess
  {
    public const string GetRolePageAccess = "dbo.uspSystemRolePageGetAccess";
    public const string GetRolePageActionAccess = "dbo.uspSystemRolePageActionGetAccess";
    public const string SaveRolePageAccess = "dbo.uspSystemRolePageSaveAccess";
    public const string SaveRolePageActionAccess = "dbo.uspSystemRolePageActionSaveAccess";
  }

  public static class ReviewTask
  {
    public const string GetReviewApprovalTasks = "dbo.uspTaskGetReviewApprovalTasks";
  }

  public static class Evidence
  {
    public const string GetTypeByCode = "dbo.uspEvidenceGetTypeByCode";
    public const string GetMetricEvidenceTypeIdByEvidenceId = "dbo.uspEvidenceGetMetricEvidenceTypeIdByEvidenceId";
    public const string GetFieldsByTypeId = "dbo.uspEvidenceGetFieldsByTypeId";
    public const string SaveDetails = "dbo.uspEvidenceSaveDetails";
    public const string SaveRawExtraction = "dbo.uspEvidenceSaveRawExtraction";
    public const string UpdateProcessingResult = "dbo.uspEvidenceUpdateProcessingResult";
    public const string SetStatus = "dbo.uspEvidenceSetStatus";
    public const string GetReviewDetails = "dbo.uspEvidenceGetReviewDetails";
    public const string SaveReviewExtractedFields = "dbo.uspSaveEvidenceReviewExtractedFields";
    public const string GetReviewIdByEvidenceId = "dbo.uspEvidenceGetReviewIdByEvidenceId";
    public const string UpdateRawAdjustedValuesForTopMetrics = "dbo.uspUpdateRawAdjustedValuesForTopMetrics";
  }

  public static class Dashboard
  {
    public const string GetPortfolioByCategories = "dbo.uspGetDashboadPortfolioAdjustmentByCategoryandPeriod";
    public const string GetPortfolioList = "dbo.usp_GetPortfolioList";
  }

  public static class Reports
  {
    public const string GetSummarySinceInceptionReport = "dbo.usp_GetSummarySinceInceptionReport";
    public const string GetReviewMetricTrackerReport = "dbo.usp_GetReviewMetricTrackerReport";
    public const string GetProjectsOnlyReport = "dbo.usp_GetProjectsOnlyReport";

    public const string GetProjectReportCPM = "dbo.usp_GetProjectReportCPM";

    public const string GetPerTopLevelMetric = "dbo.usp_GetPerTopLevelMetricReport";
    public const string GetAnnualPortfolioRiskAdjusted =
    "usp_GetAnnualPortfolioRiskAdjustedReport";

    //--------------------Filter options ----------------------
    public const string MetricFilterOptions = "dbo.usp_GetMetricCascadingFilterOptions";

    public const string ReportFilterProjectCascadingOptions = "usp_ReportFilterProjectCascadingOptions";

    public const string ReportIndependentFilterOptions = "usp_ReportFilterIndependentOptions";
  }

  public static class Audit
  {
    public const string GetAppUserAudit = "dbo.usp_GetAppUserAudit";
    public const string GetAppUserSystemRoleAudit = "dbo.usp_GetAppUserSystemRoleAudit";
    public const string GetMetricAudit = "dbo.usp_GetMetricAudit";
    public const string GetImpactMasterAudit = "dbo.usp_GetImpactMasterAudit";
    public const string GetImpactMasterMetricAudit = "dbo.usp_GetImpactMasterMetricAudit";
    public const string GetReviewAudit = "dbo.usp_GetReviewAudit";
    public const string GetReviewMetricAudit = "dbo.usp_GetReviewMetricAudit";
    public const string GetReviewApprovalAudit = "dbo.usp_GetReviewApprovalAudit";
    public const string GetRestatementAudit = "dbo.usp_GetRestatementAudit";
  }
}
