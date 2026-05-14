using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;

namespace IngestaoMed.Core.ViewModels
{
    public partial class AlarmeViewModel : ObservableObject
    {
        private readonly IDatabaseContext _db;
        private readonly IAlarmService _alarmService;

        [ObservableProperty]
        private Agendamento? agendamentoAtual;

        public AlarmeViewModel(IDatabaseContext db, IAlarmService alarmService)
        {
            _db = db;
            _alarmService = alarmService;
        }

        [RelayCommand]
        private async Task ConfirmarIngestao()
        {
            if (AgendamentoAtual == null) return;

            AgendamentoAtual.Status = "Realizado";
            AgendamentoAtual.HorarioRealizado = DateTime.Now;

            await _db.AtualizarAsync(AgendamentoAtual);

            await _alarmService.CancelarAlarmeAsync(AgendamentoAtual.Id);
        }

        [RelayCommand]
        private async Task AdiarSoneca()
        {
            if (AgendamentoAtual == null) return;

            // Define 10 minutos como padrão para a soneca
            await _alarmService.AgendarNotificacaoAsync(AgendamentoAtual);
        }
    }
}