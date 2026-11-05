using ClosedXML.Excel;
using AEPW.Application.DTOs.Report.ProjectReportCpm;
using Microsoft.AspNetCore.Hosting;

namespace AEPW.Application.Services.Report
{
  public class ProjectReportCpmExcelGeneratorService
  {
    private readonly IWebHostEnvironment _environment;

    // ============================================================
    // TEMPLATE COLUMNS
    // ============================================================

    private const int PinColumn = 1;
    private const int ProjectTitleColumn = 2;
    private const int CodeColumn = 3;
    private const int NameColumn = 4;

    // ============================================================
    // TEMPLATE ROWS
    // ============================================================

    //private const int FilterStartRow = 2;
    //private const int FilterEndRow = 6;

    //// Row 7:
    //// Reported data | Risk adjusted data
    //private const int DataTypeHeaderRow = 7;

    //// Row 8:
    //// Year
    //private const int YearHeaderRow = 8;

    //// Row 9:
    //// Q1 Q2 Q3 Q4 Total | Q1 Q2 Q3 Q4 Total
    //private const int ColumnHeaderRow = 9;

    //private const int FirstMetricRow = 10;

    //// Template row used for L1 metrics
    //private const int L1MetricTemplateRow = 11;

    //// Template row used for Total
    //private const int TotalTemplateRow = 15;

    private const int FilterStartRow = 3;
    private const int FilterEndRow = 7;

    // Row 8: Reported data | Risk adjusted data
    private const int DataTypeHeaderRow = 8;

    // Row 9: Year
    private const int YearHeaderRow = 9;

    // Row 10: Q1, Q2, Q3, Q4, Total
    private const int ColumnHeaderRow = 10;

    // First metric row
    private const int FirstMetricRow = 11;

    // Template row used for L1 metrics
    private const int L1MetricTemplateRow = 12;

    // Template row used for Total
    private const int TotalTemplateRow = 16;

    // ============================================================
    // YEAR CONFIGURATION
    // ============================================================

    // E is the first year column.
    //
    // E:I   = Reported
    // J:N   = Risk Adjusted
    //
    // Total = 10 columns per year
    private const int FirstYearColumn = 5;
    private const int ColumnsPerYear = 10;

    // ============================================================
    // TEMPLATE STYLE CACHE
    // ============================================================

    private readonly Dictionary<int, IXLStyle> _topLevelTemplateStyles = new();

    private readonly Dictionary<int, IXLStyle> _l1TemplateStyles = new();

    private double _topLevelTemplateHeight;

    private double _l1TemplateHeight;

    private readonly Dictionary<int, IXLStyle> _totalTemplateStyles = new();

    private double _totalTemplateHeight;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public ProjectReportCpmExcelGeneratorService(
        IWebHostEnvironment environment)
    {
      _environment = environment;
    }

    // ============================================================
    // MAIN EXCEL GENERATION METHOD
    // ============================================================

    public byte[] GenerateProjectReportCpmExcel(
        ProjectReportCpmReportDto report)
    {
      var templatePath = Path.Combine(
          _environment.ContentRootPath,
          "Templates",
          "Report",
          "ProjectReportCpm_API_Template.xlsx");

      if (!File.Exists(templatePath))
      {
        throw new FileNotFoundException(
            "Project Report CPM Excel template was not found.",
            templatePath);
      }

      using var workbook = new XLWorkbook(templatePath);

      var worksheet = workbook.Worksheet("ProjectReportCPM");

      // ========================================================
      // 1. GET REPORTING YEARS
      // ========================================================

      var reportingYears = GetReportingYears(report);

      // ========================================================
      // 2. SET GENERATION METADATA
      // ========================================================

      SetGenerationData(
          worksheet,
          report);

      // ========================================================
      // 2. SET FILTER DISPLAY NAMES
      // ========================================================

      SetFilterNames(
          worksheet,
          report);

      // ========================================================
      // 3. CREATE YEAR COLUMNS
      // ========================================================

      var lastColumn = CreateYearColumns(
          worksheet,
          reportingYears);

      // ========================================================
      // 4. CAPTURE TEMPLATE STYLES
      // ========================================================



      // Capture template styles BEFORE any metric rows are overwritten.
      CaptureTopLevelTemplateStyles(worksheet);
      CaptureTemplateStyles(worksheet, lastColumn);
      CaptureTotalTemplateStyles(worksheet);


      // ========================================================
      // 5. POPULATE PROJECT METRICS
      // ========================================================


      // ========================================================
      // 5. CHECK DATA AVAILABILITY
      // ========================================================

      bool hasData = report.Projects != null &&
          report.Projects
              .SelectMany(project => project.Topics ?? [])
              .SelectMany(topic => topic.TopLevelMetrics ?? [])
              .SelectMany(GetAllMetrics)
              .Any(metric =>
                  metric.YearlyData != null &&
                  metric.YearlyData.Count > 0);

      var currentRow = FirstMetricRow;

      if (hasData)
      {
        foreach (var project in report.Projects ?? [])
        {
          currentRow = PopulateProjectMetrics(
              worksheet,
              project,
              currentRow,
              lastColumn);
        }
      }
      else
      {
        // Show the message across the entire report width.
        var messageRange = worksheet.Range(
            FirstMetricRow,
            1,
            FirstMetricRow,
            lastColumn);

        messageRange.Unmerge();
        messageRange.Merge();

        var messageCell = worksheet.Cell(
            FirstMetricRow,
            1);

        messageCell.Value =
            "No data available for the selected filters";

        messageRange.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        messageRange.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        messageRange.Style.Alignment.WrapText = true;
        messageRange.Style.Font.Bold = true;

        worksheet.Row(FirstMetricRow).Height = 30;
      }

      // ========================================================
      // 6. FINAL FORMATTING
      // ========================================================

      ApplyFinalFormatting(
          worksheet,
          lastColumn);

      // Apply numeric formats AFTER all template styles
      // and final formatting have been applied.
      if (hasData)
      {
        ApplyDecimalFormatting(
            worksheet,
            FirstMetricRow,
            worksheet.LastRowUsed()?.RowNumber() ?? FirstMetricRow,
            lastColumn);
      }

      // ========================================================
      // 6. FINAL FORMATTING
      // ========================================================

      ApplyFinalFormatting(
          worksheet,
          lastColumn);

      // ========================================================
      // 7. SAVE WORKBOOK
      // ========================================================

      using var stream = new MemoryStream();

      workbook.SaveAs(stream);

      return stream.ToArray();
    }

