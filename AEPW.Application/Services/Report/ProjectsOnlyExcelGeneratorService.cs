using ClosedXML.Excel;
using AEPW.Application.DTOs.Report.ProjectsOnly;
using Microsoft.AspNetCore.Hosting;

namespace AEPW.Application.Services.Report
{
    public class ProjectsOnlyExcelGeneratorService
    {
        private readonly IWebHostEnvironment _environment;

        private static readonly HashSet<string> ExcludedMetricCodes =
            new(StringComparer.OrdinalIgnoreCase)
            {
               
                "MC1.11",
                "MC1.13",
                "MC1.21",
                "MC1.23"
            };

        public ProjectsOnlyExcelGeneratorService(
            IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public byte[] GenerateProjectsOnlyExcel(
            ProjectsOnlyReportDto report)
        {
            var templatePath = Path.Combine(
                _environment.ContentRootPath,
                "Templates",
                "Report",
                "ProjectOn_Template.xlsx"
            );

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException(
                    "Projects Only Excel template was not found.",
                    templatePath
                );
            }

            using var workbook = new XLWorkbook(templatePath);

            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                throw new InvalidOperationException(
                    "The Projects Only template does not contain a worksheet."
                );
            }

            const int headerRow = 2;
            const int dataStartRow = 3;

      //      const int pinColumn = 2;
      //      const int titleColumn = 3;
      //const int reportingPeriodColumn = 4;
      //const int firstMetricColumn = 5;

      const int pinColumn = 1;             // A
      const int titleColumn = 2;           // B
      const int reportingPeriodColumn = 3; // C
      const int firstMetricColumn = 4;     // D

      var metricCodes = GetMetricCodes(report);

      CreateMetricColumns(
          worksheet,
          headerRow,
          firstMetricColumn,
          metricCodes
      );



      var lastMetricColumn =
                firstMetricColumn + (metricCodes.Count * 2) - 1;

      // Add generation metadata in Row 1.
      SetGenerationData(
          worksheet,
          report,
          Math.Max(5, lastMetricColumn)
      );

      var lastExistingRow =
                worksheet.LastRowUsed()?.RowNumber() ?? headerRow;

            if (lastExistingRow >= dataStartRow)
            {
                worksheet
                    .Rows(dataStartRow, lastExistingRow)
                    .Clear(XLClearOptions.Contents);
      }

      var currentRow = dataStartRow;


      // Check whether data is available
      if (report.Projects == null || !report.Projects.Any())
      {
        var templateRow = dataStartRow;

        // Display message in the Excel report
        var messageRange = worksheet.Range(
            templateRow, 1,
            templateRow, 8
        );

        if (!messageRange.IsMerged())
        {
          messageRange.Merge();
        }

        worksheet.Cell(templateRow, 1).Value =
            "No data available for the selected filters";

        worksheet.Cell(templateRow, 1).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        worksheet.Cell(templateRow, 1).Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        worksheet.Cell(templateRow, 1).Style.Font.Bold = true;

        worksheet.Row(templateRow).Height = 25;

        // Return the generated Excel file containing the message
        using var emptyStream = new MemoryStream();

        workbook.SaveAs(emptyStream);

        return emptyStream.ToArray();
      }


      foreach (var project in report.Projects)
            {
                CopyRowStyle(
                    worksheet,
                    dataStartRow,
                    currentRow,
                    pinColumn,
                    lastMetricColumn
                );

                worksheet.Cell(currentRow, pinColumn).Value =
                    project.Pin;

        worksheet.Cell(currentRow, titleColumn).Value =
            project.ProjectTitle;

        worksheet.Cell(currentRow, reportingPeriodColumn).Value =
    project.ReportingPeriod;

        var currentColumn = firstMetricColumn;

                foreach (var metricCode in metricCodes)
                {
                    project.OriginalMetrics.TryGetValue(
                        metricCode,
                        out var originalValue
                    );

                    project.RiskAdjustedMetrics.TryGetValue(
                        metricCode,
                        out var riskAdjustedValue
                    );

                    SetDecimalValue(
                        worksheet.Cell(
                            currentRow,
                            currentColumn
                        ),
                        originalValue
                    );

                    SetDecimalValue(
                        worksheet.Cell(
                            currentRow,
                            currentColumn + 1
                        ),
                        riskAdjustedValue
                    );

                    currentColumn += 2;
                }

                currentRow++;
            }

