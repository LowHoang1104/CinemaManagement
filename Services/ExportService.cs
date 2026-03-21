using CinemaManagement.ViewModels.Manager;
using System.Text;

namespace CinemaManagement.Services;

public interface IExportService
{
    byte[] ExportDashboardToExcel(DashboardVm vm);
    byte[] ExportDashboardToPdf(DashboardVm vm);
}

public class ExportService : IExportService
{
    /// <summary>
    /// Export dashboard report sang Excel (dùng thư viện thuần, không cần thư viện ngoài).
    /// Production: thay bằng EPPlus hoặc ClosedXML để có format đẹp hơn.
    /// </summary>
    public byte[] ExportDashboardToExcel(DashboardVm vm)
    {
        // Dùng CSV-in-Excel approach (UTF-8 BOM) — không cần NuGet package.
        // Để production-grade hơn, swap sang EPPlus: new ExcelPackage(...)
        var sb = new StringBuilder();
        sb.AppendLine("CINEMA MANAGEMENT - REVENUE REPORT");
        sb.AppendLine($"Period:,{vm.From:dd/MM/yyyy} - {vm.To:dd/MM/yyyy}");
        sb.AppendLine();

        // KPI Summary
        sb.AppendLine("=== SUMMARY ===");
        sb.AppendLine($"Total Revenue (VND):,{vm.TotalRevenue:N0}");
        sb.AppendLine($"Total Bookings:,{vm.TotalBookings}");
        sb.AppendLine($"Tickets Sold:,{vm.TotalTicketsSold}");
        sb.AppendLine($"Cancelled/Expired:,{vm.TotalCancelled}");
        sb.AppendLine($"Cancellation Rate:,{(vm.TotalBookings > 0 ? (double)vm.TotalCancelled / vm.TotalBookings * 100 : 0):F1}%");
        sb.AppendLine();

        // Revenue by Day
        sb.AppendLine("=== REVENUE BY DAY ===");
        sb.AppendLine("Date,Revenue (VND)");
        foreach (var day in vm.RevenueByDay)
            sb.AppendLine($"{day.Date:dd/MM/yyyy},{day.Revenue:N0}");
        sb.AppendLine();

        // Top Movies
        sb.AppendLine("=== TOP MOVIES ===");
        sb.AppendLine("Rank,Movie Title,Tickets Sold,Revenue (VND)");
        for (int i = 0; i < vm.TopMovies.Count; i++)
        {
            var m = vm.TopMovies[i];
            sb.AppendLine($"{i + 1},\"{m.Title}\",{m.TicketsSold},{m.Revenue:N0}");
        }

        // UTF-8 BOM để Excel mở không bị lỗi tiếng Việt
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    /// <summary>
    /// Export dashboard sang HTML-based PDF (in từ browser).
    /// Production: dùng iText7 hoặc Playwright để render PDF thật.
    /// </summary>
    public byte[] ExportDashboardToPdf(DashboardVm vm)
    {
        var cancellationRate = vm.TotalBookings > 0
            ? (double)vm.TotalCancelled / vm.TotalBookings * 100 : 0;

        var html = new StringBuilder();
        html.Append($@"<!DOCTYPE html>
<html lang='vi'>
<head>
<meta charset='UTF-8'>
<style>
  body {{ font-family: Arial, sans-serif; margin: 30px; color: #333; }}
  h1 {{ color: #e50914; border-bottom: 2px solid #e50914; padding-bottom: 8px; }}
  h2 {{ color: #555; margin-top: 24px; }}
  .kpi-grid {{ display: grid; grid-template-columns: repeat(4,1fr); gap:12px; margin:16px 0; }}
  .kpi {{ background:#f8f8f8; border-left:4px solid #e50914; padding:12px 16px; border-radius:4px; }}
  .kpi .val {{ font-size:22px; font-weight:bold; color:#e50914; }}
  .kpi .lbl {{ font-size:12px; color:#888; margin-top:4px; }}
  table {{ width:100%; border-collapse:collapse; margin-top:8px; }}
  th {{ background:#e50914; color:#fff; padding:8px; text-align:left; font-size:13px; }}
  td {{ padding:7px 8px; border-bottom:1px solid #eee; font-size:13px; }}
  tr:nth-child(even) td {{ background:#fafafa; }}
  .footer {{ margin-top:32px; font-size:11px; color:#aaa; text-align:right; }}
</style>
</head>
<body>
<h1>🎬 Cinema Management — Revenue Report</h1>
<p><strong>Period:</strong> {vm.From:dd/MM/yyyy} — {vm.To:dd/MM/yyyy} &nbsp;|&nbsp; <strong>Generated:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>

<h2>📊 Summary</h2>
<div class='kpi-grid'>
  <div class='kpi'><div class='val'>{vm.TotalRevenue:N0}</div><div class='lbl'>Total Revenue (VND)</div></div>
  <div class='kpi'><div class='val'>{vm.TotalBookings:N0}</div><div class='lbl'>Total Bookings</div></div>
  <div class='kpi'><div class='val'>{vm.TotalTicketsSold:N0}</div><div class='lbl'>Tickets Sold</div></div>
  <div class='kpi'><div class='val'>{cancellationRate:F1}%</div><div class='lbl'>Cancellation Rate</div></div>
</div>

<h2>📅 Revenue by Day</h2>
<table>
  <tr><th>Date</th><th>Revenue (VND)</th></tr>");

        foreach (var d in vm.RevenueByDay)
            html.Append($"<tr><td>{d.Date:dd/MM/yyyy}</td><td>{d.Revenue:N0}</td></tr>");

        html.Append($@"</table>

<h2>🏆 Top Movies by Revenue</h2>
<table>
  <tr><th>#</th><th>Movie Title</th><th>Tickets Sold</th><th>Revenue (VND)</th></tr>");

        for (int i = 0; i < vm.TopMovies.Count; i++)
        {
            var m = vm.TopMovies[i];
            html.Append($"<tr><td>{i + 1}</td><td>{System.Web.HttpUtility.HtmlEncode(m.Title)}</td><td>{m.TicketsSold}</td><td>{m.Revenue:N0}</td></tr>");
        }

        html.Append($@"</table>
<div class='footer'>Cinema Management System &copy; {DateTime.Now.Year}</div>
</body></html>");

        return Encoding.UTF8.GetBytes(html.ToString());
    }
}