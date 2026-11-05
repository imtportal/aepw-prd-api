using ClosedXML.Excel;
using AEPW.Application.DTOs.Report.PerTopLevelMetric;
using Microsoft.AspNetCore.Hosting;

namespace AEPW.Application.Services.Report
{
  public class PerTopLevelMetricExcelGeneratorService
  {
    private readonly IWebHostEnvironment _environment;

    // =========================================================
    // TEMPLATE COLUMNS
    // =========================================================

    //private const int ReportingYearColumn = 1; // B
    //private const int ImIdColumn = 3;          // C
    //private const int PinColumn = 4;           // D
    //private const int ProjectTitleColumn = 5;  // E

    //private const int FirstMetricColumn = 6;   // F

    private const int ReportingYearColumn = 1; // A
    private const int ImIdColumn = 2;          // B
    private const int PinColumn = 3;           // C
    private const int ProjectTitleColumn = 4;  // D

    private const int FirstMetricColumn = 5;   // E

    /*
     * One metric occupies 13 columns:
     *
     * +0  Annual
     * +1  Evidence Rating
     * +2  Risk Adjusted
     * +3  % Contribution
     * +4  Original Q1
     * +5  Original Q2
     * +6  Original Q3
     * +7  Original Q4
     * +8  Risk Adjusted Q1
     * +9  Risk Adjusted Q2
     * +10 Risk Adjusted Q3
     * +11 Risk Adjusted Q4
     * +12 Baseline
     */

    private const int ColumnsPerMetric = 13;

    //private const int HeaderRow = 2;
    //private const int FirstProjectRow = 3;
    private const int HeaderRow = 3;
    private const int FirstProjectRow = 4;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public PerTopLevelMetricExcelGeneratorService(
        IWebHostEnvironment environment)
    {
      _environment = environment;
    }


    // =========================================================
    // MAIN METHOD
    // =========================================================

    public MemoryStream GeneratePerTopLevelMetricExcel(
        PerTopLevelMetricReportDto report)
    {
      var templatePath = Path.Combine(
          _environment.ContentRootPath,
          "Templates",
          "Report",
          "PerTopLevel_Template.xlsx");

      if (!File.Exists(templatePath))
      {
        throw new FileNotFoundException(
            "Per Top-Level Metric Excel template was not found.",
            templatePath);
      }

      using var workbook =
          new XLWorkbook(templatePath);

      var worksheet =
          workbook.Worksheets.First();

      // Add generation information in the first row
      SetGenerationData(worksheet, report);



      // -----------------------------------------------------
      // Selected metrics
      // -----------------------------------------------------

      var selectedMetricCodes =
          GetSelectedMetrics(report);

      // -----------------------------------------------------
      // Create metric headers
      // -----------------------------------------------------

      CreateMetricHeaders(
          worksheet,
          report);

      // -----------------------------------------------------
      // Populate project + year rows
      // -----------------------------------------------------

      PopulateProjects(
          worksheet,
          report,
          selectedMetricCodes);

      // -----------------------------------------------------
      // CHECK WHETHER DATA IS AVAILABLE
      // -----------------------------------------------------

      bool hasData = report.Projects != null &&
                     report.Projects.Any(project =>
                         project.Metrics != null &&
                         project.Metrics.Any(metric =>
                             metric.YearlyData != null &&
                             metric.YearlyData.Any()));

      if (!hasData)
      {
        int lastColumn = Math.Max(
            FirstMetricColumn +
            (selectedMetricCodes.Count * ColumnsPerMetric) - 1,
            ProjectTitleColumn);

        int messageRow = FirstProjectRow;

        var messageRange = worksheet.Range(messageRow, 1, messageRow, 17); // A to Q

        if (!messageRange.IsMerged())
        {
          messageRange.Merge();
        }



        worksheet.Cell(messageRow, 1).Value =
            "No data available for the selected filters";

        messageRange.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Left;

        messageRange.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        messageRange.Style.Alignment.WrapText = true;
        messageRange.Style.Font.Bold = true;

        worksheet.Row(messageRow).Height = 30;
      }

      // -----------------------------------------------------
      // Return Excel stream
      // -----------------------------------------------------

      var stream = new MemoryStream();

      workbook.SaveAs(stream);

      stream.Position = 0;

      return stream;
    }


