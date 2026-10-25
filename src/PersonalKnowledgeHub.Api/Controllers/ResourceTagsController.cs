using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;
using PersonalKnowledgeHub.Mapper;

namespace PersonalKnowledgeHub.Controllers
{
    [ApiController]
    [Route("resources/{resourceId}/tags")]
    [Authorize(Policy = "ActiveAccount")]
    [EnableRateLimiting("crud")]
    public class ResourceTagsController : ControllerBase
    {
        private readonly IResourceTagService _resourceTagService;
        private readonly IDistributedCache _distributedCache;

        public ResourceTagsController(IResourceTagService resourceTagService, IDistributedCache distributedCache)
        {
            _resourceTagService = resourceTagService;
            _distributedCache = distributedCache;
        }

        [EndpointSummary("Create a new resource tag for the current user")]
        [HttpPost]
        public async Task<ActionResult<ResourceResponseDto>> AddResourceTag(ResourceTagRequestDto resourceTagRequest, int resourceId, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Resource resource = await _resourceTagService.AddResourceTag(resourceTagRequest.TagId, resourceId, userId, cancellationToken);
            string cacheKey = $"resource:{userId}:{resourceId}";
            await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
            ResourceResponseDto resourceResponse = ResourceMapper.ToResourceResponseDto(resource);
            return CreatedAtAction(nameof(ResourcesController.GetResourceById), "Resources", new { id = resource.Id }, resourceResponse);
        }

        [EndpointSummary("Delete one of the current user's resource tag by ID")]
        [HttpDelete("{tagId}")]
        public async Task<IActionResult> DeleteResourceTag(int tagId, int resourceId, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _resourceTagService.DeleteResourceTag(tagId, resourceId, userId, cancellationToken);
            string cacheKey = $"resource:{userId}:{resourceId}";
            await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
            return NoContent();
        }
    }
}
