using SQLite;
using System.Diagnostics.CodeAnalysis;

namespace IngestaoMed.Core.Models
{
    [Table("Tratamentos")]
    public class Tratamento
    {
        [SetsRequiredMembers]
        public Tratamento() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Nome { get; set; }
        public string Descricao { get; set; }

        [Indexed]
        public int PacienteId { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        public bool Ativo { get; set; } = true;

        [Ignore]
        public string? NomePaciente { get; set; }

        [Ignore]
        public List<MedicamentoTratamento> Remedios { get; set; } = new();

        [Ignore]
        public string PeriodoFormatado => $"{DataInicio:dd/MM/yyyy} até {(DataFim.HasValue ? DataFim.Value.ToString("dd/MM/yyyy") : "Contínuo")}";
        [Ignore]
        public int QtdRemedios { get; set; }

        [Ignore]
        public int TotalDosesTratamento { get; set; }

        [Ignore]
        public int DosesTomadasTratamento { get; set; }

        [Ignore]
        public string StatusAndamento => DosesTomadasTratamento >= TotalDosesTratamento && TotalDosesTratamento > 0
            ? "Finalizado"
            : "Em andamento";
    }

}