    // =========================================================
    // SELECTED METRICS
    // =========================================================

    private List<string> GetSelectedMetrics(
        PerTopLevelMetricReportDto report)
    {
      return report.SelectedMetrics?
          .Select(x => x.MetricCode)
          .Where(x =>
              !string.IsNullOrWhiteSpace(x))
          .Distinct(
              StringComparer.OrdinalIgnoreCase)
          .ToList()
          ?? new List<string>();
    }


    // =========================================================
    // CREATE METRIC HEADERS
    // =========================================================

    private void CreateMetricHeaders(
        IXLWorksheet worksheet,
        PerTopLevelMetricReportDto report)
    {
      if (report.SelectedMetrics == null ||
          report.SelectedMetrics.Count == 0)
      {
        return;
      }

      for (
          int metricIndex = 0;
          metricIndex < report.SelectedMetrics.Count;
          metricIndex++)
      {
        var metric =
            report.SelectedMetrics[metricIndex];

        int startColumn =
            FirstMetricColumn +
            metricIndex * ColumnsPerMetric;

        // Copy the complete metric block
        // from the first metric block.
        if (metricIndex > 0)
        {
          CopyMetricBlock(
              worksheet,
              FirstMetricColumn,
              startColumn);
        }

        SetMetricHeader(
            worksheet,
            startColumn,
            metric.MetricCode);
      }
    }


    // =========================================================
    // POPULATE PROJECTS
    //
    // IMPORTANT:
    //
    // One Excel row = One Project + One Year
    //
    // Example:
    //
    // 2025 | X002D | Project A
    // 2024 | X002D | Project A
    // 2025 | K107  | Project B
    // 2024 | K107  | Project B
    //
    // =========================================================

    private void PopulateProjects(
        IXLWorksheet worksheet,
        PerTopLevelMetricReportDto report,
        List<string> selectedMetricCodes)
    {
      if (report.Projects == null ||
          report.Projects.Count == 0)
      {
        return;
      }

      int currentRow = FirstProjectRow;

      foreach (var project in report.Projects)
      {
        // -------------------------------------------------
        // Get all years available for this project
        // -------------------------------------------------

        var years =
            project.Metrics?
                .SelectMany(
                    x => x.YearlyData ??
                         new List<PerTopLevelMetricYearlyDataDto>())
                .Select(x => x.Year)
                .Distinct()
                .OrderByDescending(x => x)
                .ToList()
            ?? new List<int>();


        // -------------------------------------------------
        // If project has no yearly data
        // -------------------------------------------------

        if (years.Count == 0)
        {
          continue;
        }


        // -------------------------------------------------
        // One row per year
        // -------------------------------------------------

        foreach (var year in years)
        {
          if (currentRow != FirstProjectRow)
          {
            CopyTemplateRow(
                worksheet,
                FirstProjectRow,
                currentRow);
          }


          // =================================================
          // PROJECT INFORMATION
          // =================================================

          worksheet.Cell(
              currentRow,
              ReportingYearColumn)
              .Value = year;

          worksheet.Cell(
              currentRow,
              ImIdColumn)
              .Value =
              project.ImId ??
              string.Empty;

          worksheet.Cell(
              currentRow,
              PinColumn)
              .Value =
              project.Pin ??
              string.Empty;

          worksheet.Cell(
              currentRow,
              ProjectTitleColumn)
              .Value =
              project.ProjectTitle ??
              string.Empty;


          // =================================================
          // METRICS
          // =================================================

          foreach (var metricCode in selectedMetricCodes)
          {
            var metric =
                project.Metrics?
                    .FirstOrDefault(x =>
                        string.Equals(
                            x.MetricCode,
                            metricCode,
                            StringComparison.OrdinalIgnoreCase));

            if (metric == null)
            {
              continue;
            }


            int metricIndex =
                selectedMetricCodes.IndexOf(
                    metricCode);


            int metricStartColumn =
                FirstMetricColumn +
                metricIndex * ColumnsPerMetric;


            PopulateMetricForYear(
                worksheet,
                currentRow,
                metric,
                metricStartColumn,
                year);
          }


          currentRow++;
        }
      }






      // =========================================================
      // TOTAL ROW
      // =========================================================

      PopulateAggregationRows(
          worksheet,
          report,
          selectedMetricCodes,
          currentRow);

      ApplyReportFormatting(
    worksheet,
    currentRow - 1,
    selectedMetricCodes.Count);

    }

