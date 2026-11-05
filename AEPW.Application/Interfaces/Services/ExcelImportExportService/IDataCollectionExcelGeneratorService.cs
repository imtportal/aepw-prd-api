using AEPW.Application.DTOs.Review;
using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.Interfaces.Services.ExcelImportExportService
{
    public interface IDataCollectionExcelGeneratorService
    {
        Task<byte[]> GenerateWorkbook(List<ReviewExcelExportDto> rows
            , Dictionary<string, bool> valorisationMetricCodeMap
            , string valorisationTypeNames
            );
    }
}
