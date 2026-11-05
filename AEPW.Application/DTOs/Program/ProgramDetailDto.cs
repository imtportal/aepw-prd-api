using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Program
{
    public class ProgramDetailDto
    {
        public long ProgramId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public long CreatedBy { get; set; }

        public DateTime CreatedOnUTC { get; set; }

        public long? UpdatedBy { get; set; }

        public DateTime? UpdatedOnUTC { get; set; }
    }
}
