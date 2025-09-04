using Genealogy.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Genealogy.Api.Dtos;

public record PersonCreateDto(
    [Required, MaxLength(100)] string GivenName,
    [Required, MaxLength(100)] string Surname,
    [Required] Sex Sex,
    DateOnly? BirthDate = null,
    DateOnly? DeathDate = null,
    string? BirthPlace = null,
    string? DeathPlace = null,
    List<int>? ParentIds = null,
    List<int>? ChildIds = null
);
