using Microsoft.EntityFrameworkCore;
using Crm.Domain.Entities;
using Crm.Application.Interfaces;

namespace Crm.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserContext _userContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserContext userContext)
        : base(options)
    {
        _userContext = userContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<CrmTask> Tasks => Set<CrmTask>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<AdMetric> AdMetrics => Set<AdMetric>();
    public DbSet<ProjectAdAccount> ProjectAdAccounts => Set<ProjectAdAccount>();
    public DbSet<TaskTemplate> TaskTemplates => Set<TaskTemplate>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Global Query Filter for Multi-Tenancy
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantedEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext).GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { modelBuilder });
            }
        }

        // Configure relationships if needed (minimal for now)
        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId);

        modelBuilder.Entity<Client>()
            .HasMany(c => c.Contacts)
            .WithOne(con => con.Client)
            .HasForeignKey(con => con.ClientId);

        modelBuilder.Entity<Lead>()
            .HasOne(l => l.ConvertedClient)
            .WithMany()
            .HasForeignKey(l => l.ConvertedClientId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Explicitly configure the RefreshToken → User relationship.
        // This ensures EF Core auto-populates the UserId FK when a RefreshToken
        // is added to user.RefreshTokens, preventing DbUpdateConcurrencyException.
        modelBuilder.Entity<User>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);

        // Time Tracking & Team Relationships
        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.User)
            .WithMany(u => u.ProjectMemberships)
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TimeEntry>()
            .HasOne(te => te.Project)
            .WithMany(p => p.TimeEntries)
            .HasForeignKey(te => te.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TimeEntry>()
            .HasOne(te => te.Task)
            .WithMany(t => t.TimeEntries)
            .HasForeignKey(te => te.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TimeEntry>()
            .HasOne(te => te.User)
            .WithMany(u => u.TimeEntries)
            .HasForeignKey(te => te.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ad Account Relationships
        modelBuilder.Entity<ProjectAdAccount>()
            .HasOne(pa => pa.Project)
            .WithMany(p => p.AdAccounts)
            .HasForeignKey(pa => pa.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AdMetric>()
            .HasOne(am => am.AdAccount)
            .WithMany(pa => pa.Metrics)
            .HasForeignKey(am => am.AdAccountId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private void ApplyTenantFilter<T>(ModelBuilder modelBuilder) where T : class, ITenantedEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => 
            !_userContext.IsAuthenticated || 
            (_userContext.TenantId.HasValue && e.TenantId == _userContext.TenantId.Value));
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ITenantedEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.TenantId == Guid.Empty)
                {
                    var tenantId = _userContext.TenantId;
                    if (tenantId.HasValue && tenantId.Value != Guid.Empty)
                    {
                        entry.Entity.TenantId = tenantId.Value;
                    }
                }
            }
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
