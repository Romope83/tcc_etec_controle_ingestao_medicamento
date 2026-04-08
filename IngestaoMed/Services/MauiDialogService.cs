using IngestaoMed.Core.Interfaces;

namespace IngestaoMed.Services
{
    public class MauiDialogService : IDialogService
    {
        public async Task DisplayAlert(string title, string message, string cancel)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert(title, message, cancel);
            }
        }
    }
}