namespace DocuMind.Core.Entities
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsUser { get; set; } // true = user message, false = AI response
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int? TokenCount { get; set; } // Optional: track AI usage

        // Navigation Properties
        public ChatSession Session { get; set; } = null!;
    }
}