    // Apply formating 
    private void ApplyReportFormatting(
    IXLWorksheet worksheet,
    int lastProjectRow,
    int metricCount)
    {
      int lastColumn =
          FirstMetricColumn +
          (metricCount * ColumnsPerMetric) - 1;

      // Complete report range
      var reportRange = worksheet.Range(
          HeaderRow,
          ReportingYearColumn,
          lastProjectRow,
          lastColumn);

      // All cells - consistent thin border
      reportRange.Style.Border.TopBorder =
          XLBorderStyleValues.Thin;

      reportRange.Style.Border.BottomBorder =
          XLBorderStyleValues.Thin;

      reportRange.Style.Border.LeftBorder =
          XLBorderStyleValues.Thin;

      reportRange.Style.Border.RightBorder =
          XLBorderStyleValues.Thin;

      // Header
      var headerRange = worksheet.Range(
          HeaderRow,
          ReportingYearColumn,
          HeaderRow,
          lastColumn);

      headerRange.Style.Font.Bold = true;

      headerRange.Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Center;

      headerRange.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;

      // Project information
      var projectInfoRange = worksheet.Range(
          FirstProjectRow,
          ReportingYearColumn,
          lastProjectRow,
          ProjectTitleColumn);

      projectInfoRange.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;

      // Year
      worksheet.Column(ReportingYearColumn)
          .Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Center;

      // IM ID
      worksheet.Column(ImIdColumn)
          .Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Center;

      // PIN
      worksheet.Column(PinColumn)
          .Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Center;


      // Metric blocks
      for (int metricIndex = 0;
           metricIndex < metricCount;
           metricIndex++)
      {
        int startColumn =
            FirstMetricColumn +
            metricIndex * ColumnsPerMetric;

        int endColumn =
            startColumn + ColumnsPerMetric - 1;

        var metricRange = worksheet.Range(
            HeaderRow,
            startColumn,
            lastProjectRow,
            endColumn);

        metricRange.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        // Annual
        worksheet.Column(startColumn).Width = 16;

        // Evidence Rating %
        worksheet.Column(startColumn + 1).Width = 17;

        // Risk Adjusted Annual
        worksheet.Column(startColumn + 2).Width = 18;

        // Contribution %
        worksheet.Column(startColumn + 3).Width = 16;

        // Original Q1-Q4
        for (int offset = 4; offset <= 7; offset++)
        {
          worksheet.Column(startColumn + offset).Width = 15;
        }

        // Risk Adjusted Q1-Q4
        for (int offset = 8; offset <= 11; offset++)
        {
          worksheet.Column(startColumn + offset).Width = 17;
        }

        // Baseline
        worksheet.Column(startColumn + 12).Width = 16;

        // Strong separator after each metric block
        worksheet.Column(endColumn)
            .Style.Border.RightBorder =
            XLBorderStyleValues.Medium;
      }
      // Auto-adjust all used columns based on their content
      worksheet.ColumnsUsed().AdjustToContents();
      // Keep Reporting Year column at a fixed width
      worksheet.Column(ReportingYearColumn).Width = 12;

      // Keep first row generation text left-aligned
      worksheet.Range("A1:D1")
          .Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Left;

      worksheet.Range("A1:D1")
          .Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;
    }


    //private void SetGenerationData(
    //IXLWorksheet worksheet,
    //PerTopLevelMetricReportDto report)
    //{
    //  var generationData = report.GenerationData;

    //  if (generationData == null)
    //    return;

    //  worksheet.Cell(1, 1).Value =
    //      $"Generated by {generationData.GeneratedByUser} " +
    //      $"on {generationData.GenerationDate:dd/MM/yyyy}";

    //}

