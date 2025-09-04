// Program.cs
using Genealogy.Api.Data;
using Genealogy.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // Safer for recursive DTOs
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        o.JsonSerializerOptions.MaxDepth = 64; // Optional: increase if needed
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// In-memory demo DB; swap for a real provider as needed
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("GenealogyDb"));
builder.Services.AddScoped<IFamilyTreeService, FamilyTreeService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

// Seed demo data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(db);
}

app.Run();