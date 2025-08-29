namespace Genealogy.Api.Services;
using Genealogy.Api.Data;
using Genealogy.Api.Dto;
using Genealogy.Api.Models;
using Microsoft.EntityFrameworkCore;

public class FamilyTreeService : IFamilyTreeService
{
    private readonly AppDbContext _db;
    public FamilyTreeService(AppDbContext db) => _db = db;

    public async Task<PersonDto?> GetPersonAsync(int id)
    {
        var p = await _db.People.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? null : Map(p);
    }

    public async Task<TreeNodeDto?> GetAncestorsAsync(int id, int generations, bool includeSpouses)
    {
        var visited = new HashSet<int>();
        return await BuildNodeAsync(id,
            upDepth: generations,
            downDepth: 0,
            includeSpouses,
            visited);
    }

    public async Task<TreeNodeDto?> GetDescendantsAsync(int id, int generations, bool includeSpouses)
    {
        var visited = new HashSet<int>();
        return await BuildNodeAsync(id,
            upDepth: 0,
            downDepth: generations,
            includeSpouses,
            visited);
    }

    public async Task<TreeNodeDto?> GetFullTreeAsync(int id, int generations, bool includeSpouses)
    {
        var visited = new HashSet<int>();
        return await BuildNodeAsync(id,
            upDepth: generations,
            downDepth: generations,
            includeSpouses,
            visited);
    }

    private async Task<TreeNodeDto?> BuildNodeAsync(
        int personId,
        int upDepth,
        int downDepth,
        bool includeSpouses,
        HashSet<int> visited)
    {
        if (visited.Contains(personId)) return null;
        visited.Add(personId);

        var p = await _db.People.AsNoTracking().FirstOrDefaultAsync(x => x.Id == personId);
        if (p is null) return null;

        var node = new TreeNodeDto { Person = Map(p) };

        if (includeSpouses)
        {
            var spouseIds = await _db.Marriages.AsNoTracking()
                .Where(m => m.Spouse1Id == personId || m.Spouse2Id == personId)
                .Select(m => m.Spouse1Id == personId ? m.Spouse2Id : m.Spouse1Id)
                .Distinct()
                .ToListAsync();

            if (spouseIds.Count > 0)
            {
                var spouses = await _db.People.AsNoTracking()
                    .Where(x => spouseIds.Contains(x.Id))
                    .ToListAsync();
                node.Spouses = spouses.Select(Map).ToList();
            }
        }

        if (upDepth > 0)
        {
            var parents = new List<TreeNodeDto>();
            if (p.FatherId is int fatherId)
            {
                var fatherNode = await BuildNodeAsync(fatherId, upDepth - 1, 0, includeSpouses, visited);
                if (fatherNode != null) parents.Add(fatherNode);
            }
            if (p.MotherId is int motherId)
            {
                var motherNode = await BuildNodeAsync(motherId, upDepth - 1, 0, includeSpouses, visited);
                if (motherNode != null) parents.Add(motherNode);
            }
            node.Parents = parents;
        }

        if (downDepth > 0)
        {
            var childIds = await _db.People.AsNoTracking()
                .Where(c => c.FatherId == personId || c.MotherId == personId)
                .Select(c => c.Id)
                .ToListAsync();

            var childNodes = new List<TreeNodeDto>();
            foreach (var cid in childIds)
            {
                var childNode = await BuildNodeAsync(cid, 0, downDepth - 1, includeSpouses, visited);
                if (childNode != null) childNodes.Add(childNode);
            }
            node.Children = childNodes;
        }

        return node;
    }

    private static PersonDto Map(Person p) => new()
    {
        Id = p.Id,
        FullName = $"{p.GivenName} {p.Surname}".Trim(),
        Sex = p.Sex.ToString(),
        BirthDate = p.BirthDate,
        DeathDate = p.DeathDate
    };
}