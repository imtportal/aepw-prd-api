using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Metric
{
    public sealed class MetricSaveConfigurationDto
    {
        public long UserId { get; set; }
        public List<MetricCategoryConfigurationDto> MetricCategories { get; set; } = [];

        public List<MetricConfigurationDto> Metrics { get; set; } = [];
    }
}
