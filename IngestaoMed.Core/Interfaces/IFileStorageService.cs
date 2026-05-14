namespace IngestaoMed.Core.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SalvarArquivoAsync(byte[] dados, string nomeArquivo);

        Task<bool> DeletarArquivoAsync(string caminho);

        bool ArquivoExiste(string caminho);
    }
}