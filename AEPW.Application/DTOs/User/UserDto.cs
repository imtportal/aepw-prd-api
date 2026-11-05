using System;
using System.Collections.Generic;
using System.Text;
using static AEPW.Domain.Constants.StoredProcedures;

namespace AEPW.Application.DTOs.User
{
    public class UserDto
    {
        public string UserId { get; set; }
        public string AzureAdObjectId { get; set; }
        public string Upn { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string JobTitle { get; set; }
        //public string TimeZoneId { get; set; }
        public int IsActive { get; set; }
        public DateTime? LastLoginUtc { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }
    }
}
