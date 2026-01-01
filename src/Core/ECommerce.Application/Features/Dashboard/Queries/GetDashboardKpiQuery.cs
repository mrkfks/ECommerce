using MediatR;
using ECommerce.Application.DTOs.Dashboard;

namespace ECommerce.Application.Features.Dashboard.Queries;

/// <summary>
/// Dashboard KPI verilerini getiren query
/// </summary>
public class GetDashboardKpiQuery : IRequest<DashboardKpiDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? CompanyId { get; set; }
}
