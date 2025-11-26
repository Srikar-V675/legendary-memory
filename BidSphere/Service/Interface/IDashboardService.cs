using BidSphere.Models.Dtos.Dashboard;

namespace BidSphere.Service.Interface
{
    public interface IDashboardService
    {
        Task<DashboardMetricsDto> GetDashboardMetricsAsync();
    }
}
