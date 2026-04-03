using IngestaoMed.Data;
using IngestaoMed.Models;

namespace IngestaoMed.Services
{
    public class AuthService : IAuthService
    {
        private readonly DatabaseContext _db;

        public AuthService(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<bool> RegistrarCuidador(Cuidador cuidador)
        {
            var conn = await _db.GetConnectionAsync();
            var resultado = await conn.InsertAsync(cuidador);
            return resultado > 0;
        }

        public async Task<Cuidador?> Login(string email)
        {
            var conn = await _db.GetConnectionAsync();
            return await conn.Table<Cuidador>()
                             .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<bool> ExisteCuidadorCadastrado()
        {
            var conn = await _db.GetConnectionAsync();
            var count = await conn.Table<Cuidador>().CountAsync();
            return count > 0;
        }
    }
}