using IngestaoMed.Models;

namespace IngestaoMed.Services
{
    public interface IAuthService
    {
        Task<bool> RegistrarCuidador(Cuidador cuidador);
        Task<Cuidador?> Login(string email);
        Task<bool> ExisteCuidadorCadastrado();
    }
}