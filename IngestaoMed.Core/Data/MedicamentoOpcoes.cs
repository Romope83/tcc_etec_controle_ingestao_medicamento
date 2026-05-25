using System.Collections.Generic;

namespace IngestaoMed.Core.Data
{
    public static class MedicamentoOpcoes
    {
        public static List<string> UnidadesDosagem { get; } = new()
        {
            "Comprimido",
            "Gota",
            "ml",
            "mg",
            "Cápsula",
            "Injeção / Ampola",
            "Nebulização / Jato",
            "Sachê / Envelope",
            "UI (Unidades Internacionais)",
            "Aplicação / Centímetro"
        };

        public static List<string> FormasIngestao { get; } = new()
        {
            "Via Oral",
            "Via Intravenosa",
            "Via Intramuscular",
            "Via Subcutânea",
            "Via Tópica / Cutânea",
            "Via Inalatória",
            "Via Otológica",
            "Via Oftálmica",
            "Via Nasal",
            "Via Sublingual"
        };
    }
}