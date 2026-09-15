using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }

        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Conversation Conversation { get; set; } = null!;
    }
}
