using IngestaoMed.Core.Interfaces;

namespace IngestaoMed.Services
{
    public class MauiTimerService : ITimerService
    {
        private IDispatcherTimer _timer;

        public void Iniciar(TimeSpan intervalo, Action callback)
        {
            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = intervalo;
            _timer.Tick += (s, e) => callback();
            _timer.Start();
        }

        public void Parar() => _timer?.Stop();
    }
}