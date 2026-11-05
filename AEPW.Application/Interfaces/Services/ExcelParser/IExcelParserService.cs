using AEPW.Application.DTOs.Review;
using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.Interfaces.Services.ExcelParser
{
    public interface IExcelParserService
    {
        List<ReviewExcelImportDto> ParseWorkbook(
           Stream stream,
           long reviewId,
           out string projectPin);
    }
}
