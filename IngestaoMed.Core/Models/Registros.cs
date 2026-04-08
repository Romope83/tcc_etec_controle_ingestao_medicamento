using SQLite;

namespace IngestaoMed.Core.Models
{
    [Table("Registros")]
    public class Registro
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed, NotNull]
        public int AgendamentoId { get; set; }

        [NotNull]
        public DateTime HorarioPrevisto { get; set; }

        public DateTime? HorarioReal { get; set; }

        public bool Confirmado { get; set; } = false;

        public int QuantidadeSonecas { get; set; } = 0;
    }
}