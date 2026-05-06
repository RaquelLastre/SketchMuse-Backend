using System.Data;

namespace SketchMuse.Domain.Entities
{
    public class Album
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public DateTime UsedAt { get; set; } = DateTime.UtcNow; //para ordenarlos por fecha de uso/creacion/actualizacion
        public int UsuarioId { get; set; } 
        public Usuario Usuario { get; set; } = null!;
        public List<Imagen> Imagenes { get; set; } = new List<Imagen>();

    }
}
