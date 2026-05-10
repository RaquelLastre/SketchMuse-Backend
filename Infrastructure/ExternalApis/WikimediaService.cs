using SketchMuse.Application.Interfaces;
using SketchMuse.Domain.DTOs;
using System.Text.Json;

namespace SketchMuse.Infrastructure.ExternalApis
{
    public class WikimediaService : IImagenProvider
    {
        private readonly HttpClient _httpClient;

        public WikimediaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ImagenDTO>> BuscarImagenes(string query, int count, int offset = 0)
        {
            string queryCodificado = Uri.EscapeDataString(query);
            var url = $"https://commons.wikimedia.org/w/api.php?action=query&generator=search&gsrsearch={queryCodificado}&gsrnamespace=6&gsrlimit={count}&gsroffset={offset}&prop=imageinfo&iiprop=url|thumburl&iiurlwidth=400&format=json";

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "SketchMuse/1.0");

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty("query", out JsonElement queryResult) ||
                !queryResult.TryGetProperty("pages", out JsonElement pages))
            {
                Console.WriteLine("Wikimedia: no se encontraron resultados: " + json);
                return new List<ImagenDTO>();
            }

            var imagenes = new List<ImagenDTO>();
            foreach (var page in pages.EnumerateObject())
            {
                if (!page.Value.TryGetProperty("imageinfo", out JsonElement imageinfo)) continue;

                var info = imageinfo.EnumerateArray().FirstOrDefault();
                if (info.ValueKind == JsonValueKind.Undefined) continue;

                imagenes.Add(new ImagenDTO
                {
                    Url = info.TryGetProperty("url", out JsonElement url2) ? url2.GetString() ?? "" : "",
                    UrlSmall = info.TryGetProperty("thumburl", out JsonElement thumb) ? thumb.GetString() ?? "" : "",
                    Titulo = page.Value.TryGetProperty("title", out JsonElement titulo)
                        ? titulo.GetString()?.Replace("File:", "").Replace("_", " ") ?? ""
                        : "",
                    ExternalId = page.Value.GetProperty("pageid").GetInt32().ToString(),
                    Source = "wikimedia"
                });
            }

            return imagenes;
        }
    }
}
