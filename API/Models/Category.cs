using System.Text.Json.Serialization;

namespace API.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }

    public int? ParentCategoryId { get; set; }
    [JsonIgnore]
    public Category ParentCategory { get; set; }
    public List<Category> Subcategories { get; set; } = new();

    public List<SoftwareDevArtifact> Artifacts { get; set; } = new();

    public int OrderIndex { get; set; }

    public void AddSubcategory(Category subcategory) => Subcategories.Add(subcategory);
    public void DeleteSubcategory(Category subcategory) => Subcategories.Remove(subcategory);
    public void ModifyCategory(string name) => Name = name;
    public bool IsEmpty() => Artifacts.Count == 0 && Subcategories.Count == 0;
}
