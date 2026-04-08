namespace IngestaoMed.Core.Interfaces
{
    public interface IDialogService
    {
        Task DisplayAlert(string title, string message, string cancel);
    }
}