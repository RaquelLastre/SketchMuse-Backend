using SketchMuse.Application.Interfaces;
using SketchMuse.Domain.DTOs;
using System.Text.Json;

namespace SketchMuse.Infrastructure.ExternalApis
{
    public class PexelsService : IImagenProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public PexelsService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<ImagenDTO>> BuscarImagenes(string query, int count, int offset = 0)
        {
            var apiKey = _config["PexelsApi:ApiKey"];
            string queryCodificado = Uri.EscapeDataString(query);
            int page = (offset / count) + 1;
            var url = $"https://api.pexels.com/v1/search?query={queryCodificado}&per_page={count}&page={page}";

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", apiKey);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty("photos", out JsonElement listaFotos))
            {
                Console.WriteLine("Pexels: no se encontraron resultados: " + json);
                return new List<ImagenDTO>();
            }

            var imagenes = new List<ImagenDTO>();
            foreach (var foto in listaFotos.EnumerateArray())
            {
                if (foto.TryGetProperty("src", out JsonElement src))
                {
                    imagenes.Add(new ImagenDTO
                    {
                        Url = src.TryGetProperty("large", out JsonElement urlLarge) ? urlLarge.GetString() ?? "" : "",
                        UrlSmall = src.TryGetProperty("medium", out JsonElement urlMedium) ? urlMedium.GetString() ?? "" : "",
                        Titulo = foto.TryGetProperty("alt", out JsonElement alt) ? alt.GetString() ?? "" : "",
                        ExternalId = foto.GetProperty("id").GetInt64().ToString(),
                        Source = "pexels"
                    });
                }
            }

            return imagenes;
        }
    }
}
