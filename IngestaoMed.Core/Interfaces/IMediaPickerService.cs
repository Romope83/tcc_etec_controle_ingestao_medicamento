using System.Threading.Tasks;

namespace IngestaoMed.Core.Interfaces
{
    public interface IMediaPickerService
    {
        Task<string?> CapturarFotoAsync();
        Task<string?> SelecionarFotoGaleriaAsync();
    }
}