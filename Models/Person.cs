using System.ComponentModel.DataAnnotations;

namespace Genealogy.Api.Models;

public enum Gender { Unknown = 0, Female = 1, Male = 2, NonBinary = 3 }

public class Person
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string GivenName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Surname { get; set; } = string.Empty;

    public Gender Gender { get; set; } = Gender.Unknown;

    public DateOnly? BirthDate { get; set; }
    [MaxLength(200)] public string? BirthPlace { get; set; }

    public DateOnly? DeathDate { get; set; }
    [MaxLength(200)] public string? DeathPlace { get; set; }

    public ICollection<ParentChild> Parents { get; set; } = new List<ParentChild>();     // where this person is the child
    public ICollection<ParentChild> Children { get; set; } = new List<ParentChild>();    // where this person is the parent

    public ICollection<Marriage> MarriagesA { get; set; } = new List<Marriage>();
    public ICollection<Marriage> MarriagesB { get; set; } = new List<Marriage>();
}