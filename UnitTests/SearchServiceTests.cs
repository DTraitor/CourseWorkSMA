using API.Models;
using API.Repositories.Interfaces;
using API.Services;
using API.UnitOfWork;
using Moq;

namespace UnitTests;

public class SearchServiceTests
{
    [Fact]
    public void SearchArtifacts_ReturnsCorrectCategoryHierarchy()
    {
        // Arrange
        var leafCategory = new Category { Id = 3, Name = "Leaf", ParentCategoryId = 2 };
        var midCategory = new Category { Id = 2, Name = "Mid", ParentCategoryId = 1, Subcategories = new List<Category> { leafCategory } };
        var rootCategory = new Category { Id = 1, Name = "Root", ParentCategoryId = null, Subcategories = new List<Category> { midCategory } };

        leafCategory.ParentCategory = midCategory;
        midCategory.ParentCategory = rootCategory;

        var artifact = new SoftwareDevArtifact
        {
            Title = "Auth",
            Description = "Token-based",
            ProgrammingLanguage = "C#",
            Category = leafCategory
        };

        var unitOfWork = new Mock<IUnitOfWork>();
        var mockArtifactRepo = new Mock<ISoftwareDevArtifactRepository>();

        unitOfWork.SetupGet(u => u.SoftwareDevArtifactRepository).Returns(mockArtifactRepo.Object);
        mockArtifactRepo.Setup(r => r.GetAll()).Returns(new List<SoftwareDevArtifact> { artifact });

        var mockCategoryRepo = new Mock<ICategoryRepository>();

        var service = new SearchService(unitOfWork.Object);

        var query = new ArtifactSearchQuery { Title = "Auth" };

        // Act
        var result = service.SearchCategories(query).ToList();

        // Assert
        Assert.Single(result); // only one root category
        var root = result[0];

        Assert.Equal("Root", root.Name);
        Assert.Single(root.Subcategories);
        Assert.Single(root.Subcategories[0].Subcategories);
        Assert.Single(root.Subcategories[0].Subcategories[0].Artifacts); // artifact is in leaf
    }
}
