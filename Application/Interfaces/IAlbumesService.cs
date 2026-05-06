using SketchMuse.Domain.DTOs;

namespace SketchMuse.Application.Interfaces
{
    public interface IAlbumesService
    {
            Task CrearAlbum(string titulo, int usuarioId, List<ImagenDTO> imagenes);
            Task AgregarAlbum(int albumId, int usuarioId, int count = 10);
            Task<List<AlbumDTO>> GetAlbumesUsuario(int usuarioId);
            Task<List<ImagenDTO>> GetImagenesAlbum(int albumId, int usuarioId, int count, bool soloNuevas = false);
            Task EliminarAlbum(int albumId, int usuarioId);
    }
}
