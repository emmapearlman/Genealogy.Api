namespace Genealogy.Api.Models;

public enum Sex { Unknown = 0, Male = 1, Female = 2 }

public class Person
{
    public int Id { get; set; }
    public string GivenName { get; set; } = "";
    public string Surname { get; set; } = "";
    public Sex Sex { get; set; } = Sex.Unknown;
    public DateOnly? BirthDate { get; set; }
    public DateOnly? DeathDate { get; set; }

    // Biological parents
    public int? FatherId { get; set; }
    public int? MotherId { get; set; }
}
