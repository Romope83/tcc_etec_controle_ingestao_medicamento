namespace IngestaoMed.Core.Enums
{
    public enum TipoEventoLog
    {
        ConfirmacaoDireta = 1, // Clicou em "Tomei" de primeira
        ConfirmacaoComSoneca = 2, // Tomou após adiar
        SonecaDisparada = 3, // Apenas o ato de adiar
        Ignorado = 4, // Notificação expirou ou foi fechada sem ação
        AtrasoCritico = 5 // Passou de X horas sem resposta
    }
}