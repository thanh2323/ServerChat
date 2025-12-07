using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Core.Interfaces.IPdf
{
    public interface IPdfProcessorService
    {
        string ExtractText(string filePath, CancellationToken cancellationToken = default);
        string ExtractCleanText(string filePath);
        List<string> ChunkByTokens(string text, int maxTokens, int overlap );
        bool ValidatePdf(string filePath);
    }
}
