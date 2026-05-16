using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.Interfaces;
using IngestaoMed.Core.Models;
using System.Text.RegularExpressions;
using IngestaoMed.Core.Services;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace IngestaoMed.Core.ViewModels
{
    public partial class PacienteViewModel : ObservableObject
    {
        private readonly IPacienteService _pacienteService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;
        private readonly IMediaPickerService _mediaPickerService; 
        private readonly IMediaManagerService _mediaManagerService; 

        private int _pacienteIdAtual;

        [ObservableProperty] private string nome = string.Empty;
        [ObservableProperty] private string telefone = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private DateTime dataNascimento = DateTime.Today.AddYears(-20);
        [ObservableProperty] private string? fotoPerfilPath;
        [ObservableProperty] private bool ehEdicao = false;

        public PacienteViewModel(IPacienteService pacienteService,
                                       INavigationService navigationService,
                                       IDialogService dialogService,
                                       IAuthService authService,
                                       IMediaPickerService mediaPickerService,
                                       IMediaManagerService mediaManagerService)
        {
            _pacienteService = pacienteService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _authService = authService;
            _mediaPickerService = mediaPickerService;
            _mediaManagerService = mediaManagerService;
        }

        public async Task InicializarAsync(int pacienteId)
        {
            _pacienteIdAtual = pacienteId;

            if (_pacienteIdAtual > 0)
            {
                try
                {
                    var paciente = await _pacienteService.BuscarPacientePorIdAsync(_pacienteIdAtual);

                    if (paciente != null)
                    {
                        Nome = paciente.Nome;
                        Telefone = paciente.Telefone ?? string.Empty;
                        Email = paciente.Email ?? string.Empty;
                        DataNascimento = paciente.DataNascimento;
                        FotoPerfilPath = paciente.FotoPerfilPath;
                        EhEdicao = true;
                    }
                    else
                    {
                        LimparCampos();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao inicializar paciente: {ex.Message}");
                    LimparCampos();
                }
            }
            else
            {
                LimparCampos();
            }
        }

        private void LimparCampos()
        {
            Nome = string.Empty;
            Telefone = string.Empty;
            Email = string.Empty;
            DataNascimento = DateTime.Today.AddYears(-20);
            FotoPerfilPath = null;
            EhEdicao = false;
        }

        partial void OnTelefoneChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            var numeros = Regex.Replace(value, @"[^\d]", "");

            if (numeros.Length > 11) numeros = numeros.Substring(0, 11);

            if (numeros.Length >= 11)
            {
                Telefone = $"({numeros.Substring(0, 2)}) {numeros.Substring(2, 5)}-{numeros.Substring(7)}";
            }
            else if (numeros.Length >= 7)
            {
                Telefone = $"({numeros.Substring(0, 2)}) {numeros.Substring(2, 4)}-{numeros.Substring(6)}";
            }
        }

        [RelayCommand]
        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Nome))
            {
                await _dialogService.DisplayAlert("Erro", "O nome é obrigatório.", "OK");
                return;
            }

            if (!ValidarFormatoEmail(Email))
            {
                await _dialogService.DisplayAlert("E-mail Inválido", "Por favor, insira um e-mail válido.", "OK");
                return;
            }

            // Apenas valida a duplicidade se o e-mail mudou ou se for um novo registro
            bool emailJaExiste = await _authService.ValidarEmail(Email);
            if (emailJaExiste && !EhEdicao)
            {
                await _dialogService.DisplayAlert("Erro", "Este e-mail já está cadastrado.", "OK");
                return;
            }

            bool sucesso;

            if (EhEdicao)
            {
                var pacienteExistente = await _pacienteService.BuscarPacientePorIdAsync(_pacienteIdAtual);

                if (pacienteExistente != null)
                {
                    // Atualiza apenas os dados modificados na tela
                    pacienteExistente.Nome = Nome;
                    pacienteExistente.Telefone = Telefone;
                    pacienteExistente.Email = Email;
                    pacienteExistente.DataNascimento = DataNascimento;
                    pacienteExistente.FotoPerfilPath = FotoPerfilPath;

                    // Invoca o método de atualização (ajuste o nome se o seu serviço usar AtualizarPacienteAsync)
                    sucesso = await _pacienteService.SalvarOuAtualizarPacienteAsync(pacienteExistente);
                }
                else
                {
                    await _dialogService.DisplayAlert("Erro", "Paciente não encontrado para atualização.", "OK");
                    return;
                }
            }
            else
            {
                var novoPaciente = new Paciente
                {
                    Nome = Nome,
                    Telefone = Telefone,
                    Email = Email,
                    DataNascimento = DataNascimento,
                    FotoPerfilPath = FotoPerfilPath
                };

                sucesso = await _pacienteService.SalvarOuAtualizarPacienteAsync(novoPaciente);
            }

            if (sucesso)
            {
                await _dialogService.DisplayAlert("Sucesso", EhEdicao ? "Paciente atualizado com sucesso!" : "Paciente cadastrado com sucesso!", "OK");
                await _navigationService.GoToAsync("..");
            }
        }

        private bool ValidarFormatoEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        [RelayCommand]
        private async Task AlterarFotoAsync()
        {
            string acao = await _dialogService.DisplayActionSheet(
                "Selecione uma opção",
                "Cancelar",
                null,
                "Tirar Foto",
                "Escolher da Galeria");

            if (acao == "Cancelar" || string.IsNullOrEmpty(acao)) return;

            try
            {
                string? caminhoTemporario = null;

                if (acao == "Tirar Foto")
                {
                    caminhoTemporario = await _mediaPickerService.CapturarFotoAsync();
                }
                else if (acao == "Escolher da Galeria")
                {
                    caminhoTemporario = await _mediaPickerService.SelecionarFotoGaleriaAsync();
                }

                if (caminhoTemporario != null)
                {
                    if (_pacienteIdAtual > 0)
                    {
                        string? caminhoSalvo = await _mediaManagerService.RegistrarFotoPacienteAsync(_pacienteIdAtual, caminhoTemporario);
                        if (caminhoSalvo != null)
                        {
                            FotoPerfilPath = caminhoSalvo;
                        }
                    }
                    else
                    {
                        FotoPerfilPath = caminhoTemporario;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar imagem: {ex.Message}");
            }
        }

    }
}