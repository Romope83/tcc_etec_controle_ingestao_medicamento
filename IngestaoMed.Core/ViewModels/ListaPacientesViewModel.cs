using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace IngestaoMed.Core.ViewModels
{
    public enum FiltroPaciente { Todos, Ativos, Idosos }


    public partial class ListaPacientesViewModel : ObservableObject
    {
        private readonly IPacienteService _service;
        private readonly INavigationService _nav;
        private readonly IDialogService _dialog;
        private CancellationTokenSource? _cts;

        [ObservableProperty]
        private string _textoBusca;

        [ObservableProperty]
        private FiltroPaciente _filtroAtual = FiltroPaciente.Todos;



        public ListaPacientesViewModel(IPacienteService service, INavigationService nav, IDialogService dialog)
        {
            _service = service;
            _nav = nav;
            _dialog = dialog;
        }

        public ObservableCollection<Paciente> Pacientes { get; } = new();

        [RelayCommand]
        private async Task AlterarFiltroAsync(FiltroPaciente novoFiltro)
        {
            FiltroAtual = novoFiltro;
            await FiltrarPacientesAsync(TextoBusca, CancellationToken.None);
        }

        [RelayCommand]
        private async Task CarregarPacientesAsync()
        {
            var lista = await _service.ObterTodosAsync();
            await AtualizarListaUI(lista);
        }

        [RelayCommand]
        private async Task NavegarParaCadastroAsync()
        {
            await _nav.GoToAsync("PacientePage");
        }

        [RelayCommand]
        private async Task RemoverPacienteAsync(Paciente paciente)
        {
            if (await _dialog.DisplayConfirmationAsync("Excluir", $"Remover {paciente.Nome}?", "Sim", "Não"))
            {
                if (await _service.RemoverPacienteAsync(paciente))
                    Pacientes.Remove(paciente);
            }
        }

        [RelayCommand]
        private async Task SelecionarPacienteAsync(int pacienteId)
        {
            if (pacienteId <= 0) return;
            await _nav.GoToAsync($"PacienteDetalhesPage?id={pacienteId}");
        }

        private async Task AtualizarListaUI(List<Paciente> lista)
        {
            await Task.Run(
                () =>
                {
                    Pacientes.Clear();
                    foreach (var p in lista)
                    {
                        Pacientes.Add(p);
                    }
                }
            );
        }
        partial void OnTextoBuscaChanged(string value)
        {
            // Cancelamos a tarefa anterior (se houver uma em espera)
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _ = FiltrarPacientesAsync(value, _cts.Token);
        }
        private async Task FiltrarPacientesAsync(string nome, CancellationToken token)
        {
            try
            {
                List<Paciente> resultado;

                switch (FiltroAtual)
                {
                    case FiltroPaciente.Ativos:
                        resultado = await _service.BuscarComTratamentoAtivoAsync();
                        break;
                    case FiltroPaciente.Idosos:
                        resultado = await _service.BuscarPacientesIdososAsync();
                        break;
                    default:
                        resultado = await _service.ObterTodosAsync();
                        break;
                }

                if (!string.IsNullOrWhiteSpace(nome))
                {
                    resultado = resultado
                        .Where(p => p.Nome.Contains(nome, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (token.IsCancellationRequested) return;

                await AtualizarListaUI(resultado);
            }
            catch { /* ... */ }
        }
    }
}
    