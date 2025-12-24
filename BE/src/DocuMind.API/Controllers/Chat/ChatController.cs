using System.Security.Claims;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Chat;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.Document;
using DocuMind.Application.DTOs.User.Dashboard;
using DocuMind.Application.Interface.IDocument;
using DocuMind.Application.Interface.IRag;
using Microsoft.AspNetCore.Mvc;

namespace DocuMind.API.Controllers.Chat
{
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly IRagService _ragService;
        private readonly IDocumentService _documentService;
        private readonly ILogger<ChatController> _logger;
        public ChatController(IRagService ragService,IDocumentService documentService , ILogger<ChatController> logger)
        {

            _documentService = documentService;
            _ragService = ragService;
            _logger = logger;
        }

        [HttpPost("create-chat")]
        public async Task<IActionResult> CreateChat([FromBody] CreateSessionDto dto )
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var documentsResult =await _documentService.GetByIdsAsync(int.Parse(userId!), dto.DocumentIds);

            if (!documentsResult.Success)
                return BadRequest(ApiResponse<List<DocumentItemDto>>.ErrorResponse(documentsResult.Message));

            return Ok(ApiResponse<List<DocumentItemDto>>.SuccessResponse(documentsResult.Data!, documentsResult.Message));
        }
    }
}
