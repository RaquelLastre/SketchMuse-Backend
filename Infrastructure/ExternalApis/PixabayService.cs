using SketchMuse.Domain.DTOs;
using System.Text.Json;

namespace SketchMuse.Infrastructure.ExternalApis
{
    public class PixabayService
    {
        private readonly HttpClient _httpClient;
        //para acceder valoresde la config como appsettings.json
        private readonly IConfiguration _config;

        public PixabayService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        //async Task indica que es asincrono y el hilo no se queda bloqueado si la respuesta tarda unos segundos, puede seguir procesando peticiones
        public async Task<List<ImagenDTO>> LlamadaApiPixabay(string textoBusqueda, int numImagenes, int offset = 0)
        {
            var apiKey = _config["PixabayApi:ApiKey"];
    string busquedaSinEspacios = Uri.EscapeDataString(textoBusqueda);
    int page = (offset / numImagenes) + 1;
    var url = $"https://pixabay.com/api/?key={apiKey}&q={busquedaSinEspacios}&per_page={numImagenes}&page={page}";

            var response = await _httpClient.GetAsync(url);
            //comprueba que la respuesta sea 200-299 y si no lo es lanza una excepción
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty("hits", out JsonElement listaImagenes))
            {
                Console.WriteLine("No se encontraron resultados: " + json);
                return new List<ImagenDTO>();
            }

            var imagenes = new List<ImagenDTO>();
            foreach (var img in listaImagenes.EnumerateArray())
            {

                //intenta obtener la propiedad link del elemento json : img, si hay es true y pone el valor en url, si no, devuelve false
                if(img.TryGetProperty("webformatURL", out JsonElement enlace))
                {
                    imagenes.Add(new ImagenDTO
                    {
                        //url es un elemento json, asi que se convierte a string. Si por alguna razon no devuelve un string, no lanza excepción
                        Url = enlace.GetString() ?? "",
                        UrlSmall = img.TryGetProperty("previewURL", out JsonElement urlSmall) ? urlSmall.GetString() : "",
                        Titulo = img.TryGetProperty("tags", out JsonElement titulo) ? titulo.GetString() : ""
                    });
                }
            }
            return imagenes;
        }
    }
}
