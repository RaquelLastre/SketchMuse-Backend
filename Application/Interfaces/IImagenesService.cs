using SketchMuse.Domain.DTOs;
using SketchMuse.Infrastructure.ExternalApis;

namespace SketchMuse.Application.Interfaces
{
    public interface IImagenesService
{
    Task<List<ImagenDTO>> PedirImagenes(string query, int count, int offset = 0);
}
}
