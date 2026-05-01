using IngestaoMed.Core.Data;
using IngestaoMed.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngestaoMed.Core.Services
{
    public class PacienteService : IPacienteService
    {
        private readonly IDatabaseContext _db;

        public PacienteService(IDatabaseContext db) => _db = db;

        public async Task<List<Paciente>> ObterTodosAsync() =>
            await _db.BuscarTodosAsync<Paciente>();

       public async Task<bool> SalvarPacienteAsync(Paciente paciente)
        {
            return await _db.InserirAsync(paciente);
        }

        public async Task<bool> RemoverPacienteAsync(Paciente paciente) =>
            await _db.ExcluirAsync(paciente) > 0;
    }
}
