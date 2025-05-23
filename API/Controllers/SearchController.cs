namespace API.Controllers;

using API.Models;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        // 1. Basic Search by Text (e.g., Title or Description)
        [HttpGet("basic")]
        public IActionResult BasicSearch([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query string cannot be empty.");

            var searchQuery = new ArtifactSearchQuery
            {
                Title = query,
                Description = query
            };

            var result = _searchService.SearchCategories(searchQuery);
            return Ok(result);
        }

        // 3. Combined Advanced Search (all parameters)
        [HttpPost("advanced")]
        public ActionResult<IEnumerable<Category>> AdvancedSearch([FromBody] ArtifactSearchQuery query)
        {
            if (query == null)
                return BadRequest("Query object is required.");

            var result = _searchService.SearchCategories(query);
            return Ok(result);
        }
}
