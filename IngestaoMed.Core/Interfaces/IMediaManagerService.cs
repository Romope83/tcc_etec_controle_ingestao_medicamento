namespace IngestaoMed.Core.Interfaces
{
    public interface IMediaManagerService
    {
        Task<string?> RegistrarFotoMedicamentoAsync(int medicamentoId, byte[] fotosBytes);
    }
}