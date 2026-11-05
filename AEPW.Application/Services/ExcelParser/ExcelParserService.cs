using AEPW.Application.DTOs.Review;
using AEPW.Application.Interfaces.Services.ExcelParser;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AEPW.Application.Services.ExcelParser
{ 
    public class ExcelParserService : IExcelParserService
    {
        public List<ReviewExcelImportDto> ParseWorkbook(
           Stream stream,
           long reviewId,
           out string projectPin)  //changes 
        {
            using var workbook = new XLWorkbook(stream);

            var result = new List<ReviewExcelImportDto>();


            //condition
            projectPin = FindProjectPin(workbook);

            if (string.IsNullOrWhiteSpace(projectPin))
            {
                throw new ArgumentException(
                    "Project PIN is mandatory in the Excel file.");
            }

            foreach (var sheet in workbook.Worksheets)
            {
                if (sheet.Name.Equals(
                    "Instructions",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var rows = sheet.RowsUsed().ToList();

                foreach (var row in rows)
                {
                    // B = Metric Code
                    // C = Metric Name / Description
                    // D = Unit
                    // E = Numerical Value / Total
                    // F = FR Comment

                    var metricCode = row.Cell(2).GetString().Trim();

                    // Ignore blank rows
                    if (string.IsNullOrWhiteSpace(metricCode))
                    {
                        continue;
                    }

                    bool isMetadataRow = metricCode.Equals(
                        "MW_MetaData_L1",
                        StringComparison.OrdinalIgnoreCase);

                    bool isTotalRow = metricCode.EndsWith(
                        " (Total)",
                        StringComparison.OrdinalIgnoreCase);

                    string normalizedMetricCode = metricCode
                        .Replace(
                            " (Total)",
                            "",
                            StringComparison.OrdinalIgnoreCase)
                        .Trim();

                    bool isValidMetricCode = Regex.IsMatch(
                        normalizedMetricCode,
                        @"^[A-Z]{2}\d+\.\d+(\.\d+)?$");

                    // Accept normal metric codes and MW_MetaData_L1
                    if (!isValidMetricCode && !isMetadataRow)
                    {
                        continue;
                    }

                    var metricNameOrDescription =
                        row.Cell(3).GetString().Trim();

                    var unit =
                        row.Cell(4).GetString().Trim();

                    var rawValue =
                        row.Cell(5).GetString().Trim();

                    var frComment =
                        row.Cell(6).GetString().Trim();

                    // Skip empty parent/header rows.
                    // Do not skip total rows or metadata rows.
                    if (!isTotalRow &&
                        !isMetadataRow &&
                        string.IsNullOrWhiteSpace(rawValue) &&
                        string.IsNullOrWhiteSpace(frComment))
                    {
                        continue;
                    }

                    var dto = new ReviewExcelImportDto
                    {
                        ReviewId = reviewId,
                        CategoryName = sheet.Name,
                        MetricCode = normalizedMetricCode,
                        RawValue = string.IsNullOrWhiteSpace(rawValue)
                            ? null
                            : rawValue,
                        FRComment = string.IsNullOrWhiteSpace(frComment)
                            ? null
                            : frComment,
                        IsTopLevel = isMetadataRow
                            ? false
                            : IsTopLevelMetric(normalizedMetricCode)
                    };

                    result.Add(dto);
                }
            }

            return result;
        }

        private static string FindProjectPin(
          XLWorkbook workbook)
        {
            foreach (var sheet in workbook.Worksheets)
            {
                foreach (var row in sheet.RowsUsed())
                {
                    foreach (var cell in row.CellsUsed())
                    {
                        var label = cell.GetString().Trim();

                        if (label.Equals(
                            "Project PIN",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            // Read the value from the cell
                            // immediately to the right.
                            return cell
                                .CellRight()
                                .GetString()
                                .Trim();
                        }
                    }
                }
            }

            return string.Empty;
        }

        private bool IsTopLevelMetric(string metricCode)
        {
            var parts = metricCode.Split('.');
            if (parts.Length != 2)
                return false;

            return parts[1].Length == 1;
        }

    }
} 
