using Microsoft.EntityFrameworkCore;
using ChuBank.Domain.Entities;

namespace ChuBank.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ChuBankDbContext context)
    {
        var now = DateTime.UtcNow;

        try
        {
            if (!await context.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "System administrator", CreatedAt = now },
                    new Role { Id = Guid.NewGuid(), Name = "Manager", Description = "Bank manager", CreatedAt = now },
                    new Role { Id = Guid.NewGuid(), Name = "Employee", Description = "Bank employee", CreatedAt = now },
                    new Role { Id = Guid.NewGuid(), Name = "Customer", Description = "Bank customer", CreatedAt = now }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            if (!await context.Users.AnyAsync())
            {
                var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
                
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@chubank.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456", BCrypt.Net.BCrypt.GenerateSalt(12)),
                    FirstName = "System",
                    LastName = "Administrator",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();

                var userRole = new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id,
                    AssignedAt = now
                };

                await context.UserRoles.AddAsync(userRole);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DbInitializer warning: {ex.Message}");
        }
    }
}
