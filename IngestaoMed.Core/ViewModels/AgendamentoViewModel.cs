using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Data;
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
        private readonly IDatabaseContext _db;
        private readonly INavigationService _navigation;

        public AgendamentoViewModel(IDatabaseContext db, INavigationService navigation)
        {
            _db = db;
            _navigation = navigation;
        }

        [ObservableProperty] private int _tratamentoId;
        [ObservableProperty] private int _medicamentoTratamentoId;
        [ObservableProperty] private Medicamento? _medicamentoSelecionado;
        [ObservableProperty] private bool _temMedicamentoSelecionado;
        [ObservableProperty] private string _textoBusca = string.Empty;

        // Estados de Controle da Tela
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

        // Método de entrada ajustado para gerenciar tanto a criação quanto a edição histórica
        public async Task InicializarAsync(int tratamentoId, int medicamentoTratamentoId = 0)
        {
            TratamentoId = tratamentoId;
            MedicamentoTratamentoId = medicamentoTratamentoId;

            if (medicamentoTratamentoId > 0)
            {
                // MODO EDICAO: Carrega as doses e o medicamento do agendamento salvo
                var vinculo = await _db.BuscarPrimeiroAsync<MedicamentoTratamento>(mt => mt.Id == medicamentoTratamentoId);
                if (vinculo != null)
                {
                    Dose = vinculo.Dosagem ?? string.Empty;
                    IntervaloHoras = vinculo.IntervaloHoras;
                    Ativado = vinculo.Ativo;

                    var med = await _db.BuscarPrimeiroAsync<Medicamento>(m => m.Id == vinculo.MedicamentoId);
                    if (med != null)
                    {
                        MedicamentoSelecionado = med;
                        TemMedicamentoSelecionado = true;
                        TextoBusca = med.NomeComercial ?? string.Empty;
                    }

                    // Recupera e exibe as doses existentes gravadas no banco
                    var dosesSalvas = await _db.BuscarOndeAsync<Agendamento>(a => a.MedicamentoTratamentoId == medicamentoTratamentoId);

                    DosesGeradas.Clear();
                    if (dosesSalvas.Any())
                    {
                        // Organiza a linha cronológica e recupera os marcos de início/fim
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
                // MODO CADASTRO: Inicializa a tela em estado limpo para nova configuração
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
            _listaOriginal = await _db.BuscarTodosAsync<Medicamento>();
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
            CarregarTodosMedicamentosAsync();
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

            foreach (var dose in DosesGeradas.ToList())
            {
                if (dose.Id > 0) await _db.ExcluirAsync(dose);
                DosesGeradas.Remove(dose);
            }

            PossuiDosesGeradas = false;
            EstaEditando = false;
        }

        [RelayCommand]
        private async Task SalvarAgendamentoAsync()
        {
            if (MedicamentoSelecionado == null) return;

            if (MedicamentoTratamentoId == 0)
            {
                // Salva um novo vínculo
                var mt = new MedicamentoTratamento
                {
                    TratamentoId = TratamentoId,
                    MedicamentoId = MedicamentoSelecionado.Id,
                    Dosagem = Dose,
                    IntervaloHoras = IntervaloHoras,
                    Ativo = Ativado
                };

                await _db.InserirAsync(mt);
                MedicamentoTratamentoId = mt.Id;

                GerarDoses();
                foreach (var dose in DosesGeradas)
                {
                    dose.MedicamentoTratamentoId = mt.Id;
                    await _db.InserirAsync(dose);
                }
            }
            else
            {
                // Atualiza o vínculo existente
                var mt = await _db.BuscarPrimeiroAsync<MedicamentoTratamento>(x => x.Id == MedicamentoTratamentoId);
                if (mt != null)
                {
                    mt.Dosagem = Dose;
                    mt.IntervaloHoras = IntervaloHoras;
                    mt.Ativo = Ativado;
                    await _db.AtualizarAsync(mt);
                }
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