    private void SetGenerationData(
        IXLWorksheet worksheet,
        PerTopLevelMetricReportDto report)
    {
      var generationData = report.GenerationData;

      if (generationData == null)
        return;

      // Use the first row across the project information columns.
      var generationRange = worksheet.Range("A1:D1");

      // Ensure the generation message has enough horizontal space.
      if (!generationRange.IsMerged())
      {
        generationRange.Merge();
      }

      var cell = worksheet.Cell("A1");

      cell.Value =
          $"Generated by {generationData.GeneratedByUser ?? string.Empty} " +
          $"on {generationData.GenerationDate:dd/MM/yyyy}";

      generationRange.Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Left;

      generationRange.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;

      generationRange.Style.Alignment.WrapText = false;
      generationRange.Style.Alignment.ShrinkToFit = false;
      generationRange.Style.Alignment.Indent = 0;

      worksheet.Row(1).Height = 20;
    }

    // =========================================================
    // POPULATE ONE METRIC FOR ONE YEAR
    // =========================================================

    private void PopulateMetricForYear(
        IXLWorksheet worksheet,
        int row,
        PerTopLevelMetricDataDto metric,
        int startColumn,
        int year)
    {
      // ---------------------------------------------------------
      // Find the requested year's data
      // ---------------------------------------------------------

      var yearlyData =
          metric.YearlyData?
              .FirstOrDefault(
                  x => x.Year == year);

      if (yearlyData == null)
      {
        return;
      }


      // =========================================================
      // ANNUAL
      // =========================================================

      var annual =
          yearlyData.OriginalData?.Total;

      if (annual.HasValue)
      {
        worksheet.Cell(
            row,
            startColumn)
            .Value = annual.Value;
      }
      else
      {
        worksheet.Cell(
            row,
            startColumn)
            .Clear();
      }


      // =========================================================
      // EVIDENCE RATING
      //
      // Current DTO contains EvidenceRating at metric level.
      // Therefore we use the current metric value.
      // =========================================================

      if (yearlyData.EvidenceRating.HasValue)
      {
        worksheet.Cell(
            row,
            startColumn + 1)
            .Value =
            yearlyData.EvidenceRating.Value / 100m;

        worksheet.Cell(
            row,
            startColumn + 1)
            .Style
            .NumberFormat
            .Format = "0.000%";
      }
      else
      {
        worksheet.Cell(
            row,
            startColumn + 1)
            .Clear();
      }


      // =========================================================
      // RISK ADJUSTED ANNUAL
      // =========================================================

      var riskAdjusted =
          yearlyData.RiskAdjustedData?.Total;

      if (riskAdjusted.HasValue)
      {
        worksheet.Cell(
            row,
            startColumn + 2)
            .Value =
            riskAdjusted.Value;
      }
      else
      {
        worksheet.Cell(
            row,
            startColumn + 2)
            .Clear();
      }


      // =========================================================
      // CONTRIBUTION %
      //
      // Current DTO contains this at metric level.
      // =========================================================

      if (yearlyData.ContributionPercentage.HasValue)
      {
        worksheet.Cell(
            row,
            startColumn + 3)
            .Value =
            yearlyData.ContributionPercentage.Value / 100m;

        worksheet.Cell(
            row,
            startColumn + 3)
            .Style
            .NumberFormat
            .Format = "0.000%";
      }
      else
      {
        worksheet.Cell(
            row,
            startColumn + 3)
            .Clear();
      }


      // =========================================================
      // ORIGINAL Q1-Q4
      // =========================================================

      if (yearlyData.OriginalData != null)
      {
        SetQuarterValues(
            worksheet,
            row,
            startColumn + 4,
            yearlyData.OriginalData);
      }


      // =========================================================
      // RISK ADJUSTED Q1-Q4
      // =========================================================

      if (yearlyData.RiskAdjustedData != null)
      {
        SetQuarterValues(
            worksheet,
            row,
            startColumn + 8,
            yearlyData.RiskAdjustedData);
      }


      // =========================================================
      // BASELINE
      // =========================================================

      if (yearlyData.Baseline.HasValue)
      {
        worksheet.Cell(
            row,
            startColumn + 12)
            .Value =
            yearlyData.Baseline.Value;
      }
      else
      {
        worksheet.Cell(
            row,
            startColumn + 12)
            .Clear();
      }

      ApplyMetricNumberFormats(
    worksheet,
    row,
    startColumn);
    }


    // =========================================================
    // SET QUARTER VALUES
    // =========================================================

