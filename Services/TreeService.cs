using Genealogy.Api.Data;
using Genealogy.Api.Dtos;
using Genealogy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Genealogy.Api.Services;

public class TreeService
{
    private readonly AppDbContext _db;
    public TreeService(AppDbContext db) => _db = db;

    public async Task<object?> GetAncestorsAsync(Guid personId, int maxDepth)
    {
        var person = await _db.People.AsNoTracking().FirstOrDefaultAsync(p => p.Id == personId);
        if (person is null) return null;

        var visited = new HashSet<Guid> { personId };
        var levels = new List<List<Person>>();

        var current = await ParentsOf(personId);
        int depth = 1;
        while (current.Any() && depth <= maxDepth)
        {
            levels.Add(current);
            var next = new List<Person>();
            foreach (var p in current)
            {
                if (!visited.Add(p.Id)) continue;
                var parents = await ParentsOf(p.Id);
                foreach (var pp in parents)
                    if (!visited.Contains(pp.Id)) next.Add(pp);
            }
            current = next;
            depth++;
        }

        return new
        {
            root = person.ToReadDto(),
            depth = Math.Min(depth - 1, maxDepth),
            generations = levels.Select((gen, i) => new
            {
                level = i + 1,
                people = gen.Select(x => x.ToReadDto())
            })
        };
    }

    public async Task<object?> GetDescendantsAsync(Guid personId, int maxDepth)
    {
        var person = await _db.People.AsNoTracking().FirstOrDefaultAsync(p => p.Id == personId);
        if (person is null) return null;

        var visited = new HashSet<Guid> { personId };
        var levels = new List<List<Person>>();

        var current = await ChildrenOf(personId);
        int depth = 1;
        while (current.Any() && depth <= maxDepth)
        {
            levels.Add(current);
            var next = new List<Person>();
            foreach (var p in current)
            {
                if (!visited.Add(p.Id)) continue;
                var kids = await ChildrenOf(p.Id);
                foreach (var k in kids)
                    if (!visited.Contains(k.Id)) next.Add(k);
            }
            current = next;
            depth++;
        }

        return new
        {
            root = person.ToReadDto(),
            depth = Math.Min(depth - 1, maxDepth),
            generations = levels.Select((gen, i) => new
            {
                level = i + 1,
                people = gen.Select(x => x.ToReadDto())
            })
        };
    }

    private Task<List<Person>> ParentsOf(Guid id) =>
        _db.ParentChildren.Where(pc => pc.ChildId == id)
            .Select(pc => pc.Parent).AsNoTracking().ToListAsync();

    private Task<List<Person>> ChildrenOf(Guid id) =>
        _db.ParentChildren.Where(pc => pc.ParentId == id)
            .Select(pc => pc.Child).AsNoTracking().ToListAsync();
}