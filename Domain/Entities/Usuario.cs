namespace SketchMuse.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Rol { get; set; } = "usuario";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; //no se va a usar, pero por si en un futuro se quiere ordenar por fecha de creacion
        public HashSet<Album> Albumes { get; set; } = new HashSet<Album>(); //HashSet es mas rapido para busquedas por id
    }
}
