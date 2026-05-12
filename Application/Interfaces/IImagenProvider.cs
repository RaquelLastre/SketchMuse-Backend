using SketchMuse.Domain.DTOs;

namespace SketchMuse.Application.Interfaces
{
    public interface IImagenProvider // Interfaz para APIs de imágenes (Pexels, Unsplash...)
    {
        Task<List<ImagenDTO>> BuscarImagenes(string query, int count, int offset = 0);
    }
}
