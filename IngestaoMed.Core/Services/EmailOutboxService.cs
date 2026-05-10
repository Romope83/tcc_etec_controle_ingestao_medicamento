using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Services
{
    public class EmailOutboxService : IEmailOutboxService
    {
        private readonly IDatabaseContext _db;

        public EmailOutboxService(IDatabaseContext db)
        {
            _db = db;
        }

        public async Task<List<EmailFila>> ObterPendentesAsync()
        {
            var todos = await _db.BuscarTodosAsync<EmailFila>();
            // Filtra e-mails não enviados e que não estouraram o limite de 3 tentativas
            return todos.Where(e => !e.Enviado && e.Tentativas < 3).ToList();
        }

        public async Task AtualizarStatusEnvioAsync(int emailId, bool sucesso)
        {
            var email = await _db.BuscarPrimeiroAsync<EmailFila>(e => e.Id == emailId);
            if (email == null) return;

            if (sucesso)
            {
                email.Enviado = true;
            }
            else
            {
                email.Tentativas++;
            }

            await _db.AtualizarAsync(email);
        }

        public async Task LimparFilaAntigaAsync(int diasRetencao = 7)
        {
            var dataCorte = DateTime.Now.AddDays(-diasRetencao);
            var todos = await _db.BuscarTodosAsync<EmailFila>();

            // Remove o que já foi enviado há mais de X dias para não inflar o SQLite do celular
            var paraRemover = todos.Where(e => e.Enviado && e.DataCriacao < dataCorte).ToList();

            foreach (var email in paraRemover)
            {
                await _db.ExcluirAsync(email);
            }
        }
    }
}