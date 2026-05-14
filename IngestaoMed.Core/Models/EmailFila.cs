using SQLite;

namespace IngestaoMed.Core.Models
{
    [Table("FilaEmails")]
    public class EmailFila
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Destinatario { get; set; } = string.Empty;
        public string Assunto { get; set; } = string.Empty;
        public string Corpo { get; set; } = string.Empty;
        public int Tentativas { get; set; } = 0;
        public bool Enviado { get; set; } = false;
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}