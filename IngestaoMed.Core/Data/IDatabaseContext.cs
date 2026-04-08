using System.Linq.Expressions;

namespace IngestaoMed.Core.Data
{
    public interface IDatabaseContext
    {
        Task<bool> InserirAsync<T>(T entidade) where T : new();

        Task<T?> BuscarPrimeiroAsync<T>(Expression<Func<T, bool>> predicado) where T : new();

        Task<T?> BuscarPrimeiroAsync<T>() where T : new();

        Task<int> ContarAsync<T>() where T : new();
    }
}