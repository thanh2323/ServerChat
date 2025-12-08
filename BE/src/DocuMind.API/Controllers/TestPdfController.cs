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
        var path = @"D:\GameProject_Report_NguyenChiThanh.pdf";

        if (!_pdfService.ValidatePdf(path))
            return BadRequest("Invalid PDF");

        var text = _pdfService.ExtractCleanText(path);

        var chunks = _pdfService.ChunkSemantic(text,4000, 400);

        return Ok(new
        {
            textLength = text.Length,
            textContents = text,

       
            chunks = chunks.Count,
    
            //show all text in chunks
            chunkContents = chunks

        });
    }
}
