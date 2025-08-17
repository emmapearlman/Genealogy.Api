using System.ComponentModel.DataAnnotations;
using Genealogy.Api.Models;

namespace Genealogy.Api.Dtos;

public record PersonCreateDto(
    [property: Required, MaxLength(100)] string GivenName,
    [property: Required, MaxLength(100)] string Surname,
    Gender Gender,
    DateOnly? BirthDate,
    string? BirthPlace,
    DateOnly? DeathDate,
    string? DeathPlace
);

public record PersonUpdateDto(
    [property: MaxLength(100)] string? GivenName,
    [property: MaxLength(100)] string? Surname,
    Gender? Gender,
    DateOnly? BirthDate,
    string? BirthPlace,
    DateOnly? DeathDate,
    string? DeathPlace
);

public record PersonReadDto(
    Guid Id,
    string GivenName,
    string Surname,
    Gender Gender,
    DateOnly? BirthDate,
    string? BirthPlace,
    DateOnly? DeathDate,
    string? DeathPlace
);

public static class PersonMapping
{
    public static PersonReadDto ToReadDto(this Person p) =>
        new(p.Id, p.GivenName, p.Surname, p.Gender, p.BirthDate, p.BirthPlace, p.DeathDate, p.DeathPlace);

    public static void Apply(this Person p, PersonCreateDto dto)
    {
        p.GivenName = dto.GivenName.Trim();
        p.Surname = dto.Surname.Trim();
        p.Gender = dto.Gender;
        p.BirthDate = dto.BirthDate;
        p.BirthPlace = dto.BirthPlace;
        p.DeathDate = dto.DeathDate;
        p.DeathPlace = dto.DeathPlace;
    }

    public static void Apply(this Person p, PersonUpdateDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.GivenName)) p.GivenName = dto.GivenName.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Surname)) p.Surname = dto.Surname.Trim();
        if (dto.Gender.HasValue) p.Gender = dto.Gender.Value;
        if (dto.BirthDate.HasValue) p.BirthDate = dto.BirthDate;
        if (dto.BirthPlace is not null) p.BirthPlace = dto.BirthPlace;
        if (dto.DeathDate.HasValue) p.DeathDate = dto.DeathDate;
        if (dto.DeathPlace is not null) p.DeathPlace = dto.DeathPlace;
    }
}