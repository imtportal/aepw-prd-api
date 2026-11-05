using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Lookup
{
    public class MetricCategoryLookupDto
    {
        public string CategoryId { get; set; } = string.Empty;

        public string CategoryCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Color { get; set; }
    }
}
