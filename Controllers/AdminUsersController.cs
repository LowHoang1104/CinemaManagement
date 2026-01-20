using CinemaManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaManagement.Controllers;
public class AdminUsersController : Controller
{
    private readonly IAdminUserService _service;

    public AdminUsersController(IAdminUserService service) => _service = service;

    // GET: /AdminUsers
    public async Task<IActionResult> Index(string? keyword, string? role, int? status)
    {
        var vm = await _service.SearchUsersAsync(keyword, role, status);
        return View(vm);
    }

    // POST: /AdminUsers/ToggleLock/{id}
    [HttpPost]
    public async Task<IActionResult> ToggleLock(Guid id)
    {
        await _service.ToggleLockAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // GET: /AdminUsers/AssignRoles/{id}
    public async Task<IActionResult> AssignRoles(Guid id)
    {
        var vm = await _service.GetAssignRolesAsync(id);
        return View(vm);
    }

    // POST: /AdminUsers/AssignRoles
    [HttpPost]
    public async Task<IActionResult> AssignRoles(Guid userId, List<Guid> selectedRoleIds)
    {
        await _service.UpdateUserRolesAsync(userId, selectedRoleIds ?? new List<Guid>());
        return RedirectToAction(nameof(Index));
    }
}

