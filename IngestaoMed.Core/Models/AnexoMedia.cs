using SQLite;

namespace IngestaoMed.Core.Models
{
    [Table("AnexosMedia")]
    public class AnexoMedia
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ReferenciaId { get; set; } // Id do Medicamento ou Log
        public string TipoReferencia { get; set; } // "Medicamento" ou "Log"

        public string CaminhoLocal { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}