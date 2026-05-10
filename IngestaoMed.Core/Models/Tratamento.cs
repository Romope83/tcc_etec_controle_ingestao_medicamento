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

        [Indexed]
        public int MedicamentoId { get; set; }

        public string Dosagem { get; set; } = string.Empty;

        public int IntervaloHoras { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        public bool Ativo { get; set; } = true;

        // Propriedades de apoio (Não gravadas no banco)
        // Úteis para exibir o nome do paciente/remédio na lista sem precisar de JOINS complexos
        [Ignore]
        public string? NomePaciente { get; set; }

        [Ignore]
        public string? NomeMedicamento { get; set; }
    }
}