using IngestaoMed.Core.Models;
using System.Threading.Tasks;

namespace IngestaoMed.Core.Interfaces
{
    public interface ICuidadorService
    {
        Task<bool> SalvarCuidadorAsync(Cuidador cuidador);
        Task<Cuidador?> ObterPorIdAsync(int id);
    }
}