            ApplyDataFormatting(
                worksheet,
                dataStartRow,
                currentRow - 1,
                pinColumn,
                titleColumn,
                reportingPeriodColumn,
                firstMetricColumn,
                metricCodes.Count
            );

           

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        private static List<string> GetMetricCodes(
            ProjectsOnlyReportDto report)
        {
            var metricCodes = new List<string>();

            foreach (var project in report.Projects)
            {
                foreach (var metricCode in project.OriginalMetrics.Keys)
                {
                    if (string.IsNullOrWhiteSpace(metricCode))
                    {
                        continue;
                    }

                    if (ExcludedMetricCodes.Contains(metricCode))
                    {
                        continue;
                    }

                    if (!metricCodes.Contains(
                        metricCode,
                        StringComparer.OrdinalIgnoreCase))
                    {
                        metricCodes.Add(metricCode);
                    }
                }

                foreach (var metricCode in project.RiskAdjustedMetrics.Keys)
                {
                    if (string.IsNullOrWhiteSpace(metricCode))
                    {
                        continue;
                    }

                    if (ExcludedMetricCodes.Contains(metricCode))
                    {
                        continue;
                    }

                    if (!metricCodes.Contains(
                        metricCode,
                        StringComparer.OrdinalIgnoreCase))
                    {
                        metricCodes.Add(metricCode);
                    }
                }
            }

            return metricCodes;
        }

        private static void CreateMetricColumns(
            IXLWorksheet worksheet,
            int headerRow,
            int firstMetricColumn,
            List<string> metricCodes)
        {
            var templateOriginalColumn =
                firstMetricColumn;

            var templateRiskAdjustedColumn =
                firstMetricColumn + 1;

            var maxTemplateRow =
                worksheet.LastRowUsed()?.RowNumber() ?? headerRow;

      for (var i = 0; i < metricCodes.Count; i++)
      {
        var originalColumn =
            firstMetricColumn + (i * 2);

        var riskAdjustedColumn =
            originalColumn + 1;

        CopyColumnStyle(
            worksheet,
            templateOriginalColumn,
            originalColumn,
            maxTemplateRow
        );

        CopyColumnStyle(
            worksheet,
            templateRiskAdjustedColumn,
            riskAdjustedColumn,
            maxTemplateRow
        );

        var metricCode = metricCodes[i];

        worksheet.Cell(
            headerRow,
            originalColumn
        ).Value = $"Sum of {metricCode}";

        worksheet.Cell(
            headerRow,
            riskAdjustedColumn
        ).Value =
            $"Sum of {metricCode} (Risk Adjusted)";

        worksheet.Cell(
            headerRow,
            originalColumn
        ).Style.Alignment.WrapText = true;

        worksheet.Cell(
            headerRow,
            riskAdjustedColumn
        ).Style.Alignment.WrapText = true;

        worksheet.Column(originalColumn).AdjustToContents();
        worksheet.Column(riskAdjustedColumn).AdjustToContents();

        worksheet.Column(originalColumn).Width =
            Math.Max(15, worksheet.Column(originalColumn).Width);

        worksheet.Column(riskAdjustedColumn).Width =
            Math.Max(20, worksheet.Column(riskAdjustedColumn).Width);
      }
    }

    private static void CopyColumnStyle(
            IXLWorksheet worksheet,
            int sourceColumn,
            int targetColumn,
            int maxRow)
        {
            worksheet.Column(targetColumn).Width =
                worksheet.Column(sourceColumn).Width;

            for (var row = 1; row <= maxRow; row++)
            {
                var sourceCell =
                    worksheet.Cell(row, sourceColumn);

                var targetCell =
                    worksheet.Cell(row, targetColumn);

                targetCell.Style = sourceCell.Style;

                if (sourceCell.HasFormula)
                {
                    targetCell.FormulaA1 =
                        sourceCell.FormulaA1;
                }
            }
        }

