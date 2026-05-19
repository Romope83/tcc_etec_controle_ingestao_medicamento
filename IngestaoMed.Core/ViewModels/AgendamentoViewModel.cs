using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace IngestaoMed.Core.ViewModels
{
    public partial class AgendamentoViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;
        private readonly IDialogService _dialogService;
        private readonly IAgendamentoConflitoService _conflitoService;
        private readonly IAgendamentoService _agendamentoService;

        public AgendamentoViewModel(
            INavigationService navigation,
            IDialogService dialogService,
            IAgendamentoConflitoService @conflitoService,
            IAgendamentoService agendamentoService)
        {
            _navigation = navigation;
            _dialogService = dialogService;
            _conflitoService = @conflitoService;
            _agendamentoService = agendamentoService;
        }

        [ObservableProperty] private int _tratamentoId;
        [ObservableProperty] private int _medicamentoTratamentoId;
        [ObservableProperty] private Medicamento? _medicamentoSelecionado;
        [ObservableProperty] private bool _temMedicamentoSelecionado;
        [ObservableProperty] private string _textoBusca = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PodeDeletarDoses))]
        private bool _estaEditando;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PodeDeletarDoses))]
        private bool _possuiDosesGeradas;

        public bool PodeDeletarDoses => PossuiDosesGeradas && !DosesGeradas.Any(d => d.Status == "Tomado");

        [ObservableProperty] private string _dose = string.Empty;
        [ObservableProperty] private int _intervaloHoras = 8;
        [ObservableProperty] private DateTime _dataInicio = DateTime.Now;
        [ObservableProperty] private TimeSpan _horaInicio = DateTime.Now.TimeOfDay;
        [ObservableProperty] private DateTime _dataFim = DateTime.Now.AddDays(7);
        [ObservableProperty] private TimeSpan _horaFim = new TimeSpan(23, 59, 0);
        [ObservableProperty] private bool _ativado = true;
        [ObservableProperty] private int _tolerancia = 30;

        public ObservableCollection<Medicamento> SugestoesBusca { get; } = new();
        private List<Medicamento> _listaOriginal = new();
        public ObservableCollection<Agendamento> DosesGeradas { get; } = new();

        partial void OnToleranciaChanged(int value)
        {
            int valorArredondado = (int)Math.Round(value / 10.0) * 10;
            if (value != valorArredondado)
            {
                Tolerancia = valorArredondado;
            }
        }

        public async Task InicializarAsync(int tratamentoId, int medicamentoTratamentoId = 0)
        {
            TratamentoId = tratamentoId;
            MedicamentoTratamentoId = medicamentoTratamentoId;

            if (medicamentoTratamentoId > 0)
            {
                var vinculo = await _agendamentoService.ObterVinculoPorIdAsync(medicamentoTratamentoId);
                if (vinculo != null)
                {
                    Dose = vinculo.Dosagem ?? string.Empty;
                    IntervaloHoras = vinculo.IntervaloHoras;
                    Ativado = vinculo.Ativo;

                    var med = await _agendamentoService.ObterMedicamentoPorIdAsync(vinculo.MedicamentoId);
                    if (med != null)
                    {
                        MedicamentoSelecionado = med;
                        TemMedicamentoSelecionado = true;
                        TextoBusca = med.NomeComercial ?? string.Empty;
                    }

                    var dosesSalvas = await _agendamentoService.ObterDosesPorVinculoIdAsync(medicamentoTratamentoId);

                    DosesGeradas.Clear();
                    if (dosesSalvas.Any())
                    {
                        var dosesOrdenadas = dosesSalvas.OrderBy(a => a.HorarioOriginal).ToList();

                        DataInicio = dosesOrdenadas.First().HorarioOriginal.Date;
                        HoraInicio = dosesOrdenadas.First().HorarioOriginal.TimeOfDay;
                        DataFim = dosesOrdenadas.Last().HorarioOriginal.Date;
                        HoraFim = dosesOrdenadas.Last().HorarioOriginal.TimeOfDay;

                        foreach (var dose in dosesOrdenadas)
                        {
                            DosesGeradas.Add(dose);
                        }

                        PossuiDosesGeradas = true;
                        EstaEditando = true;
                    }
                }
            }
            else
            {
                MedicamentoSelecionado = null;
                TemMedicamentoSelecionado = false;
                TextoBusca = string.Empty;
                Dose = string.Empty;
                PossuiDosesGeradas = false;
                EstaEditando = false;
                DosesGeradas.Clear();

                await CarregarTodosMedicamentosAsync();
            }
        }

        private async Task CarregarTodosMedicamentosAsync()
        {
            _listaOriginal = await _agendamentoService.ObterTodosMedicamentosAsync();
            AtualizarSugestoes(_listaOriginal);
        }

        [RelayCommand]
        private void AlterarMedicamento()
        {
            if (PossuiDosesGeradas) return;

            MedicamentoSelecionado = null;
            TemMedicamentoSelecionado = false;
            TextoBusca = string.Empty;
            SugestoesBusca.Clear();
            DosesGeradas.Clear();
            _ = CarregarTodosMedicamentosAsync();
        }

        [RelayCommand]
        private void BuscarMedicamentos()
        {
            if (string.IsNullOrWhiteSpace(TextoBusca))
            {
                AtualizarSugestoes(_listaOriginal);
                return;
            }

            var filtrados = _listaOriginal.Where(m =>
                m.NomeComercial != null && m.NomeComercial.Contains(TextoBusca, StringComparison.OrdinalIgnoreCase)).ToList();

            AtualizarSugestoes(filtrados);
        }

        private void AtualizarSugestoes(List<Medicamento> lista)
        {
            SugestoesBusca.Clear();
            foreach (var item in lista)
                SugestoesBusca.Add(item);
        }

        [RelayCommand]
        private void SelecionarMedicamento(Medicamento med)
        {
            MedicamentoSelecionado = med;
            TemMedicamentoSelecionado = true;
            TextoBusca = med.NomeComercial ?? string.Empty;
            SugestoesBusca.Clear();
        }

        [RelayCommand]
        private void GerarDoses()
        {
            DosesGeradas.Clear();
            DateTime atual = DataInicio.Date.Add(HoraInicio);
            DateTime fim = DataFim.Date.Add(HoraFim);

            while (atual <= fim)
            {
                DosesGeradas.Add(new Agendamento
                {
                    HorarioOriginal = atual,
                    ProximoAlarme = atual,
                    Status = "Pendente",
                    QuantidadeSonecas = 0,
                    HorarioConfirmacao = null
                });

                atual = atual.AddHours(IntervaloHoras);
            }
        }

        [RelayCommand]
        private async Task DeletarDosesAsync()
        {
            if (!PodeDeletarDoses) return;

            bool sucesso = await _agendamentoService.DeletarDosesEAlarmeAsync(DosesGeradas.ToList());
            if (sucesso)
            {
                DosesGeradas.Clear();
                PossuiDosesGeradas = false;
                EstaEditando = false;
            }
        }

        [RelayCommand]
        private async Task SalvarAgendamentoAsync()
        {
            if (MedicamentoSelecionado == null) return;

            var tratamentoAtual = await _agendamentoService.ObterTratamentoPorIdAsync(TratamentoId);
            if (tratamentoAtual == null)
            {
                await _dialogService.DisplayAlert("Erro", "Tratamento de origem não encontrado.", "OK");
                return;
            }

            bool jaEstaEmAndamento = await _conflitoService.VerificarDuplicidadeMedicamentoEmAndamentoAsync(
                tratamentoAtual.PacienteId,
                MedicamentoSelecionado.Id,
                MedicamentoTratamentoId);

            if (jaEstaEmAndamento)
            {
                await _dialogService.DisplayAlert(
                    "Medicamento em Uso",
                    $"O medicamento {MedicamentoSelecionado.NomeComercial} já possui um agendamento ativo e com doses pendentes para este paciente.",
                    "OK");
                return;
            }

            var novosHorarios = new List<DateTime>();
            DateTime atual = DataInicio.Date.Add(HoraInicio);
            DateTime fim = DataFim.Date.Add(HoraFim);

            while (atual <= fim)
            {
                novosHorarios.Add(atual);
                atual = atual.AddHours(IntervaloHoras);
            }

            if (MedicamentoTratamentoId == 0)
            {
                var conflitos = await _conflitoService.VerificarConflitosAsync(tratamentoAtual, novosHorarios);
                if (conflitos.Any())
                {
                    bool prosseguir = await _dialogService.DisplayConfirmationAsync(
                        "Aviso de Conflito",
                        $"Atenção: Existem {conflitos.Count} doses mapeadas com menos de 30 minutos de diferença para outros medicamentos agendados deste mesmo paciente. Deseja manter esses horários mesmo assim?",
                        "Sim",
                        "Não");

                    if (!prosseguir) return;
                }
            }

            if (MedicamentoTratamentoId == 0)
            {
                GerarDoses();
                bool salvo = await _agendamentoService.SalvarNovoAgendamentoAsync(
                    TratamentoId,
                    MedicamentoSelecionado,
                    Dose,
                    IntervaloHoras,
                    Ativado,
                    DosesGeradas.ToList()
                );
                if (!salvo) return;
            }
            else
            {
                bool atualizado = await _agendamentoService.AtualizarAgendamentoExistenteAsync(
                    MedicamentoTratamentoId,
                    Dose,
                    IntervaloHoras,
                    Ativado
                );
                if (!atualizado) return;
            }

            PossuiDosesGeradas = true;
            EstaEditando = true;

            OnPropertyChanged(nameof(PodeDeletarDoses));
        }

        [RelayCommand]
        private async Task VoltarAsync()
        {
            await _navigation.GoToAsync("..");
        }
    }
}