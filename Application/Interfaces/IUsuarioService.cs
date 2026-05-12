using SketchMuse.Domain.DTOs;

namespace SketchMuse.Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<UsuarioResponseDTO?> Registro(string email, string password);
        Task<UsuarioResponseDTO?> Login(string email, string password);
        Task<List<UsuarioResponseDTO>> GetUsuarios();
        Task<bool> EliminarUsuario(int id);
    }
}
