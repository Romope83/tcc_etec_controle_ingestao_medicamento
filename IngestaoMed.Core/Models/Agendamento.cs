using SQLite;
using NotNull = SQLite.NotNullAttribute;
using System.Diagnostics.CodeAnalysis;

namespace IngestaoMed.Core.Models
{
    [Table("Agendamentos")]
    public class Agendamento
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed, NotNull]
        public int TratamentoId { get; set; }

        [NotNull]
        public DateTime HorarioProgramado { get; set; }

        [NotNull]
        public string Status { get; set; } = "Pendente";

        public DateTime? HorarioRealizado { get; set; }

        [Ignore]
        public Tratamento? Tratamento { get; set; }
    }
}