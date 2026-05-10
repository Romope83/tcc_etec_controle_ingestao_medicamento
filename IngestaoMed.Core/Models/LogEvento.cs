using IngestaoMed.Core.Enums;
using SQLite; // Importante para as Data Annotations

namespace IngestaoMed.Core.Models
{
    [Table("LogsEventos")]
    public class LogEvento
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed] 
        public int AgendamentoId { get; set; }

        public TipoEventoLog Tipo { get; set; }
        public DateTime DataOcorrencia { get; set; }
        public int MinutosAtraso { get; set; }

        [Ignore]
        public Agendamento? Agendamento { get; set; }
    }
}