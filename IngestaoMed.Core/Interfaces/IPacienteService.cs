using IngestaoMed.Core.Models;
using System.Linq.Expressions;

public interface IPacienteService
{
    Task<List<Paciente>> ObterTodosAsync();
    Task<bool> SalvarOuAtualizarPacienteAsync(Paciente paciente);
    Task<bool> RemoverPacienteAsync(Paciente paciente);
    Task<List<Paciente>> BuscarOndeAsync(Expression<Func<Paciente, bool>> predicado);
    Task<List<Paciente>> BuscarPacientesIdososAsync();
    Task<List<Paciente>> BuscarComTratamentoAtivoAsync();
    Task<Paciente?> ObterDetalhesCompletosAsync(int pacienteId);
    Task<Paciente?> BuscarPacientePorIdAsync(int pacienteId);
}