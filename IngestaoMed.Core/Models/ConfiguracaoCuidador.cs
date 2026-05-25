namespace IngestaoMed.Core.Models
{
    public class ConfiguracaoCuidador
    {
        public int Id { get; set; }
        public string NomeCuidador { get; set; } = string.Empty;
        public string EmailCuidador { get; set; } = string.Empty;
        public bool AlertaAtivado { get; set; }
        public int LimiteSonecasParaAlerta { get; set; } = 3;
    }
}