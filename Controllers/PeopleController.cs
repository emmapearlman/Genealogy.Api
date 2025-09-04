using Genealogy.Api.Data;
using Genealogy.Api.Dto;
using Genealogy.Api.Dtos;
using Genealogy.Api.Models;
using Genealogy.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Genealogy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PeopleController : ControllerBase
{
    private readonly IFamilyTreeService _svc;
   // public PeopleController(IFamilyTreeService svc) => _svc = svc;

    private readonly AppDbContext _db;

    public PeopleController(AppDbContext db, IFamilyTreeService svc)
    {
        _db = db;
        _svc = svc;
    }


    // GET: /api/people/5
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PersonDetailsDto>> GetPerson(int id)
    {
        var person = await _db.People
            .Where(p => p.Id == id)
            .Select(p => new PersonDetailsDto(
                p.Id,
                p.GivenName,
                p.Surname,
                p.Sex,

                // Parents
                _db.ParentChildren
                    .Where(pc => pc.ChildId == p.Id)
                    .Select(pc => new RelativeDto(
                        pc.Parent.Id,
                        pc.Parent.GivenName,
                        pc.Parent.Surname
                    ))
                    .ToList(),

                // Children
                _db.ParentChildren
                    .Where(pc => pc.ParentId == p.Id)
                    .Select(pc => new RelativeDto(
                        pc.Child.Id,
                        pc.Child.GivenName,
                        pc.Child.Surname
                    ))
                    .ToList(),

                // Spouses (via marriages)
                _db.Marriages
                    .Where(m => m.Spouse1Id == p.Id || m.Spouse2Id == p.Id)
                    .Select(m => m.Spouse1Id == p.Id
                        ? new RelativeDto(m.Spouse2Id, m.Spouse2.GivenName, m.Spouse2.Surname)
                        : new RelativeDto(m.Spouse1Id, m.Spouse1.GivenName, m.Spouse1.Surname)
                    )
                    .ToList()
            ))
            .FirstOrDefaultAsync();

        if (person == null)
            return NotFound();

        return Ok(person);
    }

    // GET: /api/people/5/ancestors?generations=3&includeSpouses=true
    [HttpGet("{id:int}/ancestors")]
    public async Task<ActionResult<TreeNodeDto>> GetAncestors(
        int id, [FromQuery] int generations = 3, [FromQuery] bool includeSpouses = true)
    {
        if (generations < 0 || generations > 10) return BadRequest("Generations must be between 0 and 10.");
        var tree = await _svc.GetAncestorsAsync(id, generations, includeSpouses);
        return tree is null ? NotFound() : Ok(tree);
    }

    // GET: /api/people/5/descendants?generations=3&includeSpouses=true
    [HttpGet("{id:int}/descendants")]
    public async Task<ActionResult<TreeNodeDto>> GetDescendants(
        int id, [FromQuery] int generations = 3, [FromQuery] bool includeSpouses = true)
    {
        if (generations < 0 || generations > 10) return BadRequest("Generations must be between 0 and 10.");
        var tree = await _svc.GetDescendantsAsync(id, generations, includeSpouses);
        return tree is null ? NotFound() : Ok(tree);
    }

    // GET: /api/people/5/tree?generations=3&includeSpouses=true
    [HttpGet("{id:int}/tree")]
    public async Task<ActionResult<TreeNodeDto>> GetFullTree(
        int id, [FromQuery] int generations = 3, [FromQuery] bool includeSpouses = true)
    {
        if (generations < 0 || generations > 10) return BadRequest("Generations must be between 0 and 10.");
        var tree = await _svc.GetFullTreeAsync(id, generations, includeSpouses);
        return tree is null ? NotFound() : Ok(tree);
    }

    [HttpPost]
    public async Task<ActionResult<Person>> AddPerson(PersonCreateDto dto)
    {
        var person = new Person
        {
            GivenName = dto.GivenName.Trim(),
            Surname = dto.Surname.Trim(),
            Sex = dto.Sex,
            BirthDate = dto.BirthDate,
            BirthPlace = dto.BirthPlace,
            DeathDate = dto.DeathDate,
            DeathPlace = dto.DeathPlace
        };

        // Link parents
        if (dto.ParentIds != null && dto.ParentIds.Any())
        {
            var parents = await _db.People
                .Where(p => dto.ParentIds.Contains(p.Id))
                .ToListAsync();

            foreach (var parent in parents)
            {
                person.Parents.Add(parent);
            }
        }

        // Link children
        if (dto.ChildIds != null && dto.ChildIds.Any())
        {
            var children = await _db.People
                .Where(p => dto.ChildIds.Contains(p.Id))
                .ToListAsync();

            foreach (var child in children)
            {
                person.Children.Add(child);
            }
        }

        _db.People.Add(person);
        await _db.SaveChangesAsync();

        // Create marriages
        if (dto.SpouseIds != null && dto.SpouseIds.Any())
        {
            var spouses = await _db.People
                .Where(p => dto.SpouseIds.Contains(p.Id))
                .ToListAsync();

            foreach (var spouse in spouses)
            {
                // Ensure consistent ordering for uniqueness
                var (a, b) = person.Id.CompareTo(spouse.Id) < 0
                    ? (person.Id, spouse.Id)
                    : (spouse.Id, person.Id);

                if (!await _db.Marriages.AnyAsync(m => m.Spouse1Id == a && m.Spouse2Id == b))
                {
                    _db.Marriages.Add(new Marriage
                    {
                        Spouse1Id = a,
                        Spouse2Id = b
                    });
                }
            }
            await _db.SaveChangesAsync();
        }

        // Reload with marriages
        var result = await _db.People
            .AsNoTracking()
            .Where(p => p.Id == person.Id)
            .Select(p => new
            {
                p.Id,
                p.GivenName,
                p.Surname,
                p.Sex,
                Marriages = _db.Marriages
                    .Where(m => m.Spouse1Id == p.Id || m.Spouse2Id == p.Id)
                    .Select(m => new
                    {
                        MarriageId = m.Id,
                        Spouse = m.Spouse1Id == p.Id
                            ? _db.People.Where(sp => sp.Id == m.Spouse2Id).Select(sp => new { sp.Id, sp.GivenName, sp.Surname }).FirstOrDefault()
                            : _db.People.Where(sp => sp.Id == m.Spouse1Id).Select(sp => new { sp.Id, sp.GivenName, sp.Surname }).FirstOrDefault()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, result);

    }
}