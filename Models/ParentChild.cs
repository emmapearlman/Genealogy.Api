namespace Genealogy.Api.Models;

public class ParentChild
{
    public Guid ParentId { get; set; }
    public Person Parent { get; set; } = default!;

    public Guid ChildId { get; set; }
    public Person Child { get; set; } = default!;
}