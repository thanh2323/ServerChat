using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.Document;

namespace DocuMind.Application.Interface.IDocument
{
    public interface IDocumentService
    {
        Task<ServiceResult<DocumentItemDto>> UploadDocument(UploadDocumentDto uploadDocumentDto, int userId);
        Task<ServiceResult<DocumentItemDto>> GetByIdAsync(int userId, int documentId, bool isAdmin);
        Task<ServiceResult<bool>> DeleteAsync(int userId, int documentId, bool isAdmin);
    }
}
