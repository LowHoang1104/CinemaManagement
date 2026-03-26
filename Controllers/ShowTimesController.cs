using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using CinemaManagement.Services; // Added for IShowTimeService

namespace CinemaManagement.Controllers;

public class ShowTimesController : Controller
{
    private readonly IShowTimeService _showTimeService;

    public ShowTimesController(IShowTimeService showTimeService)
    {
        _showTimeService = showTimeService;
    }

    // --- View Actions ---

    public async Task<IActionResult> Index(
        string? search,
        DateTime? date,
        Guid? cinemaId,
        int? status,
        int? displayStatus,
        int page = 1,
        int pageSize = 10)
    {
        var viewModel = await _showTimeService.GetShowTimeListAsync(search, date, cinemaId, status, displayStatus, page, pageSize);
        
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_ShowTimeListPartial", viewModel);

        return View(viewModel);
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();
        var viewModel = await _showTimeService.GetDetailsAsync(id.Value);
        if (viewModel == null) return NotFound();
        return PartialView("_DetailsModalPartial", viewModel);
    }

    public async Task<IActionResult> Edit(Guid? id, bool viewOnly = false)
    {
        if (id == null) return NotFound();
        var viewModel = await _showTimeService.GetEditViewModelAsync(id.Value);
        if (viewModel == null) return NotFound();

        ViewData["ViewOnly"] = viewOnly;
        return PartialView("_EditModalPartial", viewModel);
    }

    // --- API / Post Actions ---

    [HttpGet]
    public async Task<IActionResult> CheckOverlap(Guid roomId, DateTime startAt, Guid movieId, string? excludeId = null)
    {
        var result = await _showTimeService.CheckOverlapAsync(roomId, startAt, movieId, excludeId);
        return Json(new { isOverlapping = result.IsOverlapping, conflicts = result.Conflicts, isPast = result.IsPast });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "CreateForm")] ShowTimeCreateViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await _showTimeService.CreateAsync(model);
                TempData["SuccessMessage"] = "Suất chiếu đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "Đã xảy ra lỗi hệ thống khi lưu suất chiếu.");
        }

        var viewModel = await _showTimeService.GetShowTimeListAsync(null, null, null, null, null, 1, 10);
        viewModel.CreateForm = model;
        return View("Index", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ShowTimeEditViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await _showTimeService.EditAsync(id, model);
                TempData["SuccessMessage"] = "Suất chiếu đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "Lỗi hệ thống khi lưu.");
        }

        return PartialView("_EditModalPartial", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            await _showTimeService.CancelAsync(id);
            return Json(new { success = true, message = "Suất chiếu đã được hủy thành công." });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try
        {
            await _showTimeService.DeleteAsync(id);
            TempData["Success"] = "Đã xóa lịch chiếu thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
