using API.DatabaseContexts;
using API.Models;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class SoftwareDevArtifactRepository : Repository<SoftwareDevArtifact>, ISoftwareDevArtifactRepository
{
    private readonly ArtifactsDbContext _context;

    public SoftwareDevArtifactRepository(ArtifactsDbContext context) : base(context)
    {
        _context = context;
    }

    public void Delete(int id)
    {
        _context.Downloads.Where(r => r.ArtifactId == id).ExecuteDelete();
        _context.ArtifactVersions.Where(r => r.SoftwareDevArtifactId == id).ExecuteDelete();
        _context.Feedbacks.Where(r => r.ArtifactId == id).ExecuteDelete();
        _context.Artifacts.Where(r => r.Id == id).ExecuteDelete();
    }

    public new SoftwareDevArtifact GetById(int id)
    {
        return _context.Artifacts
            .Where(a => a.Id == id)
            .Include(a => a.Versions)
            .First();
    }

    public IEnumerable<SoftwareDevArtifact> GetByCategory(int categoryId)
    {
        return _context.Artifacts
            .Where(a => a.CategoryId == categoryId)
            .Include(a => a.Versions)
            .ToList();
    }

    public void AddVersion(int artifactId, ArtifactVersion version)
    {
        var artifact = _context.Artifacts
            .Include(a => a.Versions)
            .FirstOrDefault(a => a.Id == artifactId);

        if (artifact == null)
            throw new Exception("Artifact not found");

        artifact.AddVersion(version);
        _context.Artifacts.Update(artifact);
    }

    public IEnumerable<ArtifactVersion> GetVersionHistory(int artifactId)
    {
        return _context.ArtifactVersions
            .Where(v => v.SoftwareDevArtifactId == artifactId)
            .OrderByDescending(v => v.UploadDate)
            .ToList();
    }

    public IEnumerable<SoftwareDevArtifact> Search(string searchQuery)
    {
        return _context.Artifacts
            .Where(a =>
                a.Title.Contains(searchQuery) ||
                a.Description.Contains(searchQuery) ||
                a.Author.Contains(searchQuery)
            )
            .Include(a => a.Versions)
            .ToList();
    }

    public IEnumerable<SoftwareDevArtifact> FilterByProgrammingLanguage(string language)
    {
        return _context.Artifacts
            .Where(a => a.ProgrammingLanguage == language)
            .Include(a => a.Versions)
            .ToList();
    }

    public IEnumerable<SoftwareDevArtifact> FilterByFramework(string framework)
    {
        return _context.Artifacts
            .Where(a => a.Framework == framework)
            .Include(a => a.Versions)
            .ToList();
    }

    public IEnumerable<SoftwareDevArtifact> FilterByLicenseType(string licenseType)
    {
        return _context.Artifacts
            .Where(a => a.LicenseType == licenseType)
            .Include(a => a.Versions)
            .ToList();
    }

    public IEnumerable<SoftwareDevArtifact> FilterByCombinedCriteria(ArtifactSearchQueryOld queryOld)
    {
        var artifacts = _context.Artifacts.AsQueryable();

        if (!string.IsNullOrEmpty(queryOld.SearchTerm))
        {
            string term = queryOld.SearchTerm.ToLower();

            artifacts = artifacts.Where(a =>
                a.Title.ToLower().Contains(term) ||
                a.Description.ToLower().Contains(term) ||
                a.Author.ToLower().Contains(term) ||
                a.ProgrammingLanguage.ToLower().Contains(term) ||
                a.Framework.ToLower().Contains(term) ||
                a.LicenseType.ToLower().Contains(term) ||
                a.Version.ToLower().Contains(term)
            );
        }

        if (!string.IsNullOrEmpty(queryOld.ProgrammingLanguage))
        {
            artifacts = artifacts.Where(a => a.ProgrammingLanguage == queryOld.ProgrammingLanguage);
        }

        if (!string.IsNullOrEmpty(queryOld.Framework))
        {
            artifacts = artifacts.Where(a => a.Framework == queryOld.Framework);
        }

        if (!string.IsNullOrEmpty(queryOld.LicenseType))
        {
            artifacts = artifacts.Where(a => a.LicenseType == queryOld.LicenseType);
        }

        if (queryOld.CategoryId.HasValue)
        {
            artifacts = artifacts.Where(a => a.CategoryId == queryOld.CategoryId.Value);
        }

        // Sorting
        artifacts = queryOld.SortBy switch
        {
            "Title" => queryOld.SortDescending ? artifacts.OrderByDescending(a => a.Title) : artifacts.OrderBy(a => a.Title),
            "Author" => queryOld.SortDescending ? artifacts.OrderByDescending(a => a.Author) : artifacts.OrderBy(a => a.Author),
            _ => queryOld.SortDescending ? artifacts.OrderByDescending(a => a.Created) : artifacts.OrderBy(a => a.Created)
        };

        // Paging
        artifacts = artifacts
            .Skip((queryOld.PageNumber - 1) * queryOld.PageSize)
            .Take(queryOld.PageSize);

        return artifacts
            .Include(a => a.Versions)
            .ToList();
    }
}
