using SQLite;
using NotNull = SQLite.NotNullAttribute;
using System.Diagnostics.CodeAnalysis;

namespace IngestaoMed.Models
{
    [Table("Agendamentos")]
    public class Agendamento
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed, NotNull]
        public int PacienteId { get; set; }

        [Indexed, NotNull]
        public int MedicamentoId { get; set; }

        [Indexed, NotNull]
        public int TratamentoId { get; set; }

        [NotNull]
        public DateTime HorarioProgramado { get; set; }

        public int IntervaloHoras { get; set; }
    }
}