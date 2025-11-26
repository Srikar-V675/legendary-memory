using BidSphere.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BidSphere.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get comprehensive system metrics for dashboard
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            try
            {
                var metrics = await _dashboardService.GetDashboardMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard metrics");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error fetching dashboard metrics",
                    error = ex.Message
                });
            }
        }
    }
}
