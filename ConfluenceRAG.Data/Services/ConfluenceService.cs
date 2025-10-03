using System.Net;
using System.Text.Json;
using ConfluenceRAG.Data.Models.Api;
using ConfluenceRAG.Data.Models.Dto;

namespace ConfluenceRAG.Data.Services;

public class ConfluenceService(string confluenceOrg)
{
    private readonly HttpClient _httpClient = new();
    private readonly string _rootUrl = $"https://{confluenceOrg}.atlassian.net/wiki";

    public async Task<List<ConfluencePageDto>> GetPages()
    {
        var pages = new List<ConfluencePageDto>();

        // Fetch all spaces (recursively handles pagination)
        var spaces = await GetSpacesAsync($"{_rootUrl}/rest/api/space?limit=100&start=0");

        foreach (var space in spaces)
        {
            try
            {
                await GetPagesForSpaceAsync(
                    $"{_rootUrl}/rest/api/space/{Uri.EscapeDataString(space.Key)}/content/page?limit=100&start=0&expand=body.storage",
                    space.Key,
                    pages
                );
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.Error.WriteLine($"[WARN] 404 for space '{space.Key}', skipping.");
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == HttpStatusCode.Forbidden
                    || ex.StatusCode == HttpStatusCode.Unauthorized
                )
            {
                Console.Error.WriteLine(
                    $"[WARN] Space '{space.Key}' not public ({(int?)ex.StatusCode}), skipping."
                );
            }
        }

        return pages;
    }

    private async Task<List<ConfluenceSpace.Space>> GetSpacesAsync(string url)
    {
        var all = new List<ConfluenceSpace.Space>();
        var json = await _httpClient.GetStringAsync(url);
        var root = JsonSerializer.Deserialize<ConfluenceSpace.Root>(json);
        if (root?.Results != null)
        {
            all.AddRange(root.Results);
        }

        if (!string.IsNullOrWhiteSpace(root?.Links?.Next))
        {
            var nextUrl = _rootUrl + root.Links.Next;
            all.AddRange(await GetSpacesAsync(nextUrl));
        }

        return all;
    }

    private async Task GetPagesForSpaceAsync(
        string url,
        string spaceKey,
        List<ConfluencePageDto> pages
    )
    {
        var json = await _httpClient.GetStringAsync(url);
        var root = JsonSerializer.Deserialize<ConfluencePage.Root>(json);

        if (root?.Results != null)
        {
            foreach (var page in root.Results)
            {
                pages.Add(
                    new ConfluencePageDto(
                        Title: page.Title,
                        Url: $"{_rootUrl}{page.Links.Webui}",
                        Body: Utils.CleanHtml(page.Body?.Storage?.Value),
                        Space: spaceKey
                    )
                );
            }
        }

        if (!string.IsNullOrWhiteSpace(root?.Links?.Next))
        {
            var nextUrl = _rootUrl + root.Links.Next;
            await GetPagesForSpaceAsync(nextUrl, spaceKey, pages);
        }
    }
}