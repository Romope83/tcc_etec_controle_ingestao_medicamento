using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Interfaces;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var request = _mapper.MapToRequest(agendamento);
            if (request != null)
            {
                await LocalNotificationCenter.Current.Show(request);
            }
        }

        public async Task CancelarAlarmeAsync(int agendamentoId)
        {
            LocalNotificationCenter.Current.Cancel(agendamentoId);
            await Task.CompletedTask;
        }

        public async Task SincronizarJanelaAlarmesAsync(List<Agendamento> proximosAlarmes)
        {
            try
            {
                // Limpa o lixo de notificações antigas no SO
                LocalNotificationCenter.Current.CancelAll();

                // Agenda estritamente o lote enviado pelo Core
                //var umMinuto = DateTime.Now.AddMicroseconds(30);
                foreach (var dose in proximosAlarmes)
                {
                    //dose.ProximoAlarme = umMinuto;
                    var request = _mapper.MapToRequest(dose);
                    if (request != null)
                    {
                        await LocalNotificationCenter.Current.Show(request);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar SO: {ex.Message}");
            }
        }
    }
}