using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.Chat;

namespace DocuMind.Application.Interface.IChat
{
    interface IChatService
    {
        Task<ServiceResult<SessionDto>> CreateChatAsync(int userId,CreateSessionDto dto
  );
    }
}
