using CinemaManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaManagement.Controllers
{
    public class ManagerReportsController : Controller
    {
        private readonly IReportService _report;

        public ManagerReportsController(IReportService report) => _report = report;

        // GET: /ManagerReports/Dashboard?from=2026-01-01&to=2026-01-31
        public async Task<IActionResult> Dashboard(DateTime? from, DateTime? to)
        {
            var fromValue = from ?? DateTime.Today.AddDays(-7);
            var toValue = to ?? DateTime.Today;

            var vm = await _report.GetDashboardAsync(fromValue, toValue);
            return View(vm);
        }
    }
}
