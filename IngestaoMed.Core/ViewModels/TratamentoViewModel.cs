using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using IngestaoMed.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace IngestaoMed.Core.ViewModels
{
    public partial class TratamentoViewModel : ObservableObject
    {
        private readonly ITratamentoService _tratamentoService;
        private readonly IDialogService _dialog;
        private readonly INavigationService _navigation;

        private Tratamento? _tratamentoAtual;

        public TratamentoViewModel(
            ITratamentoService tratamentoService,
            IDialogService dialog,
            INavigationService navigation)
        {
            _tratamentoService = tratamentoService;
            _dialog = dialog;
            _navigation = navigation;
        }

        [ObservableProperty] private int _tratamentoId;
        [ObservableProperty] private string _nome = string.Empty;
        [ObservableProperty] private int _pacienteId;
        [ObservableProperty] private string _descricao = string.Empty;
        [ObservableProperty] private DateTime _dataInicio = DateTime.Now;
        [ObservableProperty] private DateTime? _dataFim;
        [ObservableProperty] private bool _ativado = false; // Inicia desativado por padrão conforme nova regra

        public string NomeTratamento
        {
            get => Nome;
            set => Nome = value;
        }

        public ObservableCollection<MedicamentoTratamento> MedicamentosVinculados { get; } = new();

        public async Task InicializarAsync(int id)
        {
            TratamentoId = id;
            if (id > 0)
            {
                await CarregarTratamentoAsync(id);
            }
            else
            {
                _tratamentoAtual = null;
                Nome = string.Empty;
                Descricao = string.Empty;
                Ativado = false;
                MedicamentosVinculados.Clear();
            }
        }

        public async Task InicializarAsync(int pacienteId, int tratamentoId)
        {
            PacienteId = pacienteId;
            TratamentoId = tratamentoId;

            if (TratamentoId > 0)
            {
                await CarregarTratamentoAsync(TratamentoId);
            }
        }

        private async Task CarregarTratamentoAsync(int id)
        {
            _tratamentoAtual = await _tratamentoService.ObterPorIdAsync(id);
            if (_tratamentoAtual != null)
            {
                Nome = _tratamentoAtual.Nome;
                Descricao = _tratamentoAtual.Descricao;
                DataInicio = _tratamentoAtual.DataInicio;
                DataFim = _tratamentoAtual.DataFim;
                Ativado = _tratamentoAtual.Ativo;
                PacienteId = _tratamentoAtual.PacienteId;

                await CarregarRemediosVinculadosAsync(id);
            }
        }

        private async Task CarregarRemediosVinculadosAsync(int tratamentoId)
        {
            var lista = await _tratamentoService.ObterMedicamentosVinculadosAsync(tratamentoId);
            MedicamentosVinculados.Clear();
            foreach (var r in lista)
            {
                MedicamentosVinculados.Add(r);
            }
        }

        [RelayCommand]
        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Nome))
            {
                await _dialog.DisplayAlert("Aviso", "O nome do tratamento é obrigatório.", "OK");
                return;
            }

            if (TratamentoId == 0 || _tratamentoAtual == null)
            {
                // MODO CRIAÇÃO: Grava o novo registro como inativo
                var novo = new Tratamento
                {
                    Nome = Nome,
                    Descricao = Descricao,
                    PacienteId = PacienteId,
                    DataInicio = DataInicio,
                    DataFim = DataFim,
                    Ativo = false // Salva desativado por padrão
                };

                var id = await _tratamentoService.InserirTratamentoAsync(novo);
                TratamentoId = id;
                _tratamentoAtual = novo;
                Ativado = false;

                await _dialog.DisplayAlert("Sucesso", "Tratamento criado com sucesso!", "OK");
                // Removido o avanço automático para a tela de agendamento daqui
            }
            else
            {
                // MODO EDIÇÃO: Atualiza o registro existente em vez de criar outro
                var t = await _tratamentoService.ObterPorIdAsync(TratamentoId);
                if (t != null)
                {
                    t.Nome = Nome;
                    t.Descricao = Descricao;
                    t.DataInicio = DataInicio;
                    t.DataFim = DataFim;
                    t.Ativo = Ativado; // Preserva o estado de ativação atual da tela

                    await _tratamentoService.AtualizarTratamentoAsync(t);
                    _tratamentoAtual = t;

                    await _dialog.DisplayAlert("Sucesso", "Tratamento atualizado com sucesso!", "OK");
                }
            }
        }

        private async Task SalvarTratamentoAutomatico()
        {
            var t = new Tratamento
            {
                Nome = string.IsNullOrWhiteSpace(Nome) ? "Novo Tratamento" : Nome,
                Descricao = Descricao,
                PacienteId = PacienteId,
                DataInicio = DataInicio,
                DataFim = DataFim,
                Ativo = false // Salva desativado por padrão
            };

            var id = await _tratamentoService.InserirTratamentoAsync(t);
            TratamentoId = id;
            _tratamentoAtual = t;
            Ativado = false;
        }

        [RelayCommand]
        private async Task AdicionarMedicamentoAsync()
        {
            if (TratamentoId == 0)
            {
                await SalvarTratamentoAutomatico();
            }

            // O redirecionamento ocorre explicitamente apenas quando o usuário clica neste comando
            await _navigation.GoToAsync($"AgendamentoPage?tratamentoId={TratamentoId}");
        }

        [RelayCommand]
        private async Task RemoverMedicamentoAsync(MedicamentoTratamento medicamentoRemover)
        {
            if (medicamentoRemover == null) return;

            bool confirma = await _dialog.DisplayConfirmationAsync("Confirmar", "Deseja remover este medicamento do tratamento?", "Sim", "Não");
            if (!confirma) return;

            MedicamentosVinculados.Remove(medicamentoRemover);

            if (medicamentoRemover.Id > 0)
            {
                await _tratamentoService.ExcluirMedicamentoTratamentoAsync(medicamentoRemover);
            }
        }

        [RelayCommand]
        private async Task SelecionarMedicamentoVinculadoAsync(MedicamentoTratamento medicamento)
        {
            if (medicamento == null) return;
            await _navigation.GoToAsync($"AgendamentoPage?medicamentoTratamentoId={medicamento.Id}");
        }


        [RelayCommand]
        private async Task ExcluirTratamentoAsync()
        {
            if (TratamentoId <= 0) return;

            bool confirmar = await _dialog.DisplayConfirmationAsync(
                "Excluir Tratamento",
                "Tem certeza que deseja excluir este tratamento e todos os seus agendamentos?",
                "Sim",
                "Não");


            if (!confirmar) return;

            var t = await _tratamentoService.ObterPorIdAsync(TratamentoId);
            if (t != null)
            {
                await _tratamentoService.ExcluirTratamentoAsync(t);
                await _navigation.GoToAsync("..");
            }
        }

    }
}