        private static void CopyRowStyle(
            IXLWorksheet worksheet,
            int sourceRow,
            int targetRow,
            int firstColumn,
            int lastColumn)
        {
            if (sourceRow == targetRow)
            {
                return;
            }

            for (
                var column = firstColumn;
                column <= lastColumn;
                column++)
            {
                var sourceCell =
                    worksheet.Cell(sourceRow, column);

                var targetCell =
                    worksheet.Cell(targetRow, column);

                targetCell.Style = sourceCell.Style;
            }

            worksheet.Row(targetRow).Height =
                worksheet.Row(sourceRow).Height;
    }

    private static void SetDecimalValue(
        IXLCell cell,
        decimal? value)
    {
      if (value.HasValue)
      {
        cell.Value = value.Value;

        cell.Style.NumberFormat.Format =
            "#,##0.000";
      }
      else
      {
        cell.Value = "-";

        cell.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;
      }
    }


    private static void SetGenerationData(
        IXLWorksheet worksheet,
        ProjectsOnlyReportDto report,
        int lastColumn)
    {
      var generationData = report.ReportGenerationData;

      if (generationData == null)
        return;

      var range = worksheet.Range(1, 1, 1, lastColumn);

      if (!range.IsMerged())
        range.Merge();

      range.FirstCell().Value =
          $"Generated by {generationData.GeneratedByUser ?? string.Empty} " +
          $"on {generationData.GenerationDate:dd/MM/yyyy}";

      range.Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Left;

      range.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;

      range.Style.Alignment.WrapText = false;
    }


    //private static void ApplyDataFormatting(
    //        IXLWorksheet worksheet,
    //        int startRow,
    //        int endRow,
    //        int pinColumn,
    //        int titleColumn,
    //        int reportingPeriodColumn,
    //        int firstMetricColumn,
    //        int metricCount)
    //    {
    //        if (endRow < startRow)
    //        {
    //            return;
    //        }

    //        var lastMetricColumn =
    //            firstMetricColumn +
    //            (metricCount * 2) -
    //            1;

    //        worksheet.Column(pinColumn).Width =
    //            worksheet.Column(pinColumn).Width;

    //        worksheet.Column(titleColumn).Width =
    //            worksheet.Column(titleColumn).Width;

    //          worksheet.Range(
    //              startRow,
    //              titleColumn,
    //              endRow,
    //              titleColumn
    //          ).Style.Alignment.WrapText = true;

    //          worksheet.Range(
    //            startRow,
    //            reportingPeriodColumn,
    //            endRow,
    //            reportingPeriodColumn
    //        ).Style.Alignment.WrapText = true;

    //  for (var row = startRow; row <= endRow; row++)
    //        {
    //            for (
    //                var column = firstMetricColumn;
    //                column <= lastMetricColumn;
    //                column++)
    //            {
    //                var cell =
    //                    worksheet.Cell(row, column);

    //                if (cell.Value.IsNumber)
    //                {
    //                    cell.Style.NumberFormat.Format =
    //                        "#,##0.000";
    //                }
    //            }
    //        }
    //    }


    private static void ApplyDataFormatting(
        IXLWorksheet worksheet,
        int startRow,
        int endRow,
        int pinColumn,
        int titleColumn,
        int reportingPeriodColumn,
        int firstMetricColumn,
        int metricCount)
    {
      if (endRow < startRow)
      {
        return;
      }

      var lastMetricColumn =
          firstMetricColumn + (metricCount * 2) - 1;

      // Auto-adjust PIN column
      worksheet.Column(pinColumn).AdjustToContents();

      // Auto-adjust Project Title / Name column
      worksheet.Column(titleColumn).AdjustToContents();

      worksheet.Range(
          startRow,
          titleColumn,
          endRow,
          titleColumn
      ).Style.Alignment.WrapText = true;

      worksheet.Range(
          startRow,
          reportingPeriodColumn,
          endRow,
          reportingPeriodColumn
      ).Style.Alignment.WrapText = true;

      for (var row = startRow; row <= endRow; row++)
      {
        for (
            var column = firstMetricColumn;
            column <= lastMetricColumn;
            column++)
        {
          var cell = worksheet.Cell(row, column);

          if (cell.Value.IsNumber)
          {
            cell.Style.NumberFormat.Format = "#,##0.000";
          }
        }
      }

      // Auto-adjust metric columns based on formatted values
      for (
          var column = firstMetricColumn;
          column <= lastMetricColumn;
          column++)
      {
        worksheet.Column(column).AdjustToContents();
      }
    }

  }
}
