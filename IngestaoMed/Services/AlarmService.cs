using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Interfaces; // Namespace onde está o INotificationMapper
using Plugin.LocalNotification;

namespace IngestaoMed.Services
{
    public class AlarmService : IAlarmService
    {
        private readonly INotificationMapper _mapper;

        public AlarmService(INotificationMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task AgendarNotificacaoAsync(Agendamento agendamento)
        {
            // O Mapper cuida da tradução do seu objeto de domínio para o plugin
            var request = _mapper.MapToRequest(agendamento);

            if (request != null)
            {
                await LocalNotificationCenter.Current.Show(request);
            }
        }

        public async Task CancelarAlarmeAsync(int agendamentoId)
        {
            // Cancela a notificação pendente no sistema operacional
            LocalNotificationCenter.Current.Cancel(agendamentoId);
            await Task.CompletedTask;
        }
    }
}