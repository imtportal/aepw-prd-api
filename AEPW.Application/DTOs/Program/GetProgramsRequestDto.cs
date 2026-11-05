using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Program
{
    public class GetProgramsRequestDto
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? SearchText { get; set; }
        public long? ProgramId { get; set; }
        public bool? IsActive { get; set; }
    }
}
