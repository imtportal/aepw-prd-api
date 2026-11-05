using AEPW.Application.DTOs.Review;
using AEPW.Application.Interfaces.Services.AzureBlobService;
using AEPW.Application.Interfaces.Services.ExcelImportExportService;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace AEPW.Application.Services.Services
{
    public class DataCollectionExcelGeneratorService: IDataCollectionExcelGeneratorService
    {
        IAzureBlobService _azureBlobService;
        IConfiguration _configuration;
        public DataCollectionExcelGeneratorService(IAzureBlobService azureBlobService, IConfiguration configuration)
        {
            _azureBlobService = azureBlobService;
            _configuration = configuration;
        }

        public async Task<byte[]> GenerateWorkbook(List<ReviewExcelExportDto> rows, Dictionary<string, bool> metricCodesMap, string valorisationTypeNames)
        {

            //var templatePath = Path.Combine(
            //    Directory.GetCurrentDirectory(),
            //    "Templates",
            //    "AEPWTemp.xlsx"
            //);


            string containerName = _configuration["DataCollectionTemplate:ContainerName"];
            string fileName = "AEPWTemp.xlsx";
            Stream templatePath = await _azureBlobService.GetFileAsync(containerName, fileName);
             
            if (!rows.Any())
                throw new Exception("No review data found.");

            //if (!File.Exists(templatePath))
            //    throw new FileNotFoundException($"Template not found: {templatePath}");

            using var workbook = new XLWorkbook(templatePath);

            // Instructions Sheet
            var instructionSheet = workbook.Worksheet("Instructions");
            instructionSheet.Protect();

            var first = rows.First();
            var reportingPeriod = GetReportingPeriod(first);

            var l1ReportingPeriod =
                $"{first.Year}{Environment.NewLine}{reportingPeriod}";

            instructionSheet.Cell("C6").Value = first.Pin;
            instructionSheet.Cell("C6").Style.Protection.SetLocked(true);

            instructionSheet.Cell("C8").Value = first.ProjectTitle;
            instructionSheet.Cell("C8").Style.Protection.SetLocked(true);
            // Keep multiple FR names and emails comma-separated
            var frNameCell = instructionSheet.Cell("C10");
            instructionSheet.Cell("C10").Style.Protection.SetLocked(false);

            var frEmailCell = instructionSheet.Cell("C12");
            instructionSheet.Cell("C12").Style.Protection.SetLocked(false);

            frNameCell.Value = first.FundingRecipientName ?? "";
            frEmailCell.Value = first.Email ?? "";

            // Enable wrapping so long comma-separated values can wrap naturally
            frNameCell.Style.Alignment.WrapText = true;
            frEmailCell.Style.Alignment.WrapText = true;

            frNameCell.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Top;

            frEmailCell.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Top;

            // Dynamically adjust row heights
            SetDynamicRowHeight(
                instructionSheet,
                10,
                first.FundingRecipientName ?? "",
                3,
                6,
                30,
                150
            );

            SetDynamicRowHeight(
                instructionSheet,
                12,
                first.Email ?? "",
                3,
                6,
                30,
                150
            );
            instructionSheet.Cell("C14").Value = reportingPeriod;
            instructionSheet.Cell("D14").Value = $"in {first.Year}.";

            // Group rows into categories
            var categories = rows
                .GroupBy(x => x.CategoryName)
                .OrderBy(x => x.First().CategoryDisplayOrder);

            //using var workbook = new XLWorkbook(templatePath);

            // Get the template worksheet once
            var templateSheet = workbook.Worksheet("CategoryTemplate");

            // Remove all inherited template validations before CopyTo().
            // The required validations will be created explicitly below.
            templateSheet.DataValidations.Delete(dv => true);

            foreach (var category in categories)
            {
                var topLevelMetrics = category
                    .Where(x => x.IsTopLevel)
                    .GroupBy(x => x.MetricId)
                    .OrderBy(x => x.First().MetricId);



                var sheetName = $"{category.First().CategoryCode} {category.First().CategoryName}";

                if (sheetName.Length > 31)
                {
                    sheetName = sheetName.Substring(0, 31);
                }

                templateSheet.CopyTo(sheetName);

                var ws = workbook.Worksheet(sheetName);

                // Remove inherited template validations from the copied worksheet.
                // Required dropdown validations will be created explicitly.
                ws.DataValidations.Delete(dv => true);

                ws.ShowGridLines = false;

                //ws.DataValidations.Delete(dv => dv.Ranges.Any(r => r.RangeAddress.ToString() == "F12"));    // wipe inherited/template validations first

                // Populate Category code/name
                var firstCategory = category.First();

                ws.Cell("B3").Value = firstCategory.CategoryCode;
                ws.Cell("C3").Value = firstCategory.CategoryName;

                // Category description - C4:F4
                var categoryDescRange = ws.Range("C4:F4");

                if (!categoryDescRange.IsMerged())
                {
                    categoryDescRange.Merge();
                }


                var categoryDescription =
                    firstCategory.CategoryDescription ?? "";



                var categoryDescriptionCell = ws.Cell("C4");

                categoryDescriptionCell.Clear(XLClearOptions.Contents);

                var categoryRichText = categoryDescriptionCell.GetRichText();

                var referenceIndex = categoryDescription.IndexOf(
                    "Refer to",
                    StringComparison.OrdinalIgnoreCase);

                if (referenceIndex >= 0)
                {
                    // First part: italic
                    var mainDescription =
                        categoryDescription.Substring(0, referenceIndex).Trim();

                    categoryRichText
                        .AddText(mainDescription)
                        .SetBold(false)
                        .SetItalic(true)
                        .SetFontSize(10)
                        .SetFontColor(XLColor.FromHtml("#014848"));

                    // Second part: normal, non-italic
                    var referenceDescription =
                        categoryDescription.Substring(referenceIndex).Trim();

                    categoryRichText.AddText(Environment.NewLine);

                    categoryRichText
                        .AddText(referenceDescription)
                        .SetBold(false)
                        .SetItalic(false)
                        .SetFontSize(9)
                        .SetFontColor(XLColor.Black);
                }
                else
                {
                    // If "Refer to" is not present, keep the whole description italic
                    categoryRichText
                        .AddText(categoryDescription)
                        .SetBold(false)
                        .SetItalic(true)
                        .SetFontSize(10)
                        .SetFontColor(XLColor.FromHtml("#014848"));
                }

                // Formatting for merged range C4:F4
                categoryDescRange.Style.Alignment.WrapText = true;
                categoryDescRange.Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Top;

                ws.Row(4).Height = 45;

                // Alignment only — do not apply font formatting to the whole range
                categoryDescRange.Style.Alignment.WrapText = true;
                categoryDescRange.Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Top;

                // Keep category description height separate
                ws.Row(4).Height = 45;

                // Category sheet reporting period
                ws.Cell("E7").Value = l1ReportingPeriod;
                ws.Cell("B6").Clear();

                int currentRow = 7;



                foreach (var topLevelGroup in topLevelMetrics)
                {
                    var l1Metric = topLevelGroup.First();



                    var subMetrics = category
    .Where(x =>
        !x.IsTopLevel &&
        x.ParentMetricId == l1Metric.MetricId)
    .GroupBy(x => x.MetricId)
    .OrderBy(x => GetMetricCodeOrder(
        x.First().SubMetricCode))
    .ToList();

                    var firstSubMetric = subMetrics.FirstOrDefault()?.First();

                    var firstMetric = l1Metric;

                    // Fill metric header details
                    var metricCode = l1Metric?.SubMetricCode ?? firstMetric.SubMetricCode;
                    var metricName = l1Metric?.SubMetricName ?? firstMetric.SubMetricName;
                    var metricDescription = l1Metric?.MetricDescription ?? firstMetric.MetricDescription;
                    var metricHeader = l1Metric?.MetricHeader ?? firstMetric.MetricHeader;
                    var metricUnit = l1Metric?.UnitName ?? firstMetric.UnitName;

                    int metricStartRow = currentRow;


                    int metricHeaderRow = metricStartRow - 1;

                    // Copy the template L1 metric-header row.
                    templateSheet.Range("B6:F6")
                        .CopyTo(
                            ws.Range(
                                metricHeaderRow,
                                2,
                                metricHeaderRow,
                                6));

                    // Copy the template row height
                    ws.Row(metricHeaderRow).Height =
                        templateSheet.Row(6).Height;

                    // Dynamic L1 metric code
                    var topLevelCodeCell = ws.Cell(metricHeaderRow, 2);
                    topLevelCodeCell.Clear(XLClearOptions.Contents);
                    topLevelCodeCell.Value = metricCode;



                    // Dynamic L1 metric header
                    var topLevelHeaderCell = ws.Cell(metricHeaderRow, 3);
                    topLevelHeaderCell.Clear(XLClearOptions.Contents);

                    var topLevelRichText = topLevelHeaderCell.GetRichText();

                    // L1 metric name - bold and 12 pt
                    var metricNameText = topLevelRichText.AddText(metricName ?? "");
                    metricNameText.SetBold(true);
                    metricNameText.SetFontSize(12);

                    // L1 metric header/description
                    var metricHeaderText = topLevelRichText.AddText(
                        Environment.NewLine + (metricHeader ?? ""));

                    // Keep the supporting header text smaller
                    metricHeaderText.SetFontSize(10);

                    // Cell formatting
                    topLevelHeaderCell.Style.Alignment.WrapText = true;
                    topLevelHeaderCell.Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Top;
                    // Dynamic height for the L1 metric header above the table
                    var l1MetricHeaderText =
                        (metricName ?? "") +
                        Environment.NewLine +
                        (metricHeader ?? "");

                    SetDynamicRowHeight(
                    ws,
                        metricHeaderRow,
                        l1MetricHeaderText,
                        3,
                        6,
                        30,
                        180);



                    if (metricStartRow != 7)
                    {
                        // Copy complete L1 template block
                        templateSheet.Range("B7:F14")
                            .CopyTo(
                                ws.Range(
                                    metricStartRow,
                                    2,
                                    metricStartRow + 7,
                                    6));

                        // Copy template row heights
                        for (int sourceRow = 7; sourceRow <= 14; sourceRow++)
                        {
                            int targetRow =
                                metricStartRow + (sourceRow - 7);

                            ws.Row(targetRow).Height =
                                templateSheet.Row(sourceRow).Height;
                        }
                    }
                    // Clear the template's default L2 area before rendering actual L2 rows
                    int l2TemplateStartRow = metricStartRow + 3;
                    int l2TemplateEndRow = metricStartRow + 7;

                    ws.Range(
                        l2TemplateStartRow,
                        2,
                        l2TemplateEndRow,
                        9)
                        .Clear(XLClearOptions.All);


                    // Populate the L1 table header in column C
                    var tableHeader = l1Metric?.TopLevelTableHeader ?? "";

                    var tableHeaderCell = ws.Cell(metricStartRow, 3);

                    tableHeaderCell.Value = tableHeader;
                    //MC Conditional formatting for L1 table header
                    // Set the MC-specific L1 column F header
                    if (string.Equals(
                            category.First().CategoryCode,
                            "MC",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        ws.Cell(metricStartRow, 6).Value = "Validation Checks";
                        ws.Cell(metricStartRow, 6).Style.Alignment.WrapText = true;
                    }
                    tableHeaderCell.Style.Alignment.WrapText = true;
                    tableHeaderCell.Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;

                    // Dynamic height for L1 table header
                    var l1TableHeaderRow = metricStartRow;

                    // L1 header details in columns C and D
                    var l1TableHeaderRange = ws.Range(
                        l1TableHeaderRow,
                        3,
                        l1TableHeaderRow,
                        4);

                    l1TableHeaderRange.Style.Alignment.WrapText = true;

                    // Reporting period in column E
                    var l1ReportingPeriodCell = ws.Cell(
                        l1TableHeaderRow,
                        5);

                    l1ReportingPeriodCell.Value = l1ReportingPeriod;

                    l1ReportingPeriodCell.Style.Alignment.WrapText = true;
                    l1ReportingPeriodCell.Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;
                    l1ReportingPeriodCell.Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;

                    // Give enough height for two lines
                    ws.Row(l1TableHeaderRow).Height = 35;

                    ws.Cell(metricStartRow + 1, 4).Value = l1Metric?.UnitName ?? "";

                    // L1 calculation cell - calculated automatically from L2 values
                    var numericCell = ws.Cell(metricStartRow + 1, 5);

                    numericCell.Value = "0";
                    numericCell.Style.NumberFormat.Format = "#,##0";

                    numericCell.Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    numericCell.Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;

                    // User must not edit the calculated L1 value
                    numericCell.Style.Protection.Locked = true;

                    //L1 Supporting Document / Comment - Free Text
                    var l1FreeTextCell = ws.Cell(
                        metricStartRow + 1,
                        6);

                    l1FreeTextCell.Value = "<free text>";

                    var l1FreeTextValidation =
                        l1FreeTextCell.CreateDataValidation();

                    l1FreeTextValidation.InputTitle = "<free text>";
                    l1FreeTextValidation.InputMessage =
                        "Please provide details on the evidence to support the data, training materials, event and participants details etc.";

                    l1FreeTextValidation.ShowInputMessage = true;

                    l1FreeTextCell.Style.Protection.Locked = false;



                    var l1NameRange = ws.Cell(
                        metricStartRow + 1,
                        3);

                    var l1MetricCell = l1NameRange;

                    l1MetricCell.Value = "";

                    if (l1Metric != null)
                    {
                        l1MetricCell.Clear(XLClearOptions.Contents);

                        var l1RichText = l1MetricCell.GetRichText();

                        var l1DisplayName =
                            (l1Metric.SubMetricName ?? "").Trim();

                        var l1Description =
                            (l1Metric.MetricDescription ?? "").Trim();

                        // Metric name: bold
                        l1RichText
                            .AddText(l1DisplayName)
                            .SetBold(true)
                            .SetItalic(false);

                        // Definition label: bold
                        l1RichText
                            .AddText(Environment.NewLine + "Definition: ")
                            .SetBold(true)
                            .SetItalic(false);

                        // Description: normal
                        l1RichText
                            .AddText(l1Description)
                            .SetBold(false)
                            .SetItalic(false);
                    }

                    l1NameRange.Style.Alignment.WrapText = true;
                    l1NameRange.Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Left;
                    l1NameRange.Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Top;

                    // Dynamic height for the L1 description row
                    var l1Text =
    (l1Metric?.SubMetricName ?? "").Trim() +
    Environment.NewLine +
    "Definition: " +
    (l1Metric?.MetricDescription ?? "").Trim();

                    SetDynamicRowHeight(
                        ws,
                        metricStartRow + 1,
                        l1Text,
                        3,
                        3,
                        70,
                        300);

                    // L1 Metric Data Code
                    if (l1Metric != null)
                    {
                        var l1CodeCell = ws.Cell(metricStartRow + 1, 2);
                        l1CodeCell.Clear(XLClearOptions.Contents);
                        l1CodeCell.Value = $"{metricCode} (Total)";
                        l1CodeCell.Style.Alignment.Horizontal =
                            XLAlignmentHorizontalValues.Right;
                        l1CodeCell.Style.Alignment.Vertical =
                            XLAlignmentVerticalValues.Top;
                    }


                    // Lock the complete L1 table
                    ws.Range(
                        metricStartRow,
                        2,
                        metricStartRow + 1,
                        6)
                        .Style.Protection.Locked = true;

                    // Unlock only the L1 Supporting Documentation / Comments cell
                    var l1CommentsCell = ws.Cell(
                        metricStartRow + 1,
                        6);

                    l1CommentsCell.Style.Protection.Locked = false;

                    int lastSubMetricRow = -1;



                    if (subMetrics.Any())
                    {
                        // L2 table starts below the L1 table
                        int l2StartRow = metricStartRow + 3;

                        templateSheet.Range("C10:F10")
                            .CopyTo(
                                ws.Range(
                                    l2StartRow,
                                    3,
                                    l2StartRow,
                                    6));

                        // Clear old template content outside the L2 table
                        ws.Range(
                            l2StartRow,
                            7,
                            l2StartRow,
                            9)
                            .Clear(XLClearOptions.Contents);


                        var l2HeaderCell = ws.Cell(l2StartRow, 3);

                        l2HeaderCell.Clear(XLClearOptions.Contents);

                        var l2HeaderText =
                            firstSubMetric?.MetricHeader ?? "";

                        var l2HeaderLines = l2HeaderText
                            .Split(
                                new[] { Environment.NewLine, "\n" },
                                StringSplitOptions.None);

                        var l2HeaderRichText = l2HeaderCell.GetRichText();

                        // First line: bold
                        if (l2HeaderLines.Length > 0)
                        {
                            l2HeaderRichText
                                .AddText(l2HeaderLines[0])
                                .SetBold(true);
                        }

                        // Remaining lines: unbold
                        for (int i = 1; i < l2HeaderLines.Length; i++)
                        {
                            l2HeaderRichText.AddText(Environment.NewLine);

                            l2HeaderRichText
                                .AddText(l2HeaderLines[i])
                                .SetBold(false);
                        }

                        // Reporting period -> E10
                        ws.Cell(l2StartRow, 5).Value =
                            reportingPeriod;
                        // Set the MC-specific L2 column F header
                        if (string.Equals(
                                category.First().CategoryCode,
                                "MC",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            var l2CommentsHeaderCell = ws.Cell(l2StartRow, 6);

                            l2CommentsHeaderCell.Value =
                                "Please indicate the documentation you have to support the data provided, and any additional details / comments.";

                            l2CommentsHeaderCell.Style.Alignment.WrapText = true;
                            l2CommentsHeaderCell.Style.Alignment.Vertical =
                                XLAlignmentVerticalValues.Center;
                        }

                        // Apply wrapping
                        ws.Range(
                            l2StartRow,
                            3,
                            l2StartRow,
                            6)
                            .Style.Alignment.WrapText = true;

                        // Calculate height after setting the actual values
                        SetL2HeaderRowHeight(ws, l2StartRow);
                        // Lock the complete L2 header row
                        ws.Range(
                            l2StartRow,
                            2,
                            l2StartRow,
                            6)
                            .Style.Protection.Locked = true;

                        // First L2 data row
                        currentRow = l2StartRow + 1;

                        foreach (var subMetricGroup in subMetrics)
                        {
                            var firstSub = subMetricGroup.First();

                            int l2DataRow = currentRow;

                            //  ONLY copy the main L2 data row (B11:F11)
                            templateSheet.Range("B11:F11")
                                .CopyTo(
                                    ws.Range(
                                        l2DataRow,
                                        2,
                                        l2DataRow,
                                        6));

                            // Clear old template content outside the L2 table
                            ws.Range(
                                l2DataRow,
                                7,
                                l2DataRow,
                                9)
                                .Clear(XLClearOptions.Contents);

                            ws.Row(l2DataRow).Height =
                                templateSheet.Row(11).Height;

                            var subMetricCodeCell =
                                ws.Cell(l2DataRow, 2);

                            subMetricCodeCell.Clear(
                                XLClearOptions.Contents);

                            subMetricCodeCell.Value =
                                firstSub.SubMetricCode;

                            var subMetricCell =
                                ws.Cell(l2DataRow, 3);

                            subMetricCell.Value = "";

                            var subMetricRichText = subMetricCell.GetRichText();

                            subMetricCell.Clear(XLClearOptions.Contents);

                            subMetricRichText =
                                subMetricCell.GetRichText();

                            var l2MetricName =
                                (firstSub.SubMetricName ?? "").Trim();

                            var l2GuidingNotes =
                                (firstSub.MetricDescription ?? "").Trim();

                            // L2 metric name: bold and non-italic
                            subMetricRichText
                                .AddText(l2MetricName)
                                .SetBold(true)
                                .SetItalic(false);

                            // Guiding Notes label: bold, with no blank line
                            subMetricRichText
                                .AddText(Environment.NewLine + "Guiding Notes: ")
                                .SetBold(true)
                                .SetItalic(false);

                            // Guiding Notes content: normal and non-italic
                            subMetricRichText
                                .AddText(l2GuidingNotes)
                                .SetBold(false)
                                .SetItalic(false);

                            ws.Cell(l2DataRow, 4).Value =
                                firstSub.UnitName ?? "";

                            var l2NumericCell = ws.Cell(l2DataRow, 5);
                            
                            l2NumericCell.Style.NumberFormat.Format = "#,##0";

                            var numericValidation = l2NumericCell.CreateDataValidation();

                            // Allow decimal numbers
                            numericValidation.Decimal.Between(
                                -999999999999,
                                999999999999);

                            numericValidation.InputTitle =
                                "Only Numerical";
                            numericValidation.InputMessage =
                                "Please enter a numerical value. Leave blank if not applicable.";

                            numericValidation.ShowInputMessage = true;

                            numericValidation.ShowErrorMessage = true;
                            numericValidation.ErrorTitle =
                                "Invalid Value";

                            numericValidation.ErrorMessage =
                                "Please enter a numerical value.";

                            numericCell.Style.Protection.Locked = false;
                            var frCell =
                                ws.Cell(l2DataRow, 6);

                            frCell.Value =
                                "<free text>";

                            var freeTextValidation =
                                frCell.CreateDataValidation();

                            freeTextValidation.InputTitle =
                                "<free text>";

                            freeTextValidation.InputMessage =
                                "Please provide details on the evidence to support the data, training materials, event and participants details etc.";

                            freeTextValidation.ShowInputMessage = true;

                            frCell.Style.Protection.Locked =
                                false;

                            // Lock the complete L2 data row first
                            ws.Range(
                                l2DataRow,
                                2,
                                l2DataRow,
                                6)
                                .Style.Protection.Locked = true;

                            // Unlock only the L2 numeric/value cell in column E
                            var l2ValueInputCell = ws.Cell(l2DataRow, 5);
                            l2ValueInputCell.Style.Protection.Locked = false;

                            // Unlock only the L2 Supporting Documentation / Comments cell in column F
                            var l2CommentsInputCell = ws.Cell(l2DataRow, 6);
                            l2CommentsInputCell.Style.Protection.Locked = false;

                            // Dynamic height for L2 description
                            var l2Text =
    (firstSub.SubMetricName ?? "").Trim() +
    Environment.NewLine +
    "Guiding Notes: " +
    (firstSub.MetricDescription ?? "").Trim();

                            SetDynamicRowHeight(
                                ws,
                                l2DataRow,
                                l2Text,
                                3,
                                3,
                                70,
                                300);

                            lastSubMetricRow = l2DataRow;

                            // ✅ ONLY add extra row if metric is MW1.13
                            // Add the extra valorisation row only for MW1.13
                            if (HasAdditionalRow(metricCodesMap, firstSub.SubMetricCode))
                            {
                                int additionalRow = l2DataRow + 1;

                                // Copy visible additional-row template cells C12:F12
                                // Copy visible additional-row template cells C12:F12.
                                // Template validations have already been removed.
                                templateSheet.Range("B12:F12")
                                    .CopyTo(
                                        ws.Range(
                                            additionalRow,
                                            2,
                                            additionalRow,
                                            6));

                                var additionalRowCommentCell = ws.Cell(additionalRow, 6);

                                // Clear the placeholder value
                                additionalRowCommentCell.Clear(
                                    XLClearOptions.Contents);

                                // Create the dropdown explicitly
                                var dropdownValidation =
                                    additionalRowCommentCell.CreateDataValidation();

                                //// Replace these with your actual dropdown options
                                dropdownValidation.List(
                                    ("\"" + valorisationTypeNames + "\""),

                                    true);

                                dropdownValidation.ShowErrorMessage = true;
                                dropdownValidation.ErrorTitle =
                                    "Select a valid option";

                                dropdownValidation.ErrorMessage =
                                    "Please choose an option from the dropdown list.";

                                dropdownValidation.ShowInputMessage = true;

                                additionalRowCommentCell.Style.Protection.Locked = false;





                                // Do not create another validation here.
                                // The template validation is copied with C12:F12.


                                var metadataSourceCell = templateSheet.Cell("B12");
                                var metadataTargetCell = ws.Cell(additionalRow, 2);

                                // Copy metadata value
                                metadataTargetCell.Value = metadataSourceCell.Value;

                                // Make metadata visible
                                metadataTargetCell.Style.Font.FontColor = XLColor.White;

                                // Keep metadata cell locked
                                metadataTargetCell.Style.Protection.Locked = true;

                                ws.Range(
                                    additionalRow,
                                    8,
                                    additionalRow,
                                    9)
                                    .Clear(XLClearOptions.Contents);

                                // Remove bottom border from the visible additional-row area
                                ws.Range(
                                    additionalRow,
                                    2,
                                    additionalRow,
                                    6)
                                    .Style.Border.BottomBorder =
                                        XLBorderStyleValues.None;

                                ws.Row(additionalRow).Height =
                                    templateSheet.Row(12).Height;

                                currentRow = additionalRow + 1;
                            }
                            else
                            {
                                currentRow = l2DataRow + 1;
                            }
                        }

                        int firstL2Row = l2StartRow + 1;

                        if (lastSubMetricRow >= firstL2Row)
                        {
                            var l1ValueCell =
                                ws.Cell(metricStartRow + 1, 5);

                            l1ValueCell.FormulaA1 =
                                $"=IF(COUNT(E{firstL2Row}:E{lastSubMetricRow})=0," +
                                $"0," + 
                                $"SUM(E{firstL2Row}:E{lastSubMetricRow}))";
                            l1ValueCell.Style.NumberFormat.Format = "#,##0";
                            //l1ValueCell.Style.NumberFormat.Format =
                                //"General";

                            l1ValueCell.Style.Alignment.Horizontal =
                                XLAlignmentHorizontalValues.Center;

                            l1ValueCell.Style.Alignment.Vertical =
                                XLAlignmentVerticalValues.Center;

                            l1ValueCell.Style.Protection.Locked =
                                true;
                        }

                        int lastRenderedL2Row = currentRow - 1;

                        // Keep the right border of the final comments cell
                        ws.Cell(lastRenderedL2Row, 6)
                            .Style.Border.RightBorder =
                                XLBorderStyleValues.Thin;

                        // Leave one blank row after the last L2 row
                        int separatorRow = currentRow + 1;

                        // Add one separator line
                        var separatorRange =
                            ws.Range(
                                separatorRow,
                                2,
                                separatorRow,
                                6);

                        separatorRange.Style.Border.TopBorder =
                            XLBorderStyleValues.Thin;

                        // Leave one blank row after the separator
                        currentRow = separatorRow + 2;


                    }
                }
                ws.Protect();
            }

            // Remove the CategoryTemplate sheet
            if (workbook.Worksheets.Contains("CategoryTemplate"))
            {
                workbook.Worksheet("CategoryTemplate").Delete();
            }

            workbook.RecalculateAllFormulas();
            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            //return Task.FromResult(ms.ToArray());
            return ms.ToArray();
        }
        private static bool HasAdditionalRow(Dictionary<string, bool> ValroizationRequiredMap, string metricCode)
        {
            return ValroizationRequiredMap.ContainsKey(metricCode);
        }

        private static string GetReportingPeriod(ReviewExcelExportDto review)
        {
            if (review.Quarter.HasValue)
            {
                return $"Quarter {review.Quarter} ({review.StartUtc:dd MMM} to {review.EndUtc:dd MMM})";
            }

            return $"Annual ({review.StartUtc:dd MMM} to {review.EndUtc:dd MMM})";
        }

        private static void SetDynamicRowHeight(
           IXLWorksheet worksheet,
           int rowNumber,
           string text,
           int startColumn,
           int endColumn,
           double minimumHeight = 60,
           double maximumHeight = 300)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                worksheet.Row(rowNumber).Height = minimumHeight;
                return;
            }

            // Calculate approximate available width
            double totalWidth = 0;

            for (int column = startColumn; column <= endColumn; column++)
            {
                totalWidth += worksheet.Column(column).Width;
            }

            // Approximate characters that can fit on one line
            double charactersPerLine = Math.Max(
                10,
                totalWidth * 1.1);

            int estimatedLines = 0;

            // Consider explicit line breaks
            var lines = text.Split(
                new[] { Environment.NewLine, "\n" },
                StringSplitOptions.None);

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    estimatedLines++;
                    continue;
                }

                estimatedLines += Math.Max(
                    1,
                    (int)Math.Ceiling(
                        line.Length / charactersPerLine));
            }

            // Approximate Excel row height
            double calculatedHeight = estimatedLines * 15;

            // Keep within reasonable limits
            calculatedHeight = Math.Max(
                minimumHeight,
                calculatedHeight);

            calculatedHeight = Math.Min(
                maximumHeight,
                calculatedHeight);

            worksheet.Row(rowNumber).Height = calculatedHeight;
        }

        //dynamic height for L2 header row
        private static void SetL2HeaderRowHeight(
            IXLWorksheet worksheet,
            int rowNumber)
        {
            var metricHeader = worksheet.Cell(rowNumber, 3).GetString();
            var reportingPeriod = worksheet.Cell(rowNumber, 5).GetString();

            // Approximate number of lines required in column C
            double metricHeaderWidth = worksheet.Column(3).Width;
            double metricCharactersPerLine =
                Math.Max(10, metricHeaderWidth * 1.1);

            int metricLines = CalculateRequiredLines(
                metricHeader,
                metricCharactersPerLine);

            // Approximate number of lines required in column E
            double reportingPeriodWidth = worksheet.Column(5).Width;
            double periodCharactersPerLine =
                Math.Max(10, reportingPeriodWidth * 1.1);

            int periodLines = CalculateRequiredLines(
                reportingPeriod,
                periodCharactersPerLine);

            // Use the tallest content in the header row
            int requiredLines = Math.Max(metricLines, periodLines);

            // Approximate Excel row height
            double calculatedHeight = requiredLines * 12;

            // Keep the header within a reasonable range
            calculatedHeight = Math.Max(35, calculatedHeight);
            calculatedHeight = Math.Min(80, calculatedHeight);

            worksheet.Row(rowNumber).Height = calculatedHeight;
        } 

        private static int CalculateRequiredLines(
            string text,
            double charactersPerLine)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 1;

            var lines = text.Split(
                new[] { Environment.NewLine, "\n" },
                StringSplitOptions.None);

            int totalLines = 0;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    totalLines++;
                    continue;
                }

                totalLines += Math.Max(
                    1,
                    (int)Math.Ceiling(
                        line.Length / charactersPerLine));
            }

            return totalLines;
        }

        // sorting 
        private static int GetMetricCodeOrder(string? metricCode)
        {
            if (string.IsNullOrWhiteSpace(metricCode))
                return int.MaxValue;

            var match = Regex.Match(metricCode, @"\.(\d+)$");

            return match.Success
                ? int.Parse(match.Groups[1].Value)
                : int.MaxValue;
        }

    }


}