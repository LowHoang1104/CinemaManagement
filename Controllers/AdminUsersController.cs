using CinemaManagement.Services;
using CinemaManagement.ViewModels.Admin;
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

    // GET: /AdminUsers/Create
    public async Task<IActionResult> Create()
    {
        var vm = await _service.GetCreateFormAsync();
        return View(vm);
    }

    // POST: /AdminUsers/Create
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserVm vm)
    {
        if (!ModelState.IsValid)
        {
            var form = await _service.GetCreateFormAsync();
            vm.AllRoles = form.AllRoles;
            return View(vm);
        }

        try
        {
            await _service.CreateUserAsync(vm);
            TempData["Success"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Email", ex.Message);
            var form = await _service.GetCreateFormAsync();
            vm.AllRoles = form.AllRoles;
            return View(vm);
        }
    }

    // GET: /AdminUsers/Edit/{id}
    public async Task<IActionResult> Edit(Guid id)
    {
        var vm = await _service.GetEditAsync(id);
        return View(vm);
    }

    // POST: /AdminUsers/Edit
    [HttpPost]
    public async Task<IActionResult> Edit(EditUserVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        await _service.UpdateUserAsync(vm);
        TempData["Success"] = "User updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /AdminUsers/Delete/{id}
    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteUserAsync(id);
        TempData["Success"] = "User deleted.";
        return RedirectToAction(nameof(Index));
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
        TempData["Success"] = "Roles updated.";
        return RedirectToAction(nameof(Index));
    }
}