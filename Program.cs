using Genealogy.Api.Data;
using Genealogy.Api.Models;
using Genealogy.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=genealogy.db"));

builder.Services.AddScoped<TreeService>();

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Add controllers
builder.Services.AddControllers();

// Add OpenAPI/Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register GedcomApiClient (ensure you have a suitable HttpClient registration)
builder.Services.AddHttpClient<GedcomApiClient>();

var app = builder.Build();

// Auto-migrate and seed minimal demo data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    if (!db.People.Any())
    {
        var a = new Person { GivenName = "Alice", Surname = "Reed", Gender = Gender.Female, BirthDate = new DateOnly(1950, 5, 12) };
        var b = new Person { GivenName = "Bob", Surname = "Reed", Gender = Gender.Male, BirthDate = new DateOnly(1948, 3, 9) };
        var c = new Person { GivenName = "Charlie", Surname = "Reed", Gender = Gender.Male, BirthDate = new DateOnly(1975, 7, 1) };
        db.People.AddRange(a, b, c);
        db.Marriages.Add(new Marriage { SpouseA = a, SpouseB = b, MarriageDate = new DateOnly(1970, 6, 1) });
        db.ParentChildren.Add(new ParentChild { Parent = a, Child = c });
        db.ParentChildren.Add(new ParentChild { Parent = b, Child = c });
        db.SaveChanges();
    }
}

// Enable Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();