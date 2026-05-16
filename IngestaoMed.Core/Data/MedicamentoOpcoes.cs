using System.Collections.Generic;

namespace IngestaoMed.Core.Data
{
    public static class MedicamentoOpcoes
    {
        public static List<string> UnidadesDosagem { get; } = new()
        {
            "Comprimido(s)",
            "Gota(s)",
            "ml (Mililitro)",
            "mg (Miligrama)",
            "Cápsula(s)",
            "Injeção / Ampola",
            "Nebulização / Jato",
            "Sachê / Envelope",
            "UI (Unidades Internacionais)",
            "Aplicação / Centímetro"
        };

        public static List<string> FormasIngestao { get; } = new()
        {
            "Via Oral (VO)",
            "Via Intravenosa (IV)",
            "Via Intramuscular (IM)",
            "Via Subcutânea (SC)",
            "Via Tópica / Cutânea",
            "Via Inalatória",
            "Via Otológica",
            "Via Oftálmica",
            "Via Nasal",
            "Via Sublingual (SL)"
        };
    }
}