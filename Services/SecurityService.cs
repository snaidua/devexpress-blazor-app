using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DevExpressBlazorApp.Data;
using DevExpressBlazorApp.Data.Entities;

namespace DevExpressBlazorApp.Services;

public enum PermissionType
{
    View,
    Create,
    Edit,
    Delete,
    Export
}

public class SecurityService
{
    private readonly IDbContextFactory<ErpDbContext> _contextFactory;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly ILogger<SecurityService> _logger;

    public SecurityService(
        IDbContextFactory<ErpDbContext> contextFactory,
        IPasswordHasher<AppUser> passwordHasher,
        ILogger<SecurityService> logger)
    {
        _contextFactory = contextFactory;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public string HashPassword(AppUser user, string plainPassword)
    {
        return _passwordHasher.HashPassword(user, plainPassword);
    }

    public bool VerifyPassword(AppUser user, string plainPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, plainPassword);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }

    public async Task<bool> HasPermissionAsync(int roleId, string route, PermissionType permission)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var right = await context.AppRoleRights
                .Include(r => r.Page)
                .FirstOrDefaultAsync(r => r.RoleId == roleId && r.Page != null && r.Page.Route == route);

            if (right == null)
            {
                // Fallback: If Administrator role, grant view
                var role = await context.AppRoles.FindAsync(roleId);
                return role?.Name == "Administrator";
            }

            return permission switch
            {
                PermissionType.View => right.CanView,
                PermissionType.Create => right.CanCreate,
                PermissionType.Edit => right.CanEdit,
                PermissionType.Delete => right.CanDelete,
                PermissionType.Export => right.CanExport,
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for RoleId {RoleId} on Route {Route}", roleId, route);
            return false;
        }
    }

    public async Task<List<AppPage>> GetAuthorizedPagesAsync(int roleId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Fetch all active pages
            var allPages = await context.AppPages
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .ToListAsync();

            // Fetch rights for this role
            var roleRights = await context.AppRoleRights
                .Where(r => r.RoleId == roleId && r.CanView)
                .ToDictionaryAsync(r => r.PageId);

            // Filter pages: Parent page is included if it's route="#" or if any child is authorized
            var authorizedPages = allPages.Where(p =>
                roleRights.ContainsKey(p.Id) ||
                (p.ParentId == null && p.Route == "#")
            ).ToList();

            return authorizedPages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching authorized navigation pages for RoleId {RoleId}", roleId);
            return new List<AppPage>();
        }
    }

    public async Task<List<AppRoleRight>> GetRoleRightsMatrixAsync(int roleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var pages = await context.AppPages.Where(p => p.IsActive).OrderBy(p => p.SortOrder).ToListAsync();
        var existingRights = await context.AppRoleRights
            .Where(r => r.RoleId == roleId)
            .Include(r => r.Page)
            .ToListAsync();

        var matrix = new List<AppRoleRight>();

        foreach (var page in pages)
        {
            var match = existingRights.FirstOrDefault(r => r.PageId == page.Id);
            if (match != null)
            {
                matrix.Add(match);
            }
            else
            {
                matrix.Add(new AppRoleRight
                {
                    RoleId = roleId,
                    PageId = page.Id,
                    Page = page,
                    CanView = false,
                    CanCreate = false,
                    CanEdit = false,
                    CanDelete = false,
                    CanExport = false
                });
            }
        }

        return matrix;
    }

    public async Task SaveRoleRightsMatrixAsync(int roleId, IEnumerable<AppRoleRight> rights)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        foreach (var item in rights)
        {
            var existing = await context.AppRoleRights
                .FirstOrDefaultAsync(r => r.RoleId == roleId && r.PageId == item.PageId);

            if (existing != null)
            {
                existing.CanView = item.CanView;
                existing.CanCreate = item.CanCreate;
                existing.CanEdit = item.CanEdit;
                existing.CanDelete = item.CanDelete;
                existing.CanExport = item.CanExport;
            }
            else
            {
                context.AppRoleRights.Add(new AppRoleRight
                {
                    RoleId = roleId,
                    PageId = item.PageId,
                    CanView = item.CanView,
                    CanCreate = item.CanCreate,
                    CanEdit = item.CanEdit,
                    CanDelete = item.CanDelete,
                    CanExport = item.CanExport
                });
            }
        }

        await context.SaveChangesAsync();
    }
}

