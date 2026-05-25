using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.Interfaces
{

    public interface ICalculadoraAdesao
    {
        double Calcular(IEnumerable<LogEvento> logs);
    }
}