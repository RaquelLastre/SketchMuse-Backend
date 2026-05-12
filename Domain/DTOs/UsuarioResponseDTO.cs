namespace SketchMuse.Domain.DTOs
{
    public class UsuarioResponseDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
