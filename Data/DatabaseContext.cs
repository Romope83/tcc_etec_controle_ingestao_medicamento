using SQLite;
using IngestaoMed.Models;

namespace IngestaoMed.Data
{
    public class DatabaseContext
    {
        private SQLiteAsyncConnection? _connection;

        // Caminho do banco de dados (será injetado ou definido via constante)
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

            // Criação das tabelas de forma assíncrona
            await _connection.CreateTableAsync<Cuidador>();
            await _connection.CreateTableAsync<Paciente>();
            await _connection.CreateTableAsync<Medicamento>();
            await _connection.CreateTableAsync<Tratamento>();
            await _connection.CreateTableAsync<Agendamento>();
            await _connection.CreateTableAsync<Registro>();
        }

        // Exemplo de método genérico para obter a conexão pronta
        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            await Init();
            return _connection!;
        }
    }
}