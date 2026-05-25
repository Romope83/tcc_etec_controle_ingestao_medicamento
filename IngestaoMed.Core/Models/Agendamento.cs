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
        public int MedicamentoTratamentoId { get; set; }

        [NotNull]
        public DateTime HorarioOriginal { get; set; }

        [NotNull]
        public DateTime ProximoAlarme { get; set; }

        [NotNull]
        public string Status { get; set; } = "Pendente";

        public DateTime? HorarioConfirmacao { get; set; }
        public int QuantidadeSonecas { get; set; } = 0;

        [Ignore]
        public Tratamento? Tratamento { get; set; }

        [Ignore]
        public string? NomeRemedioEspecifico { get; set; }
        [Ignore]
        public bool Atrasado { get; set; } = false;
    }
}