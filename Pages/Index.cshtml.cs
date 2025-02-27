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
                SelectedFilm.CharactersByHomeworld = await FetchCharactersByHomeworldAsync(SelectedFilm.Characters);
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



    private async Task<Dictionary<string, List<string>>> FetchCharactersByHomeworldAsync(List<string> characterUrls)
    {
        var result = new Dictionary<string, List<string>>();

        foreach (var url in characterUrls)
        {
            var response = await _httpClient.GetStringAsync(url);
            var character = JsonConvert.DeserializeObject<dynamic>(response);

            var homeworldResponse = await _httpClient.GetStringAsync((string)character.homeworld);
            var homeworld = (string)JsonConvert.DeserializeObject<dynamic>(homeworldResponse).name; 

            if (!result.ContainsKey(homeworld))
            {
                result[homeworld] = new List<string>();
            }
            result[homeworld].Add((string)character.name);
        }

        return result;
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
    public List<string> Characters { get; set; } = new List<string>();
    public Dictionary<string, List<string>> CharactersByHomeworld { get; set; } = new Dictionary<string, List<string>>();
}



public class FilmResponse
{
    public List<Film> Results { get; set; }
}
