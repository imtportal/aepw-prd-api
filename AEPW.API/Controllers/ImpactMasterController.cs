using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.ImpactMaster;
using AEPW.Application.Interfaces.Services;
using AEPW.Application.Interfaces.Services.ImpactMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace AEPW.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ImpactMasterController : ControllerBase
    {
        private readonly IImpactMasterService _impactMasterService;

        public ImpactMasterController(IImpactMasterService impactMasterService)
        {
            _impactMasterService = impactMasterService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllImpactMasters(
            [FromQuery] GetImpactMastersRequestDto request,
            CancellationToken cancellationToken)
        {
            var result =
                await _impactMasterService.GetAllImpactMastersAsync(
                    request,
                    cancellationToken);

            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetImpactMasterById(
            long id,
            CancellationToken cancellationToken)
        {
            var result = await _impactMasterService.GetImpactMasterByIdAsync(id, cancellationToken);

            if (result is null)
                return NotFound(ApiResponse<object>.Fail("Impact Master not found."));

            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("GetReviewIds")]
        public async Task<IActionResult> GetReviewIds(
            [FromQuery] long impactMasterId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            CancellationToken cancellationToken)
        {
            var result = await _impactMasterService.GetReviewIds(
                impactMasterId,
                startDate,
                endDate,
                cancellationToken);

            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpPost]
        public async Task<IActionResult> CreateImpactMaster(
            [FromBody] CreateImpactMasterRequestDto request,
            CancellationToken cancellationToken)
        {
            var result =
                await _impactMasterService.CreateImpactMasterAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetAllImpactMasters),
                ApiResponse<object>.Ok(result, "Impact Master created successfully."));
        }
        //inital metrics on create page
        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetricsByRecordType(
      [FromQuery] string recordTypeId,
      CancellationToken cancellationToken)
        {
            var response =
                await _impactMasterService.GetMetricsForImpactMasterAsync(
                    impactMasterId: null,
                    recordTypeId: recordTypeId,
                    cancellationToken);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("{id}/metrics")]
        public async Task<IActionResult> GetMetricsForImpactMaster(
            string id,
            CancellationToken cancellationToken)
        {
            var response =
                await _impactMasterService.GetMetricsForImpactMasterAsync(
                    impactMasterId: id,
                    recordTypeId: null,
                    cancellationToken);

            return Ok(response);
        }

        [HttpPost("{id}/metrics")]
        public async Task<IActionResult> SaveImpactMasterMetrics(
            string id,
            [FromBody] SaveImpactMasterMetricsRequest request,
            CancellationToken cancellationToken)
        {
            request.ImpactMasterId = id;

            var response =
                await _impactMasterService.SaveImpactMasterMetricsAsync(request);

            return Ok(response);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> UpdateImpactMasterById(
    long id,
    [FromBody] UpdateImpactMasterRequestDto request,
    CancellationToken cancellationToken)
        {
            request.ImpactMasterId = id.ToString();

            var result =
                await _impactMasterService.UpdateImpactMasterByIdAsync(
                    request,
                    cancellationToken);

            return Ok(
                ApiResponse<object>.Ok(
                    result,
                    "Impact Master updated successfully."));
        }
    
    //delete 
     [HttpDelete]
        public async Task<IActionResult> DeleteImpactMaster(
    [FromBody] DeleteImpactMasterRequestDto request,
    CancellationToken cancellationToken = default)
        {
            var response = await _impactMasterService.DeleteImpactMasterAsync(
                request,
                cancellationToken);

            if (response.HasPendingReviews)
            {
                return BadRequest(
                    ApiResponse<DeleteImpactMasterResponseDto>.Fail(
                        $"Impact Master cannot be deleted. It has pending review approvals. Review IDs : {response.PendingReviewIds}",
                        "IMPACT_MASTER_HAS_PENDING_REVIEWS"
                    ));
            }

            if (!response.IsDeleted)
            {
                return NotFound(
                    ApiResponse<DeleteImpactMasterResponseDto>.Fail(
                        "Impact Master not found or already deleted.",
                        "IMPACT_MASTER_NOT_FOUND"
                    ));
            }

            return Ok(
                ApiResponse<DeleteImpactMasterResponseDto>.Ok(
                    response,
                    "Impact Master deleted successfully"
                ));
        }

        [HttpPost("copy")]
        public async Task<IActionResult> CopyImpactMaster(
        [FromBody] CopyImpactMasterRequestDto request,
        CancellationToken cancellationToken = default)
        {
            var response = await _impactMasterService.CopyImpactMasterAsync(
                request,
                cancellationToken);

            return Ok(
                ApiResponse<CopyImpactMasterResponseDto>.Ok(
                    response,
                    "Impact Master copied successfully"
                )
            );
        }
        [HttpGet("filter-options")]

        public async Task<IActionResult> ImpactMasterFilterOptions(

  [FromQuery] ImpactMasterFilterRequestDto request,

  CancellationToken cancellationToken)

        {

            var result = await _impactMasterService.ImpactMasterFilterOptions(

                request,

                cancellationToken);


            return Ok(result);

        }
    }
}