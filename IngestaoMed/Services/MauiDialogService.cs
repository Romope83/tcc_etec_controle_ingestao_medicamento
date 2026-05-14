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
        public async Task<bool> DisplayConfirmationAsync(string title, string message, string accept, string cancel)
        {
            // O Shell.Current.DisplayAlert retorna true se clicar no 'accept' 
            // e false se clicar no 'cancel'
            return await Shell.Current.DisplayAlert(title, message, accept, cancel);
        }
        public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            if (Shell.Current != null)
            {
                return await Shell.Current.DisplayAlert(title, message, accept, cancel);
            }
            return false;
        }

    }
}