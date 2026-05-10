using IngestaoMed.Core.Interfaces;

namespace IngestaoMed.Services
{
    public class FileSystemService : IFileStorageService
    {
        private readonly string _basePath = FileSystem.AppDataDirectory;

        public async Task<string> SalvarArquivoAsync(byte[] dados, string nomeArquivo)
        {
            if (dados == null || dados.Length == 0)
                throw new ArgumentException("Dados do arquivo inválidos.");

            // Combina o caminho da pasta do app com o nome do arquivo
            string caminhoCompleto = Path.Combine(_basePath, nomeArquivo);

            // Grava os bytes fisicamente no armazenamento do celular
            await File.WriteAllBytesAsync(caminhoCompleto, dados);

            return caminhoCompleto;
        }

        public Task<bool> DeletarArquivoAsync(string caminho)
        {
            if (string.IsNullOrWhiteSpace(caminho))
                return Task.FromResult(false);

            try
            {
                if (File.Exists(caminho))
                {
                    File.Delete(caminho);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public bool ArquivoExiste(string caminho)
        {
            if (string.IsNullOrWhiteSpace(caminho)) return false;
            return File.Exists(caminho);
        }
    }
}