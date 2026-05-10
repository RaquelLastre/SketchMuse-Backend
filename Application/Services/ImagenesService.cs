using SketchMuse.Application.Interfaces;
using SketchMuse.Domain.DTOs;
using System.Web;

namespace SketchMuse.Application.Services
{
    public class ImagenesService : IImagenesService
    {
        private readonly IImagenProvider _proveedorPrincipal;
        private readonly IImagenProvider _proveedorFallback;

        public ImagenesService(IImagenProvider proveedorPrincipal, IImagenProvider proveedorFallback)
        {
            _proveedorPrincipal = proveedorPrincipal;
            _proveedorFallback = proveedorFallback;
        }

        public async Task<List<ImagenDTO>> PedirImagenes(string query, int count, int apiPrincipalOffset = 0, int apiFallbackOffset = 0)
        {
            List<ImagenDTO> imagenes;

            try
            {
                imagenes = await _proveedorPrincipal.BuscarImagenes(query, count, apiPrincipalOffset);
                if (imagenes == null || imagenes.Count == 0)
                    throw new Exception("Proveedor principal sin resultados");
            }
            catch
            {
                imagenes = await _proveedorFallback.BuscarImagenes(query, count, apiFallbackOffset);
            }

            if (imagenes == null || imagenes.Count == 0)
                throw new Exception("No se pudieron obtener imágenes de ningún servicio externo.");

            return imagenes.Select(i => new ImagenDTO
            {
                Titulo = i.Titulo,
                Url = HttpUtility.UrlDecode(i.Url),
                UrlSmall = HttpUtility.UrlDecode(i.UrlSmall),
                ExternalId = i.ExternalId,
                Source = i.Source
            }).ToList();
        }
    }
}
