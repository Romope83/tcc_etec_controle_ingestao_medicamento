using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IngestaoMed.Core.ViewModels
{
    public partial class ListaPacientesViewModel : ObservableObject
    {
        private readonly IPacienteService _service;
        private readonly INavigationService _nav;
        private readonly IDialogService _dialog;

        public ObservableCollection<Paciente> Pacientes { get; } = new();

        public ListaPacientesViewModel(IPacienteService service, INavigationService nav, IDialogService dialog)
        {
            _service = service;
            _nav = nav;
            _dialog = dialog;
        }

        [RelayCommand]
        private async Task CarregarPacientesAsync()
        {
            var lista = await _service.ObterTodosAsync();

            Pacientes.Clear();
            foreach (var p in lista) Pacientes.Add(p); }
            
        

        [RelayCommand]
        private async Task NavegarParaCadastroAsync()
        {
            await _nav.GoToAsync("CadastroPacientePage");
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
    }
}
    