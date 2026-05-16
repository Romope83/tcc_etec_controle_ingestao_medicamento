using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Data;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Collections.ObjectModel;

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


        partial void OnToleranciaChanged(int value)
        {
            int valorArredondado = (int)Math.Round(value / 10.0) * 10;

            if (value != valorArredondado)
            {
                Tolerancia = valorArredondado;
            }
        }
        public ObservableCollection<Medicamento> SugestoesBusca { get; } = new();
        private List<Medicamento> _listaOriginal = new();
        public ObservableCollection<Agendamento> DosesGeradas { get; } = new();

        public async Task InicializarAsync(int tratamentoId)
        {
            TratamentoId = tratamentoId;
            await CarregarTodosMedicamentosAsync();
        }

        private async Task CarregarTodosMedicamentosAsync()
        {
            _listaOriginal = await _db.BuscarTodosAsync<Medicamento>();
            AtualizarSugestoes(_listaOriginal);
        }

        [RelayCommand]
        private void AlterarMedicamento()
        {
            if (PossuiDosesGeradas) return; // Trava de segurança

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
                m.NomeComercial.Contains(TextoBusca, StringComparison.OrdinalIgnoreCase)).ToList();

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
            TextoBusca = med.NomeComercial;
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

            // Remove do banco (assumindo que você tenha um método para deletar múltiplos ou deletar o vínculo)
            foreach (var dose in DosesGeradas.ToList())
            {
                if (dose.Id > 0) await _db.ExcluirAsync(dose);
                DosesGeradas.Remove(dose);
            }

            PossuiDosesGeradas = false;
            EstaEditando = false; // Permite editar campos novamente
        }

        [RelayCommand]
        private async Task SalvarAgendamentoAsync()
        {
            if (MedicamentoSelecionado == null) return;

            var mt = new MedicamentoTratamento
            {
                TratamentoId = TratamentoId,
                MedicamentoId = MedicamentoSelecionado.Id,
                Dosagem = Dose,
                IntervaloHoras = IntervaloHoras,
                Ativo = Ativado
            };

            await _db.InserirAsync(mt);

            GerarDoses();
            foreach (var dose in DosesGeradas)
            {
                dose.MedicamentoTratamentoId = mt.Id;
                await _db.InserirAsync(dose);
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