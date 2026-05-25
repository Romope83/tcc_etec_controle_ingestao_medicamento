namespace IngestaoMed.Core.DTOs
{
    public class TimelineItem
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string? CaminhoImagem { get; set; } // Opcional
        public string Tipo { get; set; } // "LOG", "FOTO", "ALERTA"
    }
}