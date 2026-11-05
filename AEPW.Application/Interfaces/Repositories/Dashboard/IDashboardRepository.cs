using System;
using System.Collections.Generic;
using System.Text;
using AEPW.Application.DTOs.Dashboard;

namespace AEPW.Application.Interfaces.Repositories.Dashboard
{
  public interface IDashboardRepository
  {
    Task<DashboardPortfolioListDto> GetDashboardPortfolioListAsync(
     DashboardPortfolioListRequestDto request,
     CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DashboardPortfolioAdjustmentByCategoriesDto>>  GetDashboardPortfolioAdjustmentByCategoriesAsync(
            DashboardPortfolioByCategoriesRequestDto request,
            CancellationToken cancellationToken = default);
  }
}
