using IngestaoMed.Core.Extensions;
using SQLite;
using System.Diagnostics.CodeAnalysis;
using NotNull = SQLite.NotNullAttribute;

namespace IngestaoMed.Core.Models
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

        [NotNull]
        public DateTime DataNascimento { get; set; }

        [MaxLength(20)]
        public string? Telefone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? FotoPerfilPath { get; set; }
        
        [Ignore]
        public List<Tratamento> Tratamentos { get; set; } = new();

        [Ignore]
        public string IdadeFormatada => $"{DateTime.Today.Year - DataNascimento.Year} anos";

        [Ignore]
        public string ResumoTratamentos { get; set; } = "00/00";

        [Ignore]
        public string ProximaData { get; set; } = "--";

        [Ignore]
        public string ProximoHorario { get; set; } = "--";
        [Ignore]
        public bool TemAgendamento { get; set; }
        [Ignore]
        public string TelefoneFormatado => Telefone.FormatarTelefone();
    }
}