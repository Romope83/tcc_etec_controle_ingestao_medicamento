using SQLite;
using NotNull = SQLite.NotNullAttribute;
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

        [NotNull]
        public required string Nome { get; set; }

        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;
    }
}