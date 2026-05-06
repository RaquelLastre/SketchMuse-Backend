using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SketchMuse.Application.Interfaces;
using System.Security.Claims;
using static SketchMuse.Application.Interfaces.IAlbumesService;

namespace SketchMuse.Controllers
{
    [ApiController]
    [Route("api/imagenes")]
    public class ImagenesController : ControllerBase
    {
        private readonly IImagenesService _imageService;
        private readonly IAlbumesService _albumService;

        public ImagenesController(IImagenesService imageService, IAlbumesService albumService)
        {
            _imageService = imageService;
            _albumService = albumService;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int count = 10)
        {
            try //por si fallan ambas apis
            {
                var imagenes = await _imageService.PedirImagenes(query, count);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim != null)
                {
                    int usuarioId = int.Parse(userIdClaim);
                    await _albumService.CrearAlbum(query, usuarioId, imagenes);
                }

                return Ok(imagenes);
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    mensaje = "Tenemos problemas técnicos con las APIs externas",
                    detalles = ex.Message
                });
            }
        }

        [HttpPost("{albumId}/agregar")]
        [Authorize]
        public async Task<IActionResult> AgregarAlbum(int albumId, [FromQuery] int count = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            await _albumService.AgregarAlbum(albumId, int.Parse(userIdClaim), count);
            return Ok();
        }
    }
}