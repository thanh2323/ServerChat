using System.Security.Claims;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Chat;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.Document;
using DocuMind.Application.DTOs.User.Dashboard;
using DocuMind.Application.Interface.IChat;
using DocuMind.Application.Interface.IDocument;
using DocuMind.Application.Interface.IRag;
using DocuMind.Core.Interfaces.IRepo;
using Microsoft.AspNetCore.Mvc;

namespace DocuMind.API.Controllers.Chat
{
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly IRagService _ragService;
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        public ChatController(IRagService ragService,IChatService chatService , ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _ragService = ragService;
            _logger = logger;
        }

        [HttpPost("create-chat")]
        public async Task<IActionResult> CreateChat([FromBody] CreateSessionDto dto )
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result =await _chatService.CreateChatAsync(int.Parse(userId!), dto);

            if (!result.Success)
                return BadRequest(ApiResponse<SessionDto>.ErrorResponse(result.Message));

            return Ok(ApiResponse<SessionDto>.SuccessResponse(result.Data!, result.Message));
        }
    }
}
