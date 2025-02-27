using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

public class IndexModel : PageModel
{
    private readonly HttpClient _httpClient;
    public List<Film> Films { get; set; }
    public Film SelectedFilm { get; set; }
    public string SortOrder { get; set; }

    public IndexModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task OnGetAsync(string sortOrder, int? episodeId)
    {
        SortOrder = sortOrder;
        var response = await _httpClient.GetStringAsync("https://swapi.dev/api/films/");
        var films = JsonConvert.DeserializeObject<FilmResponse>(response)?.Results ?? new List<Film>();

        Films = sortOrder switch
        {
            "EpisodeId" => films.OrderBy(f => f.EpisodeId).ToList(),
            "ReleaseDate" => films.OrderBy(f => f.ReleaseDate).ToList(),
            _ => films
        };

        if (episodeId.HasValue)
        {
            SelectedFilm = Films.FirstOrDefault(f => f.EpisodeId == episodeId);
            if (SelectedFilm != null)
            {
                SelectedFilm.Planets = await FetchNamesAsync(SelectedFilm.Planets);
            }
        }
    }

    private async Task<List<string>> FetchNamesAsync(List<string> urls)
    {
        var names = new List<string>();
        foreach (var url in urls)
        {
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<dynamic>(response);
            names.Add((string)data.name);
        }
        return names;
    }
}

public class Film
{
    [JsonProperty("episode_id")]
    public int EpisodeId { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("release_date")]
    public string ReleaseDate { get; set; }

    [JsonProperty("opening_crawl")]
    public string OpeningCrawl { get; set; }

    public List<string> Planets { get; set; } = new List<string>();
}

public class FilmResponse
{
    public List<Film> Results { get; set; }
}