using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Data;

public static class AdminBootstrapper
{
    public static async Task RunAsync(IServiceProvider serviceProvider, IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue<bool>("BootstrapAdmin:Enabled"))
        {
            return;
        }
        
        string? email = configuration.GetValue<string>("BootstrapAdmin:Email");
        string? password = configuration.GetValue<string>("BootstrapAdmin:Password");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Bootstrap administrator email and password are required");
        }

        email = email.Trim().ToLowerInvariant();
        
        await using var strategyScope = serviceProvider.CreateAsyncScope();
        var strategyDb = strategyScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var strategy = strategyDb.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await using var transaction =
                await db.Database.BeginTransactionAsync(cancellationToken);

            await db.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(731904281);",
                cancellationToken);

            var existingUser = await db.Users
                .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .SingleOrDefaultAsync(
                    user => user.Email == email,
                    cancellationToken);

            if (existingUser is not null)
            {
                bool isAdmin = existingUser.UserRoles
                    .Any(userRole => userRole.Role.Name == "admin");

                if (!isAdmin)
                {
                    throw new InvalidOperationException(
                        "Bootstrap email belongs to an existing non-admin account.");
                }

                await transaction.CommitAsync(cancellationToken);
                return;
            }

            var adminRole = await db.Roles.SingleOrDefaultAsync(
                role => role.Name == "admin",
                cancellationToken);

            if (adminRole is null)
            {
                adminRole = new Role { Name = "admin" };
                db.Roles.Add(adminRole);
            }

            var admin = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(admin);

            db.UserRoles.Add(new UserRole
            {
                User = admin,
                Role = adminRole
            });

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}