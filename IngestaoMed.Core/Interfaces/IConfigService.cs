namespace IngestaoMed.Core.Interfaces
{
    public interface IConfigService
    {
        bool EhPrimeiroAcesso { get; set; }
        string? EmailCuidadorConfigurado { get; set; }
        // Outras flags que surgirem, como "ModoEscuro" ou "NotificacoesAtivas"
    }
}