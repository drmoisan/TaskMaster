using System;
using System.Drawing;
using System.IO;
using Tesseract;

namespace UtilitiesCS.EmailIntelligence
{
    /// <summary>
    /// Injectable seam for OCR text extraction from a bitmap. Introduced so that
    /// <see cref="ImageStripper"/> can be exercised in unit tests without constructing a
    /// live <see cref="Tesseract.TesseractEngine"/> (see issue #209).
    /// </summary>
    public interface IOcrTextExtractor
    {
        /// <summary>
        /// Extracts recognized text from the supplied bitmap using an OCR engine.
        /// </summary>
        /// <param name="bitmap">The image to run OCR against.</param>
        /// <returns>The recognized text, or an empty string when no text is found.</returns>
        string ExtractText(Bitmap bitmap);
    }

    /// <summary>
    /// Default <see cref="IOcrTextExtractor"/> implementation backed by the real Tesseract
    /// engine. This preserves the exact production behavior that previously lived directly
    /// in <see cref="ImageStripper.extract_text"/>.
    /// </summary>
    internal sealed class TesseractOcrTextExtractor : IOcrTextExtractor
    {
        /// <inheritdoc />
        public string ExtractText(Bitmap bitmap)
        {
            string tessdataPath =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{Path.DirectorySeparatorChar}TaskMaster{Path.DirectorySeparatorChar}tessdata";

            using (
                TesseractEngine engine = new TesseractEngine(
                    tessdataPath,
                    "eng",
                    EngineMode.Default
                )
            )
            {
                var page = engine.Process(bitmap);

                var text = page.GetText();
                return text;
            }
        }
    }
}
