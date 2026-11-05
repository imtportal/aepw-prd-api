using System;
using System.Collections.Generic;
using System.Data;
using AEPW.Domain.Models.Metric;
using Microsoft.Data.SqlClient;

namespace AEPW.Infrastructure.Repositories.Metric
{
    internal static class MetricRepositoryHelper
    {
        internal static SqlParameter CreateMetricCategoryTvp(
            IEnumerable<MetricCategoryConfigurationModel> categories)
        {
            var table = new DataTable();

            table.Columns.Add("CategoryId", typeof(long));
            table.Columns.Add("CategoryDescription", typeof(string));
            table.Columns.Add("L4ApproverUserId", typeof(long));

            foreach (var category in categories)
            {
                table.Rows.Add(
                    category.CategoryId,
                    (object?)category.CategoryDescription ?? DBNull.Value,
                    (object?)category.L4ApproverUserId ?? DBNull.Value);
            }

            return new SqlParameter("@MetricCategories", table)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.MetricCategoryConfigurationType"
            };
        }

        internal static SqlParameter CreateMetricTvp(
            IEnumerable<MetricConfigurationModel> metrics)
        {
            var table = new DataTable();

            table.Columns.Add("MetricId", typeof(long));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("MetricHeader", typeof(string));
            table.Columns.Add("MetricSourceId", typeof(long));
            table.Columns.Add("IsICPRequired", typeof(bool));
            table.Columns.Add("IsRiskAdjustmentRequired", typeof(bool));
            table.Columns.Add("ICPApproverUserId", typeof(long));
            table.Columns.Add("IsActive", typeof(bool));

            foreach (var metric in metrics)
            {
                table.Rows.Add(
                    metric.MetricId,
                    (object?)metric.Description ?? DBNull.Value,
                    (object?)metric.MetricHeader ?? DBNull.Value,
                    (object?)metric.MetricSourceId ?? DBNull.Value,
                    metric.IsICPRequired,
                    metric.IsRiskAdjustmentRequired,
                    (object?)metric.ICPApproverUserId ?? DBNull.Value,
                    metric.IsActive);
            }

            return new SqlParameter("@Metrics", table)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.MetricConfigurationType"
            };
        }
    }
}