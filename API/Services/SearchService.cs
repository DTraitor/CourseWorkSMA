using API.Models;
using API.Services.Interfaces;
using API.UnitOfWork;

namespace API.Services;

public class SearchService : ISearchService
{
    private readonly IUnitOfWork _unitOfWork;

    public SearchService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<Category> SearchCategories(ArtifactSearchQuery query)
    {
        var allArtifacts = _unitOfWork.SoftwareDevArtifactRepository.GetAll().AsQueryable();

        var matchedArtifacts = ArtifactQueryBuilder.ApplyFilters(allArtifacts, query).ToList();

            var categoryMap = new Dictionary<int, Category>();

            foreach (var artifact in matchedArtifacts)
            {
                if (artifact.Category == null) continue;

                var pathStack = new Stack<Category>();
                var current = artifact.Category;

                // Build path to root
                while (current != null)
                {
                    pathStack.Push(current);
                    current = current.ParentCategory;
                }

                Category? parentClone = null;
                Category? root = null;

                while (pathStack.Count > 0)
                {
                    var original = pathStack.Pop();

                    if (categoryMap.TryGetValue(original.Id, out var existing))
                    {
                        parentClone = existing;
                        continue;
                    }

                    var cloned = new Category
                    {
                        Id = original.Id,
                        Name = original.Name,
                        ParentCategoryId = original.ParentCategoryId,
                        OrderIndex = original.OrderIndex,
                        Subcategories = new List<Category>(),
                        Artifacts = new List<SoftwareDevArtifact>(),
                    };

                    categoryMap[cloned.Id] = cloned;

                    if (parentClone != null)
                    {
                        parentClone.Subcategories.Add(cloned);
                    }

                    parentClone = cloned;
                    if (root == null) root = cloned;
                }

                // Add artifact to its leaf category clone
                parentClone?.Artifacts.Add(artifact);
            }

            // Return only unique root categories
            return categoryMap.Values
                .Where(c => c.ParentCategoryId == null)
                .ToList();
    }
}