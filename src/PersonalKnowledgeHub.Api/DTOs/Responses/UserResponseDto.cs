using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.DTOs.Responses;

public class UserResponseDto
{
    public required int Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserStatus Status { get; set; }
    public DateTime? BannedAt { get; set; }
    public ICollection<string> Resources { get; set; } = new List<string>();
    public ICollection<string> Tags { get; set; } = new List<string>();
    public ICollection<RoleProperties> Roles { get; set; } = new List<RoleProperties>();
}

public record RoleProperties(int Id, string Name);