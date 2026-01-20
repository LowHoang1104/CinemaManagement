using CinemaManagement.Data;
using CinemaManagement.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Services;

public interface IAdminUserService
{
    Task<UserListVm> SearchUsersAsync(string? keyword, string? role, int? status);
    Task<AssignRolesVm> GetAssignRolesAsync(Guid userId);
    Task UpdateUserRolesAsync(Guid userId, List<Guid> selectedRoleIds);
    Task ToggleLockAsync(Guid userId);
}

public class AdminUserService : IAdminUserService
{
    private readonly CinemaManagementContext _db;

    public AdminUserService(CinemaManagementContext db) => _db = db;

    public async Task<UserListVm> SearchUsersAsync(string? keyword, string? role, int? status)
    {
        var roles = await _db.Roles.AsNoTracking()
            .Select(r => r.Name)
            .OrderBy(x => x)
            .ToListAsync();

        var query = _db.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .AsQueryable();

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

    public async Task ToggleLockAsync(Guid userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.UserId == userId)
            ?? throw new InvalidOperationException("User not found");

        user.Status = user.Status == 1 ? 0 : 1;
        user.LastUpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<AssignRolesVm> GetAssignRolesAsync(Guid userId)
    {
        var user = await _db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == userId)
            ?? throw new InvalidOperationException("User not found");

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
            ?? throw new InvalidOperationException("User not found");

        var roles = await _db.Roles
            .Where(r => selectedRoleIds.Contains(r.RoleId))
            .ToListAsync();

        // reset roles
        user.Roles.Clear();
        foreach (var r in roles)
            user.Roles.Add(r);

        user.LastUpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}
