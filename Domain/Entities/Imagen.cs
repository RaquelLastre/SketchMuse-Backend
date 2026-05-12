namespace SketchMuse.Domain.Entities
{
    public class Imagen
    {
        public int Id { get; set; }
        public string ExternalId { get; set; } = ""; //por si en dos apis distintas se usa el mismo id para dos imagenes distintas
        public string Source { get; set; } = "";
        public string UrlSmall { get; set; } = "";
        public string Url { get; set; } = "";
        public string Titulo { get; set; } = "";
        public int AlbumId { get; set; }
        public Album Album { get; set; } = null!;
    }
}
