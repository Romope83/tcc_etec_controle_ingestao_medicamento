using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Services
{
    public interface IAuthService
    {
        Task<bool> RegistrarCuidador(Cuidador cuidador, string senhaLimpa);
        Task<bool> ExisteCuidadorCadastrado();
        Task<Cuidador?> GetCuidadorAtual();
        Task<bool> ValidarEmail(string email);
        Task<bool> ValidarLogin(string email, string senhaLimpa);

    }
}