using Genealogy.Api.Data;
using Genealogy.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Genealogy.Api.Services;

public class GedcomExportService
{
    private readonly AppDbContext _db;

    public GedcomExportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> ExportToGedcomAsync()
    {
        var people = await _db.People
            .Include(p => p.Parents).ThenInclude(pc => pc.Parent)
            .Include(p => p.Children).ThenInclude(pc => pc.Child)
            .Include(p => p.MarriagesA).ThenInclude(m => m.SpouseB)
            .Include(p => p.MarriagesB).ThenInclude(m => m.SpouseA)
            .AsNoTracking()
            .ToListAsync();

        var gedcom = new StringBuilder();
        var personIdMap = new Dictionary<Guid, string>();

        // Header
        gedcom.AppendLine("0 HEAD");
        gedcom.AppendLine("1 GEDC");
        gedcom.AppendLine("2 VERS 5.5.1");
        gedcom.AppendLine("2 FORM LINEAGE-LINKED");
        gedcom.AppendLine("1 CHAR UTF-8");
        gedcom.AppendLine("1 SOUR Genealogy.Api");
        gedcom.AppendLine("2 NAME Genealogy API");
        gedcom.AppendLine("2 VERS 1.0");
        gedcom.AppendLine("1 DATE " + DateTime.Now.ToString("dd MMM yyyy"));
        gedcom.AppendLine("2 TIME " + DateTime.Now.ToString("HH:mm:ss"));
        gedcom.AppendLine("1 SUBM @SUBM@");
        gedcom.AppendLine("0 @SUBM@ SUBM");
        gedcom.AppendLine("1 NAME Genealogy API");
        gedcom.AppendLine();

        // Generate unique IDs for people
        int personCounter = 1;
        foreach (var person in people)
        {
            personIdMap[person.Id] = $"@I{personCounter:D4}@";
            personCounter++;
        }

        // Export people
        foreach (var person in people)
        {
            var personId = personIdMap[person.Id];
            gedcom.AppendLine($"0 {personId} INDI");
            
            // Name
            gedcom.AppendLine($"1 NAME {person.GivenName} /{person.Surname}/");
            gedcom.AppendLine("2 GIVN " + person.GivenName);
            gedcom.AppendLine("2 SURN " + person.Surname);
            
            // Gender
            var gender = person.Gender switch
            {
                Gender.Male => "M",
                Gender.Female => "F",
                Gender.NonBinary => "X",
                _ => "U"
            };
            gedcom.AppendLine($"1 SEX {gender}");
            
            // Birth
            if (person.BirthDate.HasValue)
            {
                gedcom.AppendLine($"1 BIRT");
                gedcom.AppendLine($"2 DATE {FormatGedcomDate(person.BirthDate.Value)}");
                if (!string.IsNullOrEmpty(person.BirthPlace))
                {
                    gedcom.AppendLine($"2 PLAC {person.BirthPlace}");
                }
            }
            
            // Death
            if (person.DeathDate.HasValue)
            {
                gedcom.AppendLine($"1 DEAT");
                gedcom.AppendLine($"2 DATE {FormatGedcomDate(person.DeathDate.Value)}");
                if (!string.IsNullOrEmpty(person.DeathPlace))
                {
                    gedcom.AppendLine($"2 PLAC {person.DeathPlace}");
                }
            }
            
            // Parents and Spouses will be handled in the family section
            gedcom.AppendLine();
            
            gedcom.AppendLine();
        }

        // Export families (marriages)
        var familyCounter = 1;
        var processedMarriages = new HashSet<Guid>();
        var familyIdMap = new Dictionary<Guid, string>();

        foreach (var person in people)
        {
            var marriages = person.MarriagesA.Concat(person.MarriagesB).ToList();
            
            foreach (var marriage in marriages)
            {
                if (processedMarriages.Contains(marriage.Id)) continue;
                processedMarriages.Add(marriage.Id);

                var spouseA = marriage.SpouseA;
                var spouseB = marriage.SpouseB;
                
                if (spouseA != null && spouseB != null && 
                    personIdMap.ContainsKey(spouseA.Id) && personIdMap.ContainsKey(spouseB.Id))
                {
                    var familyId = $"@F{familyCounter:D4}@";
                    familyIdMap[marriage.Id] = familyId;
                    
                    gedcom.AppendLine($"0 {familyId} FAM");
                    gedcom.AppendLine($"1 HUSB {personIdMap[spouseA.Id]}");
                    gedcom.AppendLine($"1 WIFE {personIdMap[spouseB.Id]}");
                    
                    // Marriage date
                    if (marriage.MarriageDate.HasValue)
                    {
                        gedcom.AppendLine($"1 MARR");
                        gedcom.AppendLine($"2 DATE {FormatGedcomDate(marriage.MarriageDate.Value)}");
                        if (!string.IsNullOrEmpty(marriage.MarriagePlace))
                        {
                            gedcom.AppendLine($"2 PLAC {marriage.MarriagePlace}");
                        }
                    }
                    
                    // Divorce date
                    if (marriage.DivorceDate.HasValue)
                    {
                        gedcom.AppendLine($"1 DIV");
                        gedcom.AppendLine($"2 DATE {FormatGedcomDate(marriage.DivorceDate.Value)}");
                    }
                    
                    // Children of this marriage
                    var children = spouseA.Children.Where(pc => pc.ChildId == spouseB.Id || 
                                                               spouseB.Children.Any(pc2 => pc2.ChildId == pc.ChildId))
                                                  .Select(pc => pc.Child)
                                                  .ToList();
                    
                    foreach (var child in children)
                    {
                        if (personIdMap.ContainsKey(child.Id))
                        {
                            gedcom.AppendLine($"1 CHIL {personIdMap[child.Id]}");
                        }
                    }
                    
                    gedcom.AppendLine();
                    familyCounter++;
                }
            }
        }

        // Export parent-child families
        var processedParentChild = new HashSet<(Guid, Guid)>();
        
        foreach (var person in people)
        {
            var parents = person.Parents.Select(pc => pc.Parent).ToList();
            if (parents.Count >= 2)
            {
                var parent1 = parents[0];
                var parent2 = parents[1];
                var key = (parent1.Id, parent2.Id);
                var reverseKey = (parent2.Id, parent1.Id);
                
                if (!processedParentChild.Contains(key) && !processedParentChild.Contains(reverseKey))
                {
                    processedParentChild.Add(key);
                    
                    var familyId = $"@F{familyCounter:D4}@";
                    gedcom.AppendLine($"0 {familyId} FAM");
                    gedcom.AppendLine($"1 HUSB {personIdMap[parent1.Id]}");
                    gedcom.AppendLine($"1 WIFE {personIdMap[parent2.Id]}");
                    gedcom.AppendLine($"1 CHIL {personIdMap[person.Id]}");
                    gedcom.AppendLine();
                    familyCounter++;
                }
            }
        }

        // Trailer
        gedcom.AppendLine("0 TRLR");

        return gedcom.ToString();
    }

    private static string FormatGedcomDate(DateOnly date)
    {
        // GEDCOM date format: DD MMM YYYY
        return date.ToString("dd MMM yyyy").ToUpper();
    }
}
