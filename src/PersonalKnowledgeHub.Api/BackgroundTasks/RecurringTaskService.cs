using Hangfire;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.BackgroundTasks;

public static class RecurringTaskService
{
    public static void RegisterRecurringJobs(this WebApplication app)
    {
        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
        
        recurringJobManager.AddOrUpdate<ITokenService>(
            "token-cleanup", 
            tokenService => tokenService.CleanUpRefreshTokens(CancellationToken.None),
            Cron.Monthly);
        recurringJobManager.AddOrUpdate<IVerificationTokenService>(
            "verification-token-cleanup",
            verificationTokenService => verificationTokenService.CleanUpVerificationTokens(CancellationToken.None),
            Cron.Monthly);
        recurringJobManager.AddOrUpdate<IUserService>(
            "deactivate-pending-users",
            userService => userService.DeactivatePendingUsers(CancellationToken.None),
            Cron.Weekly);
        recurringJobManager.AddOrUpdate<IFileResourceService>(
            "resource-cleanup",
            fileResourceService => fileResourceService.DeleteFileResourcePermanently(CancellationToken.None),
            Cron.Daily);
        recurringJobManager.AddOrUpdate<ITagService>(
            "tag-cleanup",
            tagService => tagService.CleanUpTags(CancellationToken.None),
            Cron.Daily);
    }
}