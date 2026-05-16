using IngestaoMed.Core.Interfaces;
using Microsoft.Maui.Media;
using System;
using System.Threading.Tasks;

namespace IngestaoMed.Services
{
    public class MauiMediaPickerService : IMediaPickerService
    {
        public async Task<string?> CapturarFotoAsync()
        {
            if (!MediaPicker.Default.IsCaptureSupported)
                return null;

            var foto = await MediaPicker.Default.CapturePhotoAsync();
            return foto?.FullPath;
        }

        public async Task<string?> SelecionarFotoGaleriaAsync()
        {
            var foto = await MediaPicker.Default.PickPhotoAsync();
            return foto?.FullPath;
        }
    }
}