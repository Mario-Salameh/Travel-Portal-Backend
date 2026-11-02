using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustSystem.Backend.Models;

[Table("TV_CONTACT_MESSAGES")]
public class ContactMessage
{
    [Key]
    [Column("MESSAGE_ID")]
    public int MessageId { get; set; }

    [Required]
    [Column("FULL_NAME")]
    [MaxLength(100)]
    public required string FullName { get; set; }

    [Required]
    [Column("EMAIL_ADDRESS")]
    [MaxLength(255)]
    public required string EmailAddress { get; set; }

    [Required]
    [Column("PHONE_NUMBER")]
    [MaxLength(20)]
    public required string PhoneNumber { get; set; }

    [Required]
    [Column("MESSAGE")]
    [MaxLength(4000)]
    public required string Message { get; set; }

    [Column("SUBMISSION_DATE")]
    public DateTime? SubmissionDate { get; set; }

    [Column("IS_READ")]
    public int IsRead { get; set; } = 0;
}