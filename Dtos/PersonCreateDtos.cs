using Genealogy.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Genealogy.Api.Dtos;

public record PersonCreateDto(
    [property: Required, MaxLength(100)] string GivenName,
    [property: Required, MaxLength(100)] string Surname,
    Sex Sex,
    DateOnly? BirthDate,
    string? BirthPlace,
    DateOnly? DeathDate,
    string? DeathPlace,
    List<int>? ParentIds,
    List<int>? ChildIds
);
