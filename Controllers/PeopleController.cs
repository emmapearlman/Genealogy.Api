using Genealogy.Api.Data;
using Genealogy.Api.Dtos;
using Genealogy.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Genealogy.Api.Controllers;

[ApiController]
[Route("people")]
public class PeopleController : ControllerBase
{
    private readonly AppDbContext _db;

    public PeopleController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Gets a paginated list of people, optionally filtered by search term.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPeople([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.People.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => EF.Functions.Like(p.GivenName, $"%{s}%") ||
                                     EF.Functions.Like(p.Surname, $"%{s}%"));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Surname).ThenBy(p => p.GivenName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => p.ToReadDto()).ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    /// <summary>
    /// Gets a person by their unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPerson(Guid id)
    {
        var person = await _db.People.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return person is null ? NotFound() : Ok(person.ToReadDto());
    }

    /// <summary>
    /// Creates a new person.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePerson([FromBody] PersonCreateDto dto)
    {
        var p = new Person();
        p.Apply(dto);
        _db.People.Add(p);
        await _db.SaveChangesAsync();
        return Created($"/people/{p.Id}", p.ToReadDto());
    }

    /// <summary>
    /// Updates an existing person.
    /// </summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdatePerson(Guid id, [FromBody] PersonUpdateDto dto)
    {
        var p = await _db.People.FindAsync(id);
        if (p is null) return NotFound();
        p.Apply(dto);
        await _db.SaveChangesAsync();
        return Ok(p.ToReadDto());
    }

    /// <summary>
    /// Deletes a person by their unique identifier.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePerson(Guid id)
    {
        var p = await _db.People.FindAsync(id);
        if (p is null) return NotFound();
        _db.People.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}