using SQLite;
using NotNull = SQLite.NotNullAttribute;
using System.Diagnostics.CodeAnalysis;

namespace IngestaoMed.Models
{
    [Table("Pacientes")]
    public class Paciente
    {
        [SetsRequiredMembers]
        public Paciente() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100), NotNull]
        public required string Nome { get; set; }

        [MaxLength(20)]
        public string? Telefone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? FotoPerfilPath { get; set; }
    }
}