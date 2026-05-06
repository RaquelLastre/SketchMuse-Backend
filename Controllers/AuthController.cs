using Microsoft.AspNetCore.Mvc;
using SketchMuse.Application.Interfaces;
namespace SketchMuse.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly JwtService _jwtService;

        public AuthController(IUsuarioService usuarioService, JwtService jwtService)
        {
            _usuarioService = usuarioService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Registrarse([FromBody] UsuarioDTO dto)
        {
            var user = await _usuarioService.Registro(dto.Email, dto.Password);
            if (user == null)
            {
                return BadRequest("El email ya está registrado.");
            }

            var token = _jwtService.GenerarToken(user);
            return Ok(new AuthDTO { Token = token, Email = user.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UsuarioDTO dto)
        {
            var user = await _usuarioService.Login(dto.Email, dto.Password);
            if (user == null)
            {
                return Unauthorized(new { error = "Email o contraseña incorrectos." });
            }

            var token = _jwtService.GenerarToken(user);
            return Ok(new AuthDTO { Token = token, Email = user.Email });
        }
    }
}
