using IngestaoMed.Core.Models;

public interface IPacienteService
{
    Task<List<Paciente>> ObterTodosAsync();
    Task<bool> SalvarPacienteAsync(Paciente paciente);
    Task<bool> RemoverPacienteAsync(Paciente paciente);
}