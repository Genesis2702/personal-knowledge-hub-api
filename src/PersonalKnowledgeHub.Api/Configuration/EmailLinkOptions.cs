namespace PersonalKnowledgeHub.Configuration;

public class EmailLinkOptions
{
    public const string Options = "EmailLinkOptions";

    public string VerificationUrl { get; init; } = String.Empty;
    public string PasswordResetUrl { get; init; } = String.Empty;
}