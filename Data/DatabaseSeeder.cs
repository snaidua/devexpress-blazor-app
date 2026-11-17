using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DevExpressBlazorApp.Data.Entities;

namespace DevExpressBlazorApp.Data;

public class DatabaseSeeder
{
    private readonly IDbContextFactory<ErpDbContext> _contextFactory;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        IDbContextFactory<ErpDbContext> contextFactory,
        IPasswordHasher<AppUser> passwordHasher,
        ILogger<DatabaseSeeder> logger)
    {
        _contextFactory = contextFactory;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            _logger.LogInformation("Ensuring database schema is created...");
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Companies
            if (!await context.AppCompanies.AnyAsync())
            {
                _logger.LogInformation("Seeding default ERP Companies...");
                var comp1 = new AppCompany
                {
                    Code = "CORP-01",
                    Name = "Apex Global Enterprises Inc.",
                    TaxId = "US-987654321",
                    Currency = "USD",
                    IsActive = true
                };
                var comp2 = new AppCompany
                {
                    Code = "REG-02",
                    Name = "North America Logistics Corp.",
                    TaxId = "US-123456789",
                    Currency = "USD",
                    IsActive = true
                };
                context.AppCompanies.AddRange(comp1, comp2);
                await context.SaveChangesAsync();

                // 2. Seed Locations
                var loc1 = new AppLocation
                {
                    CompanyId = comp1.Id,
                    Code = "LOC-HQ",
                    Name = "Corporate Headquarters (Campus A)",
                    Address = "100 Enterprise Blvd, Suite 800, New York, NY",
                    IsActive = true
                };
                var loc2 = new AppLocation
                {
                    CompanyId = comp1.Id,
                    Code = "LOC-PLANT1",
                    Name = "Manufacturing Facility 01",
                    Address = "450 Industrial Parkway, Detroit, MI",
                    IsActive = true
                };
                var loc3 = new AppLocation
                {
                    CompanyId = comp2.Id,
                    Code = "LOC-DC1",
                    Name = "Central Distribution Hub",
                    Address = "88 Freight Way, Chicago, IL",
                    IsActive = true
                };
                context.AppLocations.AddRange(loc1, loc2, loc3);
                await context.SaveChangesAsync();
            }

            // 3. Seed Roles
            if (!await context.AppRoles.AnyAsync())
            {
                _logger.LogInformation("Seeding default ERP Roles...");
                var adminRole = new AppRole
                {
                    Name = "Administrator",
                    Description = "Full Enterprise administrative controls and security governance",
                    IsSystem = true
                };
                var execRole = new AppRole
                {
                    Name = "Executive",
                    Description = "High-level analytics, financial reporting, and KPI exports",
                    IsSystem = true
                };
                var opsRole = new AppRole
                {
                    Name = "Operations Manager",
                    Description = "Day-to-day warehouse, inventory, and order fulfillment workflows",
                    IsSystem = false
                };
                var viewRole = new AppRole
                {
                    Name = "Auditor / Viewer",
                    Description = "Read-only audit access across standard operational screens",
                    IsSystem = false
                };

                context.AppRoles.AddRange(adminRole, execRole, opsRole, viewRole);
                await context.SaveChangesAsync();
            }

            // 4. Seed Users
            if (!await context.AppUsers.AnyAsync())
            {
                _logger.LogInformation("Seeding initial ERP Users...");
                var adminRole = await context.AppRoles.FirstAsync(r => r.Name == "Administrator");
                var execRole = await context.AppRoles.FirstAsync(r => r.Name == "Executive");
                var defaultCompany = await context.AppCompanies.FirstAsync();
                var defaultLocation = await context.AppLocations.FirstAsync();

                var adminUser = new AppUser
                {
                    Username = "admin",
                    FullName = "Chief Systems Administrator",
                    Email = "admin@apex-erp.internal",
                    RoleId = adminRole.Id,
                    DefaultCompanyId = defaultCompany.Id,
                    DefaultLocationId = defaultLocation.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, "Admin@123");

                var execUser = new AppUser
                {
                    Username = "david.vance",
                    FullName = "David Vance (COO)",
                    Email = "david.vance@apex-erp.internal",
                    RoleId = execRole.Id,
                    DefaultCompanyId = defaultCompany.Id,
                    DefaultLocationId = defaultLocation.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                execUser.PasswordHash = _passwordHasher.HashPassword(execUser, "Executive@123");

                context.AppUsers.AddRange(adminUser, execUser);
                await context.SaveChangesAsync();

                // Seed Preferences
                context.AppUserPreferences.AddRange(
                    new AppUserPreference
                    {
                        UserId = adminUser.Id,
                        ThemeName = "blazing-berry",
                        CompactMode = false,
                        SidebarCollapsed = false,
                        Language = "en-US",
                        UpdatedAt = DateTime.UtcNow
                    },
                    new AppUserPreference
                    {
                        UserId = execUser.Id,
                        ThemeName = "blazing-dark",
                        CompactMode = false,
                        SidebarCollapsed = false,
                        Language = "en-US",
                        UpdatedAt = DateTime.UtcNow
                    }
                );
                await context.SaveChangesAsync();
            }

            // 5. Seed Pages & Navigation
            if (!await context.AppPages.AnyAsync())
            {
                _logger.LogInformation("Seeding Navigation hierarchy and Pages...");
                var dashPage = new AppPage
                {
                    Title = "Executive Dashboard",
                    Route = "/",
                    Icon = "chart-line",
                    BadgeText = "LIVE",
                    BadgeColor = "badge bg-success",
                    SortOrder = 1,
                    IsActive = true
                };

                var opsParent = new AppPage
                {
                    Title = "Enterprise Operations",
                    Route = "#",
                    Icon = "layers",
                    SortOrder = 2,
                    IsActive = true
                };

                var adminParent = new AppPage
                {
                    Title = "Administration & Governance",
                    Route = "#",
                    Icon = "gear",
                    SortOrder = 3,
                    IsActive = true
                };

                context.AppPages.AddRange(dashPage, opsParent, adminParent);
                await context.SaveChangesAsync();

                // Operations Children
                var invPage = new AppPage
                {
                    ParentId = opsParent.Id,
                    Title = "Inventory & Stock",
                    Route = "/ops/inventory",
                    Icon = "box",
                    BadgeText = "Alert",
                    BadgeColor = "badge bg-warning text-dark",
                    BadgeDapperQuery = "INVENTORY_ALERT",
                    SortOrder = 1,
                    IsActive = true
                };

                var salesPage = new AppPage
                {
                    ParentId = opsParent.Id,
                    Title = "Sales & Invoicing",
                    Route = "/ops/sales",
                    Icon = "receipt",
                    BadgeText = "14 New",
                    BadgeColor = "badge bg-info text-dark",
                    BadgeDapperQuery = "PENDING_ORDERS",
                    SortOrder = 2,
                    IsActive = true
                };

                // Admin Children
                var usersPage = new AppPage
                {
                    ParentId = adminParent.Id,
                    Title = "Users & Directory",
                    Route = "/admin/users",
                    Icon = "people",
                    SortOrder = 1,
                    IsActive = true
                };

                var pagesPage = new AppPage
                {
                    ParentId = adminParent.Id,
                    Title = "Pages & Navigation",
                    Route = "/admin/pages",
                    Icon = "list-nested",
                    SortOrder = 2,
                    IsActive = true
                };

                var rightsPage = new AppPage
                {
                    ParentId = adminParent.Id,
                    Title = "Roles & Rights Matrix",
                    Route = "/admin/roles-rights",
                    Icon = "shield-check",
                    SortOrder = 3,
                    IsActive = true
                };

                var themesPage = new AppPage
                {
                    ParentId = adminParent.Id,
                    Title = "Themes & Appearance",
                    Route = "/admin/themes",
                    Icon = "palette",
                    SortOrder = 4,
                    IsActive = true
                };

                var customElementsPage = new AppPage
                {
                    ParentId = adminParent.Id,
                    Title = "Customizable Elements",
                    Route = "/admin/custom-elements",
                    Icon = "input-cursor-text",
                    SortOrder = 5,
                    IsActive = true
                };

                context.AppPages.AddRange(invPage, salesPage, usersPage, pagesPage, rightsPage, themesPage, customElementsPage);
                await context.SaveChangesAsync();

                // 6. Seed Role Rights for Administrator
                var adminRole = await context.AppRoles.FirstAsync(r => r.Name == "Administrator");
                var allPages = await context.AppPages.ToListAsync();

                foreach (var page in allPages)
                {
                    context.AppRoleRights.Add(new AppRoleRight
                    {
                        RoleId = adminRole.Id,
                        PageId = page.Id,
                        CanView = true,
                        CanCreate = true,
                        CanEdit = true,
                        CanDelete = true,
                        CanExport = true
                    });
                }
                await context.SaveChangesAsync();
            }

            // 7. Seed Custom Element Metadata
            if (!await context.CustomElementMetadata.AnyAsync())
            {
                _logger.LogInformation("Seeding Custom Element Metadata definitions...");
                context.CustomElementMetadata.AddRange(
                    new CustomElementMetadata
                    {
                        EntityName = "Customer",
                        FieldName = "CreditTier",
                        DisplayLabel = "Credit Rating Tier",
                        FieldType = "Dropdown",
                        DropdownOptions = "AAA (Prime Tier),AA (High Grade),A (Upper Medium),BBB (Investment Standard)",
                        DefaultValue = "AA (High Grade)",
                        IsRequired = true,
                        SortOrder = 1,
                        IsActive = true
                    },
                    new CustomElementMetadata
                    {
                        EntityName = "Customer",
                        FieldName = "DedicatedRep",
                        DisplayLabel = "Dedicated Account Executive",
                        FieldType = "Text",
                        DefaultValue = "Enterprise Desk",
                        IsRequired = false,
                        SortOrder = 2,
                        IsActive = true
                    },
                    new CustomElementMetadata
                    {
                        EntityName = "Customer",
                        FieldName = "CreditLimitUsd",
                        DisplayLabel = "Authorized Credit Limit ($)",
                        FieldType = "Number",
                        DefaultValue = "250000",
                        IsRequired = true,
                        SortOrder = 3,
                        IsActive = true
                    },
                    new CustomElementMetadata
                    {
                        EntityName = "Customer",
                        FieldName = "TaxExempt",
                        DisplayLabel = "Tax Exempt Certificate on File",
                        FieldType = "Boolean",
                        DefaultValue = "False",
                        IsRequired = false,
                        SortOrder = 4,
                        IsActive = true
                    },
                    new CustomElementMetadata
                    {
                        EntityName = "SalesOrder",
                        FieldName = "ExpeditedShipping",
                        DisplayLabel = "Expedited Air Freight Required",
                        FieldType = "Boolean",
                        DefaultValue = "False",
                        IsRequired = false,
                        SortOrder = 1,
                        IsActive = true
                    },
                    new CustomElementMetadata
                    {
                        EntityName = "SalesOrder",
                        FieldName = "QualityInspectionLevel",
                        DisplayLabel = "QA Verification Protocol",
                        FieldType = "Dropdown",
                        DropdownOptions = "Standard QA,ISO-9001 Batch Testing,Military Spec MIL-STD-105E",
                        DefaultValue = "Standard QA",
                        IsRequired = true,
                        SortOrder = 2,
                        IsActive = true
                    }
                );
                await context.SaveChangesAsync();
            }

            _logger.LogInformation("ERP Database Seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during ERP Database Seeding.");
        }
    }
}

