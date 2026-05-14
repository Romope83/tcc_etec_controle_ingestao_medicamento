using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngestaoMed.Core.DTOs;
using IngestaoMed.Core.Interfaces;
using System.Collections.ObjectModel;

namespace IngestaoMed.Core.ViewModels
{
    public partial class WelcomeViewModel : ObservableObject
    {
        private readonly INavigationService _navegacao;
        private readonly IConfigService _configuracao;

        [ObservableProperty]
        private ObservableCollection<OnboardingItem> slides = new();

        [ObservableProperty]
        private int posicaoAtual;

        [ObservableProperty]
        private string textoBotaoPrincipal = "Próximo";

        [ObservableProperty]
        private bool ehUltimoSlide;

        [ObservableProperty]
        private bool mostrarBotaoPular = true;

        public WelcomeViewModel(INavigationService navegacao, IConfigService configuracao)
        {
            _navegacao = navegacao;
            _configuracao = configuracao;

            CarregarSlides();
        }

        private void CarregarSlides()
        {
            Slides.Add(new OnboardingItem
            {
                Titulo = "Bem-vindo",
                Descricao = "Cuidar de quem você ama nunca foi tão seguro.",
                Imagem = "boas_vindas_mae_filha.jpg"
            });
            Slides.Add(new OnboardingItem
            {
                Titulo = "Segurança",
                Descricao = "Notificamos seu cuidador caso você esqueça a dose.",
                Imagem = "seguranca_onboarding.png"
            });
            Slides.Add(new OnboardingItem
            {
                Titulo = "Notificação ao Cuidador",
                Descricao = "Se uma dose for esquecida, o cuidador é avisado automaticamente por e-mail",
                Imagem = "seguranca_onboarding.png"
            });
        }

        // Este método roda automaticamente sempre que a PosicaoAtual mudar (via CarouselView)
        partial void OnPosicaoAtualChanged(int value)
        {
            EhUltimoSlide = value == Slides.Count - 1;
            TextoBotaoPrincipal = EhUltimoSlide ? "Começar Agora" : "Próximo";
            MostrarBotaoPular = !EhUltimoSlide;
        }

        [RelayCommand]
        private async Task AvancarOuFinalizar()
        {
            if (EhUltimoSlide)
            {
                await FinalizarApresentacao();
            }
            else
            {
                PosicaoAtual++;
            }
        }

        [RelayCommand]
        private async Task FinalizarApresentacao()
        {
            _configuracao.EhPrimeiroAcesso = false;
            await _navegacao.GoToAsync("//CadastroPage");
        }
    }
}