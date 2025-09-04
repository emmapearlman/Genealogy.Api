namespace Genealogy.Api.Models;

public class Marriage
{
    public int Id { get; set; }
    public int Spouse1Id { get; set; }
    public int Spouse2Id { get; set; }
    public DateOnly? MarriageDate { get; set; }
    public DateOnly? DivorceDate { get; set; }
    public Person Spouse1 { get; set; } = null!;
    public Person Spouse2 { get; set; } = null!;
}
