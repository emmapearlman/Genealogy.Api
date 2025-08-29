namespace Genealogy.Api.Services;
using Genealogy.Api.Dto;

public interface IFamilyTreeService
{
    Task<PersonDto?> GetPersonAsync(int id);
    Task<TreeNodeDto?> GetAncestorsAsync(int id, int generations, bool includeSpouses);
    Task<TreeNodeDto?> GetDescendantsAsync(int id, int generations, bool includeSpouses);
    Task<TreeNodeDto?> GetFullTreeAsync(int id, int generations, bool includeSpouses);
}
