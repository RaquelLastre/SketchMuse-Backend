using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SketchMuse.Application.Interfaces;
using SketchMuse.Domain.DTOs;
using SketchMuse.Domain.Entities;
using SketchMuse.Infrastructure.Data;

namespace SketchMuse.Application.Services
{
    public class AlbumesService : IAlbumesService
    {
        private readonly MiDbcontext _context;
        private readonly IImagenesService _imagenesService;
        private readonly IMapper _mapper;

        public AlbumesService(MiDbcontext context, IImagenesService imagenesService, IMapper mapper)
        {
            _context = context;
            _imagenesService = imagenesService;
            _mapper = mapper;
        }

        public async Task CrearAlbum(string titulo, int usuarioId, List<ImagenDTO> imagenes)
        {
            var albumExistente = await _context.Albumes
                .Include(a => a.Imagenes)
                .FirstOrDefaultAsync(a => a.UsuarioId == usuarioId && a.Titulo.ToLower() == titulo.ToLower());

            if (albumExistente != null)
            {
                var existentes = albumExistente.Imagenes
                    .Select(i => $"{i.Source}:{i.ExternalId}")
                    .ToHashSet();

                var nuevasImagenes = imagenes
                    .Where(i => !existentes.Contains($"{i.Source}:{i.ExternalId}"))
                    .Select(i => new Imagen
                    {
                        Url = i.Url,
                        UrlSmall = i.UrlSmall ?? i.Url ?? "",
                        Titulo = i.Titulo,
                        ExternalId = i.ExternalId,
                        Source = i.Source,
                        AlbumId = albumExistente.Id
                    })
                    .ToList();

                if (nuevasImagenes.Any())
                    albumExistente.Imagenes.AddRange(nuevasImagenes);

                albumExistente.UsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return;
            }

            var album = new Album
            {
                Titulo = titulo,
                UsuarioId = usuarioId,
                UsedAt = DateTime.UtcNow,
                Imagenes = imagenes.Select(i => new Imagen
                {
                    Url = i.Url,
                    UrlSmall = i.UrlSmall ?? i.Url ?? "",
                    Titulo = i.Titulo,
                    ExternalId = i.ExternalId,
                    Source = i.Source
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

            int apiPrincipalOffset = album.Imagenes.Count(i => i.Source == "pexels");
            int apiFallbackOffset = album.Imagenes.Count(i => i.Source == "wikimedia");

            var imagenesNuevas = await _imagenesService.PedirImagenes(album.Titulo, count, apiPrincipalOffset, apiFallbackOffset);

            var existentes = album.Imagenes.Select(i => $"{i.Source}:{i.ExternalId}").ToHashSet();

            album.Imagenes.AddRange(
                imagenesNuevas
                    .Where(i => !existentes.Contains($"{i.Source}:{i.ExternalId}"))
                    .Select(i => new Imagen
                    {
                        Url = i.Url,
                        UrlSmall = i.UrlSmall ?? i.Url ?? "",
                        Titulo = i.Titulo,
                        ExternalId = i.ExternalId,
                        Source = i.Source,
                        AlbumId = album.Id
                    })
            );

            album.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<List<AlbumDTO>> GetAlbumesUsuario(int usuarioId)
        {
            var albumes = await _context.Albumes
                .Include(a => a.Imagenes)
                .Where(a => a.UsuarioId == usuarioId)
                .OrderByDescending(a => a.UsedAt)
                .ToListAsync();

            return _mapper.Map<List<AlbumDTO>>(albumes);
        }

        public async Task<List<ImagenDTO>> GetImagenesAlbum(int albumId, int usuarioId, int count, bool soloNuevas = false)
        {
            var album = await _context.Albumes
                .Include(a => a.Imagenes)
                .FirstOrDefaultAsync(a => a.Id == albumId && a.UsuarioId == usuarioId);

            if (album == null) throw new Exception("Álbum no encontrado.");

            int apiPrincipalOffset = album.Imagenes.Count(i => i.Source == "pexels");
            int apiFallbackOffset = album.Imagenes.Count(i => i.Source == "wikimedia");

            var existentes = album.Imagenes.Select(i => $"{i.Source}:{i.ExternalId}").ToHashSet();

            if (soloNuevas)
            {
                var nuevasFinales = new List<Imagen>();
                int intentos = 0;

                while (nuevasFinales.Count < count && intentos < 5)
                {
                    int pedirCantidad = (count - nuevasFinales.Count) * 2;
                    var nuevas = await _imagenesService.PedirImagenes(album.Titulo, pedirCantidad, apiPrincipalOffset, apiFallbackOffset);

                    var filtradas = nuevas
                        .Where(i => !existentes.Contains($"{i.Source}:{i.ExternalId}"))
                        .Select(i => new Imagen
                        {
                            Url = i.Url,
                            UrlSmall = i.UrlSmall ?? i.Url ?? "",
                            Titulo = i.Titulo,
                            ExternalId = i.ExternalId,
                            Source = i.Source,
                            AlbumId = album.Id
                        })
                        .ToList();

                    foreach (var img in filtradas)
                        existentes.Add($"{img.Source}:{img.ExternalId}");

                    nuevasFinales.AddRange(filtradas);
                    apiPrincipalOffset += filtradas.Count(i => i.Source == "pexels");
                    apiFallbackOffset += filtradas.Count(i => i.Source == "wikimedia");
                    intentos++;
                }

                var paraDevolver = nuevasFinales.Take(count).ToList();

                if (paraDevolver.Any())
                {
                    _context.Imagenes.AddRange(paraDevolver);
                    album.UsedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return _mapper.Map<List<ImagenDTO>>(paraDevolver);
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
                        var nuevas = await _imagenesService.PedirImagenes(album.Titulo, pedirCantidad, apiPrincipalOffset, apiFallbackOffset);

                        var filtradas = nuevas
                            .Where(i => !existentes.Contains($"{i.Source}:{i.ExternalId}"))
                            .Select(i => new Imagen
                            {
                                Url = i.Url,
                                UrlSmall = i.UrlSmall ?? i.Url ?? "",
                                Titulo = i.Titulo,
                                ExternalId = i.ExternalId,
                                Source = i.Source,
                                AlbumId = album.Id
                            })
                            .ToList();

                        foreach (var img in filtradas)
                            existentes.Add($"{img.Source}:{img.ExternalId}");

                        nuevasFinales.AddRange(filtradas);
                        apiPrincipalOffset += filtradas.Count(i => i.Source == "pexels");
                        apiFallbackOffset += filtradas.Count(i => i.Source == "wikimedia");
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
                    .OrderBy(i => i.Id)
                    .Take(count)
                    .ToListAsync();

                return _mapper.Map<List<ImagenDTO>>(imagenesActualizadas);
            }
        }

        public async Task EliminarAlbum(int albumId, int usuarioId)
        {
            var album = await _context.Albumes
                .Include(a => a.Imagenes)
                .FirstOrDefaultAsync(a => a.Id == albumId && a.UsuarioId == usuarioId);

            if (album == null)
                throw new Exception("Álbum no encontrado o no pertenece al usuario");

            _context.Imagenes.RemoveRange(album.Imagenes);
            _context.Albumes.Remove(album);
            await _context.SaveChangesAsync();
        }
    }
}
