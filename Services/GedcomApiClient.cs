namespace Genealogy.Api.Services;
using Genealogy.GedcomService.Models;
using System.Net.Http.Json;

public class GedcomApiClient(HttpClient httpClient)
{
    // Method to call the import endpoint
    public async Task<GedcomDocument?> ImportGedcomAsync(IFormFile file)
    {
        // Use multipart form data to send the file
        using var content = new MultipartFormDataContent();
        using var fileStream = file.OpenReadStream();
        content.Add(new StreamContent(fileStream), "file", file.FileName);

        var response = await httpClient.PostAsync("/api/gedcom/import", content);

        response.EnsureSuccessStatusCode(); // Throws an exception if the call fails

        return await response.Content.ReadFromJsonAsync<GedcomDocument>();
    }

    // Method to call the export endpoint
    public async Task<Stream> ExportGedcomAsync(GedcomDocument document)
    {
        var response = await httpClient.PostAsJsonAsync("/api/gedcom/export", document);

        response.EnsureSuccessStatusCode();

        // Return the file stream from the response
        return await response.Content.ReadAsStreamAsync();
    }
}