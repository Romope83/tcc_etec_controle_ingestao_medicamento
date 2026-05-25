namespace IngestaoMed.Core.Interfaces
{
    public interface IConfigService
    {
        bool EhPrimeiroAcesso { get; set; }
        string? EmailCuidadorConfigurado { get; set; }
        Models.ConfiguracaoCuidador? ConfiguracaoCuidador { get; set; }
        void RemoverSessaoCuidador();
    }
}