using Genealogy.Api.Services;
using Genealogy.GedcomService.Models;
using Microsoft.AspNetCore.Mvc;

namespace Genealogy.Api.Controllers;

[ApiController]
[Route("api/client")]
public class ClientApiController : ControllerBase
{
    private readonly GedcomApiClient _client;

    public ClientApiController(GedcomApiClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Forwards a GEDCOM file to the processing service.
    /// </summary>
    [HttpPost("Import")]
    public async Task<IActionResult> ProcessGedcom([FromForm] FormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        try
        {
            var processedDocument = await _client.ImportGedcomAsync(file);
            return Ok(processedDocument);
        }
        catch (HttpRequestException ex)
        {
            return Problem($"Error calling the GEDCOM service: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a sample document and gets the exported file from the service.
    /// </summary>
    [HttpPost("Export")]
    public async Task<IActionResult> DownloadSimpleGedcom()
    {
        try
        {
            // Create some sample data
            var sampleDoc = new GedcomDocument
            {
                Individuals =
                [
                    new() { Id = "@I1@", Name = "John Smith", Sex = "M", BirthDate = "01 JAN 1970" },
                    new() { Id = "@I2@", Name = "Jane Doe", Sex = "F", BirthDate = "01 JAN 1975" }
                ],
                Families =
                [
                    new() { Id = "@F1@", HusbandId = "@I1@", WifeId = "@I2@", MarriageDate = "15 JUN 1995" }
                ]
            };

            var fileStream = await _client.ExportGedcomAsync(sampleDoc);

            // Stream the file back to the original caller
            return File(fileStream, "application/octet-stream", "downloaded.ged");
        }
        catch (HttpRequestException ex)
        {
            return Problem($"Error calling the GEDCOM service: {ex.Message}");
        }
    }
}