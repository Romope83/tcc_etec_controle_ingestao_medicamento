using System.Globalization;
using IngestaoMed.Core.Constants;
using Microsoft.Maui.Graphics;

namespace IngestaoMed.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int tentativas)
            {
                if (tentativas == 0) return Colors.Green;

                if (tentativas < NotificationConstants.LimiteMaximoSonecas) return Colors.Orange;

                return Colors.Red;
            }

            return Colors.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}