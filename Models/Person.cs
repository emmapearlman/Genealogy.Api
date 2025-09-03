namespace Genealogy.Api.Models;

public enum Sex { Unknown, Male, Female, NonBinary }

public class Person
{
    public int Id { get; set; }
    public string GivenName { get; set; } = "";
    public string Surname { get; set; } = "";
    public Sex Sex { get; set; } = Sex.Unknown;
    public DateOnly? BirthDate { get; set; }
    public DateOnly? DeathDate { get; set; }
    public string? BirthPlace { get; set; }
    public string? DeathPlace { get; set; }

    // Biological parents
    public int? FatherId { get; set; }
    public int? MotherId { get; set; }

    public ICollection<Person> Parents { get; set; } = new List<Person>();
    public ICollection<Person> Children { get; set; } = new List<Person>();
}
