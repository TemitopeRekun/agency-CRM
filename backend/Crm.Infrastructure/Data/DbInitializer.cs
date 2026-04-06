using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureCreatedAsync();

        await SeedTenantsAsync(context);
        await context.SaveChangesAsync();

        await SeedCrmDataAsync(context);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTenantsAsync(AppDbContext context)
    {
        if (await context.Tenants.IgnoreQueryFilters().AnyAsync()) return;

        // Tenant A
        var tenantAId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var tenantA = new Tenant { Id = tenantAId, Name = "Tenant A - Tech Corp" };
        
        var adminA = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@tenanta.com",
            FullName = "Admin Tenant A",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = UserRole.Admin,
            TenantId = tenantAId
        };

        // Tenant B
        var tenantBId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var tenantB = new Tenant { Id = tenantBId, Name = "Tenant B - Creative Agency" };
        
        var adminB = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@tenantb.com",
            FullName = "Admin Tenant B",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = UserRole.Admin,
            TenantId = tenantBId
        };

        await context.Tenants.AddRangeAsync(tenantA, tenantB);
        await context.Users.AddRangeAsync(adminA, adminB);
    }

    private static async Task SeedCrmDataAsync(AppDbContext context)
    {
        if (await context.Clients.IgnoreQueryFilters().AnyAsync()) return;

        var tenantAId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var tenantBId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var clientA1 = new Client { Id = Guid.NewGuid(), Name = "Tech Solutions Ltd", TenantId = tenantAId };
        var leadA1 = new Lead { Id = Guid.NewGuid(), Title = "Website Redesign", Description = "Tenant A Lead", TenantId = tenantAId };
        await context.Clients.AddAsync(clientA1);
        await context.Leads.AddAsync(leadA1);
        await context.Offers.AddAsync(new Offer { Id = Guid.NewGuid(), Title = "Web Quote A", TotalAmount = 5000, LeadId = leadA1.Id, TenantId = tenantAId });

        // Seed CRM Data for Tenant B
        var clientB1 = new Client { Id = Guid.NewGuid(), Name = "Creative Studio", TenantId = tenantBId };
        var leadB1 = new Lead { Id = Guid.NewGuid(), Title = "Mobile App Design", Description = "Tenant B Lead", TenantId = tenantBId };
        await context.Clients.AddAsync(clientB1);
        await context.Leads.AddAsync(leadB1);
        await context.Offers.AddAsync(new Offer { Id = Guid.NewGuid(), Title = "Mobile Quote B", TotalAmount = 12000, LeadId = leadB1.Id, TenantId = tenantBId });

        await context.SaveChangesAsync();
    }
}
