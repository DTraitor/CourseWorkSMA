namespace API.Models;

public class ArtifactSearchQuery
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public string? Version { get; set; }
    public string? ProgrammingLanguage { get; set; }
    public string? Framework { get; set; }
    public string? LicenseType { get; set; }
}