using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.ViewModels.Admin;
public class UserListItemVm
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? Phone { get; set; }
    public int Status { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class UserListVm
{
    public string? Keyword { get; set; }
    public string? Role { get; set; }
    public int? Status { get; set; }
    public List<UserListItemVm> Items { get; set; } = new();
    public List<string> AllRoles { get; set; } = new();
}

public class CreateUserVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(6)]
    public string Password { get; set; } = "";

    [Required]
    public string FullName { get; set; } = "";

    public string? Phone { get; set; }

    public List<Guid> SelectedRoleIds { get; set; } = new();
    public List<RoleOptionVm> AllRoles { get; set; } = new();
}

public class EditUserVm
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = "";

    [Required]
    public string FullName { get; set; } = "";

    public string? Phone { get; set; }
    public int Status { get; set; }
}

public class AssignRolesVm
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = "";
    public List<RoleOptionVm> Roles { get; set; } = new();
}

public class RoleOptionVm
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = "";
    public bool Selected { get; set; }
}