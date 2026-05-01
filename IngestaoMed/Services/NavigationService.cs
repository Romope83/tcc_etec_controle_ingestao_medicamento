using IngestaoMed.Core.Interfaces;

namespace IngestaoMed.Services
{
    public class NavigationService : INavigationService
    {
        public Task GoToAsync(string route)
        {
            return Shell.Current.GoToAsync(route);
        }

        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }



}
