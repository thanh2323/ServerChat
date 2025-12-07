using DocuMind.Core.Interfaces.IPdf;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/test-pdf")]
public class TestPdfController : ControllerBase
{
    private readonly IPdfProcessorService _pdfService;

    public TestPdfController(IPdfProcessorService pdfService)
    {
        _pdfService = pdfService;
    }

    [HttpGet]
    public IActionResult Test()
    {
        var path = @"D:\tv.pdf";

        if (!_pdfService.ValidatePdf(path))
            return BadRequest("Invalid PDF");

        var text = _pdfService.ExtractCleanText(path);
        var chunks = _pdfService.ChunkByTokens(text,500, 50);

        return Ok(new
        {
            textLength = text.Length,
            chunks = chunks.Count,
            //show all text in chunks
            chunkContents = chunks

        });
    }
}
