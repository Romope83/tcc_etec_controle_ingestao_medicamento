using SQLite;
using NotNull = SQLite.NotNullAttribute;
using System.Diagnostics.CodeAnalysis;

namespace IngestaoMed.Core.Models
{
    [Table("MedicamentosTratamento")]
    public class MedicamentoTratamento
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int TratamentoId { get; set; }

        public int MedicamentoId { get; set; }
        public string Dosagem { get; set; } = string.Empty;
        public int IntervaloHoras { get; set; }
        public string Instrucoes { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
        public int Tolerancia { get; set; } = 30;

        [NotNull]
        public DateTime DataCriacao { get; private set; } = DateTime.Now;

        [Ignore]
        public string? NomeMedicamento { get; set; }
        [Ignore]
        public string? FotoPath { get; set; }

        [Ignore]
        public string? FormaIngestao { get; set; }

        [Ignore]
        public int DosesTomadas { get; set; }

        [Ignore]
        public int TotalDoses { get; set; }

        [Ignore]
        public string ResumoDoses => $"{DosesTomadas:D2}/{TotalDoses:D2}";

        [Ignore]
        public DateTime? DataPrimeiraDose { get; set; }
        [Ignore]
        public DateTime? DataUltimaDose { get; set; }
        [Ignore] 
        public string? UnidadeDosagem { get; set; }
    }
}
