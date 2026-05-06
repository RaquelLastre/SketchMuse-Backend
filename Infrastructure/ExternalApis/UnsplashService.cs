using SketchMuse.Domain.DTOs;
using System.Text.Json;

namespace SketchMuse.Infrastructure.ExternalApis
{
    public class UnsplashService
    {
         private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public UnsplashService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<ImagenDTO>> LlamadaApiUnsplash(string textoBusqueda, int numImagenes, int offset = 0)
        {
            var apiKey = _config["UnsplashApi:ApiKey"];
    string busquedaSinEspacios = Uri.EscapeDataString(textoBusqueda);
    int page = (offset / numImagenes) + 1; // calcular página según offset
    var url = $"https://api.unsplash.com/search/photos?query={busquedaSinEspacios}&per_page={numImagenes}&page={page}&client_id={apiKey}";

            var response = await _httpClient.GetAsync(url);
            //comprueba que la respuesta sea 200-299 y si no lo es lanza una excepción
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty("results", out JsonElement listaImagenes))
            {
                Console.WriteLine("No se encontraron resultados: " + json);
                return new List<ImagenDTO>();
            }

            var imagenes = new List<ImagenDTO>();

            foreach (var img in listaImagenes.EnumerateArray())
            {
               if (img.GetProperty("urls").TryGetProperty("raw", out JsonElement enlace))
                {
                    imagenes.Add(new ImagenDTO
                    {
                        //url es un elemento json, asi que se convierte a string. Si por alguna razon no devuelve un string, no lanza excepción
                        Url = enlace.GetString() ?? "",
                        UrlSmall = img.TryGetProperty("small", out JsonElement urlSmall) ? urlSmall.GetString() : "",
                        Titulo = img.TryGetProperty("alt_description", out JsonElement titulo) ? titulo.GetString() : ""
                    });
                }
            }

            return imagenes;
        }
    }
}
