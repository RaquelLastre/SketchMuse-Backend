using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SketchMuse.Application.Interfaces;
using SketchMuse.Domain.DTOs;
using SketchMuse.Domain.Entities;
using SketchMuse.Infrastructure.Data;

namespace SketchMuse.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly MiDbcontext _context;
        private readonly IMapper _mapper;

        public UsuarioService(MiDbcontext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UsuarioResponseDTO?> Registro(string email, string password)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == email))
                return null;

            var usuario = new Usuario
            {
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return _mapper.Map<UsuarioResponseDTO>(usuario);
        }

        public async Task<UsuarioResponseDTO?> Login(string email, string password)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(password, usuario.Password)) return null;

            return _mapper.Map<UsuarioResponseDTO>(usuario);
        }

        public async Task<List<UsuarioResponseDTO>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<UsuarioResponseDTO>>(usuarios);
        }

        public async Task<bool> EliminarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
