using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Chat;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.Interface.IChat;
using DocuMind.Application.Interface.IDocument;
using DocuMind.Application.Services.DocumentService;
using DocuMind.Core.Entities;
using DocuMind.Core.Interfaces.IRepo;

namespace DocuMind.Application.Services.ChatService
{
    public class ChatService : IChatService
    {
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly ISessionDocumentRepository _sessionDocumentRepository;

        private readonly IDocumentService _documentService;

        public ChatService(ISessionDocumentRepository sessionDocumentRepository, IDocumentService documentService, IChatSessionRepository chatSessionRepository)
        {
            _sessionDocumentRepository = sessionDocumentRepository;
            _documentService = documentService;
            _chatSessionRepository = chatSessionRepository;
        }
        public async Task<ServiceResult<SessionDto>> CreateChatAsync(int userId, CreateSessionDto dto)
        {
            var documents = await _documentService.GetByIdsAsync(userId, dto.DocumentIds);

            if (documents == null)
                return ServiceResult<SessionDto>.Fail("One or more documents are not processed yet or encountered an error");

            var session = new ChatSession
            {
                UserId = userId,
                Title = dto.Title,
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow
            };
            await _chatSessionRepository.AddAsync(session);
            await _chatSessionRepository.SaveChangesAsync();

            var sessionDocuments = dto.DocumentIds.Select(docId => new SessionDocument
            {
                SessionId = session.Id,
                DocumentId = docId,
                AddedAt = DateTime.UtcNow
            }).ToList();

            await _sessionDocumentRepository.AddRangeAsync(sessionDocuments);
            await _sessionDocumentRepository.SaveChangesAsync();

            var sessionDto = new SessionDto
            {
                Id = session.Id,
                Title = session.Title,
                CreatedAt = session.CreatedAt,
                LastActiveAt = session.LastActiveAt,
                MessageCount = 0,
                Documents = documents.Data!
            };
            return ServiceResult<SessionDto>.Ok(sessionDto);


        }
    }
}
