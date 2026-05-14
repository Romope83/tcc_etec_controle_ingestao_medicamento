using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IngestaoMed.Core.Constants;
using IngestaoMed.Core.Interfaces;
using System.Windows.Input;

namespace IngestaoMed.Core.ViewModels
{
    public partial class SnoozeViewModel : ObservableObject
    {
        private readonly ISnoozeService _snoozeService;
        private readonly ISnoozeScheduler _snoozeScheduler;

        [ObservableProperty]
        private string _tempoRestante = "00:00";

        [ObservableProperty]
        private int _quantidadeSonecas;

        [ObservableProperty]
        private bool _podeAdiarNovamente;

        [ObservableProperty]
        private string _nomeMedicamento = string.Empty;

        public SnoozeViewModel(ISnoozeService snoozeService, ISnoozeScheduler snoozeScheduler)
        {
            _snoozeService = snoozeService;
            _snoozeScheduler = snoozeScheduler;
            WeakReferenceMessenger.Default.Register<string>(this, (r, tempo) =>
            {
                TempoRestante = tempo;
            });
        }

        public void Inicializar(int agendamentoId, string nomeMed, DateTime alvo)
        {
            NomeMedicamento = nomeMed;
            QuantidadeSonecas = _snoozeScheduler.ObterTentativas(agendamentoId);
            PodeAdiarNovamente = _snoozeScheduler.PodeAdiar(agendamentoId);

            // Aqui você conectará o SnoozeTimerHelper (que está no MAUI) 
            // através de um evento ou mensagem se necessário.
        }

        [RelayCommand]
        private async Task ConfirmarIngestao(int agendamentoId)
        {
            _snoozeService.CancelarSoneca(agendamentoId);
            // Lógica para marcar como tomado no banco...
            await Task.CompletedTask;
        }
    }
}