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
        public string Instrucoes { get; set; } = string.Empty; // ex: "via oral", "40 gotas"

        [Ignore]
        public string? NomeMedicamento { get; set; }
    }
}
