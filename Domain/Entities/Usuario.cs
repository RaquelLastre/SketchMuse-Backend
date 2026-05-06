namespace SketchMuse.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public HashSet<Album> Albumes { get; set; } = new HashSet<Album>();
    }
}
