using System;
using System.Collections.Generic;
using System.Text;
using AEPW.Application.DTOs.Dashboard;

namespace AEPW.Application.Interfaces.Services.Dashboard
{
  public interface IDashboardService
  {
    Task<DashboardPortfolioListDto> GetDashboardPortfolioListAsync(
       DashboardPortfolioListRequestDto request,
       CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DashboardPortfolioAdjustmentByCategoriesDto>>
        GetDashboardPortfolioAdjustmentByCategoriesAsync(
            DashboardPortfolioByCategoriesRequestDto request,
            CancellationToken cancellationToken = default);
  }


}
