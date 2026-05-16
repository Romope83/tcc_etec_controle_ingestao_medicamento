using SQLite;
using NotNull = SQLite.NotNullAttribute;
using System.Diagnostics.CodeAnalysis;

namespace IngestaoMed.Core.Models
{
    [Table("Medicamentos")]
    public class Medicamento
    {

        [SetsRequiredMembers]
        public Medicamento() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100), NotNull]
        public required string? NomeComercial { get; set; }

        [MaxLength(50)]
        public string? FormaIngestao { get; set; }

        [MaxLength(255)]
        public string? FotoPath { get; set; }
        [MaxLength(30)]
        public string? UnidadeDosagem { get; set; }
    }
}