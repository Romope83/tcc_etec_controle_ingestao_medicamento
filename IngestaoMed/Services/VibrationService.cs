using IngestaoMed.Core.Interfaces;

namespace IngestaoMed.Services
{
    public class VibrationService : IVibrationService
    {
        public void VibrarSucesso()
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
        }

        public void VibrarAlerta()
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
        }
    }
}