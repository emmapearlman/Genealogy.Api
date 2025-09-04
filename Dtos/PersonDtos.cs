namespace Genealogy.Api.Dto;
public class PersonDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Sex { get; set; } = "Unknown";
    public DateOnly? BirthDate { get; set; }
    public DateOnly? DeathDate { get; set; }
    public string? BirthPlace { get; set; }
    public string? DeathPlace { get; set; }
}

public class TreeNodeDto
{
    public PersonDto Person { get; set; } = new();
    public List<PersonDto> Spouses { get; set; } = new();
    public List<TreeNodeDto> Parents { get; set; } = new();   // Upwards (size 0–2)
    public List<TreeNodeDto> Children { get; set; } = new();  // Downwards
}

public class TreeRequest
{
    public int Generations { get; set; } = 3;     // Depth in each direction
    public bool IncludeSpouses { get; set; } = true;
}

public record PersonDetailsDto(
    int Id,
    string GivenName,
    string Surname,
    Models.Sex Gender,
    List<RelativeDto> Parents,
    List<RelativeDto> Children,
    List<RelativeDto> Spouses
);

public record RelativeDto(
    int Id,
    string GivenName,
    string Surname
);
