using Genealogy.Api.Dto;
using Genealogy.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Genealogy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PeopleController : ControllerBase
{
    private readonly IFamilyTreeService _svc;
    public PeopleController(IFamilyTreeService svc) => _svc = svc;

    // GET: /api/people/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PersonDto>> GetPerson(int id)
    {
        var person = await _svc.GetPersonAsync(id);
        return person is null ? NotFound() : Ok(person);
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
}