using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Interfaces.IPdf;
using Microsoft.Extensions.Logging;
using SharpToken;

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace DocuMind.Infrastructure.Services
{
    class PdfProcessorService : IPdfProcessorService
    {
        private readonly ILogger<PdfProcessorService> _logger;

        public PdfProcessorService(ILogger<PdfProcessorService> logger)
        {
            _logger = logger;
        }

        public string ExtractText(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("PDF file not found.", filePath);

                var textBuilder = new StringBuilder();

                using var reader = new PdfReader(filePath);
                using var pdfDoc = new PdfDocument(reader);

                var totalPages = pdfDoc.GetNumberOfPages();

                _logger.LogInformation("Processing PDF: {Pages} pages", totalPages);

                for (int i = 1; i <= totalPages; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var page = pdfDoc.GetPage(i);

                    var pageText = PdfTextExtractor.GetTextFromPage(page);

                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        textBuilder.AppendLine(pageText);
                        textBuilder.AppendLine();
                    }
                }
                var extractedText = textBuilder.ToString();
                if (string.IsNullOrWhiteSpace(extractedText))
                    throw new InvalidOperationException("No text could be extracted from PDF");

                _logger.LogInformation("Extracted {Characters} characters from PDF", extractedText.Length);

                return extractedText;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PDF: {FilePath}", filePath);
                throw;
            }
        }

        public string ExtractCleanText(string filePath)
        {
            var raw = ExtractText(filePath);

            // Fix broken hyphen words
            raw = Regex.Replace(raw, @"(\w+)-\s*\n(\w+)", "$1$2");

            // Merge broken lines inside paragraphs
            raw = Regex.Replace(raw, @"(?<!\n)\n(?!\n)", " ");

            // Normalize whitespace
            raw = Regex.Replace(raw, @"[ \t]+", " ");
            raw = Regex.Replace(raw, @"\n{3,}", "\n\n");

            // Normalize bullets
            raw = raw.Replace("•", "-");

            return raw.Trim();
        }
        public List<string> ChunkByTokens(string text, int maxTokens = 500, int overlap = 50)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            if (overlap >= maxTokens)
                throw new ArgumentException("overlap must be smaller than maxTokens");

            var encoding = GptEncoding.GetEncoding("cl100k_base");
            var tokens = encoding.Encode(text);

            var chunks = new List<string>();
            int start = 0;

            while (start < tokens.Count)
            {
                int end = Math.Min(start + maxTokens, tokens.Count);
                var chunkTokens = tokens.GetRange(start, end - start);

                var chunkText = encoding.Decode(chunkTokens);
                chunks.Add(chunkText);


                if (end == tokens.Count)
                    break;

                start = end - overlap;


                if (start <= 0)
                    start = 0;
            }
            _logger.LogInformation("Tokens: {TokenCount}", tokens.Count);
            _logger.LogInformation("Chunks: {ChunkCount}", chunks.Take(2));
            _logger.LogInformation("Chunks Context: {chunks}",
     JsonSerializer.Serialize(chunks, new JsonSerializerOptions
     {
         WriteIndented = true,
         Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
     })
 );

            return chunks;
        }



        public bool ValidatePdf(string filePath)
        {
            try
            {
                // 1. File existence
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("PDF file not found: {FilePath}", filePath);
                    return false;
                }

                // 2. Size limit (50MB)
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 50 * 1024 * 1024)
                {
                    _logger.LogWarning("PDF too large: {Size}", fileInfo.Length);
                    return false;
                }

                // 3. Deep validation
                using var reader = new PdfReader(filePath);
                using var pdfDoc = new PdfDocument(reader);

                if (reader.IsEncrypted())
                {
                    _logger.LogWarning("PDF encrypted: {FilePath}", filePath);
                    return false;
                }

                var pages = pdfDoc.GetNumberOfPages();
                if (pages <= 0)
                {
                    _logger.LogWarning("PDF has no pages: {FilePath}", filePath);
                    return false;
                }

                return true;
            }
            catch
            {
                _logger.LogWarning("PDF validation failed: {FilePath}", filePath);
                return false;
            }
        }
    }
}