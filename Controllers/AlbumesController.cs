using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SketchMuse.Application.Interfaces;
using SketchMuse.Infrastructure.Data;
using System.Security.Claims;
using static SketchMuse.Application.Interfaces.IAlbumesService;

namespace SketchMuse.Controllers
{
    [ApiController]
    [Route("api/albumes")]
    public class AlbumesController:ControllerBase
    {
                private readonly IAlbumesService _albumService;
        public AlbumesController(IAlbumesService albumService)
        {
            _albumService = albumService;
        }

        [HttpGet("user-albumes")]
        [Authorize]
        public async Task<IActionResult> GetMisAlbumes()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int usuarioId = int.Parse(userIdClaim);
            var albumes = await _albumService.GetAlbumesUsuario(usuarioId);
            return Ok(albumes);
        }

       [HttpGet("{albumId}/imagenes")]
[Authorize]
public async Task<IActionResult> GetImagenesAlbum(
    int albumId,
    [FromQuery] int count = 10,
    [FromQuery] bool soloNuevas = false)
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim == null) return Unauthorized();

    int usuarioId = int.Parse(userIdClaim);
    var imagenes = await _albumService.GetImagenesAlbum(albumId, usuarioId, count, soloNuevas);
    return Ok(imagenes);
}

        [HttpDelete("delete-album/{albumId}")]
        [Authorize]
        public async Task<IActionResult> EliminarAlbum(int albumId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int usuarioId = int.Parse(userIdClaim);
            await _albumService.EliminarAlbum(albumId, usuarioId);
            return Ok();
        }
    }
}
