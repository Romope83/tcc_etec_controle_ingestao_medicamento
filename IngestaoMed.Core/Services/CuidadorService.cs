using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;
using System;
using System.Threading.Tasks;

namespace IngestaoMed.Core.Services
{
    public class CuidadorService : ICuidadorService
    {
        private readonly IDatabaseContext _db;
        private readonly IAuthService _authService; // Injeção da autoridade de autenticação

        public CuidadorService(IDatabaseContext db, IAuthService authService)
        {
            _db = db;
            _authService = authService;
        }

        public async Task<bool> SalvarCuidadorAsync(Cuidador cuidador)
        {
            if (cuidador == null) return false;

            try
            {
                if (cuidador.Id > 0)
                {
                    return await _db.InserirAsync(cuidador);
                }

                string senhaLimpa = cuidador.PasswordHash;

                return await _authService.RegistrarCuidador(cuidador, senhaLimpa);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar cuidador: {ex.Message}");
                return false;
            }
        }
    }
}