    // ============================================================
    // REPORTING YEARS
    // ============================================================

    private List<int> GetReportingYears(
        ProjectReportCpmReportDto report)
    {
      var years = report.Filters?.ReportingYears?
          .Where(x => !string.IsNullOrWhiteSpace(x))
          .Select(x =>
              int.TryParse(
                  x,
                  out var year)
                  ? year
                  : 0)
          .Where(year => year > 0)
          .Distinct()
          .OrderBy(year => year)
          .ToList() ?? [];

      // Fallback:
      // If filter years are not available, get years
      // from project metric yearly data.
      if (years.Count == 0)
      {
        years = report.Projects?
            .SelectMany(p => p.Topics ?? [])
            .SelectMany(t => t.TopLevelMetrics ?? [])
            .SelectMany(GetAllMetrics)
            .SelectMany(m => m.YearlyData ?? [])
            .Select(y => y.Year)
            .Where(y => y > 0)
            .Distinct()
            .OrderBy(x => x)
            .ToList() ?? [];
      }

      return years;
    }

    // ============================================================
    // GET TOP LEVEL + L1 METRICS
    // ============================================================

    private IEnumerable<ProjectReportCpmMetricDto> GetAllMetrics(
        ProjectReportCpmMetricDto metric)
    {
      yield return metric;

      foreach (var child in metric.L1Metrics ?? [])
      {
        yield return child;
      }
    }

    // ============================================================
    // FILTER DISPLAY NAMES
    // ============================================================

    private void SetFilterNames(
        IXLWorksheet worksheet,
        ProjectReportCpmReportDto report)
    {
      var filters = report.Filters;

      SetFilterValue(
          worksheet,
          3,
          "PIN",
          filters?.Pin);

      SetFilterValue(
          worksheet,
          4,
          "PROJECT TITLE",
          filters?.ProjectTitle);

      SetFilterValue(
          worksheet,
          5,
          "CPM",
          filters?.CPM);

      SetFilterValue(
          worksheet,
          6,
          "PROGRAM",
          filters?.Program);

      SetFilterValue(
          worksheet,
          7,
          "TOPIC",
          filters?.Topics);
    }

    // ============================================================
    // GENERATION METADATA
    // ============================================================

    private void SetGenerationData(
        IXLWorksheet worksheet,
        ProjectReportCpmReportDto report)
    {
      var generationData = report.GenerationData;

      if (generationData == null)
        return;

      worksheet.Cell(1, 1).Value =
          $"Generated by {generationData.GeneratedByUser} " +
          $"on {generationData.GenerationDate:dd/MM/yyyy}";
    }

    // ============================================================
    // SET INDIVIDUAL FILTER
    // ============================================================

    private void SetFilterValue(
    IXLWorksheet worksheet,
    int row,
    string label,
    IEnumerable<string>? values)
    {
      // Label cell
      var labelCell = worksheet.Cell(row, 1);

      // Value area B:D
      var valueRange = worksheet.Range(row, 2, row, 4);

      // In case template already has merge
      valueRange.Unmerge();
      valueRange.Merge();

      var valueCell = valueRange.FirstCell();

      var value = values == null
          ? string.Empty
          : string.Join(
              ", ",
              values.Where(x => !string.IsNullOrWhiteSpace(x)));

      labelCell.Value = label;

      valueCell.Value = string.IsNullOrWhiteSpace(value)
          ? "All"
          : value;

      // =========================
      // LABEL FORMAT
      // =========================

      labelCell.Style.Font.Bold = true;

      labelCell.Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Left;

      labelCell.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Top;

      labelCell.Style.Alignment.WrapText = true;

      labelCell.Style.Alignment.Indent = 1;

      // =========================
      // VALUE FORMAT
      // =========================

      valueCell.Style.Font.Bold = false;

      valueCell.Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Left;

      valueCell.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Top;

      valueCell.Style.Alignment.WrapText = true;

      valueCell.Style.Alignment.Indent = 1;

      // =========================
      // BORDER
      // =========================

      //var fullRange =
      //    worksheet.Range(row, 1, row, 4);

      //fullRange.Style.Border.TopBorder =
      //    XLBorderStyleValues.Thin;

      //fullRange.Style.Border.BottomBorder =
      //    XLBorderStyleValues.Thin;

      //fullRange.Style.Border.LeftBorder =
      //    XLBorderStyleValues.Thin;

      //fullRange.Style.Border.RightBorder =
      //    XLBorderStyleValues.Thin;

      // =========================
      // ROW HEIGHT
      // =========================

      worksheet.Row(row).AdjustToContents();

      if (worksheet.Row(row).Height < 22)
        worksheet.Row(row).Height = 22;

      if (worksheet.Row(row).Height > 60)
        worksheet.Row(row).Height = 60;
    }

    // ============================================================
    // POPULATE PROJECT METRICS
    // ============================================================

    private int PopulateProjectMetrics(
        IXLWorksheet worksheet,
        ProjectReportCpmProjectDto project,
        int currentRow,
        int lastColumn)
    {
      if (project.Topics == null ||
          project.Topics.Count == 0)
      {
        return currentRow;
      }

      // --------------------------------------------------------
      // ORDER TOPICS
      // --------------------------------------------------------

      var orderedTopics = project.Topics
          .OrderBy(x => x.DisplayOrder)
          .ThenBy(x => x.CategoryId)
          .ThenBy(x => x.TopicCode);

      foreach (var topic in orderedTopics)
      {
        if (topic.TopLevelMetrics == null ||
            topic.TopLevelMetrics.Count == 0)
        {
          continue;
        }

        // ----------------------------------------------------
        // ORDER TOP LEVEL METRICS
        //
        // Required:
        //
        // MW
        // CV
        // EE
        // MC
        // SB
        // CI
        // ----------------------------------------------------

        var orderedMetrics = topic.TopLevelMetrics
            .OrderBy(x =>
                GetTopLevelCategoryOrder(
                    x.MetricCode))
            .ThenBy(x =>
                GetMetricSortParts(
                    x.MetricCode).Number)
            .ThenBy(x => x.MetricCode);

        foreach (var metric in orderedMetrics)
        {
          currentRow = AddTopLevelMetric(
              worksheet,
              project,
              topic,
              metric,
              currentRow,
              lastColumn);
        }
      }

      return currentRow;
    }

