using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Services;

public interface IAdminUserService
{
    Task<UserListVm> SearchUsersAsync(string? keyword, string? role, int? status);
    Task<CreateUserVm> GetCreateFormAsync();
    Task CreateUserAsync(CreateUserVm vm);
    Task<EditUserVm> GetEditAsync(Guid userId);
    Task UpdateUserAsync(EditUserVm vm);
    Task<AssignRolesVm> GetAssignRolesAsync(Guid userId);
    Task UpdateUserRolesAsync(Guid userId, List<Guid> selectedRoleIds);
    Task ToggleLockAsync(Guid userId);
    Task DeleteUserAsync(Guid userId);
}

public class AdminUserService : IAdminUserService
{
    private readonly CinemaManagementContext _db;

    public AdminUserService(CinemaManagementContext db) => _db = db;

    public async Task<UserListVm> SearchUsersAsync(string? keyword, string? role, int? status)
    {
        var roles = await _db.Roles.AsNoTracking()
            .Select(r => r.Name).OrderBy(x => x).ToListAsync();

        var query = _db.Users.AsNoTracking().Include(u => u.Roles).AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim().ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(keyword) ||
                u.FullName.ToLower().Contains(keyword));
        }

        if (status.HasValue)
            query = query.Where(u => u.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Roles.Any(r => r.Name == role));

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserListItemVm
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Status = u.Status,
                Roles = u.Roles.Select(r => r.Name).OrderBy(x => x).ToList()
            })
            .ToListAsync();

        return new UserListVm
        {
            Keyword = keyword,
            Role = role,
            Status = status,
            Items = items,
            AllRoles = roles
        };
    }

    // ── CREATE ────────────────────────────────────────────────────────────────
    public async Task<CreateUserVm> GetCreateFormAsync()
    {
        var allRoles = await _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
        return new CreateUserVm
        {
            AllRoles = allRoles.Select(r => new RoleOptionVm
            {
                RoleId = r.RoleId,
                Name = r.Name
            }).ToList()
        };
    }

    public async Task CreateUserAsync(CreateUserVm vm)
    {
        // Kiểm tra email trùng
        if (await _db.Users.AnyAsync(u => u.Email == vm.Email))
            throw new InvalidOperationException("Email already exists.");

        var roles = await _db.Roles
            .Where(r => vm.SelectedRoleIds.Contains(r.RoleId))
            .ToListAsync();

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = vm.Email,
            // Production: dùng BCrypt.HashPassword(vm.Password)
            PasswordHash = BCryptHash(vm.Password),
            FullName = vm.FullName,
            Phone = vm.Phone,
            Status = 1,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var r in roles)
            user.Roles.Add(r);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    // ── EDIT ──────────────────────────────────────────────────────────────────
    public async Task<EditUserVm> GetEditAsync(Guid userId)
    {
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId)
            ?? throw new InvalidOperationException("User not found.");

        return new EditUserVm
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Status = user.Status
        };
    }

    public async Task UpdateUserAsync(EditUserVm vm)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == vm.UserId)
            ?? throw new InvalidOperationException("User not found.");

        user.FullName = vm.FullName;
        user.Phone = vm.Phone;
        user.Status = vm.Status;
        user.LastUpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    // ── TOGGLE LOCK ───────────────────────────────────────────────────────────
    public async Task ToggleLockAsync(Guid userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId)
            ?? throw new InvalidOperationException("User not found.");

        user.Status = user.Status == 1 ? 0 : 1;
        user.LastUpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── DELETE (soft) ─────────────────────────────────────────────────────────
    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId)
            ?? throw new InvalidOperationException("User not found.");

        // Soft delete: set Status = -1 thay vì xóa cứng
        user.Status = -1;
        user.LastUpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── ASSIGN ROLES ──────────────────────────────────────────────────────────
    public async Task<AssignRolesVm> GetAssignRolesAsync(Guid userId)
    {
        var user = await _db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == userId)
            ?? throw new InvalidOperationException("User not found.");

        var allRoles = await _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
        var userRoleIds = user.Roles.Select(r => r.RoleId).ToHashSet();

        return new AssignRolesVm
        {
            UserId = user.UserId,
            Email = user.Email,
            Roles = allRoles.Select(r => new RoleOptionVm
            {
                RoleId = r.RoleId,
                Name = r.Name,
                Selected = userRoleIds.Contains(r.RoleId)
            }).ToList()
        };
    }

    public async Task UpdateUserRolesAsync(Guid userId, List<Guid> selectedRoleIds)
    {
        var user = await _db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == userId)
            ?? throw new InvalidOperationException("User not found.");

        var roles = await _db.Roles
            .Where(r => selectedRoleIds.Contains(r.RoleId))
            .ToListAsync();

        user.Roles.Clear();
        foreach (var r in roles)
            user.Roles.Add(r);

        user.LastUpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    // Production: cài BCrypt.Net-Next NuGet rồi dùng BCrypt.Net.BCrypt.HashPassword()
    private static string BCryptHash(string password)
        => $"$2a$11$DEMO_HASH_{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password))}";
}