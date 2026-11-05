//namespace AEPW.API.Controllers
//{
//    public class TestController
//    {
//        // Gets Azure AD users by Object Ids.

//        [HttpPost("azure-ad/users-by-email")]
//        public async Task<IActionResult> GetAzureAdUsersByEmail(
//            [FromBody] IReadOnlyList<string> emails)
//        {
//            if (emails == null || emails.Count == 0)
//            {
//                return BadRequest(ApiResponse<object>.Fail("At least one email is required."));
//            }

//            var response = await _azureAdService.GetUsersByEmailsAsync(emails);

//            return Ok(response);
//        }

//    }
//}
