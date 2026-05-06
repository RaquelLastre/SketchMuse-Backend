namespace SketchMuse.Domain.DTOs
{
    public class AlbumDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public DateTime UsedAt { get; set; }
        public List<string> PreviewImagenes { get; set; }
    }
}
