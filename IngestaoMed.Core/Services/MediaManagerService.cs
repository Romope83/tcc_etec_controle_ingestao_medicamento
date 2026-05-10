using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Data;

namespace IngestaoMed.Core.Services
{
    public class MediaManagerService : IMediaManagerService
    {
        private readonly IFileStorageService _storage;
        private readonly IDatabaseContext _db;

        public MediaManagerService(IFileStorageService storage, IDatabaseContext db)
        {
            _storage = storage;
            _db = db;
        }

        public async Task<string?> RegistrarFotoMedicamentoAsync(int medicamentoId, byte[] fotosBytes)
        {
            if (fotosBytes == null || fotosBytes.Length == 0) return null;

            string nomeUnico = $"med_{medicamentoId}_{Guid.NewGuid()}.jpg";

            string caminhoFinal = await _storage.SalvarArquivoAsync(fotosBytes, nomeUnico);

            var anexo = new AnexoMedia
            {
                ReferenciaId = medicamentoId,
                TipoReferencia = "Medicamento",
                CaminhoLocal = caminhoFinal
            };

            await _db.InserirAsync(anexo);
            return caminhoFinal;
        }
    }
}