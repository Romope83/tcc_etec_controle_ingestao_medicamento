namespace IngestaoMed.Core.Constants
{
    public static class NotificationConstants
    {
        // Categorias de Notificação
        public const string CategoryStatus = "STATUS_MEDICAMENTO";

        // IDs de Ação (Botões)
        public const int ActionTomeiId = 1010;
        public const int ActionSonecaId = 1020;

        // Configurações de Soneca
        public const int LimiteMaximoSonecas = 3;
        public const int TempoSonecaPadraoMinutos = 10;

        // Canais e Prioridades (Android)
        public const string CanalCriticoId = "canal_critico_medicamentos";
        public const string CanalPadraoId = "canal_lembretes";

        // Títulos e Mensagens
        public const string TituloAlertaNormal = "💊 Hora do seu Medicamento";
        public const string TituloAlertaCritico = "⚠️ ALERTA CRÍTICO: Dose Atrasada!";
        public const string PrefixoSoneca = "[ADICIONADO]";
    }
}