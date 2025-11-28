namespace DocuMind.Core.Entities
{
    public class SessionDocument
    {
        public int SessionId { get; set; }
        public int DocumentId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ChatSession Session { get; set; } = null!;
        public Document Document { get; set; } = null!;
    }
}