    private void SetQuarterValues(
        IXLWorksheet worksheet,
        int row,
        int startColumn,
        PerTopLevelMetricQuarterDataDto data)
    {
      SetDecimalValue(
          worksheet,
          row,
          startColumn,
          data.Q1);

      SetDecimalValue(
          worksheet,
          row,
          startColumn + 1,
          data.Q2);

      SetDecimalValue(
          worksheet,
          row,
          startColumn + 2,
          data.Q3);

      SetDecimalValue(
          worksheet,
          row,
          startColumn + 3,
          data.Q4);
    }


    // =========================================================
    // SET DECIMAL
    // =========================================================


    private void SetDecimalValue(
        IXLWorksheet worksheet,
        int row,
        int column,
        decimal? value)
    {
      var cell = worksheet.Cell(row, column);

      if (value.HasValue)
      {
        cell.Value = value.Value;

        cell.Style.NumberFormat.Format = "#,##0.000";
      }
      else
      {
        cell.Clear();
      }
    }


    // =========================================================
    // COPY TEMPLATE ROW
    // =========================================================

    private void CopyTemplateRow(
        IXLWorksheet worksheet,
        int templateRow,
        int targetRow)
    {
      if (templateRow == targetRow)
      {
        return;
      }

      int lastColumn =
          worksheet.LastColumnUsed()?
              .ColumnNumber()
          ?? 1;

      var sourceRange =
          worksheet.Range(
              templateRow,
              1,
              templateRow,
              lastColumn);

      var targetRange =
          worksheet.Range(
              targetRow,
              1,
              targetRow,
              lastColumn);

      sourceRange.CopyTo(
          targetRange);

      worksheet.Row(targetRow).Height =
          worksheet.Row(templateRow).Height;
    }


    // =========================================================
    // COPY METRIC BLOCK
    // =========================================================

    private void CopyMetricBlock(
        IXLWorksheet worksheet,
        int sourceStartColumn,
        int destinationStartColumn)
    {
      int sourceEndColumn =
          sourceStartColumn +
          ColumnsPerMetric -
          1;

      int destinationEndColumn =
          destinationStartColumn +
          ColumnsPerMetric -
          1;

      var sourceRange =
          worksheet.Range(
              1,
              sourceStartColumn,
              HeaderRow,
              sourceEndColumn);

      var destinationRange =
          worksheet.Range(
              1,
              destinationStartColumn,
              HeaderRow,
              destinationEndColumn);

      sourceRange.CopyTo(
          destinationRange);

      for (
          int i = 0;
          i < ColumnsPerMetric;
          i++)
      {
        worksheet.Column(
            destinationStartColumn + i)
            .Width =
            worksheet.Column(
                sourceStartColumn + i)
            .Width;
      }
    }


    // =========================================================
    // SET METRIC HEADER
    // =========================================================

    private void SetMetricHeader(
        IXLWorksheet worksheet,
        int startColumn,
        string? metricCode)
    {
      if (string.IsNullOrWhiteSpace(metricCode))
      {
        return;
      }


      // Annual
      worksheet.Cell(
          HeaderRow,
          startColumn)
          .Value =
          $"{metricCode} (Annual)";


      // Evidence Rating
      worksheet.Cell(
          HeaderRow,
          startColumn + 1)
          .Value =
          $"{metricCode} Evidence Rating %";


      // Risk Adjusted
      worksheet.Cell(
          HeaderRow,
          startColumn + 2)
          .Value =
          $"{metricCode} (Risk Adjusted)";


      // Contribution
      worksheet.Cell(
          HeaderRow,
          startColumn + 3)
          .Value =
          "% contribution";
    }


    // =========================================================
    // POPULATE TOTAL / AGGREGATION ROW
    //
    // Requirement:
    //
    // TOTAL = all selected projects + all selected years
    // =========================================================

