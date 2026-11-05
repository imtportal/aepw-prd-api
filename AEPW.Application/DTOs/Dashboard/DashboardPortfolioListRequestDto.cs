using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Dashboard
{
  public sealed class DashboardPortfolioListRequestDto
  {
    public List<string> ReviewPeriodCodes { get; set; } = new();

    public string? CategoryCode { get; set; }

    public string? ProgramId { get; set; }
    public string? StatusId { get; set; }
    public string? RecordTypeId { get; set; }

    public string? SearchTerm { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public bool SinceInception { get; set; }
  }


}
