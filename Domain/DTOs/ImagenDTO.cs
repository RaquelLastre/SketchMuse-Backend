namespace SketchMuse.Domain.DTOs
{
    public class ImagenDTO
    {
        public string Url { get; set; } 
        public string UrlSmall { get; set; }
        public string Titulo { get; set; }
        public string ExternalId { get; set; } = "";
        public string Source { get; set; } = "";
    }
}