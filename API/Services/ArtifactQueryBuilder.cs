using API.Models;

namespace API.Services;

public static class ArtifactQueryBuilder
{
    public static IQueryable<SoftwareDevArtifact> ApplyFilters(
        IQueryable<SoftwareDevArtifact> query,
        ArtifactSearchQuery search)
    {
        if (!string.IsNullOrWhiteSpace(search.Title))
        {
            query = query.Where(a => a.Title.Contains(search.Title, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search.Description))
        {
            query = query.Where(a => a.Description.Contains(search.Description, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search.Author))
        {
            query = query.Where(a => a.Author.Contains(search.Author, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search.Version))
        {
            query = query.Where(a => a.Version == search.Version);
        }

        if (!string.IsNullOrWhiteSpace(search.ProgrammingLanguage))
        {
            query = query.Where(a => a.ProgrammingLanguage == search.ProgrammingLanguage);
        }

        if (!string.IsNullOrWhiteSpace(search.Framework))
        {
            query = query.Where(a => a.Framework == search.Framework);
        }

        if (!string.IsNullOrWhiteSpace(search.LicenseType))
        {
            query = query.Where(a => a.LicenseType == search.LicenseType);
        }

        return query;
    }
}