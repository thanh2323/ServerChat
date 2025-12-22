using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Application.DTOs.Chat
{
    public class SessionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActiveAt { get; set; }
        public int MessageCount { get; set; }
        public List<SessionDocumentDto> Documents { get; set; } = new();
    }
}
