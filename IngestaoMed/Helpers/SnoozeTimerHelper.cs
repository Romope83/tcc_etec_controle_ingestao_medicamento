namespace IngestaoMed.Helpers
{
    public class SnoozeTimerHelper
    {
        private IDispatcherTimer _timer;
        public event Action<TimeSpan>? OnTick;
        public event Action? OnFinished;
        private DateTime _targetTime;

        public SnoozeTimerHelper(IDispatcher dispatcher)
        {
            _timer = dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => Tick();
        }

        public void Start(DateTime targetTime)
        {
            _targetTime = targetTime;
            _timer.Start();
        }

        public void Stop() => _timer.Stop();

        private void Tick()
        {
            var remaining = _targetTime - DateTime.Now;

            if (remaining.TotalSeconds <= 0)
            {
                _timer.Stop();
                OnFinished?.Invoke();
            }
            else
            {
                OnTick?.Invoke(remaining);
            }
        }
    }
}