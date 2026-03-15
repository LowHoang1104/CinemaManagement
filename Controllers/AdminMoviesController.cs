using CinemaManagement.Services;
using CinemaManagement.ViewModels.AdminMovies;
using Microsoft.AspNetCore.Mvc;

namespace CinemaManagement.Controllers;

public class AdminMoviesController : Controller
{
    private readonly IAdminMovieTcpService _adminMovieTcpService;

    public AdminMoviesController(IAdminMovieTcpService adminMovieTcpService)
    {
        _adminMovieTcpService = adminMovieTcpService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _adminMovieTcpService.GetAllAsync(cancellationToken);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }

        return View(result.Data ?? Array.Empty<AdminMovieViewModel>());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new AdminMovieViewModel
        {
            ReleaseDate = DateTime.Today,
            Status = 1
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminMovieViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _adminMovieTcpService.CreateAsync(model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["Success"] = "Tạo phim thành công qua TCP service.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return BadRequest();
        }

        var result = await _adminMovieTcpService.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AdminMovieViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.MovieId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _adminMovieTcpService.UpdateAsync(id, model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["Success"] = "Cập nhật phim thành công qua TCP service.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminMovieTcpService.DeleteAsync(id, cancellationToken);
        if (result.Success)
        {
            TempData["Success"] = "Xóa phim thành công qua TCP service.";
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
