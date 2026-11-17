using Microsoft.EntityFrameworkCore;
using DevExpressBlazorApp.Data.Entities;

namespace DevExpressBlazorApp.Data;

public class ErpDbContext : DbContext
{
    public ErpDbContext(DbContextOptions<ErpDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppRole> AppRoles => Set<AppRole>();
    public DbSet<AppCompany> AppCompanies => Set<AppCompany>();
    public DbSet<AppLocation> AppLocations => Set<AppLocation>();
    public DbSet<AppPage> AppPages => Set<AppPage>();
    public DbSet<AppRoleRight> AppRoleRights => Set<AppRoleRight>();
    public DbSet<AppUserPreference> AppUserPreferences => Set<AppUserPreference>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<CustomElementMetadata> CustomElementMetadata => Set<CustomElementMetadata>();
    public DbSet<CustomElementValue> CustomElementValues => Set<CustomElementValue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AppUser
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email);
            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.DefaultCompany)
                  .WithMany()
                  .HasForeignKey(u => u.DefaultCompanyId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(u => u.DefaultLocation)
                  .WithMany()
                  .HasForeignKey(u => u.DefaultLocationId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(u => u.Preference)
                  .WithOne(p => p.User)
                  .HasForeignKey<AppUserPreference>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // AppCompany
        modelBuilder.Entity<AppCompany>(entity =>
        {
            entity.HasIndex(c => c.Code).IsUnique();
        });

        // AppLocation
        modelBuilder.Entity<AppLocation>(entity =>
        {
            entity.HasOne(l => l.Company)
                  .WithMany(c => c.Locations)
                  .HasForeignKey(l => l.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // AppPage Hierarchy
        modelBuilder.Entity<AppPage>(entity =>
        {
            entity.HasOne(p => p.Parent)
                  .WithMany(p => p.ChildPages)
                  .HasForeignKey(p => p.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // AppRoleRight
        modelBuilder.Entity<AppRoleRight>(entity =>
        {
            entity.HasIndex(rr => new { rr.RoleId, rr.PageId }).IsUnique();

            entity.HasOne(rr => rr.Role)
                  .WithMany(r => r.RoleRights)
                  .HasForeignKey(rr => rr.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rr => rr.Page)
                  .WithMany(p => p.RoleRights)
                  .HasForeignKey(rr => rr.PageId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // CustomElementMetadata
        modelBuilder.Entity<CustomElementMetadata>(entity =>
        {
            entity.HasIndex(c => new { c.EntityName, c.FieldName }).IsUnique();
        });

        // CustomElementValue
        modelBuilder.Entity<CustomElementValue>(entity =>
        {
            entity.HasIndex(v => new { v.EntityName, v.RecordKey });

            entity.HasOne(v => v.Metadata)
                  .WithMany(m => m.Values)
                  .HasForeignKey(v => v.CustomElementMetadataId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

