using Genealogy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Genealogy.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Person> People => Set<Person>();
    public DbSet<Marriage> Marriages => Set<Marriage>();
    public DbSet<ParentChild> ParentChildren => Set<ParentChild>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>()
            .HasMany(p => p.Parents)
            .WithMany(p => p.Children)
            .UsingEntity<Dictionary<string, object>>(
                "ParentChild",
                j => j
                    .HasOne<Person>()
                    .WithMany()
                    .HasForeignKey("ParentId")
                    .OnDelete(DeleteBehavior.Restrict),
                j => j
                    .HasOne<Person>()
                    .WithMany()
                    .HasForeignKey("ChildId")
                    .OnDelete(DeleteBehavior.Restrict)
            );
    }
}

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.People.Any()) return;

        var people = new[]
        {
            new Person { Id = 1, GivenName = "John",  Surname = "Smith", Sex = Sex.Male,   BirthDate = new DateOnly(1950,1,10) },
            new Person { Id = 2, GivenName = "Mary",  Surname = "Johnson", Sex = Sex.Female, BirthDate = new DateOnly(1952,6,2) },
            new Person { Id = 3, GivenName = "Anna",  Surname = "Smith", Sex = Sex.Female, BirthDate = new DateOnly(1975,5,14), FatherId = 1, MotherId = 2 },
            new Person { Id = 4, GivenName = "Ben",   Surname = "Smith", Sex = Sex.Male,   BirthDate = new DateOnly(1978,9,3),  FatherId = 1, MotherId = 2 },
            new Person { Id = 5, GivenName = "Robert",Surname = "Smith", Sex = Sex.Male,   BirthDate = new DateOnly(1925,2,18) },
            new Person { Id = 6, GivenName = "Helen", Surname = "Brown", Sex = Sex.Female, BirthDate = new DateOnly(1927,11,7) },
        };

        // Link John's parents
        people.Single(p => p.Id == 1).FatherId = 5;
        people.Single(p => p.Id == 1).MotherId = 6;

        db.People.AddRange(people);
        db.Marriages.Add(new Marriage
        {
            Id = 1,
            Spouse1Id = 1,
            Spouse2Id = 2,
            MarriageDate = new DateOnly(1974, 7, 1)
        });

        db.SaveChanges();
    }
}

