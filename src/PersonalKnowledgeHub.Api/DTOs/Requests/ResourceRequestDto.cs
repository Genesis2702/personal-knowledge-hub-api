using PersonalKnowledgeHub.Entities;
using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests
{
    public class ResourceRequestDto
    {
        [Required]
        [StringLength(200)]
        public required string Title { get; set; }
        [StringLength(4096)]
        public string? Url { get; set; }
        [StringLength(5000)]
        public string? Description { get; set; }
        [Required]
        public required ResourceType ResourceType { get; set; }
    }
}
