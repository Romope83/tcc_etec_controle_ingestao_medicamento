using IngestaoMed.Core.Data;
using IngestaoMed.Core.Models;
using BC = BCrypt.Net.BCrypt;

namespace IngestaoMed.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDatabaseContext _db;

        public AuthService(IDatabaseContext db)
        {
            _db = db;
        }

        public async Task<bool> RegistrarCuidador(Cuidador cuidador, string senhaLimpa)
        {
            cuidador.PasswordHash = BC.HashPassword(senhaLimpa);
            return await _db.InserirAsync(cuidador);
        }

        public async Task<bool> ValidarLogin(string email, string senhaLimpa)
        {
            var usuario = await _db.BuscarPrimeiroAsync<Cuidador>(c =>
                c.Email.ToLower() == email.ToLower());

            if (usuario == null) return false;

            return BC.Verify(senhaLimpa, usuario.PasswordHash);
        }

        public async Task<bool> ExisteCuidadorCadastrado()
        {
            var total = await _db.ContarAsync<Cuidador>();
            return total > 0;
        }

        public async Task<Cuidador?> GetCuidadorAtual()
        {
            return await _db.BuscarPrimeiroAsync<Cuidador>();
        }

        public async Task<bool> ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var usuario = await _db.BuscarPrimeiroAsync<Cuidador>(c =>
                c.Email.ToLower() == email.ToLower());

            return usuario != null;
        }
    }
}