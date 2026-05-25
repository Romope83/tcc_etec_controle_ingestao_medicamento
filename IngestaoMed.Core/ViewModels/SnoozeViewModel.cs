using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IngestaoMed.Core.Constants;
using IngestaoMed.Core.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IngestaoMed.Core.ViewModels
{
    public partial class SnoozeViewModel : ObservableObject
    {
        private readonly ISnoozeService _snoozeService;
        private readonly ISnoozeScheduler _snoozeScheduler;
        private readonly IMonitorFalhaService _monitorFalhaService;

        private int _agendamentoId;

        [ObservableProperty]
        private string _tempoRestante = "00:00";

        [ObservableProperty]
        private int _quantidadeSonecas;

        [ObservableProperty]
        private bool _podeAdiarNovamente;

        [ObservableProperty]
        private string _nomeMedicamento = string.Empty;

        public SnoozeViewModel(
            ISnoozeService snoozeService,
            ISnoozeScheduler snoozeScheduler,
            IMonitorFalhaService monitorFalhaService)
        {
            _snoozeService = snoozeService;
            _snoozeScheduler = snoozeScheduler;
            _monitorFalhaService = monitorFalhaService;

            WeakReferenceMessenger.Default.Register<string>(this, (r, tempo) =>
            {
                TempoRestante = tempo;
            });

            WeakReferenceMessenger.Default.Register<string, string>(this, "SnoozeFinished", async (r, token) =>
            {
                await TratarTempoExpiradoAsync();
            });
        }

        public void Inicializar(int agendamentoId, string nomeMed)
        {
            _agendamentoId = agendamentoId;
            NomeMedicamento = nomeMed;
            QuantidadeSonecas = _snoozeScheduler.ObterTentativas(agendamentoId);
            PodeAdiarNovamente = _snoozeScheduler.PodeAdiar(agendamentoId);
        }

        [RelayCommand]
        private async Task ConfirmarIngestao()
        {
            _snoozeScheduler.LimparHistorico(_agendamentoId);

            await Task.CompletedTask;
        }

        private async Task TratarTempoExpiradoAsync()
        {
            if (!_snoozeScheduler.PodeAdiar(_agendamentoId))
            {
                int totalSonecas = _snoozeScheduler.ObterTentativas(_agendamentoId);
                await _monitorFalhaService.VerificarELoggerFalhaAsync(_agendamentoId, totalSonecas);
            }
        }
    }
}