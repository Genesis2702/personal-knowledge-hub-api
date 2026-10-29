namespace PersonalKnowledgeHub.DTOs.Responses;

public class RoleResponseDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<PermissionProperties> Permissions { get; set; } = new List<PermissionProperties>();
}

public record PermissionProperties(int Id, string Name);