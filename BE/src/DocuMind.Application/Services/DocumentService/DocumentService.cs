using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.Document;
using DocuMind.Application.Interface.IDocument;
using DocuMind.Application.Options;
using DocuMind.Core.Entities;
using DocuMind.Core.Enum;
using DocuMind.Core.Interfaces.IBackgroundJob;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Core.Interfaces.IStorage;
using DocuMind.Core.Interfaces.IVectorDb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocuMind.Application.Services.DocumentService
{
    public class DocumentService : IDocumentService
    {
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IVectorDbService _vectorDbService;
        private readonly FileUploadOptions _options;
        private readonly IStorageService _storageService;
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<DocumentService> _logger;
        public DocumentService(
            IStorageService storageService,
            IBackgroundJobService backgroundJobService,
            IDocumentRepository documentRepository,
            IOptions<FileUploadOptions> options, 
            IVectorDbService vectorDbService,
            ILogger<DocumentService> logger)
        {
            _documentRepository = documentRepository;
            _storageService = storageService;
            _backgroundJobService = backgroundJobService;
            _vectorDbService = vectorDbService;
            _options = options.Value;
            _logger = logger;
        }
        public Task<ServiceResult<bool>> DeleteAsync(int userId, int documentId, bool isAdmin)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<DocumentItemDto>> GetByIdAsync(int userId, int documentId, bool isAdmin)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<DocumentItemDto>> UploadDocument(UploadDocumentDto dto, int userId)
        {
            var file = dto.File ?? throw new ArgumentException("No file uploaded");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_options.AllowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type");

            if (file.Length > _options.MaxFileSizeMB * 1024 * 1024)
                throw new ArgumentException($"File exceeds {_options.MaxFileSizeMB}MB");


            var supabasePath = await _storageService.UploadAsync(dto.File.OpenReadStream(), file.FileName, userId);
           
            var document = new Document
            {
                UserId = userId,
                FileName = file.FileName,
                FileSize = file.Length,
                FilePath = supabasePath,
                Status = DocumentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            _backgroundJobService.EnqueueDocumentProcessing(document.Id);

            var returnDto =  new DocumentItemDto
            {
                Id = document.Id,
                FileName = document.FileName,
                FileSize = document.FileSize,
                Status = document.Status,
                CreatedAt = document.CreatedAt
            };

            return ServiceResult<DocumentItemDto>.Ok(returnDto);
        }
    }
}
