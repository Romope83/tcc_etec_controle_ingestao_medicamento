using System.Text.RegularExpressions;

namespace IngestaoMed.Core.Extensions
{
    public static class StringExtensions
    {
        public static string FormatarTelefone(this string? telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return string.Empty;

            string numeros = Regex.Replace(telefone, @"[^\d]", "");

            if (numeros.Length == 11)
            {
                return $"({numeros.Substring(0, 2)}) {numeros.Substring(2, 5)}-{numeros.Substring(7)}";
            }
            if (numeros.Length == 10)
            {
                return $"({numeros.Substring(0, 2)}) {numeros.Substring(2, 4)}-{numeros.Substring(6)}";
            }

            return telefone;
        }
    }
}