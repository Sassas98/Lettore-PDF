using System.Speech.Synthesis;
using System.Text.Json;
using UglyToad.PdfPig;

namespace WinFormsLettorePDF
{
    public static class Funzioni
    {
        public static List<List<string>> Extract(string pdfPath)
        {
            var result = new List<List<string>>();

            using var document = PdfDocument.Open(pdfPath);

            foreach (UglyToad.PdfPig.Content.Page page in document.GetPages())
            {
                var pageLines = new List<string>();

                // Raggruppa le parole per riga usando la coordinata Y
                var wordsByLine = page
                    .GetWords()
                    .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
                    .OrderByDescending(g => g.Key);

                foreach (var line in wordsByLine)
                {
                    var lineText = string.Join(
                        " ",
                        line.OrderBy(w => w.BoundingBox.Left)
                            .Select(w => w.Text)
                    );

                    if (!string.IsNullOrWhiteSpace(lineText))
                        pageLines.Add(lineText);
                }

                result.Add(pageLines);
            }

            return result;
        }

        public static void ReadLine(string line)
        {
            using var synth = new SpeechSynthesizer();
            synth.SelectVoiceByHints(
                VoiceGender.NotSet,
                VoiceAge.NotSet,
                0,
                new System.Globalization.CultureInfo("it-IT")
            );
            synth.Speak(line.Fix());
        }

        public static string Fix(this string text)
        {
            string result = "";

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsLetterOrDigit(c)
                    && BetweenSpaces(text, i)) i++;
                result += c;
            }
            return result;
        }

        private static bool BetweenSpaces(string text, int index)
        {
            if(index < 0 || index >= text.Length) return false;
            bool p = index == 0 || char.IsWhiteSpace(text[index - 1]);
            bool n = index == text.Length - 1 || char.IsWhiteSpace(text[index + 1]);
            return p && n;
        }

        public class Memo
        {
            public int Page { get; set; }
            public int PageIndex { get; set; }
            public string Path { get; set; } = string.Empty;
        }

        public static void Save(string path, int page, int pageIndex)
        {
            File.WriteAllText(GetMemoPath(), JsonSerializer.Serialize(new Memo{Page = page, PageIndex = pageIndex, Path = path}));
        }
        public static (string, int, int) Load()
        {
            if(File.Exists(GetMemoPath()) == false) return (string.Empty, 0, 0);
            var lines = File.ReadAllLines(GetMemoPath());
            var memo = JsonSerializer.Deserialize<Memo>(string.Join("", lines));
            if(memo == null) return (string.Empty, 0, 0);
            return (memo.Path, memo.Page, memo.PageIndex);
        }
        private static string GetMemoPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDir = Path.Combine(appData, "PDFReader");
            Directory.CreateDirectory(appDir);
            return Path.Combine(appDir, "memo.json");

        }
    }
}