    private void PopulateAggregationRows(
        IXLWorksheet worksheet,
        PerTopLevelMetricReportDto report,
        List<string> selectedMetricCodes,
        int totalRow)
    {
      if (report.Aggregations == null ||
          report.Aggregations.Count == 0)
      {
        return;
      }


      // ---------------------------------------------------------
      // Copy template row
      // ---------------------------------------------------------

      CopyTemplateRow(
          worksheet,
          FirstProjectRow,
          totalRow);


      // ---------------------------------------------------------
      // Last metric column
      // ---------------------------------------------------------

      int lastMetricColumn =
          FirstMetricColumn +
          selectedMetricCodes.Count *
          ColumnsPerMetric -
          1;


      // ---------------------------------------------------------
      // Clear metric area
      // ---------------------------------------------------------

      worksheet.Range(
          totalRow,
          FirstMetricColumn,
          totalRow,
          lastMetricColumn)
          .Clear();


      // ---------------------------------------------------------
      // Total label
      // Merge C:E so the complete TOTAL text stays inside the box
      // ---------------------------------------------------------

      var totalLabelRange =
          worksheet.Range(
              totalRow,
              ImIdColumn,
              totalRow,
              ProjectTitleColumn);

      totalLabelRange.Merge();

      totalLabelRange.Value =
          "TOTAL (all selected projects, all selected years)";

      totalLabelRange.Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Left;

      totalLabelRange.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;

      totalLabelRange.Style.Font.Bold = true;

      // Year is not applicable for all-years total
      worksheet.Cell(
          totalRow,
          ReportingYearColumn)
          .Clear();


      // =========================================================
      // METRIC TOTALS
      // =========================================================

      for (
          int metricIndex = 0;
          metricIndex < selectedMetricCodes.Count;
          metricIndex++)
      {
        string metricCode =
            selectedMetricCodes[metricIndex];


        var aggregation =
            report.Aggregations
                .FirstOrDefault(x =>
                    string.Equals(
                        x.MetricCode,
                        metricCode,
                        StringComparison.OrdinalIgnoreCase));

        if (aggregation == null)
        {
          continue;
        }


        int startColumn =
            FirstMetricColumn +
            metricIndex * ColumnsPerMetric;


        PopulateAggregation(
            worksheet,
            totalRow,
            aggregation,
            startColumn);
      }


      // =========================================================
      // TOTAL ROW STYLE
      // =========================================================
      var totalRange =
    worksheet.Range(
        totalRow,
        ReportingYearColumn,
        totalRow,
        lastMetricColumn);

      totalRange.Style.Font.Bold = true;

      totalRange.Style.Border.TopBorder =
          XLBorderStyleValues.Thin;

      totalRange.Style.Border.BottomBorder =
          XLBorderStyleValues.Thin;

      totalRange.Style.Border.LeftBorder =
          XLBorderStyleValues.Thin;

      totalRange.Style.Border.RightBorder =
          XLBorderStyleValues.Thin;
    }

    // =========================================================
    // POPULATE AGGREGATION
    //
    // Aggregation is across ALL selected years.
    // =========================================================

