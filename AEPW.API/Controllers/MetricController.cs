using AEPW.Application.Contracts.Responses;
using AEPW.Application.Interfaces.Services.Metric;
using AEPW.Application.Services.Metric;
using Microsoft.AspNetCore.Mvc;
using AEPW.Application.DTOs.Metric;
using Microsoft.AspNetCore.Authorization;
namespace AEPW.API.Controllers

{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class MetricController : ControllerBase
    {
        private readonly IMetricService _metricService;

        public MetricController(IMetricService metricService)
        {
            _metricService = metricService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMetricHierarchy()
        {
            var response =
                await _metricService.GetMetricHierarchyAsync();

            return Ok(response);
        }
        //save
        [HttpPost("configuration")]
        public async Task<IActionResult> SaveMetricConfiguration(
            [FromBody] MetricSaveConfigurationDto request)
        {
            var response =
                await _metricService.SaveMetricConfigurationAsync(request);

            return Ok(response);
        }

        [HttpGet("GetAllMetrics")]
        public async Task<IActionResult> GetAllMetrics()
        {
            var response =
                await _metricService.GetAllMetricsAsync();

            return Ok(response);
        }

        [HttpGet("GetMetricsForProject/{ProjectId}")]
        public async Task<IActionResult> GetMetricsForProject(int ProjectId)
        {
            var response =  
                await _metricService.GetMetricsForProject(ProjectId);

            return Ok(response);
        }


        [HttpGet("GetAssignableMetricsForReview/{reviewId}")]
        public async Task<IActionResult> GetAssignableMetricsForReview(long reviewId)
        {
            var response =
                await _metricService.GetAssignableMetricsForReview(reviewId);

            return Ok(response);
        }
        
        [HttpGet("GetMetricsForReview/{ReviewId}")]
        public async Task<IActionResult> GetMetricsForReview(int ReviewId)
        {
            var response =
                await _metricService.GetMetricsForReview(ReviewId);

            return Ok(response);
        }
        
        [HttpGet("GetEvidenceTypeForMetric")]
        public async Task<IActionResult> GetEvidenceTypeForMetric()
        {
            string metricCodes = Request.Headers["MetricCodes"].ToString();
            string metricIds = Request.Headers["MetricIds"].ToString();
            var response =
                await _metricService.GetEvidenceTypeForMetric(metricCodes, metricIds);

            ApiResponse<List<MetricEvidenceTypeDto>> result = new ApiResponse<List<MetricEvidenceTypeDto>>()
            {
                Data = response,
                Success = true

            };
            //result.Data = response;

            return Ok(result);
        }

    }
 }