    // ============================================================
    // ADD TOP LEVEL METRIC
    // ============================================================

    private int AddTopLevelMetric(
        IXLWorksheet worksheet,
        ProjectReportCpmProjectDto project,
        ProjectReportCpmTopicDto topic,
        ProjectReportCpmMetricDto metric,
        int currentRow,
        int lastColumn)
    {
      // --------------------------------------------------------
      // APPLY TOP LEVEL TEMPLATE STYLE
      // --------------------------------------------------------

      ApplyTopLevelTemplateStyle(
          worksheet,
          currentRow,
          lastColumn);


      // --------------------------------------------------------
      // PROJECT INFORMATION
      // --------------------------------------------------------

      SetProjectValues(
          worksheet,
          currentRow,
          project);

      // --------------------------------------------------------
      // METRIC CODE
      // --------------------------------------------------------

      worksheet.Cell(
          currentRow,
          CodeColumn).Value =
          metric.MetricCode;

      // --------------------------------------------------------
      // METRIC NAME
      // --------------------------------------------------------

      worksheet.Cell(
          currentRow,
          NameColumn).Value =
          metric.MetricName;

      // --------------------------------------------------------
      // YEARLY DATA
      // --------------------------------------------------------

      PopulateYearlyValues(
          worksheet,
          currentRow,
          metric.YearlyData);

      // --------------------------------------------------------
      // FORMAT TOP LEVEL
      // --------------------------------------------------------

      FormatTopLevelRow(
          worksheet,
          currentRow,
          lastColumn);

      currentRow++;

      // ========================================================
      // L1 METRICS
      // ========================================================

      // IMPORTANT:
      // Do not filter based on values.
      //
      // Even if all values are ZERO,
      // the L1 metric must still be rendered.
      //
      // This keeps:
      //
      // MW1.1
      //   MW1.11
      //   MW1.12
      //   MW1.13
      //   MW1.14
      //
      // in sequence.

      var l1Metrics = metric.L1Metrics?
          .OrderBy(x =>
              GetTopLevelCategoryOrder(
                  x.MetricCode))
          .ThenBy(x =>
              GetMetricSortParts(
                  x.MetricCode).Number)
          .ThenBy(x => x.MetricCode)
          .ToList() ?? [];

      var l1StartRow = currentRow;

      foreach (var l1Metric in l1Metrics)
      {
        currentRow = AddL1Metric(
            worksheet,
            project,
            l1Metric,
            currentRow,
            lastColumn);
      }

      var l1EndRow = currentRow - 1;

      // ========================================================
      // TOTAL ROW
      // ========================================================

      currentRow = AddTotalRow(
        worksheet,
        project,
        metric,
        currentRow,
        lastColumn);

      return currentRow;
    }

    // ============================================================
    // ADD L1 METRIC
    // ============================================================

    private int AddL1Metric(
        IXLWorksheet worksheet,
        ProjectReportCpmProjectDto project,
        ProjectReportCpmMetricDto metric,
        int currentRow,
        int lastColumn)
    {
      // --------------------------------------------------------
      // APPLY L1 TEMPLATE STYLE
      // --------------------------------------------------------

      ApplyL1TemplateStyle(
          worksheet,
          currentRow,
          lastColumn);

      // --------------------------------------------------------
      // PROJECT VALUES
      // --------------------------------------------------------

      SetProjectValues(
          worksheet,
          currentRow,
          project);

      // --------------------------------------------------------
      // CODE
      // --------------------------------------------------------

      worksheet.Cell(
          currentRow,
          CodeColumn).Value =
          metric.MetricCode;

      // --------------------------------------------------------
      // NAME
      // --------------------------------------------------------

      worksheet.Cell(
          currentRow,
          NameColumn).Value =
          metric.MetricName;

      // --------------------------------------------------------
      // YEARLY DATA
      // --------------------------------------------------------

      PopulateYearlyValues(
          worksheet,
          currentRow,
          metric.YearlyData);

      // --------------------------------------------------------
      // FORMAT
      // --------------------------------------------------------

      FormatL1Row(
          worksheet,
          currentRow,
          lastColumn);

      AdjustMetricRowHeight(
          worksheet,
          currentRow);

      return currentRow + 1;
    }

    // ============================================================
    // ADD TOTAL ROW
    // ============================================================

    private int AddTotalRow(
        IXLWorksheet worksheet,
        ProjectReportCpmProjectDto project,
        ProjectReportCpmMetricDto metric,
        int currentRow,
        int lastColumn)
    {
      // Apply cached Total-row template style.
      // This is independent of the number of reporting years.
      ApplyTotalTemplateStyle(
          worksheet,
          currentRow,
          lastColumn);

      SetProjectValues(
          worksheet,
          currentRow,
          project);

      worksheet.Cell(currentRow, CodeColumn).Value = string.Empty;
      worksheet.Cell(currentRow, NameColumn).Value = "Total";

      PopulateYearlyValues(
          worksheet,
          currentRow,
          metric.YearlyData);

      ApplyTotalRowStyle(
          worksheet,
          currentRow,
          lastColumn);

      return currentRow + 1;
    }
    private void ApplyTotalTemplateStyle(
        IXLWorksheet worksheet,
        int targetRow,
        int lastColumn)
    {
      // Project-information columns.
      for (int column = 1; column < FirstYearColumn; column++)
      {
        if (_totalTemplateStyles.TryGetValue(
            column,
            out var style))
        {
          worksheet.Cell(targetRow, column).Style = style;
        }
      }

      // Repeat the FIRST YEAR Total style across
      // every selected reporting year.
      for (
          int column = FirstYearColumn;
          column <= lastColumn;
          column++)
      {
        int offset =
            (column - FirstYearColumn) % ColumnsPerYear;

        if (_totalTemplateStyles.TryGetValue(
            100 + offset,
            out var style))
        {
          worksheet.Cell(targetRow, column).Style = style;
        }
      }

      worksheet.Row(targetRow).Height =
          _totalTemplateHeight;
    }

