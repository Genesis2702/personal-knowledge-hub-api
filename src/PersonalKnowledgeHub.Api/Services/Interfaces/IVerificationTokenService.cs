using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IVerificationTokenService
{
    public Task<string> GenerateVerificationToken(int userId, Purpose purpose, CancellationToken cancellationToken);
    public Task<VerificationToken> GetVerificationTokenWithoutTracking(string rawToken, CancellationToken cancellationToken);
    public Task ValidateVerificationToken(string rawToken, CancellationToken cancellationToken);
    public Task<int> ValidatePasswordResetToken(string rawToken, CancellationToken cancellationToken);
    public Task CleanUpVerificationTokens(CancellationToken cancellationToken);
}