using SketchMuse.Domain.DTOs;

namespace SketchMuse.Application.Interfaces
{
    public interface IImagenProvider
    {
        Task<List<ImagenDTO>> BuscarImagenes(string query, int count, int offset = 0);
    }
}
