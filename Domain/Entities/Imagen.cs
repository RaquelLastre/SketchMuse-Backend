namespace SketchMuse.Domain.Entities
{
    public class Imagen
    {
        public int Id { get; set; }
        public string UrlSmall { get; set; }
        public string Url { get; set; }
        public string Titulo { get; set; }
        public int AlbumId { get; set; }
        public Album Album { get; set; } = null!;
    }
}