    private void ApplyTotalRowStyle(
     IXLWorksheet worksheet,
     int row,
     int lastColumn)
    {
      var range = worksheet.Range(
          row,
          1,
          row,
          lastColumn);

      // Total row
      range.Style.Font.Bold = true;

      range.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;

      range.Style.Alignment.WrapText = true;

      // Project info should remain normal
      worksheet.Range(
          row,
          PinColumn,
          row,
          ProjectTitleColumn)
          .Style.Font.Bold = false;

      // Total label
      worksheet.Cell(
          row,
          NameColumn)
          .Style.Font.Bold = true;

      // Keep all year values centered
      for (
          int column = FirstYearColumn;
          column <= lastColumn;
          column++)
      {
        worksheet.Cell(row, column)
            .Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

        worksheet.Cell(row, column)
            .Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

        worksheet.Cell(row, column)
            .Style.Alignment.WrapText = true;
      }

      ApplyRowBorders(
          worksheet,
          row,
          lastColumn);

      worksheet.Row(row).Height = 25;
    }

    // ============================================================
    // SET PROJECT VALUES
    // ============================================================

    private void SetProjectValues(
        IXLWorksheet worksheet,
        int row,
        ProjectReportCpmProjectDto project)
    {
      worksheet.Cell(
          row,
          PinColumn).Value =
          project.PIN;

      worksheet.Cell(
          row,
          ProjectTitleColumn).Value =
          project.ProjectTitle;

      // Keep project information normal,
      // not globally bold.
      worksheet.Cell(
          row,
          PinColumn)
          .Style.Font.Bold = false;

      worksheet.Cell(
          row,
          ProjectTitleColumn)
          .Style.Font.Bold = false;

      worksheet.Cell(
          row,
          ProjectTitleColumn)
          .Style.Alignment.WrapText = true;
    }

    // ============================================================
    // POPULATE YEARLY DATA
    // ============================================================

    private void PopulateYearlyValues(
        IXLWorksheet worksheet,
        int row,
        List<ProjectReportCpmYearlyDataDto>? yearlyData)
    {
      if (yearlyData == null ||
          yearlyData.Count == 0)
      {
        return;
      }

      foreach (var yearly in yearlyData)
      {
        var yearColumn = GetYearStartColumn(
            worksheet,
            yearly.Year);

        if (yearColumn <= 0)
        {
          continue;
        }

        var reported =
            yearly.ReportedData;

        var risk =
            yearly.RiskAdjustedData;

        // ----------------------------------------------------
        // REPORTED DATA
        // ----------------------------------------------------

        SetDecimalValue(
            worksheet,
            row,
            yearColumn,
            reported?.Q1);

        SetDecimalValue(
            worksheet,
            row,
            yearColumn + 1,
            reported?.Q2);

        SetDecimalValue(
            worksheet,
            row,
            yearColumn + 2,
            reported?.Q3);

        SetDecimalValue(
            worksheet,
            row,
            yearColumn + 3,
            reported?.Q4);

        SetDecimalValue(
            worksheet,
            row,
            yearColumn + 4,
            reported?.Total);

        // ----------------------------------------------------
        // RISK ADJUSTED DATA
        // ----------------------------------------------------

        SetDecimalValue(
            worksheet,
            row,
            yearColumn + 5,
            risk?.Q1);

        SetDecimalValue(
            worksheet,
            row,
            yearColumn + 6,
            risk?.Q2);

        SetDecimalValue(
            worksheet,
            row,
            yearColumn + 7,
            risk?.Q3);

        SetDecimalValue(
            worksheet,
            row,
            yearColumn + 8,
            risk?.Q4);

        SetDecimalValue(
            worksheet,
            row,
            yearColumn + 9,
            risk?.Total);
      }
    }

    // ============================================================
    // SET DECIMAL VALUE
    // ============================================================

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
        cell.Clear(XLClearOptions.Contents);
        cell.Style.NumberFormat.Format = "#,##0.000";
      }

      cell.Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Center;

