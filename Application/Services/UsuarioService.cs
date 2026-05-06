using Microsoft.EntityFrameworkCore;
using SketchMuse.Domain.Entities;
using SketchMuse.Infrastructure.Data;

namespace SketchMuse.Application.Interfaces
{
    public class UsuarioService : IUsuarioService
    {
        private readonly MiDbcontext _context;

        public UsuarioService(MiDbcontext context)
        {
            _context = context;
        }

        public async Task<Usuario?> Registro(string email, string password)
        {
            // Comprobar si el email ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Email == email))
            {
                return null;
            }
                
            var usuario = new Usuario
            {
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password) // Hash de la contraseña
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario?> Login(string email, string password)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return null;
            }

            bool passwordCorrecta = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!passwordCorrecta)
            {
                return null;
            }

            return user;
        }
    }
}
