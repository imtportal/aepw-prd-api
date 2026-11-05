using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Dashboard
{
  public sealed class DashboardPortfolioByCategoriesRequestDto
  {
    public string? CategoryCode { get; set; }

    public List<string> ReviewPeriodCodes { get; set; } = new();

    public bool SinceInception { get; set; }
  }
}
