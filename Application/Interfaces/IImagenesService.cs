using SketchMuse.Domain.DTOs;
using SketchMuse.Infrastructure.ExternalApis;

namespace SketchMuse.Application.Interfaces
{
    public interface IImagenesService
{
    Task<List<ImagenDTO>> PedirImagenes(string query, int count, int apiPrincipalOffset = 0, int apiFallbackOffset = 0);
}
}
