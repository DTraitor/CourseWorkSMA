using API.Models;

namespace API.Services.Interfaces;

public interface ISearchService
{
    IEnumerable<Category> SearchCategories(ArtifactSearchQuery query);
}