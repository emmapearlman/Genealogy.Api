using Genealogy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Genealogy.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Person> People => Set<Person>();
    public DbSet<ParentChild> ParentChildren => Set<ParentChild>();
    public DbSet<Marriage> Marriages => Set<Marriage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Person
        b.Entity<Person>()
            .HasIndex(p => new { p.Surname, p.GivenName });

        // ParentChild (composite key)
        b.Entity<ParentChild>()
            .HasKey(pc => new { pc.ParentId, pc.ChildId });

        b.Entity<ParentChild>()
            .HasOne(pc => pc.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(pc => pc.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<ParentChild>()
            .HasOne(pc => pc.Child)
            .WithMany(p => p.Parents)
            .HasForeignKey(pc => pc.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        // Marriage (order-independent uniqueness)
        b.Entity<Marriage>()
            .HasOne(m => m.SpouseA).WithMany(p => p.MarriagesA)
            .HasForeignKey(m => m.SpouseAId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Marriage>()
            .HasOne(m => m.SpouseB).WithMany(p => p.MarriagesB)
            .HasForeignKey(m => m.SpouseBId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Marriage>()
            .HasIndex(m => new { m.SpouseAId, m.SpouseBId }).IsUnique();

        // Ensure spouses aren't identical
        b.Entity<Marriage>()
            .HasCheckConstraint("CK_Marriage_DistinctSpouses", "SpouseAId <> SpouseBId");
    }
}