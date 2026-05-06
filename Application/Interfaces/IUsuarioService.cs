using SketchMuse.Domain.Entities;

namespace SketchMuse.Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<Usuario?> Registro(string email, string password);
        Task<Usuario?> Login(string email, string password);
    }
}
