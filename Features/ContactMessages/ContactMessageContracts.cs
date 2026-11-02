using System.Text.Json.Serialization;
using TrustSystem.Backend.Models;

namespace TrustSystem.Backend.DTOs
{
    public class CreateContactMessageRequest
    {
        public required string fullName { get; set; }
        public required string emailAddress { get; set; }
        public required string phoneNumber { get; set; }
        public required string message { get; set; }
    }

    public record AgentViewContactMessages([property: JsonIgnore] List<ContactMessage> contactMessage, int totalCount)
    {
        public int totalResults { get; set; } = totalCount;
        public List<AgentViewContactMessage> contactMessages { get; set; } =
            contactMessage.Select(t => new AgentViewContactMessage(t)).ToList();
    }

    public record AgentViewContactMessage([property: JsonIgnore] ContactMessage contactMessage)
    {
        public int messageId { get; set; } = contactMessage.MessageId;
        public string fullName { get; set; } = contactMessage.FullName;
        public string emailAddress { get; set; } = contactMessage.EmailAddress;
        public string phoneNumber { get; set; } = contactMessage.PhoneNumber;
        public string message { get; set; } = contactMessage.Message;
        public DateTime? submissionDate { get; set; } = contactMessage.SubmissionDate;
        public bool isRead { get; set; } = contactMessage.IsRead == 1;
    }
}
