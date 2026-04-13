using SQLite;
using IngestaoMed.Core.Models;
using System.Linq.Expressions;

namespace IngestaoMed.Core.Data
{
    public class DatabaseContext : IDatabaseContext
    {
        private SQLiteAsyncConnection? _connection;
        private readonly string _dbPath;

        public DatabaseContext(string dbPath)
        {
            _dbPath = dbPath;
        }

        private async Task Init()
        {
            if (_connection is not null)
                return;

            _connection = new SQLiteAsyncConnection(_dbPath);

            await _connection.CreateTableAsync<Cuidador>();
            await _connection.CreateTableAsync<Paciente>();
            await _connection.CreateTableAsync<Medicamento>();
            await _connection.CreateTableAsync<Tratamento>();
            await _connection.CreateTableAsync<Agendamento>();
            await _connection.CreateTableAsync<Registro>();
        }

        public async Task<bool> InserirAsync<T>(T entidade) where T : new()
        {
            await Init();
            return await _connection!.InsertAsync(entidade) > 0;
        }

        public async Task<T?> BuscarPrimeiroAsync<T>(Expression<Func<T, bool>> predicado) where T : new()
        {
            await Init();
            return await _connection!.Table<T>().FirstOrDefaultAsync(predicado);
        }

        public async Task<T?> BuscarPrimeiroAsync<T>() where T : new()
        {
            await Init();
            return await _connection!.Table<T>().FirstOrDefaultAsync();
        }

        public async Task<int> ContarAsync<T>() where T : new()
        {
            await Init();
            return await _connection!.Table<T>().CountAsync();
        }
        public async Task<List<T>> BuscarTodosAsync<T>() where T : new()
        {
            await Init();
            return await _connection!.Table<T>().ToListAsync();
        }
    }
}