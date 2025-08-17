namespace Genealogy.Api.Models;

public class Marriage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SpouseAId { get; set; }
    public Person SpouseA { get; set; } = default!;

    public Guid SpouseBId { get; set; }
    public Person SpouseB { get; set; } = default!;

    public DateOnly? MarriageDate { get; set; }
    public string? MarriagePlace { get; set; }
    public DateOnly? DivorceDate { get; set; }
}