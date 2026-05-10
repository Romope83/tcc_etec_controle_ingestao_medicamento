using SQLite;

namespace IngestaoMed.Core.Models
{
    [Table("LogsMedicamentos")]
    public class LogMedicamento
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int AgendamentoId { get; set; }
        public int TratamentoId { get; set; }

        public DateTime DataHora { get; set; } = DateTime.Now;

        public string Mensagem { get; set; } = string.Empty;

        public string Status { get; set; } = "Realizado";
    }
}