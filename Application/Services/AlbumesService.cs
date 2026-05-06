using Microsoft.EntityFrameworkCore;
using SketchMuse.Domain.DTOs;
using SketchMuse.Domain.Entities;
using SketchMuse.Infrastructure.Data;

namespace SketchMuse.Application.Interfaces
{
    public class AlbumesService : IAlbumesService
    {
        private readonly MiDbcontext _context;
        private readonly IImagenesService _imagenesService;

        public AlbumesService(MiDbcontext context, IImagenesService imagenesService)
        {
            _context = context;
            _imagenesService = imagenesService;
        }

        public async Task CrearAlbum(string titulo, int usuarioId, List<ImagenDTO> imagenes)
        {
            var album = new Album
            {
                Titulo = titulo,
                UsuarioId = usuarioId,
                UsedAt = DateTime.UtcNow,
                Imagenes = imagenes.Select(i => new Imagen
                {
                    Url = i.Url,
                    UrlSmall = i.UrlSmall,
                    Titulo = i.Titulo
                }).ToList()
            };

            _context.Albumes.Add(album);
            await _context.SaveChangesAsync();
        }

        public async Task AgregarAlbum(int albumId, int usuarioId, int count = 10)
        {
            var album = await _context.Albumes
                .Include(a => a.Imagenes)
                .FirstOrDefaultAsync(a => a.Id == albumId && a.UsuarioId == usuarioId);

            if (album == null) throw new Exception("Álbum no encontrado.");

            var imagenesNuevas = await _imagenesService.PedirImagenes(album.Titulo, count);

            // HashSet de URLs existentes para filtrar duplicados
            var urlsExistentes = album.Imagenes.Select(i => i.Url).ToHashSet();

            album.Imagenes.AddRange(
                imagenesNuevas
                    .Where(i => !urlsExistentes.Contains(i.Url))
                    .Select(i => new Imagen { Url = i.Url,UrlSmall = i.UrlSmall ?? i.Url, Titulo = i.Titulo, AlbumId = album.Id })
            );

            album.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<List<AlbumDTO>> GetAlbumesUsuario(int usuarioId)
        {
            var albumes = await _context.Albumes
                .Include(a => a.Imagenes)
                .Where(a => a.UsuarioId == usuarioId)
                .ToListAsync();

            return albumes
                .OrderByDescending(a => a.UsedAt)
                .Select(a => new AlbumDTO
                {
                    Id = a.Id,
                    Titulo = a.Titulo,
                    UsedAt = a.UsedAt,
                    PreviewImagenes = a.Imagenes
                        .Take(3)
                        .Select(i => i.UrlSmall?? i.Url)
                        .ToList()
                })
                .ToList();
        }

public async Task<List<ImagenDTO>> GetImagenesAlbum(int albumId, int usuarioId, int count, bool soloNuevas = false)
{
    var album = await _context.Albumes
        .Include(a => a.Imagenes)
        .FirstOrDefaultAsync(a => a.Id == albumId && a.UsuarioId == usuarioId);

    if (album == null)
        throw new Exception("Álbum no encontrado.");

    var urlsExistentes = album.Imagenes.Select(i => i.Url).ToHashSet();

    if (soloNuevas)
    {
        var nuevasFinales = new List<Imagen>();
        int intentos = 0;
        int offset = album.Imagenes.Count; 

        while (nuevasFinales.Count < count && intentos < 5)
        {
            int pedirCantidad = (count - nuevasFinales.Count) * 2;
            var nuevas = await _imagenesService.PedirImagenes(album.Titulo, pedirCantidad, offset);

            var filtradas = nuevas
                .Where(i => !urlsExistentes.Contains(i.Url))
                .Select(i => new Imagen
                {
                    Url = i.Url,
                    UrlSmall = i.UrlSmall ?? i.Url,
                    Titulo = i.Titulo,
                    AlbumId = album.Id
                })
                .ToList();

            foreach (var img in filtradas)
                urlsExistentes.Add(img.Url);

            nuevasFinales.AddRange(filtradas);
            offset += pedirCantidad;
            intentos++;
        }

        var paraDevolver = nuevasFinales.Take(count).ToList();

        if (paraDevolver.Any())
        {
            _context.Imagenes.AddRange(paraDevolver);
            await _context.SaveChangesAsync();
        }

        return paraDevolver
            .Select(i => new ImagenDTO { Url = i.Url, UrlSmall = i.UrlSmall, Titulo = i.Titulo })
            .ToList();
    }
    else
    {
        int faltan = count - album.Imagenes.Count;

        if (faltan > 0)
        {
            var nuevasFinales = new List<Imagen>();
            int intentos = 0;

            while (nuevasFinales.Count < faltan && intentos < 5)
            {
                int pedirCantidad = (faltan - nuevasFinales.Count) * 2;
                var nuevas = await _imagenesService.PedirImagenes(album.Titulo, pedirCantidad);

                var filtradas = nuevas
                    .Where(i => !urlsExistentes.Contains(i.Url))
                    .Select(i => new Imagen
                    {
                        Url = i.Url,
                        UrlSmall = i.UrlSmall ?? i.Url,
                        Titulo = i.Titulo,
                        AlbumId = album.Id
                    })
                    .ToList();

                foreach (var img in filtradas)
                    urlsExistentes.Add(img.Url);

                nuevasFinales.AddRange(filtradas);
                intentos++;
            }

            if (nuevasFinales.Any())
            {
                _context.Imagenes.AddRange(nuevasFinales);
                await _context.SaveChangesAsync();
            }
        }

        var imagenesActualizadas = await _context.Imagenes
            .Where(i => i.AlbumId == albumId)
            .Take(count)
            .ToListAsync();

        return imagenesActualizadas
            .Select(i => new ImagenDTO { Url = i.Url, UrlSmall = i.UrlSmall, Titulo = i.Titulo })
            .ToList();
    }
}

        public async Task EliminarAlbum(int albumId, int usuarioId)
        {
            var album = await _context.Albumes.Include(a => a.Imagenes).FirstOrDefaultAsync(a => a.Id == albumId && a.UsuarioId == usuarioId);

            if (album == null){
                 throw new Exception("Álbum no encontrado o no pertenece al usuario");
            }
           
            _context.Imagenes.RemoveRange(album.Imagenes); //eliminar las imagenes de la bd 
            _context.Albumes.Remove(album);
            await _context.SaveChangesAsync();
        }
    }
}