      cell.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;
    }
    // ============================================================
    // FIND YEAR START COLUMN
    // ============================================================

    private int GetYearStartColumn(
        IXLWorksheet worksheet,
        int year)
    {
      var lastColumn =
          worksheet.LastColumnUsed()
              ?.ColumnNumber()
          ?? FirstYearColumn;

      for (
          int column = FirstYearColumn;
          column <= lastColumn;
          column++)
      {
        var cellValue =
            worksheet.Cell(
                YearHeaderRow,
                column)
            .GetString();

        if (cellValue ==
            year.ToString())
        {
          return column;
        }
      }

      return -1;
    }

    // ============================================================
    // CREATE YEAR COLUMNS
    // ============================================================

    private int CreateYearColumns(
        IXLWorksheet worksheet,
        List<int> reportingYears)
    {
      if (reportingYears == null ||
          reportingYears.Count == 0)
      {
        return worksheet.LastColumnUsed()
            ?.ColumnNumber()
            ?? 4;
      }

      var firstYear =
          reportingYears.First();

      // --------------------------------------------------------
      // FIRST YEAR
      // --------------------------------------------------------

      SetYearHeader(
          worksheet,
          FirstYearColumn,
          firstYear);

      var lastColumn =
          FirstYearColumn +
          ColumnsPerYear -
          1;

      // --------------------------------------------------------
      // ADD OTHER YEARS
      // --------------------------------------------------------

      for (
          int index = 1;
          index < reportingYears.Count;
          index++)
      {
        var year =
            reportingYears[index];

        var startColumn =
            FirstYearColumn +
            index * ColumnsPerYear;

        CopyYearBlock(
            worksheet,
            FirstYearColumn,
            startColumn);

        SetYearHeader(
            worksheet,
            startColumn,
            year);

        lastColumn =
            startColumn +
            ColumnsPerYear -
            1;
      }

      return lastColumn;
    }

    // ============================================================
    // SET YEAR HEADER
    // ============================================================

    private void SetYearHeader(
      IXLWorksheet worksheet,
      int startColumn,
      int year)
    {
      var endColumn =
          startColumn +
          ColumnsPerYear -
          1;

      // ============================================================
      // REMOVE STATIC "YEAR" FROM TEMPLATE
      // ============================================================

      worksheet.Cell(
          YearHeaderRow,
          NameColumn)
          .Clear(XLClearOptions.Contents);

      // ============================================================
      // ROW 7
      // Reported Data / Risk Adjusted Data
      // ============================================================

      var reportedStart =
          startColumn;

      var reportedEnd =
          startColumn + 4;

      var riskStart =
          startColumn + 5;

      var riskEnd =
          startColumn + 9;

      var reportedRange =
          worksheet.Range(
              DataTypeHeaderRow,
              reportedStart,
              DataTypeHeaderRow,
              reportedEnd);

      reportedRange.Unmerge();

      reportedRange.Clear(
          XLClearOptions.Contents);

      reportedRange.Merge();

      reportedRange.FirstCell().Value =
          "Reported data";

      var riskRange =
          worksheet.Range(
              DataTypeHeaderRow,
              riskStart,
              DataTypeHeaderRow,
              riskEnd);

      riskRange.Unmerge();

      riskRange.Clear(
          XLClearOptions.Contents);

      riskRange.Merge();

      riskRange.FirstCell().Value =
          "Risk adjusted data";

      // ============================================================
      // STYLE ROW 7
      // ============================================================

      foreach (var range in new[]
      {
        reportedRange,
        riskRange
    })
      {
        range.Style.Font.Italic = true;

        range.Style.Font.FontColor =
            XLColor.Red;

        range.Style.Font.Bold = false;

        range.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        range.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        range.Style.Alignment.WrapText = true;
      }

      // ============================================================
      // ROW 8 - YEAR
      // ============================================================

      var yearRange =
          worksheet.Range(
              YearHeaderRow,
              startColumn,
              YearHeaderRow,
              endColumn);

      yearRange.Unmerge();

      yearRange.Clear(
          XLClearOptions.Contents);

      yearRange.Merge();

      var yearCell =
          yearRange.FirstCell();

      yearCell.Value = year;

      yearCell.Style.Font.Bold = true;

      yearCell.Style.Font.FontSize = 14;

      yearCell.Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Center;

      yearCell.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;

      yearRange.Style.Border.TopBorder =
          XLBorderStyleValues.Medium;

      yearRange.Style.Border.BottomBorder =
          XLBorderStyleValues.Medium;

      yearRange.Style.Border.LeftBorder =
          XLBorderStyleValues.Medium;

      yearRange.Style.Border.RightBorder =
          XLBorderStyleValues.Medium;

      // ============================================================
      // ROW 9
      // ============================================================

      FormatColumnHeaders(
          worksheet,
          startColumn);

      worksheet.Row(
          DataTypeHeaderRow).Height = 22;

      worksheet.Row(
          YearHeaderRow).Height = 28;

      worksheet.Row(
          ColumnHeaderRow).Height = 35;
    }

    // ============================================================
    // FORMAT COLUMN HEADERS
    // ============================================================

    private void FormatColumnHeaders(
        IXLWorksheet worksheet,
        int startColumn)
    {
      var endColumn =
          startColumn +
          ColumnsPerYear -
          1;

      var range =
          worksheet.Range(
              ColumnHeaderRow,
              startColumn,
              ColumnHeaderRow,
              endColumn);

      range.Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Center;

      range.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;

      range.Style.Font.Bold = true;

      range.Style.Alignment.WrapText = true;

      // --------------------------------------------------------
      // Reported
      // --------------------------------------------------------

      worksheet.Cell(
          ColumnHeaderRow,
          startColumn).Value = "Q1";

      worksheet.Cell(
          ColumnHeaderRow,
          startColumn + 1).Value = "Q2";

      worksheet.Cell(
          ColumnHeaderRow,
          startColumn + 2).Value = "Q3";

      worksheet.Cell(
          ColumnHeaderRow,
          startColumn + 3).Value = "Q4";

      worksheet.Cell(
          ColumnHeaderRow,
          startColumn + 4).Value = "Total";

      // --------------------------------------------------------
      // Risk adjusted
      // --------------------------------------------------------

      worksheet.Cell(
          ColumnHeaderRow,
          startColumn + 5).Value = "Q1";

      worksheet.Cell(
          ColumnHeaderRow,
          startColumn + 6).Value = "Q2";

      worksheet.Cell(
          ColumnHeaderRow,
          startColumn + 7).Value = "Q3";

      worksheet.Cell(
          ColumnHeaderRow,
          startColumn + 8).Value = "Q4";

      worksheet.Cell(
          ColumnHeaderRow,
          startColumn + 9).Value = "Total";
    }

    // ============================================================
    // COPY YEAR BLOCK
    // ============================================================

    private void CopyYearBlock(
        IXLWorksheet worksheet,
        int sourceStartColumn,
        int targetStartColumn)
    {
      var sourceEndColumn =
          sourceStartColumn +
          ColumnsPerYear -
          1;

      var targetEndColumn =
          targetStartColumn +
          ColumnsPerYear -
          1;

      // Copy rows 7-9:
      //
      // 7 = Reported / Risk adjusted
      // 8 = Year
      // 9 = Q1...Total

      var sourceRange =
          worksheet.Range(
              DataTypeHeaderRow,
              sourceStartColumn,
              ColumnHeaderRow,
              sourceEndColumn);

      var targetRange =
          worksheet.Range(
              DataTypeHeaderRow,
              targetStartColumn,
              ColumnHeaderRow,
              targetEndColumn);

      sourceRange.CopyTo(
          targetRange);

      // Copy column widths
      for (
          int i = 0;
          i < ColumnsPerYear;
          i++)
      {
        worksheet.Column(
            targetStartColumn + i)
            .Width =
            worksheet.Column(
                sourceStartColumn + i)
            .Width;
      }

      // Row 7 merge
      worksheet.Range(
              DataTypeHeaderRow,
              targetStartColumn,
              DataTypeHeaderRow,
              targetStartColumn + 4)
          .Merge();

      worksheet.Range(
              DataTypeHeaderRow,
              targetStartColumn + 5,
              DataTypeHeaderRow,
              targetEndColumn)
          .Merge();

      // Row 8 merge
      worksheet.Range(
              YearHeaderRow,
              targetStartColumn,
              YearHeaderRow,
              targetEndColumn)
          .Merge();
    }

    // ============================================================
    // CAPTURE L1 TEMPLATE STYLE
    // ============================================================

    private void CaptureTemplateStyles(
        IXLWorksheet worksheet,
        int lastColumn)
    {
      _l1TemplateStyles.Clear();

      for (
          int column = 1;
          column <= lastColumn;
          column++)
      {
        _l1TemplateStyles[column] =
            worksheet.Cell(
                L1MetricTemplateRow,
                column)
            .Style;
      }

      _l1TemplateHeight =
          worksheet.Row(
              L1MetricTemplateRow)
          .Height;
    }


    // ============================================================
    // CAPTURE TOP LEVEL TEMPLATE STYLE
    // ============================================================

    private void CaptureTopLevelTemplateStyles(
        IXLWorksheet worksheet)
    {
      _topLevelTemplateStyles.Clear();

      // Cache project-information columns separately.
      for (int column = 1; column < FirstYearColumn; column++)
      {
        _topLevelTemplateStyles[column] =
            worksheet.Cell(FirstMetricRow, column).Style;
      }

      // Cache the first year's 10-column style.
      // It will be repeated for every reporting year.
      for (int offset = 0; offset < ColumnsPerYear; offset++)
      {
        int column = FirstYearColumn + offset;

        _topLevelTemplateStyles[100 + offset] =
            worksheet.Cell(FirstMetricRow, column).Style;
      }

      _topLevelTemplateHeight =
          worksheet.Row(FirstMetricRow).Height;
    }

    // ============================================================
    // CAPTURE TOTAL TEMPLATE STYLE
    // ============================================================

    private void CaptureTotalTemplateStyles(
        IXLWorksheet worksheet)
    {
      _totalTemplateStyles.Clear();

      // Cache project-information columns.
      for (int column = 1; column < FirstYearColumn; column++)
      {
        _totalTemplateStyles[column] =
            worksheet.Cell(TotalTemplateRow, column).Style;
      }

      // Cache only the first year's 10-column Total style.
      // This style will be reused for every reporting year.
      for (int offset = 0; offset < ColumnsPerYear; offset++)
      {
        int column = FirstYearColumn + offset;

        _totalTemplateStyles[100 + offset] =
            worksheet.Cell(TotalTemplateRow, column).Style;
      }

      _totalTemplateHeight =
          worksheet.Row(TotalTemplateRow).Height;
    }
    // ============================================================
    // APPLY TOP LEVEL TEMPLATE STYLE
    // ============================================================

    private void ApplyTopLevelTemplateStyle(
        IXLWorksheet worksheet,
        int targetRow,
        int lastColumn)
    {
      // Project-information columns.
      for (int column = 1; column < FirstYearColumn; column++)
      {
        if (_topLevelTemplateStyles.TryGetValue(
            column,
            out var style))
        {
          worksheet.Cell(targetRow, column).Style = style;
        }
      }

      // Repeat the first year's 10-column style
      // across every reporting year.
      for (
          int column = FirstYearColumn;
          column <= lastColumn;
          column++)
      {
        int offset =
            (column - FirstYearColumn) % ColumnsPerYear;

        if (_topLevelTemplateStyles.TryGetValue(
            100 + offset,
            out var style))
        {
          worksheet.Cell(targetRow, column).Style = style;
        }
      }

      worksheet.Row(targetRow).Height =
          _topLevelTemplateHeight;
    }

    // ============================================================
    // APPLY L1 TEMPLATE STYLE
    // ============================================================

    private void ApplyL1TemplateStyle(
        IXLWorksheet worksheet,
        int targetRow,
        int lastColumn)
    {
      for (
          int column = 1;
          column <= lastColumn;
          column++)
      {
        if (_l1TemplateStyles.TryGetValue(
            column,
            out var style))
        {
          worksheet.Cell(
              targetRow,
              column).Style = style;
        }
      }

      worksheet.Row(
          targetRow).Height =
          _l1TemplateHeight;
    }

    // ============================================================
    // FORMAT TOP LEVEL ROW
    // ============================================================

    private void FormatTopLevelRow(
        IXLWorksheet worksheet,
        int row,
        int lastColumn)
    {
      var range =
          worksheet.Range(
              row,
              1,
              row,
              lastColumn);

      // --------------------------------------------------------
      // Top Level is bold
      // --------------------------------------------------------

      range.Style.Font.Bold = true;

      range.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;

      range.Style.Alignment.WrapText = true;

      // --------------------------------------------------------
      // Project info remains normal
      // --------------------------------------------------------

      worksheet.Range(
              row,
              PinColumn,
              row,
              ProjectTitleColumn)
          .Style.Font.Bold = false;

      // --------------------------------------------------------
      // Code + Name bold
      // --------------------------------------------------------

      worksheet.Cell(
          row,
          CodeColumn)
          .Style.Font.Bold = true;

      worksheet.Cell(
          row,
          NameColumn)
          .Style.Font.Bold = true;

      // --------------------------------------------------------
      // Borders
      // --------------------------------------------------------

      ApplyRowBorders(
          worksheet,
          row,
          lastColumn);

      // --------------------------------------------------------
      // Year values
      // --------------------------------------------------------

      FormatYearCells(
          worksheet,
          row,
          lastColumn);

      // --------------------------------------------------------
      // Alignment
      // --------------------------------------------------------

      worksheet.Cell(
          row,
          CodeColumn)
          .Style.Alignment.Indent = 0;

      worksheet.Cell(
          row,
          NameColumn)
          .Style.Alignment.Indent = 0;

      worksheet.Row(row).Height = 35;
    }

    // ============================================================
    // FORMAT L1 ROW
    // ============================================================

    private void FormatL1Row(
      IXLWorksheet worksheet,
      int row,
      int lastColumn)
    {
      var range = worksheet.Range(
          row, 1, row, lastColumn);

      range.Style.Font.Bold = false;

      range.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Top;

      range.Style.Alignment.WrapText = true;

      ApplyRowBorders(
          worksheet,
          row,
          lastColumn);

      FormatYearCells(
          worksheet,
          row,
          lastColumn);

      // L1 Code
      worksheet.Cell(row, CodeColumn)
          .Style.Alignment.Horizontal =
              XLAlignmentHorizontalValues.Left;

      worksheet.Cell(row, CodeColumn)
          .Style.Alignment.Vertical =
              XLAlignmentVerticalValues.Top;

      worksheet.Cell(row, CodeColumn)
          .Style.Alignment.Indent = 1;

      // L1 Metric Name
      worksheet.Cell(row, NameColumn)
          .Style.Alignment.Horizontal =
              XLAlignmentHorizontalValues.Left;

      worksheet.Cell(row, NameColumn)
          .Style.Alignment.Vertical =
              XLAlignmentVerticalValues.Top;

      worksheet.Cell(row, NameColumn)
          .Style.Alignment.WrapText = true;

      worksheet.Cell(row, NameColumn)
          .Style.Alignment.Indent = 1;
    }

    // ============================================================
    // FORMAT TOTAL ROW
    // ============================================================

    private void FormatTotalRow(
        IXLWorksheet worksheet,
        int row,
        int lastColumn)
    {
      var range =
          worksheet.Range(
              row,
              1,
              row,
              lastColumn);

      // --------------------------------------------------------
      // Total bold
      // --------------------------------------------------------

      range.Style.Font.Bold = true;

      range.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;

      range.Style.Alignment.WrapText = true;

      // --------------------------------------------------------
      // Project info normal
      // --------------------------------------------------------

      worksheet.Range(
              row,
              PinColumn,
              row,
              ProjectTitleColumn)
          .Style.Font.Bold = false;

      // --------------------------------------------------------
      // Total label bold
      // --------------------------------------------------------

      worksheet.Cell(
          row,
          NameColumn)
          .Style.Font.Bold = true;

      // --------------------------------------------------------
      // Borders
      // --------------------------------------------------------

      ApplyRowBorders(
          worksheet,
          row,
          lastColumn);

      // --------------------------------------------------------
      // Year cells
      // --------------------------------------------------------

      FormatYearCells(
          worksheet,
          row,
          lastColumn);

      worksheet.Row(row).Height = 25;

      worksheet.Cell(
          row,
          NameColumn)
          .Style.Alignment.Indent = 0;
    }

    // ============================================================
    // FORMAT YEAR CELLS
    // ============================================================

    private void FormatYearCells(
        IXLWorksheet worksheet,
        int row,
        int lastColumn)
    {
      for (
          int column = FirstYearColumn;
          column <= lastColumn;
          column++)
      {
        var cell =
            worksheet.Cell(
                row,
                column);

        cell.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        cell.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        cell.Style.Alignment.WrapText = true;

        cell.Style.NumberFormat.Format = "General";
      }
    }

    // ============================================================
    // APPLY ROW BORDERS
    // ============================================================

    private void ApplyRowBorders(
        IXLWorksheet worksheet,
        int row,
        int lastColumn)
    {
      var range =
          worksheet.Range(
              row,
              1,
              row,
              lastColumn);

      range.Style.Border.TopBorder =
          XLBorderStyleValues.Thin;

      range.Style.Border.BottomBorder =
          XLBorderStyleValues.Thin;

      range.Style.Border.LeftBorder =
          XLBorderStyleValues.Thin;

      range.Style.Border.RightBorder =
          XLBorderStyleValues.Thin;
    }

    // ============================================================
    // ADJUST METRIC ROW HEIGHT
    // ============================================================

    //private void AdjustMetricRowHeight(
    //    IXLWorksheet worksheet,
    //    int row)
    //{
    //  worksheet.Row(row)
    //      .AdjustToContents();

    //  // Minimum
    //  if (worksheet.Row(row).Height < 28)
    //  {
    //    worksheet.Row(row).Height = 28;
    //  }

    //  // Maximum
    //  if (worksheet.Row(row).Height > 75)
    //  {
    //    worksheet.Row(row).Height = 75;
    //  }
    //}

    private void AdjustMetricRowHeight(
        IXLWorksheet worksheet,
        int row)
    {
      worksheet.Cell(row, NameColumn)
          .Style.Alignment.WrapText = true;

      worksheet.Cell(row, NameColumn)
          .Style.Alignment.Vertical =
              XLAlignmentVerticalValues.Center;

      worksheet.Row(row).AdjustToContents();

      if (worksheet.Row(row).Height < 28)
        worksheet.Row(row).Height = 28;

      if (worksheet.Row(row).Height > 100)
        worksheet.Row(row).Height = 100;
    }

    // ============================================================
    // COPY TEMPLATE METRIC ROW
    // ============================================================

    private void CopyTemplateMetricRow(
        IXLWorksheet worksheet,
        int templateRow,
        int targetRow)
    {
      if (templateRow == targetRow)
      {
        return;
      }

      worksheet.Row(templateRow)
          .CopyTo(
              worksheet.Row(targetRow));
    }

    // ============================================================
    // FINAL FORMATTING
    // ============================================================

    private void ApplyFinalFormatting(
        IXLWorksheet worksheet,
        int lastColumn)
    {
      // --------------------------------------------------------
      // Header formatting
      // --------------------------------------------------------

      FormatYearHeaders(
          worksheet,
          lastColumn);

      // --------------------------------------------------------
      // FILTER FORMATTING
      // --------------------------------------------------------

      worksheet.Range(
              FilterStartRow,
              PinColumn,
              FilterEndRow,
              lastColumn)
          .Style.Alignment.Vertical =
              XLAlignmentVerticalValues.Center;

      worksheet.Range(
              FilterStartRow,
              ProjectTitleColumn,
              FilterEndRow,
              lastColumn)
          .Style.Alignment.WrapText = true;

      // --------------------------------------------------------
      // COLUMN WIDTHS
      // --------------------------------------------------------

      worksheet.Column(
          PinColumn).Width = 14;

      worksheet.Column(
          ProjectTitleColumn).Width = 32;

      worksheet.Column(
          CodeColumn).Width = 14;

      worksheet.Column(
          NameColumn).Width = 65;

      for (
          int column = FirstYearColumn;
          column <= lastColumn;
          column++)
      {
        worksheet.Column(column).Width = 12;
      }

      // --------------------------------------------------------
      // DO NOT GLOBALLY BOLD PIN / PROJECT TITLE
      // --------------------------------------------------------

      var lastRow =
          worksheet.LastRowUsed()
              ?.RowNumber() ?? 0;

      if (lastRow >= FirstMetricRow)
      {
        worksheet.Range(
                FirstMetricRow,
                PinColumn,
                lastRow,
                ProjectTitleColumn)
            .Style.Font.Bold = false;

        for (int row = FirstMetricRow; row <= lastRow; row++)
        {
          worksheet.Row(row).Unhide();
        }
      }

      // --------------------------------------------------------
      // PAGE SETUP
      // --------------------------------------------------------

      worksheet.PageSetup.PageOrientation =
          XLPageOrientation.Landscape;

      worksheet.PageSetup.PaperSize =
          XLPaperSize.A4Paper;

      worksheet.PageSetup.FitToPages(
          1,
          0);

      worksheet.PageSetup.Margins.Top = 0.5;
      worksheet.PageSetup.Margins.Bottom = 0.5;
      worksheet.PageSetup.Margins.Left = 0.3;
      worksheet.PageSetup.Margins.Right = 0.3;

      // --------------------------------------------------------
      // REPEAT HEADER
      // --------------------------------------------------------

      worksheet.PageSetup.SetRowsToRepeatAtTop(
          1,
          ColumnHeaderRow);

      // --------------------------------------------------------
      // NO FREEZE PANE
      // --------------------------------------------------------

      // Intentionally not using FreezeRows / FreezeColumns.
    }

    // ============================================================
    // FORMAT YEAR HEADERS
    // ============================================================

    private void FormatYearHeaders(
        IXLWorksheet worksheet,
        int lastColumn)
    {
      // --------------------------------------------------------
      // Row 7
      // Reported / Risk Adjusted
      // --------------------------------------------------------

      var dataTypeRange =
          worksheet.Range(
              DataTypeHeaderRow,
              FirstYearColumn,
              DataTypeHeaderRow,
              lastColumn);

      dataTypeRange.Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Center;

      dataTypeRange.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;

      dataTypeRange.Style.Alignment.WrapText = true;

      // --------------------------------------------------------
      // Row 8
      // Year
      // --------------------------------------------------------

      var yearHeaderRange =
          worksheet.Range(
              YearHeaderRow,
              FirstYearColumn,
              YearHeaderRow,
              lastColumn);

      yearHeaderRange.Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Center;

      yearHeaderRange.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;

      yearHeaderRange.Style.Font.Bold = true;

      yearHeaderRange.Style.Alignment.WrapText = true;

      // --------------------------------------------------------
      // Row 9
      // Q1...Total
      // --------------------------------------------------------

      var columnHeaderRange =
          worksheet.Range(
              ColumnHeaderRow,
              FirstYearColumn,
              ColumnHeaderRow,
              lastColumn);

      columnHeaderRange.Style.Alignment.Horizontal =
          XLAlignmentHorizontalValues.Center;

      columnHeaderRange.Style.Alignment.Vertical =
          XLAlignmentVerticalValues.Center;

      columnHeaderRange.Style.Font.Bold = true;

      columnHeaderRange.Style.Alignment.WrapText = true;

      worksheet.Row(
          DataTypeHeaderRow).Height = 22;

      worksheet.Row(
          YearHeaderRow).Height = 28;

      worksheet.Row(
          ColumnHeaderRow).Height = 35;
    }

    // ============================================================
    // TOP LEVEL ORDER
    // ============================================================

    private static int GetTopLevelCategoryOrder(
        string? metricCode)
    {
      if (string.IsNullOrWhiteSpace(metricCode))
      {
        return int.MaxValue;
      }

      var code =
          metricCode.Trim()
              .ToUpperInvariant();

      if (code.StartsWith("MW"))
      {
        return 1;
      }

      if (code.StartsWith("CV"))
      {
        return 2;
      }

      if (code.StartsWith("EE"))
      {
        return 3;
      }

      if (code.StartsWith("MC"))
      {
        return 4;
      }

      if (code.StartsWith("SB"))
      {
        return 5;
      }

      if (code.StartsWith("CI"))
      {
        return 6;
      }

      return 99;
    }

    // ============================================================
    // METRIC SORT
    // ============================================================

    private static (
        string Prefix,
        int Number)
        GetMetricSortParts(
            string? metricCode)
    {
      if (string.IsNullOrWhiteSpace(
          metricCode))
      {
        return (
            string.Empty,
            int.MaxValue);
      }

      var code =
          metricCode.Trim();

      var lastDot =
          code.LastIndexOf('.');

      if (lastDot < 0)
      {
        return (
            code,
            int.MaxValue);
      }

      var prefix =
          code[..lastDot];

      var numberPart =
          code[(lastDot + 1)..];

      return int.TryParse(
          numberPart,
          out var number)
          ? (
              prefix,
              number)
          : (
              prefix,
              int.MaxValue);
    }

    // ============================================================
    // COLUMN NUMBER -> EXCEL LETTER
    // ============================================================

    private static string ColumnLetter(
        int columnNumber)
    {
      var dividend =
          columnNumber;

      var columnName =
          string.Empty;

      while (dividend > 0)
      {
        var modulo =
            (dividend - 1) % 26;

        columnName =
            Convert.ToChar(
                65 + modulo) +
            columnName;

        dividend =
            (dividend - modulo) / 26;
      }

      return columnName;
    }


    private void ApplyDecimalFormatting(
        IXLWorksheet worksheet,
        int firstRow,
        int lastRow,
        int lastColumn)
    {
      for (int row = firstRow; row <= lastRow; row++)
      {
        for (int column = FirstYearColumn;
             column <= lastColumn;
             column++)
        {
          var cell = worksheet.Cell(row, column);

          // Every year contains 10 numeric columns:
          // Reported Q1, Q2, Q3, Q4, Total
          // Risk-adjusted Q1, Q2, Q3, Q4, Total
          cell.Style.NumberFormat.Format = "#,##0.000";
        }
      }
    }

  }
}
