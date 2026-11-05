using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Evidence
{
    public class EvidenceTypeDto
    { 
        public long EvidenceTypeId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public long UpdatedBy { get; set; }
        public DateTime UpdatedOnUTC { get; set; }
        public string UOMCode { get; set; }
        public string ModelId { get; set; }
    }
}
