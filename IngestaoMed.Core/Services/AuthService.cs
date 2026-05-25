using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Threading.Tasks;
using BCryptNet = BCrypt.Net.BCrypt;

namespace IngestaoMed.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDatabaseContext _db;
        private readonly IConfigService _config;

        public AuthService(IDatabaseContext db, IConfigService config)
        {
            _db = db;
            _config = config;
        }

        public async Task<bool> RegistrarCuidador(Cuidador cuidador, string senhaLimpa)
        {
            cuidador.PasswordHash = BCryptNet.HashPassword(senhaLimpa);
            bool gravou = await _db.InserirAsync(cuidador);

            if (gravou)
            {
                // Se acabou de registrar no primeiro acesso, inicializa as configurações da sessão
                _config.ConfiguracaoCuidador = new ConfiguracaoCuidador
                {
                    Id = cuidador.Id,
                    NomeCuidador = cuidador.Nome,
                    EmailCuidador = cuidador.Email,
                    AlertaAtivado = true,
                    LimiteSonecasParaAlerta = 3
                };
                _config.EmailCuidadorConfigurado = cuidador.Email;
                _config.EhPrimeiroAcesso = false;
            }

            return gravou;
        }

        public async Task<bool> ValidarLogin(string email, string senhaLimpa)
        {
            var usuario = await _db.BuscarPrimeiroAsync<Cuidador>(c =>
                c.Email.ToLower() == email.ToLower());

            if (usuario == null) return false;

            bool senhaValida = BCryptNet.Verify(senhaLimpa, usuario.PasswordHash);

            if (senhaValida)
            {
                _config.ConfiguracaoCuidador = new ConfiguracaoCuidador
                {
                    Id = usuario.Id,
                    NomeCuidador = usuario.Nome,
                    EmailCuidador = usuario.Email,
                    AlertaAtivado = true,
                    LimiteSonecasParaAlerta = 3
                };
                _config.EmailCuidadorConfigurado = usuario.Email;
                _config.EhPrimeiroAcesso = false;
            }

            return senhaValida;
        }

        public async Task<bool> ExisteCuidadorCadastrado()
        {
            var total = await _db.ContarAsync<Cuidador>();
            return total > 0;
        }

        public async Task<Cuidador?> GetCuidadorAtual()
        {
            if (_config.ConfiguracaoCuidador == null) return null;

            int idLogado = _config.ConfiguracaoCuidador.Id;

            return await _db.BuscarPrimeiroAsync<Cuidador>(c => c.Id == idLogado);
        }

        public async Task<bool> ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var usuario = await _db.BuscarPrimeiroAsync<Cuidador>(c =>
                c.Email.ToLower() == email.ToLower());

            return usuario != null;
        }

        public async Task FazerLogout()
        {
            _config.ConfiguracaoCuidador = null;
            _config.EmailCuidadorConfigurado = null;
            _config.RemoverSessaoCuidador();
            await Task.CompletedTask;
        }
    }
}