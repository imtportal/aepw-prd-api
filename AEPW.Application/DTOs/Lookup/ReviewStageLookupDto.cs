using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Lookup
{
    public class ReviewStageLookupDto
    {
        public long? ReviewStageId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public short? DefaultSequence { get; set; }
        public int? IsActive { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedOnUTC { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }
        public int IsApprovalRequired { get; set; }
    }
}
