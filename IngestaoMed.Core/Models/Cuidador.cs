using SQLite;
using NotNull = SQLite.NotNullAttribute;
using System.Diagnostics.CodeAnalysis;

namespace IngestaoMed.Core.Models
{
    [Table("Cuidadores")]
    public class Cuidador
    {
        [SetsRequiredMembers]
        public Cuidador() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100), NotNull]
        public required string Nome { get; set; }

        [MaxLength(20)]
        public string? Telefone { get; set; }

        [Unique, MaxLength(100)]
        public required string Email { get; set; }
        [MaxLength(255), NotNull]
        public required string PasswordHash { get; set; }

        [MaxLength(20)]
        public int LimiteSonecasParaAlerta { get; set; } = 3;
    }
}