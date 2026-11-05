using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Program
{
    public class ProgramDto
    {
        public long ProgramId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
