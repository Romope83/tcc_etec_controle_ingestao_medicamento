using IngestaoMed.Core.Interfaces;
using Microsoft.Maui.Networking;

namespace IngestaoMed.Services
{
    public class ConnectivityService : IConnectivityService
    {
        public bool TemInternet => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
    }
}