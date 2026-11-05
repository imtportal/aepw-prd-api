using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Metric
{
    public sealed class MetricSaveConfigurationModel
    {
        public long UserId { get; set; }
        public List<MetricCategoryConfigurationModel> MetricCategories { get; set; } = [];

        public List<MetricConfigurationModel> Metrics { get; set; } = [];
    }
}
