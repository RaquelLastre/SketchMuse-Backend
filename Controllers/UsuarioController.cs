using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SketchMuse.Application.Interfaces;
using System.Security.Claims;

namespace SketchMuse.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _usuarioService.GetUsuarios();
            var resultado = usuarios.Select(u => new
            {
                u.Id,
                u.Email,
                u.Rol,
                u.CreatedAt
            });
            return Ok(resultado);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var eliminado = await _usuarioService.EliminarUsuario(id);
            if (!eliminado) return NotFound("Usuario no encontrado.");
            return NoContent();
        }
    }
}