    private void PopulateAggregation(
        IXLWorksheet worksheet,
        int row,
        PerTopLevelMetricAggregationDto aggregation,
        int startColumn)
    {
      if (aggregation.YearlyData == null ||
          aggregation.YearlyData.Count == 0)
      {
        return;
      }


      // =========================================================
      // ANNUAL TOTAL
      // =========================================================

      decimal annualTotal =
          aggregation.YearlyData
              .Where(x =>
                  x.Annual.HasValue)
              .Sum(x =>
                  x.Annual!.Value);


      worksheet.Cell(
          row,
          startColumn)
          .Value =
          annualTotal;


      // =========================================================
      // EVIDENCE RATING
      //
      // Aggregation result does not provide a year-wise
      // evidence rating, so keep this blank.
      // =========================================================

      worksheet.Cell(
          row,
          startColumn + 1)
          .Clear();


      // =========================================================
      // RISK ADJUSTED TOTAL
      // =========================================================

      decimal riskAdjustedTotal =
          aggregation.YearlyData
              .Where(x =>
                  x.RiskAdjusted.HasValue)
              .Sum(x =>
                  x.RiskAdjusted!.Value);


      worksheet.Cell(
          row,
          startColumn + 2)
          .Value =
          riskAdjustedTotal;


      // =========================================================
      // CONTRIBUTION %
      //
      // Not available from current aggregation result.
      // =========================================================

      worksheet.Cell(
          row,
          startColumn + 3)
          .Clear();


      // =========================================================
      // ORIGINAL Q1-Q4
      //
      // Sum selected years.
      // =========================================================

      var original =
          new PerTopLevelMetricQuarterDataDto
          {
            Q1 =
                  aggregation.YearlyData
                      .Where(x =>
                          x.OriginalData?.Q1.HasValue == true)
                      .Sum(x =>
                          x.OriginalData!.Q1!.Value),

            Q2 =
                  aggregation.YearlyData
                      .Where(x =>
                          x.OriginalData?.Q2.HasValue == true)
                      .Sum(x =>
                          x.OriginalData!.Q2!.Value),

            Q3 =
                  aggregation.YearlyData
                      .Where(x =>
                          x.OriginalData?.Q3.HasValue == true)
                      .Sum(x =>
                          x.OriginalData!.Q3!.Value),

            Q4 =
                  aggregation.YearlyData
                      .Where(x =>
                          x.OriginalData?.Q4.HasValue == true)
                      .Sum(x =>
                          x.OriginalData!.Q4!.Value)
          };


      SetQuarterValues(
          worksheet,
          row,
          startColumn + 4,
          original);


      // =========================================================
      // RISK ADJUSTED Q1-Q4
      // =========================================================

      var riskAdjusted =
          new PerTopLevelMetricQuarterDataDto
          {
            Q1 =
                  aggregation.YearlyData
                      .Where(x =>
                          x.RiskAdjustedData?.Q1.HasValue == true)
                      .Sum(x =>
                          x.RiskAdjustedData!.Q1!.Value),

            Q2 =
                  aggregation.YearlyData
                      .Where(x =>
                          x.RiskAdjustedData?.Q2.HasValue == true)
                      .Sum(x =>
                          x.RiskAdjustedData!.Q2!.Value),

            Q3 =
                  aggregation.YearlyData
                      .Where(x =>
                          x.RiskAdjustedData?.Q3.HasValue == true)
                      .Sum(x =>
                          x.RiskAdjustedData!.Q3!.Value),

            Q4 =
                  aggregation.YearlyData
                      .Where(x =>
                          x.RiskAdjustedData?.Q4.HasValue == true)
                      .Sum(x =>
                          x.RiskAdjustedData!.Q4!.Value)
          };


      SetQuarterValues(
          worksheet,
          row,
          startColumn + 8,
          riskAdjusted);


      // =========================================================
      // BASELINE
      //
      // No all-years baseline aggregation is available
      // in the current DTO/result set.
      // =========================================================

      worksheet.Cell(
          row,
          startColumn + 12)
          .Clear();

      // =========================================================
      // FORMAT METRIC VALUES TO 3 DECIMAL PLACES
      // =========================================================

      // Annual Total
      //worksheet.Cell(row, startColumn)
      //    .Style.NumberFormat.Format = "#,##0.000";

      //// Risk Adjusted Total
      //worksheet.Cell(row, startColumn + 2)
      //    .Style.NumberFormat.Format = "#,##0.000";

      //// Original Q1-Q4 and Risk Adjusted Q1-Q4
      //for (int offset = 4; offset <= 11; offset++)
      //{
      //  worksheet.Cell(row, startColumn + offset)
      //      .Style.NumberFormat.Format = "#,##0.000";
      //}
      ApplyMetricNumberFormats(
      worksheet,
      row,
      startColumn);

      // =========================================================
      // BOLD
      // =========================================================

      worksheet.Range(
          row,
          startColumn,
          row,
          startColumn + ColumnsPerMetric - 1)
          .Style
          .Font
          .Bold = true;
    }



    private void ApplyMetricNumberFormats(
        IXLWorksheet worksheet,
        int row,
        int startColumn)
    {
      // Annual
      worksheet.Cell(row, startColumn)
          .Style.NumberFormat.Format = "#,##0.000";

      // Risk Adjusted Annual
      worksheet.Cell(row, startColumn + 2)
          .Style.NumberFormat.Format = "#,##0.000";

      // Original Q1-Q4 and Risk Adjusted Q1-Q4
      for (int offset = 4; offset <= 11; offset++)
      {
        worksheet.Cell(row, startColumn + offset)
            .Style.NumberFormat.Format = "#,##0.000";
      }

      // Baseline
      worksheet.Cell(row, startColumn + 12)
          .Style.NumberFormat.Format = "#,##0.000";
    }

  }
}
