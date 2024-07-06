using System.Net;
using Microsoft.AspNetCore.Mvc;
using NOS.Engineering.Challenge.API.Models;
using NOS.Engineering.Challenge.Managers;

namespace NOS.Engineering.Challenge.API.Controllers
{

    [Route("api/v1/[controller]")]
    [ApiController]
    public class ContentController : Controller
    {
        private readonly IContentsManager _manager;
        private readonly ILogger<ContentController> _logger;
        private readonly IMemoryCache _cache;

        public ContentController(IContentsManager manager, ILogger<ContentController> logger, IMemoryCache cache)
        {
            _manager = manager;
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        [Obsolete("This endpoint is deprecated. Use GET /api/v1/Content/filter instead.")]
        public async Task<IActionResult> GetManyContents(int page = 1, int pageSize = 10)
        {
            _logger.LogInformation("Getting many contents");

            var cacheKey = $"Contents_Page{page}_PageSize{pageSize}";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<Content> contents))
            {
                return Ok(contents);
            }

            var skip = (page - 1) * pageSize;

            var contents = await _manager.GetManyContents().Skip(skip).Take(pageSize).ConfigureAwait(false);

            if (!contents.Any())
            {
                _logger.LogWarning("No Contents found");
                return NotFound();
            }

            _cache.Set(cacheKey, content, TimeSpan.FromMinutes(5));

            _logger.LogInformation("Contents fetched successfully");
            return Ok(contents);
        }

        // New endpoint with filtering capabilities
        [HttpGet("filter")]
        public async Task<IActionResult> GetFilteredContents(string title = null, string genre = null)
        {
            _logger.LogInformation("Fetching filtered contents");

            var contents = await _manager.GetManyContents().ConfigureAwait(false);

            // Apply filters if provided
            if (!string.IsNullOrWhiteSpace(title))
            {
                contents = contents.Where(c => c.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                contents = contents.Where(c => c.GenreList.Any(g => g.Contains(genre, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (!contents.Any())
            {
                _logger.LogWarning("No filtered contents found");
                return NotFound();
            }

            _logger.LogInformation("Filtered contents fetched successfully");
            return Ok(contents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContent(Guid id)
        {
            _logger.LogInformation("Getting content with ID {ContentId}", id);

            var cacheKey = $"Content_{id}";
            if (_cache.TryGetValue(cacheKey, out Content content))
            {
                return Ok(content);
            }

            var content = await _manager.GetContent(id).ConfigureAwait(false);

            if (content == null)
            {
                _logger.LogWarning("Content with ID {ContentId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Content with ID {ContentId} fetched successfully", id);
            return Ok(content);
        }

        [HttpPost]
        public async Task<IActionResult> CreateContent(
            [FromBody] ContentInput content
            )
        {
            _logger.LogInformation("Creating new content");
            var createdContent = await _manager.CreateContent(content.ToDto()).ConfigureAwait(false);

            if (createdContent == null)
            {
                _logger.LogError("Error creating content");
                return Problem();
            }

            _logger.LogInformation("Content created successfully with ID {ContentId}", createdContent.Id);
            return Ok(createdContent);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateContent(
            Guid id,
            [FromBody] ContentInput content
            )
        {
            _logger.LogInformation("Updating content with ID {ContentId}", id);
            var updatedContent = await _manager.UpdateContent(id, content.ToDto()).ConfigureAwait(false);

            if (updatedContent == null)
            {
                _logger.LogWarning("Content with ID {ContentId} not found", id);
                return NotFound();
            }
            _logger.LogInformation("Content with ID {ContentId} updated successfully", id);
            return Ok(updatedContent);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContent(
            Guid id
        )
        {
            _logger.LogInformation("Deleting content with ID {ContentId}", id);

            var deletedId = await _manager.DeleteContent(id).ConfigureAwait(false);
            if (deletedId == null)
            {
                _logger.LogWarning("Content with ID {ContentId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Content with ID {ContentId} deleted successfully", id);

            return Ok(deletedId);
        }

        [HttpPost("{id}/genre")]
        public async Task<IActionResult> AddGenres(
            Guid id,
            [FromBody] IEnumerable<string> genres
        )
        {
            _logger.LogInformation("Adding genres to content with ID {ContentId}", id);
            // Validate genre
            if (genres == null || !genres.Any())
            {
                _logger.LogWarning("Genres is empty or does not exist for content with ID {ContentId}", id);
                return BadRequest("Genres is empty or does not exist");
            }

            // Check if entity exists
            var entity = await _manager.GetContent(id).ConfigureAwait(false);
            if (entity == null)
            {
                _logger.LogWarning("Content with ID {ContentId} not found", id);

                return NotFound($"Entity with id {id} not found!");
            }

            // Create a new list of genres (with the new one)
            var updatedGenreList = entity.GenreList.ToList();
            updatedGenreList.AddRange(genres);

            // Create a new object with updated list
            var newContent = new Content(
                entity.Id,
                entity.Title,
                entity.SubTitle,
                entity.Description,
                entity.ImageUrl,
                entity.Duration,
                entity.StartTime,
                entity.EndTime,
                updatedGenreList
            );

            var updatedContent = await _manager.UpdateContent(id, newContent.ToDto()).ConfigureAwait(false);


            // Update the content in the database
            if (updatedContent == null)
            {
                _logger.LogWarning("Failed to update genres for content with ID {ContentId}", id);
                return NotFound();
            }

            _logger.LogInformation("Genres added to content with ID {ContentId} successfully", id);
            return Ok(updatedContent);
        }

        [HttpDelete("{id}/genre")]
        public Task<IActionResult> RemoveGenres(
            Guid id,
            [FromBody] IEnumerable<string> genres
        )
        {
            _logger.LogInformation("Removing genres from content with ID {ContentId}", id);
            // Validate genre
            if (genres == null || !genres.Any())
            {
                _logger.LogWarning("Genres is empty or does not exist for content with ID {ContentId}", id);
                return BadRequest("Genres cannot be null or empty.");
            }

            // Check if entity exists
            var entity = await _manager.GetContent(id).ConfigureAwait(false);
            if (entity == null)
            {
                _logger.LogWarning("Content with ID {ContentId} not found", id);
                return NotFound($"Entity with id {id} not found.");
            }

            // Create a new list of genres (without the removed ones)
            var updatedGenreList = entity.GenreList.Except(genres).ToList();

            // Create a new Content object with the updated genre list
            var newContent = new Content(
                entity.Id,
                entity.Title,
                entity.SubTitle,
                entity.Description,
                entity.ImageUrl,
                entity.Duration,
                entity.StartTime,
                entity.EndTime,
                updatedGenreList
            );

            // Update the content in the database
            var updatedContent = await _manager.UpdateContent(id, newContent.ToDto()).ConfigureAwait(false);

            if (updatedContent == null)
            {
                _logger.LogWarning("Failed to remove genres from content with ID {ContentId}", id);
                return NotFound();
            }

            _logger.LogInformation("Genres removed from content with ID {ContentId} successfully", id);
            return Ok(updatedContent);
        }
    }
}
