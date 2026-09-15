using System;
using System.Collections.Generic;
using System.Text;

namespace RAGA.Domain.Entities
{
    public class Conversation
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Message> Messages { get; set; } = [];
    }
}
