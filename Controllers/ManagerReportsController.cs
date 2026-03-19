using CinemaManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaManagement.Controllers;

public class ManagerReportsController : Controller
{
    private readonly IReportService _report;
    private readonly IExportService _export;

    public ManagerReportsController(IReportService report, IExportService export)
    {
        _report = report;
        _export = export;
    }

    // GET: /ManagerReports/Dashboard?from=2026-01-01&to=2026-01-31
    public async Task<IActionResult> Dashboard(DateTime? from, DateTime? to)
    {
        var fromValue = from ?? DateTime.Today.AddDays(-7);
        var toValue = to ?? DateTime.Today;

        var vm = await _report.GetDashboardAsync(fromValue, toValue);
        return View(vm);
    }

    // GET: /ManagerReports/ExportExcel?from=...&to=...
    public async Task<IActionResult> ExportExcel(DateTime? from, DateTime? to)
    {
        var fromValue = from ?? DateTime.Today.AddDays(-7);
        var toValue = to ?? DateTime.Today;

        var vm = await _report.GetDashboardAsync(fromValue, toValue);
        var bytes = _export.ExportDashboardToExcel(vm);

        var fileName = $"revenue-report-{fromValue:yyyyMMdd}-{toValue:yyyyMMdd}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    // GET: /ManagerReports/ExportPdf?from=...&to=...
    public async Task<IActionResult> ExportPdf(DateTime? from, DateTime? to)
    {
        var fromValue = from ?? DateTime.Today.AddDays(-7);
        var toValue = to ?? DateTime.Today;

        var vm = await _report.GetDashboardAsync(fromValue, toValue);
        var bytes = _export.ExportDashboardToPdf(vm);

        var fileName = $"revenue-report-{fromValue:yyyyMMdd}-{toValue:yyyyMMdd}.html";
        // Trả về HTML — browser in thành PDF bằng Ctrl+P → Save as PDF
        // Production: dùng Playwright/PuppeteerSharp để render PDF server-side
        return File(bytes, "text/html; charset=utf-8", fileName);
    }
}