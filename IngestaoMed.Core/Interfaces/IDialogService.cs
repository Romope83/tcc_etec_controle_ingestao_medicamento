namespace IngestaoMed.Core.Interfaces
{
    public interface IDialogService
    {
        Task DisplayAlert(string title, string message, string cancel);

        Task<bool> DisplayConfirmationAsync(string title, string message, string accept, string cancel);


    }



}