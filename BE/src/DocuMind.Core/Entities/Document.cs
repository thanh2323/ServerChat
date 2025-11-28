using DocuMind.Core.Enum;

namespace DocuMind.Core.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; } // in bytes
        public string FilePath { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;
        public ICollection<SessionDocument> SessionDocuments { get; set; } = new List<SessionDocument>();
    }
}