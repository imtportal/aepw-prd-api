using System;
using System.Collections.Generic;
using System.Text;
using AEPW.Application.Interfaces.Services;
using AEPW.Application.DTOs.Dashboard;
using AEPW.Application.Interfaces.Repositories;
using AEPW.Application.Interfaces.Repositories.Dashboard;
using AEPW.Application.Interfaces.Services.Dashboard;

namespace AEPW.Application.Services.Dashboard
{
  public sealed class DashboardService : IDashboardService
  {
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(
        IDashboardRepository dashboardRepository)
    {
      _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardPortfolioListDto>
     GetDashboardPortfolioListAsync(
         DashboardPortfolioListRequestDto request,
         CancellationToken cancellationToken = default)
    {
      return await _dashboardRepository
          .GetDashboardPortfolioListAsync(
              request,
              cancellationToken);
    }

    public async Task<IReadOnlyList<DashboardPortfolioAdjustmentByCategoriesDto>>
        GetDashboardPortfolioAdjustmentByCategoriesAsync(
            DashboardPortfolioByCategoriesRequestDto request,
            CancellationToken cancellationToken = default)
    {
      return await _dashboardRepository
          .GetDashboardPortfolioAdjustmentByCategoriesAsync(
              request,
              cancellationToken);
    }
  }
}
